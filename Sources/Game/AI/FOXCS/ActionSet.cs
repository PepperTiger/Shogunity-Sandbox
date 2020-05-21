using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sandbox.Sources.Game.AI.FOXCS
{
    public partial class FOXCS
    {
        /// <summary>
        /// Initialise <see cref="FOXCS.actionSet"/> à partir de la liste <see cref="FOXCS.matchSet"/> et une action donnée en paramêtre.
        /// </summary>
        /// <param name="action">Action servant d'index pour l'ensemble de concordance.</param>
        private void GetActionSet(Attribute action)
        {
            Console.WriteLine("Chosed action : " + action);
            actionSet = ObjectCopier.Clone(matchSet[action]);
            Console.WriteLine("Current action set : " + actionSet.Count);
        }

        /// <summary>
        /// Met à jour la prédiciton de récompense <see cref="Classifier.payOffPred"/>, d'erreur <see cref="Classifier.errorPred"/> et la taille moyenne de l'ensemble d'actions <see cref="Classifier.averSize"/>.
        /// </summary>
        /// <param name="actionSet">Ensemble d'action à traiter.</param>
        /// <param name="reward">Récompense prédite perçue.</param>
        /// <returns></returns>
        private List<Classifier> UpdateActionSet(List<Classifier> toUpdateActionSet, double reward)
        {
            List<Classifier> actionSet = ObjectCopier.Clone(toUpdateActionSet);
            for (int i = 0; i < actionSet.Count; i++)
            {
                double sum = 0;
                actionSet[i].exp++;

                if (actionSet[i].exp < 1 / fo.learningRate)
                {
                    actionSet[i].payOffPred += (reward - actionSet[i].payOffPred) / actionSet[i].exp;
                    actionSet[i].errorPred += (Math.Abs(reward - actionSet[i].payOffPred) - actionSet[i].errorPred) / actionSet[i].exp;
                    foreach (Classifier cl in actionSet)
                        sum += cl.numerosity;
                    actionSet[i].averSize += (sum - actionSet[i].averSize) / actionSet[i].exp;
                }
                else
                {
                    actionSet[i].payOffPred += fo.learningRate * (reward - actionSet[i].payOffPred);
                    actionSet[i].errorPred += fo.learningRate * (Math.Abs(reward - actionSet[i].payOffPred) - actionSet[i].errorPred);
                    foreach (Classifier cl in actionSet)
                        sum += cl.numerosity;
                    actionSet[i].averSize += fo.learningRate * (sum - actionSet[i].averSize);
                }
            }
            List<Classifier> updatedActionSet = UpdateActionSetFitness(actionSet);
            if (fo.doActionSetSubsumption)
                SubsumptionActionSet();
            ReplacePopFromActionSet(updatedActionSet);
            return updatedActionSet;
        }

        /// <summary>
        /// Remplace les classifieurs correspondants de l'ensemble de population par ceux de l'ensemble d'action. 
        /// </summary>
        /// <param name="actionSet">Ensemble d'action à traiter.</param>
        private void ReplacePopFromActionSet(List<Classifier> actionSet)
        {
            for (int i = 0; i < popSet.Count; i++)
            {
                for(int j = 0; j < actionSet.Count; j++)
                {
                    if (popSet[i].rule.Equals(actionSet[j].rule))
                    {
                        popSet[i] = actionSet[j];
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Met à jour l'aptitude de chaque classifieur présent dans l'ensemble d'action.
        /// </summary>
        /// <param name="actionSet">Ensemble d'action à traiter.</param>
        /// <returns></returns>
        private List<Classifier> UpdateActionSetFitness(List<Classifier> actionSet)
        {
            double accuracySum = 0;
            Dictionary<Classifier, double> accuracySet = new Dictionary<Classifier, double>();

            foreach (Classifier cl in actionSet)
            {
                if (cl.errorPred < fo.errorThresh)
                    accuracySet.Add(cl, 1);
                else
                    accuracySet.Add(cl, fo.alpha * Math.Pow(cl.errorPred / fo.errorThresh, -fo.power));
                accuracySum += accuracySet[cl] * cl.numerosity;
            }

            for (int i = 0; i < actionSet.Count; i++)
                actionSet[i].fitness += fo.learningRate * (accuracySet[actionSet[i]] * actionSet[i].numerosity / accuracySum - actionSet[i].fitness);

            return actionSet;
        }

        private void SubsumptionActionSet()
        {
            throw new NotImplementedException();
        }
    }
}
