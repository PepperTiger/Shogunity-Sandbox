using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using YieldProlog;

namespace Sandbox.Sources.Game.AI.FOXCS
{
    public partial class FOXCS
    {

        /// <summary>
        /// Classe chargé dynamiquement gérant la concordance d'un classifieur par rapport à l'environnement contenu dans la base de prédicats dans <see cref="YieldProlog"/>.
        /// A utiliser dans un autre AppDomain, la méthode <see cref="HornClause.YPwriteAndCompile"/> créant des bibliothèques dynamiques qui doivent être détruites.
        /// </summary>
        internal class MatchMarshalRefByType : MarshalByRefObject
        {
            private static object _lock = new object();

            public KeyValuePair<List<Attribute>, KeyValuePair<List<Attribute>, Dictionary<string, int>>> AssertEnvironnement(PerceivedEnvironnement env, List<HornClause> knowledgeBase, Dictionary<ArgType, Dictionary<string, string>> boundVarList, FOXCSOptions fo)
            {
                List<Attribute> newStates = new List<Attribute>();
                List<Attribute> deleteStates = new List<Attribute>();
                Dictionary<string, int> usedPredicate = new Dictionary<string, int>();

                foreach (Attribute action in env.actions)
                {
                    if (!usedPredicate.ContainsKey(action.name))
                        usedPredicate.Add(action.name, action.arity);
                    action.YPassert();
                }

                foreach (Attribute state in env.states)
                {
                    if (!usedPredicate.ContainsKey(state.name))
                        usedPredicate.Add(state.name, state.arity);
                    state.YPassert();
                }

                
                if (knowledgeBase != null || knowledgeBase.Count == 0)
                {
                    foreach (HornClause clause in knowledgeBase)
                    {
                        if (!usedPredicate.ContainsKey(clause.head.name))
                            usedPredicate.Add(clause.head.name, clause.head.arity);
                        if (clause.clause == null)
                            clause.YPwriteAndCompile();

                        foreach(string tok1 in boundVarList[ArgType.TOKEN].Keys)
                        {
                            foreach (string tok2 in boundVarList[ArgType.TOKEN].Keys)
                            {
                                if (tok1 != tok2 && tok1.ToCharArray()[0] != tok2.ToCharArray()[0])
                                {
                                    foreach(string x in boundVarList[ArgType.X].Keys)
                                    {
                                        foreach (string y in boundVarList[ArgType.Y].Keys)
                                        {
                                            foreach (bool l1 in clause.Match(new object[] { tok1, tok2, x, y }))
                                            {
                                                Attribute acc;
                                                if (!newStates.Contains(acc = new Attribute("ennemyInRange", new string[] { tok1, tok2, x, y }, fo.statePredicateOptions["ennemyInRange"])))
                                                {
                                                    newStates.Insert(0, new Attribute("ennemyInRange", new string[] { tok1, tok2, x, y }, fo.statePredicateOptions["ennemyInRange"]));
                                                    newStates[0].YPassert();
                                                }
                                                if (!deleteStates.Contains(acc = new Attribute("legalMove", new string[] { tok1, x, y }, fo.statePredicateOptions["legalMove"])))
                                                {
                                                    deleteStates.Insert(0, new Attribute("legalMove", new string[] { tok1, x, y }, fo.statePredicateOptions["legalMove"]));
                                                }
                                                if (!deleteStates.Contains(acc = new Attribute("legalMove", new string[] { tok1, x, y }, fo.statePredicateOptions["legalMove"])))
                                                {
                                                    deleteStates.Insert(0, new Attribute("inRange", new string[] { tok1, tok2, x, y }, fo.statePredicateOptions["inRange"]));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        
                    }

                }
                

                return new KeyValuePair<List<Attribute>, KeyValuePair<List<Attribute>, Dictionary<string, int>>>
                    (newStates, 
                    new KeyValuePair<List<Attribute>, Dictionary<string, int>>
                    (deleteStates, usedPredicate));
            } 
 
            /// <summary>
            /// Méthode gérant la concordance d'un classifieur
            /// </summary>
            /// <param name="cl">Classifieur à vérifier.</param>
            /// <param name="env">Environnement à insérer dans la base de prédicats.</param>
            /// <param name="knowledgeBase">Clause constiuant la base de connaissance.</param>
            /// <returns></returns>
            public List<Attribute> GetClassifierMatchList(Classifier cl, PerceivedEnvironnement env)
            {
                List<Attribute> clMatchList = new List<Attribute>();
                Dictionary<string, int> usedPredicate = new Dictionary<string, int>();

                List<YPMatchThread> ypts = new List<YPMatchThread>();
                List<Task> threads = new List<Task>();

                foreach (Attribute action in env.actions)
                {
                    if (cl.rule.head.name.Equals(action.name))
                    {
                        if (cl.rule.clause == null)
                        {
                            lock(_lock)
                                cl.rule.YPwriteAndCompile();
                        }
                        ypts.Insert(0, new YPMatchThread(ref cl, action));
                        threads.Insert(0, new Task(ypts[0].ActionMatch));
                        threads[0].Start();
                    }
                    /*
                    
                                    
                    if (cl.rule.head.name.Equals(action.name))
                    {
                        if (cl.rule.clause == null)
                            lock (_lock)
                                cl.rule.YPwriteAndCompile();
                        foreach (bool l1 in cl.rule.Match(action.values))
                        {
                            clMatchList.Add(action);
                            break;
                        }
                    }
                    
                    */
                }


                for (int j = 0; j < threads.Count; j++)
                {
                    try
                    {
                        threads[j].Wait();
                        threads[j].Dispose();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Erreur provenant de YieldProlog, relancement du thread YPThread.");
                        Console.ReadKey();
                        Console.WriteLine(e);
                        threads[j].Dispose();
                        threads.Insert(0, new Task(ypts[j].ActionMatch));

                    }
                }

                foreach (YPMatchThread ypt in ypts)
                {
                    foreach(Attribute actionMatch in ypt.clMatchList)
                    {
                        if (!clMatchList.Contains(actionMatch))
                            clMatchList.Add(actionMatch);
                    }
                }

                return clMatchList;
            }

            public void AbolishEnvironnement(Dictionary<string, int> usedPredicate)
            {
                foreach (KeyValuePair<string, int> predicate in usedPredicate)
                    new Attribute(predicate.Key, predicate.Value).YPretractAll();
            }

            internal class YPMatchThread
            {
                public Classifier cl { get; set; }
                public Attribute action { get; set; }
                public List<Attribute> clMatchList { get; set; }

                public YPMatchThread(ref Classifier cl, Attribute action)
                {
                    this.cl = cl;
                    this.action = action;
                    this.clMatchList = new List<Attribute>();
                }

                public void ActionMatch()
                {
                    IEnumerable<bool> matchResult;
                    bool end = false ;
                    if (cl.rule.head.name.Equals(action.name))
                    {
                        while(!end)
                        try
                        {
                                
                                lock (_lock)
                                {
                                    matchResult = cl.rule.Match(action.values);

                                    foreach (bool l1 in matchResult)
                                    {
                                        clMatchList.Add(action);
                                        break;
                                    }
                                }

                            end = true;

                        } catch(Exception e)
                            {
                                Console.WriteLine("Erreur de YieldProlog, recompilation en cours.");
                                cl.rule.clause = null;
                                lock (_lock)
                                    cl.rule.YPwriteAndCompile();
                                    
                            }
                    }
                }
            }
        }
    }
}
