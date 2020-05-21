using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ShogiUtils;

namespace Sandbox.Sources.Game.AI.FOXCS
{

    public partial class FOXCS
    {
        [Serializable]
        public class FOXCSData
        {
            public int currentTime { get; set; }
            public List<Classifier> popSet { get; set; }
            public Dictionary<ArgType, int> varCount { get; set; }
            public Dictionary<ArgType, Dictionary<string, string>> boundVarList { get; set; }

            public FOXCSData(int currentTime, List<Classifier> popSet, Dictionary<ArgType, int> varCount, Dictionary<ArgType, Dictionary<string, string>> boundVarList)
            {
                this.currentTime = currentTime;
                this.popSet = popSet;
                this.varCount = varCount;
                this.boundVarList = boundVarList;
            }
        }

        private void LoadFOXCSData()
        {
            if (!File.Exists(fo.classifierFilePath))
            {
                FileStream fs = File.Create(fo.classifierFilePath);
                fs.Close();
            }

            else
            {
                FileStream fs = File.OpenRead(fo.classifierFilePath);
                BinaryFormatter bf = new BinaryFormatter();
                BinaryReader br = new BinaryReader(fs);

                if (fs.Length != 0)
                {
                    FOXCSData foxcsData = (FOXCSData)bf.Deserialize(fs);

                    currentTime = foxcsData.currentTime;
                    popSet = foxcsData.popSet;
                    varCount = foxcsData.varCount;
                    boundVarList = foxcsData.boundVarList;
                }

                fs.Close();
            }
        }

        private void SaveFOXCSData()
        {
            FileStream fs;
            if (!File.Exists(fo.classifierFilePath))
            {
                fs = File.Create(fo.classifierFilePath);
                fs.Close();
            }

            BinaryFormatter bf = new BinaryFormatter();
            fs = File.OpenWrite(fo.classifierFilePath);
            BinaryWriter bw = new BinaryWriter(fs);

            bf.Serialize(fs, new FOXCSData(currentTime, popSet, varCount, boundVarList));
            fs.Close();
        }

        private void WritePopSet()
        {
            string acc = "";
            for(int i=0; i<popSet.Count; i++)
                acc += "***Classifieur n°" + i + "***\n" + popSet[i].ToStringFile() + "\n";

            System.IO.File.WriteAllText(fo.classifierFilePath + ".txt", acc);
        }

        private int AvailableActionCount()
        {
            return env.actions.Count();
        }

        private int MatchSetActionCount()
        {
            return matchSet.Count;
        }

        private int ActionSetIndexOf(List<Classifier> actionSet, Classifier cl)
        {
            for (int i = 0; i < actionSet.Count; i++)
            {
                if (cl.rule.Equals(actionSet[i].rule))
                    return i;
            }
            return -1;
        }

        private int PopSetIndexOf(Classifier cl)
        {
            for (int i = 0; i < actionSet.Count; i++)
            {
                if (cl.rule.Equals(popSet[i].rule))
                    return i;
            }
            return -1;
        }

        private object AttributeToAction(Attribute predicate)
        {
            if(predicate == null)
                return null;
            if (predicate.name.Equals("promote"))
                return Convert.ToBoolean(predicate.values[1]);

            else if (predicate.name.Equals("move") || predicate.name.Equals("drop"))
            {
                Token tokToMove = ToToken((string)predicate.values[0], predicate.name == "drop" ? true : false);
                string[] acc = predicate.values as string[];
                return new Move(tokToMove, _GameManager.players[_GameManager.currentPlayerIndex], tokToMove.box.coord, new Coordinates(Convert.ToInt32(acc[1].ToCharArray()[1] - '0'), Convert.ToInt32(acc[2].ToCharArray()[1] - '0')));
            }
            else
                return null;
        }

