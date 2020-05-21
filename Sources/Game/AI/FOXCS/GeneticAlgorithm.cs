using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Sources.Game.AI.FOXCS
{
    public partial class FOXCS
    {
        /// <summary>
        /// Crée de nouveaux classifieurs dérivant de ceux présents dans l'ensemble d'action en appliquant des opérations de mutations.
        /// Insère les nouveaux classifieurs dans la population et en supprime si la population est dépassée en terme de capacité.
        /// Ne se lance que si le temps moyen écoulé depuis le dernier lancement de l'algorithme génétique sur l'ensemble des classifieurs de l'ensemble d'action est plus grand qu'une limite <see cref="FOXCSOptions.geneticThresh"/>.
        /// </summary>
        /// <param name="actionSet">Ensemble d'action à traiter.</param>
        /// <param name="env">Environnement à utiliser.</param>
        private void RunGeneticAlgorithm(List<Classifier> actionSet, PerceivedEnvironnement env)
        {
            int timeNumSum = 0;
            int numSum = 0;
            foreach (Classifier cl in actionSet)
            {
                timeNumSum += cl.timeStamp * cl.numerosity;
                numSum += cl.numerosity;
            }

            if (numSum == 0)
                return;
            if (currentTime - timeNumSum / numSum > fo.geneticThresh)
            {
                Console.WriteLine("Executing Genetic Algorithm.");
                Classifier newClassifier = null;
                Random rand = new Random();
                double prob = -1;
                for (int i = 0; i < actionSet.Count; i++)
                {
                    actionSet[i].timeStamp = currentTime;
                    while (newClassifier == null)
                    {
                        prob = rand.NextDouble();

                        if (fo.probMutationRange.mutRanges[0].InRange(prob))
                            newClassifier = DeleteAtom(i, actionSet, env);
                        else if (fo.probMutationRange.mutRanges[1].InRange(prob))
                            newClassifier = ConstToVar(i, actionSet, env);
                        else if (fo.probMutationRange.mutRanges[2].InRange(prob))
                            newClassifier = VarToAnonym(i, actionSet, env);
                        else if (fo.probMutationRange.mutRanges[3].InRange(prob))
                            newClassifier = AddAtom(i, actionSet, env);
                        else if (fo.probMutationRange.mutRanges[4].InRange(prob))
                            newClassifier = VarToConst(i, actionSet, env);
                        else if (fo.probMutationRange.mutRanges[5].InRange(prob))
                            newClassifier = AnonymToVar(i, actionSet, env);
                        else if (fo.probMutationRange.mutRanges[6].InRange(prob))
                            newClassifier = Reproduction(i, actionSet, env);
                        if (newClassifier != null)
                        {
                            int popIndex = ActionSetIndexOf(actionSet, newClassifier);
                            if (popIndex != -1)
                                actionSet[popIndex].numerosity++;
                            else
                            {
                                popIndex = PopSetIndexOf(newClassifier);
                                if (popIndex != -1)
                                    popSet[popIndex].numerosity++;
                                else
                                    popSet.Add(newClassifier);
                            }
                            DeleteFromPopulation();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Supprime un prédicat aléatoirement dans la règle du classifieur. 
        /// Echoue si le nombre minimum de ce type de prédicat est déjà atteint.
        /// </summary>
        /// <param name="index">Index dans l'ensemble d'action.</param>
        /// <param name="actionSet">Ensemble d'action à traiter.</param>
        /// <param name="env">Environnement à prendre en compte.</param>
        /// <returns>Le classifieur fils muté, null si la mutation échoue.</returns>
        private Classifier DeleteAtom(int index, List<Classifier> parentActionSet, PerceivedEnvironnement env)
        {
            List<Classifier> actionSet = ObjectCopier.Clone(parentActionSet);
            List<Attribute> acc = ObjectCopier.Clone(actionSet[index].rule.body.ToList());

            Random rand = new Random();
            int randIndex = rand.Next(acc.Count);
            int countAtom = 0;
            foreach (Attribute predicate in acc)
            {
                if (predicate.name.Equals(acc[randIndex].name))
                    countAtom++;
            }
            if (countAtom > acc[randIndex].predOp.min && actionSet[index].rule.body.Length > fo.minPredicate)
            {
                acc.RemoveAt(randIndex);
                return new Classifier(currentTime, new HornClause(actionSet[index].rule.head, acc.ToArray()), fo, actionSet[index]);
            }
            return null;
        }

        /// <summary>
        /// Remplace une variable lié ou libre aléatoire par une variable anonyme. 
        /// Echoue si l'argument séléctionné ne peut être une variable anonyme.
        /// </summary>
        /// <param name="index">Index dans l'ensemble d'action.</param>
        /// <param name="actionSet">Ensemble d'action à traiter.</param>
        /// <param name="env">Environnement à prendre en compte.</param>
        /// <returns>Le classifieur fils muté, null si la mutation échoue.</returns>
        private Classifier VarToAnonym(int index, List<Classifier> parentActionSet, PerceivedEnvironnement env)
        {
            List<Classifier> actionSet = ObjectCopier.Clone(parentActionSet);
            Dictionary<ArgType, Dictionary<string, List<int[]>>> varList = GetAllValues(ArgMode.BOUND, actionSet, index);
            Random rand = new Random();
            if (varList.Keys.Count != 0)
            {
                ArgType randArgType = new List<ArgType>(varList.Keys)[rand.Next(varList.Keys.Count)];
                string randArgVar = new List<string>(varList[randArgType].Keys)[rand.Next(varList[randArgType].Keys.Count)];
                int[] randPredicateArgIndex = varList[randArgType][randArgVar][rand.Next(varList[randArgType][randArgVar].Count)];

                if (randPredicateArgIndex[0] != -1)
                {
                    if (actionSet[index].rule.body[randPredicateArgIndex[0]].predOp.argsOptions[randPredicateArgIndex[1]].argsMode.Contains(ArgMode.ANONYMOUS))
                    {
                        string newArg = "_";
                        HornClause newRule = actionSet[index].rule;
                        newRule.body[randPredicateArgIndex[0]].values[randPredicateArgIndex[1]] = newArg;
                        return new Classifier(currentTime, newRule, fo, actionSet[index]);
                    }
                }
                else
                {
                    if (actionSet[index].rule.head.predOp.argsOptions[randPredicateArgIndex[1]].argsMode.Contains(ArgMode.ANONYMOUS))
                    {
                        string newArg = "_";
                        HornClause newRule = actionSet[index].rule;
                        newRule.head.values[randPredicateArgIndex[1]] = newArg;
                        return new Classifier(currentTime, newRule, fo, actionSet[index]);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Remplace chaque occurence d'une constante aléatoire par une variable libre ou lié aléatoire.
        /// Echoue si l'argument ne peut être une variable lié.
        /// </summary>
        /// <param name="index">Index dans l'ensemble d'action.</param>
        /// <param name="actionSet">Ensemble d'action à traiter.</param>
        /// <param name="env">Environnement à prendre en compte.</param 
        /// <returns>Le classifieur fils muté, null si la mutation échoue.</returns>
        private Classifier ConstToVar(int index, List<Classifier> parentActionSet, PerceivedEnvironnement env)
        {
            List<Classifier> actionSet = ObjectCopier.Clone(parentActionSet);
            Dictionary<ArgType, Dictionary<string, List<int[]>>> constList = GetAllValues(ArgMode.CONST, actionSet, index);
            Dictionary<ArgType, Dictionary<string, List<int[]>>> varList = GetAllValues(ArgMode.BOUND, actionSet, index);
            Random rand = new Random();

            Attribute newHead = ObjectCopier.Clone(actionSet)[index].rule.head;
            Attribute[] newBody = ObjectCopier.Clone(actionSet)[index].rule.body;

            if (constList.Keys.Count != 0)
            {
                ArgType randArgType = constList.Keys.ToArray()[rand.Next(constList.Keys.Count)];
                string randArgConst = constList[randArgType].Keys.ToArray()[rand.Next(constList[randArgType].Keys.Count)];

                int randVar = -1;
                if (varList.Keys.Contains(randArgType))
                    randVar = rand.Next(varList[randArgType].Keys.Count + 1);
                string newArg;
                if (randVar == -1 || randVar == varList[randArgType].Keys.Count)
                {
                    newArg = randArgType.ToString() + varCount[randArgType];
                    varCount[randArgType]++;
                }
                else
                    newArg = varList[randArgType].Keys.ToArray()[randVar];

                HornClause newRule = actionSet[index].rule;
                foreach (int[] predicateArgIndex in constList[randArgType][randArgConst])
                {
                    if (predicateArgIndex[0] != -1)
                    {
                        if (actionSet[index].rule.body[predicateArgIndex[0]].predOp.argsOptions[predicateArgIndex[1]].argsMode.Contains(ArgMode.BOUND) ||
                            actionSet[index].rule.body[predicateArgIndex[0]].predOp.argsOptions[predicateArgIndex[1]].argsMode.Contains(ArgMode.VAR) &&
                            randVar == varList[randArgType].Keys.Count)
                        {
                            newRule.body[predicateArgIndex[0]].values[predicateArgIndex[1]] = newArg;
                        }
                        else
                            return null;
                    }
                    else
                    {
                        if (actionSet[index].rule.head.predOp.argsOptions[predicateArgIndex[1]].argsMode.Contains(ArgMode.BOUND) ||
                            actionSet[index].rule.head.predOp.argsOptions[predicateArgIndex[1]].argsMode.Contains(ArgMode.VAR) &&
                            randVar == varList[randArgType].Keys.Count)
                        {
                            newRule.head.values[predicateArgIndex[1]] = newArg;
                        }
                        else
                            return null;
                    }
                }
                return new Classifier(currentTime, newRule, fo, actionSet[index]);
            }
            return null;
        }

        /// <summary>
        /// Ajoute un prédicat au corps de la règle et lui octroie des arguments aléatoires entres constantes, variables libres ou liées.
        /// Si une constante est à ajouter, la règle finale doit correspondre à un ou plusieurs états de l'environnement.
        /// </summary>
        /// <param name="index">Index dans l'ensemble d'action.</param>
        /// <param name="actionSet">Ensemble d'action à traiter.</param>
        /// <param name="env">Environnement à prendre en compte.</param>
        /// <returns>Le classifieur fils muté, null si la mutation échoue.</returns>
        private Classifier AddAtom(int index, List<Classifier> actionSet, PerceivedEnvironnement env)
        {
            bool retry = false;

            Attribute newHead = ObjectCopier.Clone(actionSet)[index].rule.head;
            Attribute[] newBody = ObjectCopier.Clone(actionSet)[index].rule.body;
            Classifier newCl = null;

            List<string> availablePredicates = new List<string>();
            foreach (Attribute state in env.states)
            {
                if (!availablePredicates.Contains(state.name))
                    availablePredicates.Add(state.name);
            }


            Random rand = new Random();
            string randAtomName = availablePredicates[rand.Next(availablePredicates.Count)];
            string[] randValues = new string[fo.statePredicateOptions[randAtomName].argsOptions.Count()];
            Dictionary<int, List<string>> constList = new Dictionary<int, List<string>>();

            int atomCount = 0;
            foreach (Attribute predicate in actionSet[index].rule.body)
            {
                if (predicate.name.Equals(randAtomName))
                    atomCount++;
            }

            if (atomCount <= fo.statePredicateOptions[randAtomName].max && actionSet[index].rule.body.Length < fo.maxPredicate)
            {
                for (int i = 0; i < randValues.Count(); i++)
                {
                    int accRand;
                    do
                    {
                        retry = false;
                        switch (fo.statePredicateOptions[randAtomName].argsOptions[i].argsMode[accRand = rand.Next(fo.statePredicateOptions[randAtomName].argsOptions[i].argsMode.Count())])
                        {
                            case ArgMode.CONST:
                                if (fo.statePredicateOptions[randAtomName].argsOptions[i].argType is ArgType.TOKEN)
                                {
                                    if (!constList.ContainsKey(i))
                                        constList.Add(i, env.tokenOnBoard);
                                }
                                else
                                {
                                    if (!constList.ContainsKey(i))
                                        constList.Add(i, boundVarList[fo.statePredicateOptions[randAtomName].argsOptions[i].argType].Keys.ToList());
                                }
                                break;
                            case ArgMode.VAR:
                                randValues[i] = fo.statePredicateOptions[randAtomName].argsOptions[i].argType.ToString() + varCount[fo.statePredicateOptions[randAtomName].argsOptions[i].argType];
                                varCount[fo.statePredicateOptions[randAtomName].argsOptions[i].argType]++;
                                break;
                            case ArgMode.BOUND:
                                Dictionary<ArgType, Dictionary<string, List<int[]>>> boundList = GetAllValues(ArgMode.BOUND, actionSet, index);
                                if (!boundList.ContainsKey(fo.statePredicateOptions[randAtomName].argsOptions[i].argType) ||
                                    boundList[fo.statePredicateOptions[randAtomName].argsOptions[i].argType].Count == 0)
                                    return null;
                                string randBound = new List<string>(boundList[fo.statePredicateOptions[randAtomName].argsOptions[i].argType].Keys)[rand.Next(boundList[fo.statePredicateOptions[randAtomName].argsOptions[i].argType].Keys.Count)];
                                randValues[i] = randBound;
                                break;
                            default:
                                retry = true;
                                break;

                        }
                    } while (retry);
                }

                if (constList.Count != 0)
                {

                    AppDomainSetup ads = new AppDomainSetup();
                    ads.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
                    AppDomain appDomain = AppDomain.CreateDomain("PrologScript", null, ads);
                    string assembly = Assembly.GetEntryAssembly().FullName;
                    MatchMarshalRefByType mmrbt =
                        (MatchMarshalRefByType)appDomain.CreateInstanceAndUnwrap(assembly,
                        typeof(MatchMarshalRefByType).FullName);

                    List<HornClause> knowBase = fo.knowledgeBase.ToList();
                    var assertResult = mmrbt.AssertEnvironnement(env, knowBase, boundVarList, fo);
                    Dictionary<string, int> usedPredicate = assertResult.Value.Value;
                    Dictionary<Classifier, List<Attribute>> usedClassifiers = new Dictionary<Classifier, List<Attribute>>();

                    Dictionary<Attribute, List<HornClause>> accMatchSet = new Dictionary<Attribute, List<HornClause>>();
                    List<Classifier> matchSetCount = new List<Classifier>();

                    List<Attribute> accMatchList = new List<Attribute>();

                    Dictionary<int, int> accIterators = new Dictionary<int, int>();
                    foreach (int i in constList.Keys)
                        accIterators.Add(i, 0);
                    bool endIterators = false;

                    int accIndex = fo.statePredicateOptions[randAtomName].argsOptions.Count();

                    while (!endIterators)
                    {
                        newHead = ObjectCopier.Clone(actionSet)[index].rule.head;
                        newBody = ObjectCopier.Clone(actionSet)[index].rule.body;

                        foreach (int i in constList.Keys)
                            randValues[i] = constList[i][accIterators[i]];

                        var acc = ObjectCopier.Clone(newBody).ToList();
                        acc.Add(new Attribute(randAtomName, randValues.ToArray(), fo.statePredicateOptions[randAtomName]));
                        newBody = acc.ToArray();
                        newCl = new Classifier(currentTime, new HornClause(newHead, newBody.ToArray()), fo);

                        if ((accMatchList = mmrbt.GetClassifierMatchList(newCl, env)).Count != 0)
                            matchSetCount.Add(newCl);

                        for (int i = 0; i <= constList.Last().Key; i++)
                        {
                            if (constList.Keys.Contains(i))
                            {
                                accIterators[i]++;
                                while (i <= constList.Last().Key && (!constList.Keys.Contains(i) || accIterators[i] >= constList[i].Count))
                                {
                                    if (constList.Keys.Contains(i))
                                        accIterators[i] = 0;
                                    i++;
                                }

                                if (accIterators.Values.Sum() == 0 || accIterators[i] < constList[i].Count())
                                {
                                    if (accIterators.Values.Sum() == 0)
                                        endIterators = true;
                                    break;
                                }
                            }
                        }
                    }

                    mmrbt.AbolishEnvironnement(usedPredicate);
                    AppDomain.Unload(appDomain);
                    if (matchSetCount.Count == 0)
                        return null;
                    else
                        return matchSetCount[rand.Next(matchSetCount.Count)];
                }

                List<Attribute> predicateList = new List<Attribute>(actionSet[index].rule.body.ToArray());
                predicateList.Add(new Attribute(randAtomName, randValues, fo.statePredicateOptions[randAtomName]));
                Attribute[] newRule = predicateList.ToArray();
                return new Classifier(currentTime, new HornClause(actionSet[index].rule.head, newRule), fo);

            }
            return null;
        }

        /// <summary>
        /// Remplace chaque occurence d'une variable aléatoirement choisi par une constante, tel que la règle final correspond à un ou plusieurs états de l'environnement.
        /// Echoue si une variable ne peut être remplacé en constante ou que la règle ne correspond à un ou plusieurs états.
        /// </summary>
        /// <param name="index">Index dans l'ensemble d'action.</param>
        /// <param name="actionSet">Ensemble d'action à traiter.</param>
        /// <param name="env">Environnement à prendre en compte.</param>
        /// <returns>Le classifieur fils muté, null si la mutation échoue.</returns>
        private Classifier VarToConst(int index, List<Classifier> actionSet, PerceivedEnvironnement env)
        {

            AppDomainSetup ads = new AppDomainSetup();
            ads.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            AppDomain appDomain = AppDomain.CreateDomain("PrologScript", null, ads);
            string assembly = Assembly.GetEntryAssembly().FullName;
            MatchMarshalRefByType mmrbt =
                (MatchMarshalRefByType)appDomain.CreateInstanceAndUnwrap(assembly,
                typeof(MatchMarshalRefByType).FullName);

            List<HornClause> knowBase = fo.knowledgeBase.ToList();
            var assertResult = mmrbt.AssertEnvironnement(env, knowBase, boundVarList, fo);
            Dictionary<string, int> usedPredicate = assertResult.Value.Value;
            Dictionary<Classifier, List<Attribute>> usedClassifiers = new Dictionary<Classifier, List<Attribute>>();

            Dictionary<Attribute, List<HornClause>> accMatchSet = new Dictionary<Attribute, List<HornClause>>();
            List<Classifier> matchSetCount = new List<Classifier>();

            bool isMatching = false;

            List<Classifier> actSet = ObjectCopier.Clone(actionSet);
            Random rand = new Random();
            Dictionary<ArgType, Dictionary<string, List<int[]>>> varList = GetAllValues(ArgMode.VAR, actionSet, index);

            if (varList.Count != 0)
            {
                ArgType randArgType = varList.Keys.ToArray()[rand.Next(varList.Keys.Count)]; ;
                string randVar = varList[randArgType].Keys.ToArray()[rand.Next(varList[randArgType].Count)];
                if (varList[randArgType][randVar].Count != 0)
                {
                    Attribute newHead = actSet[index].rule.head;
                    Attribute[] newBody = actSet[index].rule.body;

                    if (randArgType is ArgType.TOKEN)
                    {
                        foreach (string constVar in env.tokenOnBoard)
                        {
                            foreach (int[] argIndex in varList[randArgType][randVar])
                            {
                                if (argIndex[0] == -1)
                                {
                                    if (newHead.predOp.argsOptions[argIndex[1]].argsMode.Contains(ArgMode.CONST))
                                        newHead.values[argIndex[1]] = constVar;
                                }
                                else
                                {
                                    if (newBody[argIndex[0]].predOp.argsOptions[argIndex[1]].argsMode.Contains(ArgMode.CONST))
                                        newBody[argIndex[0]].values[argIndex[1]] = constVar;
                                }
                            }

                            Classifier newCl = new Classifier(currentTime, new HornClause(ObjectCopier.Clone(newHead), ObjectCopier.Clone(newBody)), fo);
                            string[] accValues = newHead.values.ToArray() as string[];

                            List<Attribute> actionMatchList = mmrbt.GetClassifierMatchList(newCl, env);
                            if (actionMatchList.Count != 0)
                                matchSetCount.Add(newCl);
                        }
                    }

                    else
                    {
                        foreach (string constVar in boundVarList[randArgType].Keys)
                        {
                            foreach (int[] argIndex in varList[randArgType][randVar])
                            {
                                if (argIndex[0] == -1)
                                {
                                    if (newHead.predOp.argsOptions[argIndex[1]].argsMode.Contains(ArgMode.CONST))
                                        newHead.values[argIndex[1]] = constVar;
                                }
                                else
                                {
                                    if (newBody[argIndex[0]].predOp.argsOptions[argIndex[1]].argsMode.Contains(ArgMode.CONST))
                                        newBody[argIndex[0]].values[argIndex[1]] = constVar;
                                }
                            }

                            Classifier newCl = new Classifier(currentTime, new HornClause(ObjectCopier.Clone(newHead), ObjectCopier.Clone(newBody)), fo);
                            string[] accValues = newHead.values.ToArray() as string[];

                            List<Attribute> actionMatchList = mmrbt.GetClassifierMatchList(newCl, env);
                            if (actionMatchList.Count != 0)
                                matchSetCount.Add(newCl);
                        }
                    }

                    mmrbt.AbolishEnvironnement(usedPredicate);
                    AppDomain.Unload(appDomain);

                    if (matchSetCount.Count != 0)
                        return matchSetCount[rand.Next(matchSetCount.Count)];
                }
            }
            return null;
        }

        /// <summary>
        /// Remplace une variable anonyme aléatoire par une variable libre ou lié.
        /// Echoue si la variable ne peut être une variable libre ou lié.
        /// </summary>
        /// <param name="index">Index dans l'ensemble d'action.</param>
        /// <param name="actionSet">Ensemble d'action à traiter.</param>
        /// <param name="env">Environnement à prendre en compte.</param>
        /// <returns>Le classifieur fils muté, null si la mutation échoue.</returns>
        public Classifier AnonymToVar(int index, List<Classifier> parentActionSet, PerceivedEnvironnement env)
        {

            List<Classifier> actionSet = ObjectCopier.Clone(parentActionSet);
            Dictionary<ArgType, Dictionary<string, List<int[]>>> anonymList = GetAllValues(ArgMode.ANONYMOUS, actionSet, index);
            Dictionary<ArgType, Dictionary<string, List<int[]>>> varList = GetAllValues(ArgMode.BOUND, actionSet, index);
            Random rand = new Random();

            if (anonymList.Keys.Count != 0 && varList.Keys.Count != 0)
            {
                ArgType randArgType = new List<ArgType>(anonymList.Keys)[rand.Next(anonymList.Keys.Count)];
                string randArgAnonym = new List<string>(anonymList[randArgType].Keys)[rand.Next(anonymList[randArgType].Keys.Count)];
                int[] randPredicateArgIndex = anonymList[randArgType][randArgAnonym][rand.Next(anonymList[randArgType][randArgAnonym].Count)];

                if (randPredicateArgIndex[0] != -1)
                {
                    if (actionSet[index].rule.body[randPredicateArgIndex[0]].predOp.argsOptions[randPredicateArgIndex[1]].argsMode.Contains(ArgMode.BOUND))
                    {
                        int randVar = rand.Next(varList[randArgType].Keys.Count
                            + (actionSet[index].rule.body[randPredicateArgIndex[0]].predOp.argsOptions[randPredicateArgIndex[1]].argsMode.Contains(ArgMode.VAR) ? 1 : 0));
                        string newArg;
                        if (randVar == varList[randArgType].Keys.Count || !actionSet[index].rule.body[randPredicateArgIndex[0]].predOp.argsOptions[randPredicateArgIndex[1]].argsMode.Contains(ArgMode.BOUND))
                        {
                            newArg = randArgType.ToString() + varCount[randArgType];
                            varCount[randArgType]++;
                        }
                        else
                            newArg = new List<string>(varList[randArgType].Keys)[randVar];

                        HornClause newRule = actionSet[index].rule;
                        newRule.body[randPredicateArgIndex[0]].values[randPredicateArgIndex[1]] = newArg;
                        return new Classifier(currentTime, newRule, fo, actionSet[index]);
                    }
                }

                else
                {
                    if (actionSet[index].rule.head.predOp.argsOptions[randPredicateArgIndex[1]].argsMode.Contains(ArgMode.BOUND))
                    {
                        int randVar = rand.Next(varList[randArgType].Keys.Count
                            + (actionSet[index].rule.head.predOp.argsOptions[randPredicateArgIndex[1]].argsMode.Contains(ArgMode.VAR) ? 1 : 0));
                        string newArg;
                        if (randVar == varList[randArgType].Keys.Count || !actionSet[index].rule.head.predOp.argsOptions[randPredicateArgIndex[1]].argsMode.Contains(ArgMode.BOUND))
                        {
                            newArg = randArgType.ToString() + varCount[randArgType];
                            varCount[randArgType]++;
                        }
                        else
                            newArg = new List<string>(varList[randArgType].Keys)[randVar];

                        HornClause newRule = actionSet[index].rule;
                        newRule.head.values[randPredicateArgIndex[1]] = newArg;
                        return new Classifier(currentTime, newRule, fo, actionSet[index]);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Reproduit une règle de l'ensemble d'action.
        /// Ne peut échouer.
        /// </summary>
        /// <param name="index">Index dans l'ensemble d'action.</param>
        /// <param name="actionSet">Ensemble d'action à traiter.</param>
        /// <param name="env">Environnement à prendre en compte.</param>
        /// <returns>Le classifieur fils muté, null si la mutation échoue.</returns>
        private Classifier Reproduction(int index, List<Classifier> actionSet, PerceivedEnvironnement env)
        {
            return ObjectCopier.Clone(actionSet[index]);
        }

        /// <summary>
        /// Récupère la liste de tous les arguments selon leurs modes, nom et position dans le corps de la règle.
        /// </summary>
        /// <param name="argMode">Mode des arguments à récupérer/</param>
        /// <param name="actionSet">Ensemble d'action à traiter.</param>
        /// <param name="index">Index de l'ensemble d'action. </param>
        /// <returns>La liste des arguments selon un mode, leurs noms et positions respectives.</returns>
        private Dictionary<ArgType, Dictionary<string, List<int[]>>> GetAllValues(ArgMode argMode, List<Classifier> parentActionSet, int index)
        {
            List<Classifier> actionSet = ObjectCopier.Clone(parentActionSet);
            Dictionary<ArgType, Dictionary<string, List<int[]>>> argList = new Dictionary<ArgType, Dictionary<string, List<int[]>>>();
            switch (argMode)
            {
                case ArgMode.ANONYMOUS:
                    for (int i = 0; i < actionSet[index].rule.head.arity; i++)
                    {
                        if (actionSet[index].rule.head.values[i].Equals("_"))
                        {
                            if (!argList.ContainsKey(actionSet[index].rule.head.predOp.argsOptions[i].argType))
                                argList.Add(actionSet[index].rule.head.predOp.argsOptions[i].argType, new Dictionary<string, List<int[]>>());
                            if (!argList[actionSet[index].rule.head.predOp.argsOptions[i].argType].ContainsKey("_"))
                                argList[actionSet[index].rule.head.predOp.argsOptions[i].argType].Add("_", new List<int[]>());
                            argList[actionSet[index].rule.head.predOp.argsOptions[i].argType]["_"].Add(new int[2] { -1, i });
                        }
                    }

                    for (int i = 0; i < actionSet[index].rule.body.Count(); i++)
                    {
                        for (int j = 0; j < actionSet[index].rule.body[i].arity; j++)
                        {
                            if (actionSet[index].rule.body[i].values[j].Equals("_"))
                            {
                                if (!argList.ContainsKey(actionSet[index].rule.body[i].predOp.argsOptions[j].argType))
                                    argList.Add(actionSet[index].rule.body[i].predOp.argsOptions[j].argType, new Dictionary<string, List<int[]>>());
                                if (!argList[actionSet[index].rule.body[i].predOp.argsOptions[j].argType].ContainsKey("_"))
                                    argList[actionSet[index].rule.body[i].predOp.argsOptions[j].argType].Add("_", new List<int[]>());
                                argList[actionSet[index].rule.body[i].predOp.argsOptions[j].argType]["_"].Add(new int[2] { i, j });
                            }
                        }
                    }
                    return argList;

                case ArgMode.VAR:
                case ArgMode.BOUND:
                    for (int i = 0; i < actionSet[index].rule.head.arity; i++)
                    {
                        string value = (string)actionSet[index].rule.head.values[i];
                        if (!IsMinChar(value.ToCharArray()[0]))
                        {
                            if (!argList.ContainsKey(actionSet[index].rule.head.predOp.argsOptions[i].argType))
                                argList.Add(actionSet[index].rule.head.predOp.argsOptions[i].argType, new Dictionary<string, List<int[]>>());
                            if (!argList[actionSet[index].rule.head.predOp.argsOptions[i].argType].ContainsKey((string)actionSet[index].rule.head.values[i]))
                                argList[actionSet[index].rule.head.predOp.argsOptions[i].argType].Add((string)actionSet[index].rule.head.values[i], new List<int[]>());
                            argList[actionSet[index].rule.head.predOp.argsOptions[i].argType][(string)actionSet[index].rule.head.values[i]].Add(new int[2] { -1, i });
                        }
                    }

                    for (int i = 0; i < actionSet[index].rule.body.Count(); i++)
                    {
                        for (int j = 0; j < actionSet[index].rule.body[i].arity; j++)
                        {
                            string value = (string)actionSet[index].rule.body[i].values[j];
                            if (!IsMinChar(value.ToCharArray()[0]))
                            {
                                if (!argList.ContainsKey(actionSet[index].rule.body[i].predOp.argsOptions[j].argType))
                                    argList.Add(actionSet[index].rule.body[i].predOp.argsOptions[j].argType, new Dictionary<string, List<int[]>>());
                                if (!argList[actionSet[index].rule.body[i].predOp.argsOptions[j].argType].ContainsKey((string)actionSet[index].rule.body[i].values[j]))
                                    argList[actionSet[index].rule.body[i].predOp.argsOptions[j].argType].Add((string)actionSet[index].rule.body[i].values[j], new List<int[]>());
                                argList[actionSet[index].rule.body[i].predOp.argsOptions[j].argType][(string)actionSet[index].rule.body[i].values[j]].Add(new int[2] { i, j });
                            }
                        }
                    }
                    return argList;

                case ArgMode.CONST:
                    for (int i = 0; i < actionSet[index].rule.head.arity; i++)
                    {
                        string value = (string)actionSet[index].rule.head.values[i];
                        if (IsMinChar(value.ToCharArray()[0]))
                        {
                            if (!argList.ContainsKey(actionSet[index].rule.head.predOp.argsOptions[i].argType))
                                argList.Add(actionSet[index].rule.head.predOp.argsOptions[i].argType, new Dictionary<string, List<int[]>>());
                            if (!argList[actionSet[index].rule.head.predOp.argsOptions[i].argType].ContainsKey((string)actionSet[index].rule.head.values[i]))
                                argList[actionSet[index].rule.head.predOp.argsOptions[i].argType].Add((string)actionSet[index].rule.head.values[i], new List<int[]>());
                            argList[actionSet[index].rule.head.predOp.argsOptions[i].argType][(string)actionSet[index].rule.head.values[i]].Add(new int[2] { -1, i });
                        }
                    }

                    for (int i = 0; i < actionSet[index].rule.body.Count(); i++)
                    {
                        for (int j = 0; j < actionSet[index].rule.body[i].arity; j++)
                        {
                            string value = (string)actionSet[index].rule.body[i].values[j];
                            if (IsMinChar(value.ToCharArray()[0]))
                            {
                                if (!argList.ContainsKey(actionSet[index].rule.body[i].predOp.argsOptions[j].argType))
                                    argList.Add(actionSet[index].rule.body[i].predOp.argsOptions[j].argType, new Dictionary<string, List<int[]>>());
                                if (!argList[actionSet[index].rule.body[i].predOp.argsOptions[j].argType].ContainsKey((string)actionSet[index].rule.body[i].values[j]))
                                    argList[actionSet[index].rule.body[i].predOp.argsOptions[j].argType].Add((string)actionSet[index].rule.body[i].values[j], new List<int[]>());
                                argList[actionSet[index].rule.body[i].predOp.argsOptions[j].argType][(string)actionSet[index].rule.body[i].values[j]].Add(new int[2] { i, j });
                            }
                        }
                    }
                    return argList;

                default:
                    return null;
            }
        }

        private bool IsMinChar(char c)
            => c >= 'a' && c <= 'z';
    }
}
