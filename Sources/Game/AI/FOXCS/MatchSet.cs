using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YieldProlog;

namespace Sandbox.Sources.Game.AI.FOXCS
{

    public partial class FOXCS
    {

        /// <summary>
        /// Récupère tous les classifieurs correspondant à la situation actuelle de l'environnement.
        /// Si il n'y a pas assez classifieurs répondant à ce critère, en génère à partir de l'environnement.
        /// </summary>
        public void GetMatchSet()
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

            List<MatchingThread> matList = new List<MatchingThread>();
            List<Task> threads = new List<Task>();

            Dictionary<Attribute, List<HornClause>> accMatchSet = new Dictionary<Attribute, List<HornClause>>();
            List<Classifier> matchSetCount = new List<Classifier>();

            bool isMatching = false;

            foreach (Attribute delState in assertResult.Value.Key)
            {
                if (env.states.Contains(delState))
                    env.states.Remove(delState);
            }

            foreach (Attribute newState in assertResult.Key)
            {
                if (!env.states.Contains(newState))
                    env.states.Add(newState);
            }

            this.matchSet.Clear();
            while (MatchSetActionCount() < AvailableActionCount() + 1)
            {
                List<Classifier> checkSet = disjointList(popSet, matchSet);
                foreach (Classifier cl in checkSet)
                {
                    matList.Insert(0, new MatchingThread(cl, env, usedPredicate.Keys.ToArray(), mmrbt));
                    threads.Insert(0, new Task(matList[0].MatchingProcess));
                    threads[0].Start();

                    /*
                    foreach (Attribute actionMatch in mmrbt.GetClassifierMatchList(cl, env))
                    {
                        isMatching = true;
                        if (!this.matchSet.ContainsKey(actionMatch))
                            this.matchSet.Add(actionMatch, new List<Classifier>());
                        if(!this.matchSet[actionMatch].Contains(cl))
                            this.matchSet[actionMatch].Add(cl);

                    }
                    if (!isMatching)
                        this.matchSet[new Attribute("", new string[0], null)].Add(cl);
                    isMatching = false;
                    */

                }

                foreach (Task thread in threads)
                {
                    try
                    {
                        thread.Wait();
                        thread.Dispose();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Erreur provenant de YieldProlog, relancement du thread YPThread.");
                        Console.WriteLine(e);
                        thread.Dispose();
                        Console.ReadKey();
                    }
                }


                foreach (MatchingThread mat in matList)
                {
                    foreach (KeyValuePair<Attribute, List<Classifier>> actionClassifiers in mat.matchSet)
                    {
                        if (!this.matchSet.ContainsKey(actionClassifiers.Key))
                        {
                            this.matchSet.Add(actionClassifiers.Key, new List<Classifier>());
                            if(!accMatchSet.ContainsKey(actionClassifiers.Key))
                                accMatchSet.Add(actionClassifiers.Key, new List<HornClause>());
                        }

                        foreach (Classifier cl in actionClassifiers.Value)
                        {
                            if (!accMatchSet[actionClassifiers.Key].Contains(cl.rule))
                            {
                                if (!matchSetCount.Contains(cl))
                                    matchSetCount.Add(cl);
                                if (!matchSet[actionClassifiers.Key].Contains(cl))
                                    this.matchSet[actionClassifiers.Key].Add(cl);
                                if (!accMatchSet[actionClassifiers.Key].Contains(cl.rule))
                                    accMatchSet[actionClassifiers.Key].Add(cl.rule);
                            }
                        }
                    }
                }

                if (MatchSetActionCount() < AvailableActionCount() + 1)
                {
                    HornClause newRule;
                    if ((newRule = GenerateCoveringRule()) != null)
                        popSet.Add(new Classifier(currentTime, newRule, fo));
                    DeleteFromPopulation();
                }
                else
                    break;
            }

            foreach(Attribute key in matchSet.Keys.ToList())
            {
                if (matchSet[key].Count == 0)
                    matchSet.Remove(key);
            }
            matchSet.Remove(new Attribute("", new string[0], null));
            mmrbt.AbolishEnvironnement(usedPredicate);
            AppDomain.Unload(appDomain);
            Console.WriteLine("Current popSet count : " + popSet.Count);
            Console.WriteLine("Current matchSet count : " + matchSetCount.Count + "for" + matchSet.Keys.Count + "actions") ;
            Console.WriteLine("Current actions count : " + matchSet.Keys.Count);
        }

        internal class MatchingThread
        {
            public Dictionary<Attribute, List<Classifier>> matchSet { get; set; }
            public Classifier cl { get; set; }
            public PerceivedEnvironnement env { get; set; }
            public string[] usedPredicates;
            public MatchMarshalRefByType mmrbt { get; set; }

            public MatchingThread(Classifier cl, PerceivedEnvironnement env, string[] usedPredicates, MatchMarshalRefByType mmrbt)
            {
                this.matchSet = new Dictionary<Attribute, List<Classifier>>();
                this.matchSet.Add(new Attribute("", new string[0], null), new List<Classifier>());
                this.cl = cl;
                this.env = env;
                this.usedPredicates = usedPredicates;
                this.mmrbt = mmrbt;
            }