        private Token ToToken(string token, bool isCaptured)
        {
            TokenType tokenType;
            GameColor tokenColor;
            int internalId;

            switch(token.ToArray()[0])
            {
                case 'b':
                    tokenColor = GameColor.SENTE;
                    break;
                case 'w':
                    tokenColor = GameColor.GOTE;
                    break;
                default:
                    return null;
            }

            switch(token.ToArray()[1])
            {
                case 'b':
                    tokenType = TokenType.BISHOP;
                    break;
                case 's':
                    tokenType = TokenType.SILVER;
                    break;
                case 'k':
                    tokenType = TokenType.KING;
                    break;
                case 'p':
                    tokenType = TokenType.PAWN;
                    break;
                case 'r':
                    tokenType = TokenType.ROOK;
                    break;
                case 'l':
                    tokenType = TokenType.LANCE;
                    break;
                case 'g':
                    tokenType = TokenType.GOLD;
                    break;
                case 'n':
                    tokenType = TokenType.KNIGHT;
                    break;
                default:
                    return null;
            }

            internalId = token.ToArray()[2] - '0';

            foreach(Token tok in _GameManager.GetAllTokens())
            {
                if (tok.internalID == internalId && tok.owner.color.Equals(tokenColor) && tok.type.Equals(tokenType) && tok.isCaptured == isCaptured)
                    return tok;
            }
            return null;
        }

        public static string ToPrologCode(bool var)
            => var ? "true" : "false";

        private void UpdateBoundVarList()
        {
            foreach (Attribute state in env.actions)
            {
                for (int i = 0; i < state.arity; i++)
                {
                    if (!boundVarList[state.predOp.argsOptions[i].argType].ContainsKey((string)state.values[i]) && (state.predOp.argsOptions[i].argsMode.Contains(ArgMode.BOUND) || state.predOp.argsOptions[i].argsMode.Contains(ArgMode.VAR)))
                    {
                        boundVarList[state.predOp.argsOptions[i].argType].Add((string)state.values[i], fo.actionPredicateOptions[state.name].argsOptions[i].argType.ToString() + varCount[fo.actionPredicateOptions[state.name].argsOptions[i].argType]);
                        varCount[fo.actionPredicateOptions[state.name].argsOptions[i].argType]++;
                    }
                }
            }

            foreach (Attribute state in env.states)
            {
                for(int i = 0; i<state.arity; i++)
                {
                    if(!boundVarList[state.predOp.argsOptions[i].argType].ContainsKey((string)state.values[i]) && (state.predOp.argsOptions[i].argsMode.Contains(ArgMode.BOUND) || state.predOp.argsOptions[i].argsMode.Contains(ArgMode.VAR)))
                    {
                        boundVarList[state.predOp.argsOptions[i].argType].Add((string)state.values[i], fo.statePredicateOptions[state.name].argsOptions[i].argType.ToString() + varCount[fo.statePredicateOptions[state.name].argsOptions[i].argType]);
                        varCount[fo.statePredicateOptions[state.name].argsOptions[i].argType]++;
                    }
                        
                }
            }
        }

        /// <summary>
        /// Reference Article http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
        /// Provides a method for performing a deep copy of an object.
        /// Binary Serialization is used to perform the copy.
        /// </summary>
        public static class ObjectCopier
        {
            /// <summary>
            /// Perform a deep Copy of the object.
            /// </summary>
            /// <typeparam name="T">The type of object being copied.</typeparam>
            /// <param name="source">The object instance to copy.</param>
            /// <returns>The copied object.</returns>
            public static T Clone<T>(T source)
            {
                if (!typeof(T).IsSerializable)
                {
                    throw new ArgumentException("The type must be serializable.", "source");
                }

                // Don't serialize a null object, simply return the default for that object
                if (Object.ReferenceEquals(source, null))
                {
                    return default(T);
                }

                IFormatter formatter = new BinaryFormatter();
                Stream stream = new MemoryStream();
                using (stream)
                {
                    formatter.Serialize(stream, source);
                    stream.Seek(0, SeekOrigin.Begin);
                    return (T)formatter.Deserialize(stream);
                }
            }
        }
    }  
}
