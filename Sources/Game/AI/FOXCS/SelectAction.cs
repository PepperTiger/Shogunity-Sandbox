using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sandbox.Sources.Game.AI.FOXCS
{
    public partial class FOXCS
    {
        /// <summary>
        /// Sélectionne la meilleur action ou une action aléatoire selon la probabilité d'exploration <see cref="FOXCSOptions.explorationProb"/>
        /// </summary>
        /// <param name="predicArray"></param>
        /// <returns>L'action à réaliser.</returns>
        private Attribute selectAction(Dictionary<Attribute, double> predicArray)
        {
            Random rand = new Random();
            if (rand.NextDouble() <= fo.explorationProb && fo.learning)
            {
                Console.WriteLine("Random action chosed.");
                return ImmediateBestMove(predicArray.Keys.ToList());
            }
            else
            {
                KeyValuePair<List<Attribute>, double> accAction = MaxKey(predicArray);
                Console.WriteLine("Best action chosed");
                if (accAction.Key.Count == 1)
                    return accAction.Key[0];
                else
                    return ImmediateBestMove(accAction.Key);
            }
        }

        private KeyValuePair<List<T>, double> MaxKey<T>(Dictionary<T, double> keyValues)
        {
            double Max = Double.MinValue;
            List<T> acc = new List<T>();
            foreach(KeyValuePair<T, double> keyValue in keyValues)
            {
                if (keyValue.Value >= Max)
                {
                    if (keyValue.Value != Max)
                        acc.Clear();
                    acc.Add(keyValue.Key);
                    Max = keyValue.Value;
                }
            }
            return new KeyValuePair<List<T>, double>(acc, Max);
        }

        private Attribute ImmediateBestMove(List<Attribute> actions)
        {
            List<Attribute> keys = new List<Attribute>();
            double acc = 0;
            double reward;
            foreach(Attribute action in actions)
            {
                if((reward = _GameManager.getRewardOfAction(AttributeToAction(action), fo.rewardMode)) >= acc) {
                    if (reward != acc)
                        keys.Clear();
                    keys.Add(action);
                    acc = reward;
                }
            }

            Random rand = new Random();
            return keys[rand.Next(keys.Count)];
        }
    }
    
}