            public void MatchingProcess()
            {
                bool isMatching;
                if (IsValid(cl, usedPredicates))
                {
                    isMatching = false;
                    foreach (Attribute actionMatch in mmrbt.GetClassifierMatchList(cl, env))
                    {
                        isMatching = true;
                        if (!this.matchSet.ContainsKey(actionMatch))
                            this.matchSet.Add(actionMatch, new List<Classifier>());
                        if (!this.matchSet[actionMatch].Contains(cl))
                            this.matchSet[actionMatch].Add(cl);

                    }
                    if (!isMatching)
                        this.matchSet[new Attribute("", new string[0], null)].Add(cl);
                    isMatching = false;
                }
            }

            private bool IsValid(Classifier cl, string[] usedPredicates)
            {
                Attribute[] body = cl.rule.body.ToArray();
                List<string> accPredicate = new List<string>();
                foreach (Attribute predicate in body)
                {
                    if (!accPredicate.Contains(predicate.name))
                        accPredicate.Add(predicate.name);
                }

                foreach (string usedPredicate in usedPredicates)
                {
                    if (accPredicate.Contains(usedPredicate))
                        accPredicate.Remove(usedPredicate);
                }

                if (accPredicate.Count == 0)
                    return true;
                else
                    return false;
            }
        }

        private List<Classifier> disjointList(List<Classifier> popSet, Dictionary<Attribute, List<Classifier>> matchSet)
        {
            List<Classifier> allVal = new List<Classifier>();
            if (matchSet.Count == 0)
                return popSet;
            foreach (List<Classifier> val in matchSet.Values)
                allVal = allVal.Concat(val).ToList();

            List<Classifier> acc = new List<Classifier>();
            for (int i = 0; i < popSet.Count; i++)
            {
                for (int j = 0; j < allVal.Count; j++)
                {
                    if (!acc.Contains(popSet[i]) && !allVal.Contains(popSet[i]))
                        acc.Add(popSet[i]);
                }
            }

            return acc;
        }

        /// <summary>
        /// Génère une régle correpondant à la situation actuelle.
        /// </summary>
        /// <returns> Une règle correspondant à la situation actuelle.</returns>
        private HornClause GenerateCoveringRule()
        {
            Attribute ruleHead = ObjectCopier.Clone(RandomActionNotFromMatchSet());
            Attribute[] ruleBody = ObjectCopier.Clone(env.states.ToArray());
            Random rand = new Random();

            if (ruleHead == null)
                return null;

            for (int i = 0; i < ruleHead.arity; i++)
            {
                if ((ruleHead.predOp.argsOptions[i].argsMode.Contains(ArgMode.BOUND) || ruleHead.predOp.argsOptions[i].argsMode.Contains(ArgMode.VAR)) && rand.NextDouble() <= ruleHead.predOp.argsOptions[i].probVar)
                    ruleHead.values[i] = boundVarList[ruleHead.predOp.argsOptions[i].argType][(string)ruleHead.values[i]];
            }

            List<Attribute> redRulBody = new List<Attribute>();
            int acc;
            Attribute accVar;
            int nbPredicate = rand.Next(fo.minPredicate, fo.maxPredicate + 1);

            while (redRulBody.Count < nbPredicate && redRulBody.Count < env.states.Count)
            {
                acc = rand.Next(ruleBody.Length);
                accVar = ObjectCopier.Clone(ruleBody[acc]);

                if (!redRulBody.Contains(ruleBody[acc]))
                    redRulBody.Add(accVar);
            }

            for (int i = 0; i < redRulBody.Count; i++)
            {
                for (int j = 0; j < redRulBody[i].arity; j++)
                    if ((redRulBody[i].predOp.argsOptions[j].argsMode.Contains(ArgMode.BOUND) || redRulBody[i].predOp.argsOptions[j].argsMode.Contains(ArgMode.VAR)) && rand.NextDouble() <= redRulBody[i].predOp.argsOptions[j].probVar)
                        redRulBody[i].values[j] = boundVarList[redRulBody[i].predOp.argsOptions[j].argType][(string)redRulBody[i].values[j]];
            }

            return new HornClause(ruleHead, redRulBody.ToArray());
        }

        /// <summary>
        /// Retourne une action aléatoire non présente dans l'ensemble de concordance.
        /// </summary>
        /// <returns></returns>
        private Attribute RandomActionNotFromMatchSet()
        {

            List<Attribute> actionsNotInMatchSet = Disjoint(env.actions, matchSet.Keys.ToList());
            if (actionsNotInMatchSet.Count == 0)
                return null;

            Random rand = new Random();
            return actionsNotInMatchSet[rand.Next(actionsNotInMatchSet.Count)];
        }

        private List<Attribute> Disjoint(List<Attribute> actions, List<Attribute> keys)
        {
            List<Attribute> acc = new List<Attribute>(actions);
            for (int i = 0; i < keys.Count; i++)
            {
                if (acc.Contains(keys[i]))
                    acc.Remove(keys[i]);
            }

            if (acc.Count == 0)
                Console.WriteLine("oups");

            return acc;
        }

        private Attribute[] StatesToArray()
        {
            return env.states.ToArray();
        }
    }


}