using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShogiUtils;

namespace Sandbox.Sources.Game.AI.FOXCS
{
    /// <summary>
    /// Un classifieur, comprenant une règle et des variables montrant sa capacité à survivre.
    /// </summary>
    [Serializable]
    public class Classifier : IEquatable<Classifier>
    {
        /// <summary>
        /// Règle du classifieur
        /// </summary>
        public HornClause rule { get; set; }
        
        /// <summary>
        /// Prédicition de la récompense perçue.
        /// </summary>
        public double payOffPred { get; set; }

        /// <summary>
        /// Prédiction de l'erreur faire dans le calcul de la prédiction de la récompense.
        /// </summary>
        public double errorPred { get; set; }

        /// <summary>
        /// Aptitude d'un classifieur, utilisé pour renforcer la survie d'un classifieur dans une population.
        /// </summary>
        public double fitness { get; set; }

        /// <summary>
        /// Expérience du classifieur, représentant le nombre d'occurences du classifieur dans l'ensemble d'actions <see cref="FOXCS.actionSet"/>.
        /// </summary>
        public int exp { get; set; }

        /// <summary>
        /// Temps de la dernière occurence de l'algorithme génétique dans un ensemble d'actions contenant ce classifieur.
        /// </summary>
        public int timeStamp { get; set; }

        /// <summary>
        /// Taille moyenne des ensembles d'actions ayant contenu ce classifieur.
        /// </summary>
        public double averSize { get; set; }

        /// <summary>
        /// Nombre de micro-classifieurs contenanu dans un macro-classifieur.
        /// </summary>
        public int numerosity { get; set; }

        /// <summary>
        /// Constructeur d'un classifieur.
        /// </summary>
        /// <param name="timeStamp">Moment de création</param>
        /// <param name="rule">Règle définissant le classifieur></param>
        /// <param name="fo"></param>
        public Classifier(int timeStamp, HornClause rule, FOXCSOptions fo)
        {
            this.payOffPred = 0;
            this.errorPred = 0.01 * (fo.rewardMode == true ? 1 : 504);
            this.fitness = 0;
            this.exp = 0;
            this.timeStamp = timeStamp;
            this.averSize = 1;
            this.numerosity = 1;
            this.rule = rule;
        }

        /// <summary>
        /// Constructeur d'un classifieur à partir d'un classifieur parent.
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="rule"></param>
        /// <param name="fo"></param>
        /// <param name="parent"></param>
        public Classifier(int timeStamp, HornClause rule, FOXCSOptions fo, Classifier parent)
        {
            this.payOffPred = parent.payOffPred;
            this.errorPred = parent.errorPred;
            this.fitness = 0.1 * parent.fitness;
            this.exp = 0;
            this.timeStamp = timeStamp;
            this.averSize = 1;
            this.numerosity = 1;
            this.rule = rule;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Classifier);
        }

        public bool Equals(Classifier other)
        {
            return other != null &&
                   rule.Equals(other.rule);
        }

        public override int GetHashCode()
        {
            return 1140682955 + EqualityComparer<HornClause>.Default.GetHashCode(rule);
        }

        public static bool operator ==(Classifier classifier1, Classifier classifier2)
        {
            return EqualityComparer<Classifier>.Default.Equals(classifier1, classifier2);
        }

        public static bool operator !=(Classifier classifier1, Classifier classifier2)
        {
            return !(classifier1 == classifier2);
        }

        public override string ToString()
        {
            return rule.ToString();
        }

        public string ToStringFile()
        {
            string acc = "";
            int center = (rule.body.Length) / 2;
            for(int i=0; i<rule.body.Length; i++)
            {
                if (i == 0)
                    acc += rule.head.ToPrologCode() + ":- \n";

                acc += "\t" + rule.body[i].ToPrologCode() + (i==rule.body.Length-1 ? ".": ",");
                if (i == center)
                    acc += "   " + Math.Round(payOffPred, 2) + 
                        "   " + Math.Round(errorPred, 2) +
                        "   " + Math.Round(fitness, 2) + 
                        "   " + exp + 
                        "   " + timeStamp + 
                        "   " + Math.Round(averSize,2) + 
                        "   " + numerosity;
                acc += "\n";
            }

            return acc;
        }
    }

}