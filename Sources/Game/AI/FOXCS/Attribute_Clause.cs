using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YieldProlog;
using static YieldProlog.YP;

namespace Sandbox.Sources.Game.AI.FOXCS
{
    /// <summary>
    /// Predicat simplifié de la bibliothèque <see cref="YieldProlog"/>.
    /// Un prédicat est une proposition vraie constituée d'un nom et des valeurs en arguments.
    /// Exemple : Parent(Pierre, Jacques) => Pierre est un parent de Jacques.
    /// Dû à des soucis de notations, le terme "prédicat" sera remplacé dans le code par le terme "attribute".
    /// </summary>
    [Serializable]
    public class Attribute : IEquatable<Attribute>
    {
        /// <summary>
        /// Nom du prédicat.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Liste des arguments du prédicat.
        /// </summary>
        public object[] values { get; set; }

        /// <summary>
        /// Nombres d'arguments du prédicat.
        /// </summary>
        public int arity { get; set; }

        /// <summary>
        /// Options et informations complémentaires sur le prédicat.
        /// </summary>
        public PredicateOptions predOp { get; }

        /// <summary>
        /// Constructeur à partir d'arguments en <see cref="string"/>.
        /// </summary>
        /// <param name="name"> Nom du prédicat.</param>
        /// <param name="values"> Liste des arguments du prédicat.</param>
        /// <param name="predOp"> Options du prédicat.</param>
        public Attribute(string name, string[] values, PredicateOptions predOp)
        {
            this.name = name;
            this.values = values;
            this.arity = this.values.Count();
            this.predOp = predOp;
        }

        /// <summary>
        /// Constructeur à partir du nom et de l'arité d'un prédicat.
        /// Utilisé seulement pour l'abolition de prédicat.
        /// </summary>
        /// <param name="name"> Nom du prédicat.</param>
        /// <param name="arity"> Arité du prédicat</param>
        public Attribute(string name, int arity)
        {
            this.name = name;
            this.arity = arity;
            this.values = null;
        }

        /// <summary>
        /// Constructeur à partir d'une liste d'objet.
        /// Utilisé uniquement pour <see cref="HornClause.YPassertFromMatch"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="values"></param>
        /// <param name="predOp"></param>
        public Attribute(string name, object[] values, PredicateOptions predOp)
        {
            this.name = name;
            //this.values = values;
            this.arity = this.values.Count();
            this.predOp = predOp;
        }

        /// <summary>
        /// Insère un prédicat dans la base de données <see cref="YieldProlog"/>.
        /// A utiliser uniquement dans un nouveau <see cref="AppDomain"/>.
        /// </summary>
        public void YPassert()
        {
            YP.assertFact(Atom.a(name), values);
        }

        public void YPretract()
        {
            YP.retractall(Functor.make(Atom.a(name), values));
        }

        /// <summary>
        /// Détruit tous les prédicats de nom et d'arité identique de la base de données de <see cref="YieldProlog"/>.
        /// A utiliser uniquement dans un nouveau <see cref="AppDomain"/>.
        /// </summary>
        public void YPretractAll()
        {
            object[] values = new object[arity];
            for (int i = 0; i < arity; i++)
                values[i] = new Variable();
            YP.retract(Functor.make(Atom.a(name), values));
        }

        public override string ToString()
        {
            return ToPrologCode();
        }

        /// <summary>
        /// Renvoie le prédicat sous forme de code Prolog.
        /// </summary>
        /// <returns></returns>
        public string ToPrologCode()
        {
            string s = name; 
            string[] values = this.values as string[];
            s += "(" + values[0].ToString();
            for (int i = 1; i < values.Count(); i++)
                s += "," + values[i].ToString();
            s += ")";
            return s;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Attribute);
        }

