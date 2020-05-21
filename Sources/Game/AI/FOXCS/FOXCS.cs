using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ShogiUtils;

namespace Sandbox.Sources.Game.AI.FOXCS
{
    /// <summary>
    /// Fichier principal de FOXCS, contenant les variables le constructeur et l'algorithme principal de traitement.
    /// Pour plus d'informations, suivre les liens ci-dessous :
    /// </summary>
    public partial class FOXCS : AIHandler
    {
        /// <summary>
        /// Options de FOXCS, comprenant : 
        /// - Les constantes d'apprentissage.
        /// - Les informations des prédicats et de ses arguments.
        /// </summary>
        private FOXCSOptions fo;

        /// <summary>
        /// L'environnement de FOXCS, représenté sous forme de prédicats.
        /// </summary>
        private PerceivedEnvironnement env;

        /// <summary>
        /// Environnement précédent.
        /// </summary>
        private PerceivedEnvironnement prevEnv;

        /// <summary>
        /// Temps actuel d'une partie, représenté par le nombre de tours déjà réalisé.
        /// </summary>
        private int currentTime;

        /// <summary>
        /// Ensemble des classifieurs existant à l'instant même dans FOXCS.
        /// </summary>
        private List<Classifier> popSet;

        /// <summary>
        /// Ensemble des classifieurs de la populatution <see cref="popSet"/> correspondant à l'environnement actuel. 
        /// Cet ensemble est constitué de sous-ensembles non-disjoints de classifieurs selon les différentes actions qu'un ou plusieurs classifieurs peuvent correspondre. 
        /// </summary>
        private Dictionary<Attribute, List<Classifier>> matchSet;

        /// <summary>
        /// Ensemble des classifieurs, constitué d'un des sous-ensemnle de <see cref="matchSet"> à partir de l'action déterminée lors de la procédure <see cref="selectAction(Dictionary{Attribute, double})"/>. 
        /// </summary>
        private List<Classifier> actionSet;

        /// <summary>
        /// Occurence précédente de <see cref="actionSet"/>.
        /// </summary>
        private List<Classifier> prevActionSet;

        /// <summary>
        /// Récompense reçue au moment actuel du traitement.
        /// </summary>
        private double reward;

        /// <summary>
        /// Précédente récompense reçue
        /// </summary>
        private double prevReward;

        /// <summary>
        /// Compteur du nombres de variables selon leur type, utile pour l'initialisation de nouvelles variables nécessaires aux prédicats des <see cref="Classifier"/>.
        /// </summary>
        private Dictionary<ArgType, int> varCount;

        private Dictionary<ArgType, Dictionary<string, string>> boundVarList;

        private bool boundVarCheck = false;

        /// <summary>
        /// Constructeur d'une instance de FOXCS et initialise ses variables.
        /// </summary>
        /// <param name="fo"> </param>
        public FOXCS(FOXCSOptions fo)
        {
            this.env = new PerceivedEnvironnement();
            this.prevEnv = new PerceivedEnvironnement();
            this.currentTime = 0;
            this.popSet = new List<Classifier>();
            this.matchSet = new Dictionary<Attribute, List<Classifier>>();
            this.actionSet = new List<Classifier>();
            this.prevActionSet = new List<Classifier>();
            this.varCount = new Dictionary<ArgType, int>();
            this.boundVarList = new Dictionary<ArgType, Dictionary<string, string>>();
            this.reward = 0;
            this.prevReward = 0;
            this.fo = fo;

            if (!File.Exists(fo.classifierFilePath))
            {
                FileStream fsCreate = File.Create(fo.classifierFilePath);
                fsCreate.Close();
            }

            LoadFOXCSData();

            foreach (ArgType argType in (ArgType[])Enum.GetValues(typeof(ArgType)))
            {
                if (!varCount.Keys.Contains(argType))
                    varCount.Add(argType, 0);
                if (!boundVarList.Keys.Contains(argType))
                    boundVarList.Add(argType, new Dictionary<string, string>());
            }
        }

        /// <summary>
        /// Réimplémentation de la fonction Run de la classe IAHandler"/>
        /// </summary>
        /// <param name="oppReward"></param>
        public override void Run()
        {
            if (_GameManager.toPromoteToken == null)
                moveToPlay = (Move)FOXCSRun();
            else
                toPromote = (bool)FOXCSRun();
            isDone = true;
        }

        /// <summary>
        /// Procédure principale de traitement de FOXCS.
        /// </summary>
        /// <param name="oppReward"></param>
        /// <returns></returns>
        public object FOXCSRun()
        {
            double predictedPayoff = 0;
            Dictionary<Attribute, double> predictArray = new Dictionary<Attribute, double>();
            Attribute action = null;

            //try
            //{
            LoadFOXCSData();
            env.UpdateSensors(_GameManager.board, fo);
            UpdateBoundVarList();
            boundVarCheck = true;
            currentTime++;

            if (!_GameManager.endOfGame)
            {
                GetMatchSet();
                predictArray = GeneratePredictionArray();
                action = selectAction(predictArray);
                GetActionSet(action);
                prevEnv = env;
            }

            reward = _GameManager.getRewardOfAction(AttributeToAction(action), fo.rewardMode) - _GameManager.lastRewardPlayer[(_GameManager.currentPlayerIndex + 1) % 2];
            Console.WriteLine("Received reward : " + reward);

            if (prevActionSet.Count != 0 && fo.learning)
            {
                predictedPayoff = prevReward + fo.discountFactor * MaxPredict(predictArray);
                prevActionSet = UpdateActionSet(prevActionSet, predictedPayoff);
                RunGeneticAlgorithm(prevActionSet, prevEnv);
            }

            if (fo.rewardMode && reward == Math.Abs(1) || !fo.rewardMode && reward > Math.Abs(400))
            {
                if (fo.learning)
                {
                    predictedPayoff = reward;
                    actionSet = UpdateActionSet(actionSet, predictedPayoff);
                    RunGeneticAlgorithm(actionSet, env);
                }
                matchSet.Clear();
                actionSet.Clear();
                prevActionSet.Clear();
                prevReward = 0;
            }
            else
            {
                prevActionSet = ObjectCopier.Clone(actionSet);
                prevReward = reward;
                prevEnv = ObjectCopier.Clone(env);
            }

            Console.WriteLine("Predicted payoff : " + predictedPayoff);


            SaveFOXCSData();
            WritePopSet();

            //}
            /*catch (Exception e)
            {
                Console.WriteLine(e.Message);
                SaveFOXCSData();
                WritePopSet();
                Console.Read();
            }*/

            return action == null ? action : AttributeToAction(action);
        }

    }
}

