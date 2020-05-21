using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sandbox.Sources.Game.AI.FOXCS
{
    public partial class FOXCS
    {
        /// <summary>
        /// Généère la prédiction de récompense de chaque action à partir de la prédiction de récompense et l'aptitude de chaque classifieur dans l'ensemble de concordance.
        /// </summary>
        /// <returns>Liste des prédictions selon chaque action.</returns>
        private Dictionary<Attribute, double> GeneratePredictionArray()
        {
            Dictionary<Attribute, double> predicArray = new Dictionary<Attribute, double>();
            Dictionary<Attribute, double> sumArray = new Dictionary<Attribute, double>();

            foreach (KeyValuePair<Attribute, List<Classifier>> matchSubSet in matchSet)
            {
                foreach (Classifier cl in matchSubSet.Value)
                {
                    if (!predicArray.ContainsKey(matchSubSet.Key))
                        predicArray.Add(matchSubSet.Key, 0);
                    predicArray[matchSubSet.Key] += cl.payOffPred * cl.fitness;

                    if (!sumArray.ContainsKey(matchSubSet.Key))
                        sumArray.Add(matchSubSet.Key, 0);
                    sumArray[matchSubSet.Key] += cl.fitness;
                }
            }

            foreach (KeyValuePair<Attribute, List<Classifier>> matchSubSet in matchSet)
                if(sumArray[matchSubSet.Key] != 0)
                    predicArray[matchSubSet.Key] /= sumArray[matchSubSet.Key];

            return predicArray;
        }

        private double MaxPredict(Dictionary<Attribute, double> predicArray)
        {
            double max = Double.MinValue;
            foreach(Attribute attribute in predicArray.Keys)
            {
                if (predicArray[attribute] > max)
                    max = predicArray[attribute];
            }

            return max;
        }
    }
}