        public bool Equals(Attribute other)
        {
            if (other == null || arity != other.arity)
                return false;

            if (name == other.name)
            {
                for(int i=0; i<arity; i++)
                {
                    if (values[i] as string != other.values[i] as string)
                        return false;
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
            {
            var hashCode = 1240601617;
            hashCode = hashCode * -1521134295 + name.GetHashCode();
            string[] values = this.values as string[];
            int hc = 0;
            foreach (string value in values)
                hc += value.GetHashCode();
            hashCode = hashCode * -1521134295 + hc.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Attribute attribute1, Attribute attribute2)
        {
            return EqualityComparer<Attribute>.Default.Equals(attribute1, attribute2);
        }

        public static bool operator !=(Attribute attribute1, Attribute attribute2)
        {
            return !(attribute1 == attribute2);
        }

        private bool isMinChar(char c)
         => c > 97 && c < 122;

        private bool isNumber(char c)
            => c > 47 && c < 58;
    }

    /// <summary>
    /// Une clause de Horn est une suite de prédicat de forme logique : H => B1 && B2 && ... Bn.
    /// H est le prédicat de tête, définissant la clause.
    /// B1,...Bn est le corps de la clause, constituant la clause.
    /// </summary>
    [Serializable]
    public class HornClause : IEquatable<HornClause>
    {
        /// <summary>
        /// Tête de clause.
        /// </summary>
        public Attribute head { get; set; }

        /// <summary>
        /// Corps de clause.
        /// </summary>
        public Attribute[] body { get; set; }

        public string prologCode { get; set; }

        public YP.IClause clause { get; set; }

        /// <summary>
        /// Constructeur d'une clause de Horn.
        /// </summary>
        /// <param name="head"> Prédicat de tête de clause.</param>
        /// <param name="body"> Liste de prédicats de corps de clause.</param>
        public HornClause(Attribute head, Attribute[] body)
        {
            this.head = head;
            this.body = body;
            prologCode = null;
            clause = null;
        }

        public HornClause(Attribute head, string prologCode)
        {
            this.head = head;
            this.body = null;
            this.prologCode = prologCode;
            clause = null;
        }

        /// <summary>
        /// Renvoie la clause sous forme de code Prolog.
        /// </summary>
        /// <returns></returns>
        private string ToPrologCode()
        {
            if (prologCode == null)
            {
                prologCode = head.ToPrologCode() + ":- \n";
                int i = 0;
                for (i = 0; i < body.Count() - 1; i++)
                    prologCode += body[i].ToPrologCode() + ", \n";
                prologCode += body[i].ToPrologCode() + ". \n";
            }
            return prologCode;
        }

        /// <summary>
        /// Traduit le code Prolog d'une clause de Horn en fonction en C# et l'exécute à partir des prédicats contenus dans la base de données. 
        /// A utiliser uniquement dans un nouveau <see cref="AppDomain"/> sous peine de fuite de mémoire.
        /// </summary>
        /// <returns> Un objet <see cref="YP.IClause"/>, qui peut être manipulé pour obtenir une liste de concordance <see cref="IEnumerable{bool}"/> de la clause par rapport aux prédicats dans la base de données. </returns>
        public void YPwriteAndCompile()
        {
            string codeToCorrect;
            using (StringWriter sw = new StringWriter())
            {
                //Console.WriteLine("// Compiled code:" + ToPrologCode());
                YP.tell(sw);
                YP.see(new StringReader(ToPrologCode()));
                Variable TermList = new Variable();
                Variable PseudoCode = new Variable();
                foreach (bool l1 in Parser.parseInput(TermList))
                {
                    foreach (bool l2 in Compiler.makeFunctionPseudoCode
                             (TermList, PseudoCode))
                        Compiler.convertFunctionCSharp(PseudoCode);
                }
                YP.seen();
                codeToCorrect = sw.ToString();
            }

            StringBuilder sb = new StringBuilder(codeToCorrect);

            sb.Remove(0, 115);

            int i = 0;
            while (!sb.ToString().ToCharArray()[i].Equals('('))
                i++;

            sb.Remove(0, i);
            sb.Insert(0, "public static IEnumerable<bool> function");
            sb.Replace(head.name, "function");

            string finalCode = sb.ToString();
            clause = Compiler.compileAnonymousFunction(finalCode, head.arity, null);


        }

        /// <summary>
        /// Compile, execute et renvoie les valeurs concordantes à la clause et la base de prédicats.
        /// </summary>
        /// <param name="values"></param>
        /// <returns>Une liste de concordance <see cref="IEnumerable{bool}"/> de la clause par rapport aux prédicats dans la base de données.</returns>
        public IEnumerable<bool> Match(object[] values)
        {
            /*
            object[] values2 = new object[values.Length];
            for(int i=0; i<values.Count(); i++)
            {
                if(values[i] is string)
                {
                    string acc = values[i] as string;
                    object acc2;
                    if (acc.ToCharArray()[0] > 47 && acc.ToCharArray()[0] < 58)
                        acc2 = acc.ToCharArray()[0] - '0';
                    else
                        acc2 = acc;
                    values2[i] = acc2;
                        
                }
            }
            */
            
            object[] values2 = new object[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] is string)
                {
                    string acc = values[i] as string;
                    if (isMinChar(acc.ToCharArray()[0]))
                        values2[i] = Atom.a(values[i] as string);
                    else
                        values2[i] = new Variable();
                }
            }
                
            return clause.match(values2);
        }

        /// <summary>
        /// Insère les valeurs concordantes à la clause dans la base de prédicats.
        /// </summary>
        public void YPassertFromMatch()
        {
            /*
            if (clause is ClauseHeadAndBody)
                ((ClauseHeadAndBody)clause).setHeadAndBody(Head, Body);

            // Add the clause to the entry in _predicatesStore.
            NameArity nameArity = new NameArity(name, args.Length);
            List<IClause> clauses;
            if (!YP._predicatesStore.TryGetValue(nameArity, out clauses))
                // Create an entry for the nameArity.
                YP._predicatesStore[nameArity] = (clauses = new List<YP.IClause>());
                */

        }
            
        /// <summary>
        /// Initialise les variables à unifier lors de l'assertion de clause par concordance.
        /// </summary>
        /// <returns>Une liste constitué de constantes ou de variables.</returns>
            private object[] GetVariableArguments()
            {
            object[] varArray = new object[head.arity];
            Dictionary<string, Variable> boundVar = new Dictionary<string, Variable>();
            for (int i = 0; i < head.arity; i++)
            {
                if (head.values[i] is string)
                {
                    string acc = (string)head.values[i];
                    if (isMinChar(acc.ToCharArray()[0]))
                        varArray[i] = head.values[i];

                    else
                    {
                        if (!boundVar.ContainsKey(acc))
                            boundVar.Add(acc, new Variable());
                        varArray[i] = boundVar[acc];
                    }
                }
            }
            return varArray;
        }

        private bool isMinChar(char c)
            => c > 97 && c < 122;

        public override bool Equals(object obj)
        {
            return Equals(obj as HornClause);
        }

        public bool Equals(HornClause other)
        {
            if((head.Equals(other.head)) && body.Length == other.body.Length)
            {
                for(int i=0; i<body.Length; i++)
                {
                        if (!body[i].Equals(other.body[i]))
                            return false;
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            var hashCode = -1150395728;
            hashCode = hashCode * -1521134295 + EqualityComparer<Attribute>.Default.GetHashCode(head);
            hashCode = hashCode * -1521134295 + EqualityComparer<Attribute[]>.Default.GetHashCode(body);
            return hashCode;
        }

        public static bool operator ==(HornClause clause1, HornClause clause2)
        {
            return EqualityComparer<HornClause>.Default.Equals(clause1, clause2);
        }

        public static bool operator !=(HornClause clause1, HornClause clause2)
        {
            return !(clause1 == clause2);
        }

        public override string ToString()
        {
            return ToPrologCode();
        }
    }
}
