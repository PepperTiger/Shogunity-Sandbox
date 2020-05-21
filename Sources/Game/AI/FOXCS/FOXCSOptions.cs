using System;
using System.Collections.Generic;
using YieldProlog;

namespace Sandbox.Sources.Game.AI.FOXCS
{
    /// <summary>
    /// Classe contenant toutes les variables nécessaires au fonctionnement de FOXCS.
    /// </summary>
    [Serializable]
    public class FOXCSOptions
    {
        public readonly string name;

        /// <summary>
        /// Nombre de classifieurs maximum présent dans le <see cref="FOXCS.popSet"/>?.
        /// </summary>
        public readonly int popMaxSize;

        /// <summary>
        /// Taux d'apprentissage des variables <see cref="Classifier.payOffPred"/>, <see cref="Classifier.errorPred"/>, <see cref="Classifier.fitness"/> et <see cref="Classifier.averSize"/>.
        /// </summary>
        public readonly double learningRate;

        /// <summary>
        /// Constante utilisé dans le calcul de <see cref="Classifier.fitness"/>.
        /// </summary>
        public readonly double alpha;

        /// <summary>
        /// Constante utilisé dans le calcul de <see cref="Classifier.fitness"/>.
        /// </summary>
        public readonly double errorThresh;

        /// <summary>
        /// Constante utilisé dans le calcul de <see cref="Classifier.fitness"/>.
        /// </summary>
        public readonly int power;

        /// <summary>
        /// Constante utilisé dans le calcul de <see cref="Classifier.payOffPred"/> et <see cref="Classifier.errorPred"/>
        /// </summary>
        public readonly double discountFactor;

        /// <summary>
        /// Limite de déclenchement de l'algorithme génétique.
        /// </summary>
        public readonly int geneticThresh;
        public readonly double mutationProb;
        public readonly int deleteThresh;
        public readonly double fitnessThresh;
        public readonly int subsumptionThresh;
        public readonly double explorationProb;
        public readonly bool doGASubsumption;
        public readonly bool doActionSetSubsumption;

        public readonly string classifierFilePath;

        public readonly Dictionary<string, PredicateOptions> actionPredicateOptions;
        public readonly Dictionary<string, PredicateOptions> statePredicateOptions;

        public readonly HornClause[] knowledgeBase;

        public readonly RewardType rewardType;

        public readonly MutationRange probMutationRange;

        public readonly int minPredicate;
        public readonly int maxPredicate;

        public readonly bool learning;
        public readonly bool rewardMode;

        public FOXCSOptions
            (
            string name,
            int popMaxSize, double learningRate, double alpha, double errorThresh,
            int power, double discountFactor, int geneticThresh,
            int deleteThresh, double fitnessThresh, int subsumptionThresh,
            double explorationProb, bool doGASubsumption,
            bool doActionSetSubsumption,
            string classifierFilePath,
            Dictionary<string, PredicateOptions> statePredicateOptions,
            Dictionary<string, PredicateOptions> actionPredicateOptions,
            HornClause[] knowledgeBase,
            double[] mutationWeights,
            int minPredicate,
            int maxPredicate,
            bool learning,
            bool rewardMode
            )
        {
            this.name = name;
            this.popMaxSize = popMaxSize;
            this.learningRate = learningRate;
            this.alpha = alpha;
            this.errorThresh = errorThresh;
            this.power = power;
            this.discountFactor = discountFactor;
            this.geneticThresh = geneticThresh;
            this.deleteThresh = deleteThresh;
            this.fitnessThresh = fitnessThresh;
            this.subsumptionThresh = subsumptionThresh;
            this.explorationProb = explorationProb;
            this.doGASubsumption = doGASubsumption;
            this.doActionSetSubsumption = doActionSetSubsumption;
            this.classifierFilePath = classifierFilePath;
            this.statePredicateOptions = statePredicateOptions;
            this.actionPredicateOptions = actionPredicateOptions;
            this.knowledgeBase = knowledgeBase;
            this.probMutationRange = new MutationRange(mutationWeights);
            this.minPredicate = minPredicate;
            this.maxPredicate = maxPredicate;
            this.learning = learning;
            this.rewardMode = rewardMode;
        }
    }
    
    [Serializable]
    public class PredicateOptions 
    {

        public readonly int min;
        public readonly int max;

        public readonly bool isNegetable;

        public readonly ArgOptions[] argsOptions;

        public PredicateOptions(int min, int max, bool isNegetable, ArgOptions[] argsOptions)
        {
            this.min = min;
            this.max = max;
            this.isNegetable = isNegetable;
            this.argsOptions = argsOptions;
        }
    }

    [Serializable]
    public class ArgOptions
    {
        public readonly ArgType argType;
        public readonly ArgMode[] argsMode;

        public double probVar;

        public ArgOptions(ArgType argType, ArgMode[] argsMode, double probVar)
        {
            this.argType = argType;
            this.argsMode = argsMode;
            this.probVar = probVar;
        }
    }

    [Serializable]
    public enum ArgType
    {
        TOKEN,
        X,
        Y,
        BOOL
    }

    [Serializable]
    public enum ArgMode
    {
        CONST,
        VAR,
        BOUND,
        ANONYMOUS
    }

    [Serializable]
    public enum RewardType
    {
        INT,
        DOUBLE_NEG,
        DOUBLE_POS
    }
    
    [Serializable]
    public class Range
    {
        public double min { get; set; }
        public double max { get; set; }

        public Range(double min, double range)
        {
            this.min = min;
            this.max = min+range;
        }

        public Range(Range prevRange, double range)
        {
            this.min = prevRange.max;
            this.max = min + range;
        }

        public bool InRange(double x)
            => (x >= min && x < max) ? true : false;
    }

    [Serializable]
    public class MutationRange
    {

        public Range[] mutRanges;

        public MutationRange(double[] mutWeights)
        {
            mutRanges = new Range[7];

            double totalWeight = 0;
            foreach (int e in mutWeights)
                totalWeight += e;

            double min = -1;
            for(int i=0; i<mutRanges.Length; i++)
            {
                if (mutWeights[i] == 0)
                    mutRanges[i] = new Range(-1, 0);
                else
                {
                    if (min == -1)
                    {
                        mutRanges[i] = new Range(0, mutWeights[i] / totalWeight);
                        min = mutWeights[i] / totalWeight;
                    }
                    else
                    {
                        mutRanges[i] = new Range(min, mutWeights[i] / totalWeight);
                        min += mutWeights[i] / totalWeight;
                    }
                }
            }
        }
    }
}