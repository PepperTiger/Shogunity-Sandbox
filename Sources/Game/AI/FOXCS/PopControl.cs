using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sandbox.Sources.Game.AI.FOXCS
{
    public partial class FOXCS
    {
        private void DeleteFromPopulation()
        {
            int popSize = 0;
            double fitnessSum = 0;
            foreach (Classifier cl in popSet)
            {

                popSize += cl.numerosity;
                fitnessSum += cl.fitness;
            }

            while (popSize > fo.popMaxSize)
            {
                double averageFitnessInPop = fitnessSum / popSize;

                double voteSum = 0;
                foreach (Classifier cl in popSet)
                    voteSum += DeletionVote(cl, averageFitnessInPop);

                double choicePoint = new Random().NextDouble() * voteSum;
                voteSum = 0;

                for (int i = 0; i < popSet.Count; i++)
                {
                    voteSum += DeletionVote(popSet[i], averageFitnessInPop);
                    if (voteSum > choicePoint)
                    {
                        if (popSet[i].numerosity > 1)
                            popSet[i].numerosity--;
                        else
                        {
                            Classifier accCl = popSet[i];
                            popSet.RemoveAt(i);
                            if(matchSet.Count != 0)
                            {
                                foreach (KeyValuePair<Attribute, List<Classifier>> matchSubSet in ObjectCopier.Clone(matchSet))
                                {
                                    if (matchSubSet.Value.Contains(accCl))
                                    {
                                        matchSubSet.Value.RemoveAll(cl => cl.rule == accCl.rule);
                                        if (matchSubSet.Value.Count == 0)
                                            matchSet.Remove(matchSubSet.Key);
                                    }
                                }
                            }
                        }
                            
                        break;
                    }

                    popSize = 0;
                    fitnessSum = 0;
                    foreach (Classifier cl in popSet)
                    {

                        popSize += cl.numerosity;
                        fitnessSum += cl.fitness;
                    }
                }
            }
        }

        private double DeletionVote(Classifier cl, double averageFitnessInPopulation)
        {
            double vote = cl.averSize * cl.numerosity;
            if (cl.exp > fo.deleteThresh && cl.fitness / cl.numerosity < fo.fitnessThresh * averageFitnessInPopulation)
                vote += averageFitnessInPopulation / (cl.fitness / cl.numerosity);
            return vote;
        }
    }
}
