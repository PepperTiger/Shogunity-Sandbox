using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

/// <summary>
/// Classes et enumerations utiles au jeu.
/// </summary>
namespace ShogiUtils
{

    /// <summary>
    /// Enumération caractérisant les environs locaux.
    /// </summary>
    public enum Neighbor
    {
        TOP,
        LEFT,
        BOTTOM,
        RIGHT,
        TOP_LEFT,
        TOP_RIGHT,
        BOTTOM_RIGHT,
        BOTTOM_LEFT,
        KNIGHT_RIGHT,
        KNIGHT_LEFT
    }

    /// <summary>
    /// Enumération caractérisant les directions.
    /// </summary>
    public enum Direction
    {
        TOP,
        LEFT,
        BOTTOM,
        RIGHT,
        TOP_LEFT,
        TOP_RIGHT,
        BOTTOM_RIGHT,
        BOTTOM_LEFT,
        KNIGHT_RIGHT,
        KNIGHT_LEFT,
        PROMOTION_YES, //Ces deux dernières "directions" servent à indiquer les actions de promotions
        PROMOTION_NO

    }

    /// <summary>
    /// Enumération caractérisant les différents types d'état
    /// </summary>
    public enum StateType
    {
        NORMAL,
        PROMOTION,
        MANDATORY_PROMOTION,
        RETROGRADATION

    }

    /// <summary>
    /// Enumération caractérisant la couleur des joueurs.
    /// </summary>
    public enum GameColor
    {
        // FIRST PLAYER (BLACK)
        SENTE,
        // SECOND PLAYER (WHITE)
        GOTE
    }

    /// <summary>
    /// Enumération caractérisant les modes de jeu.
    /// </summary>
    public enum GameMode
    {
        AI_VS_AI,
        UNDEFINED
    }

    /// <summary>
    /// Enumération caractérisant le type de joueur.
    /// </summary>
    public enum PlayerType
    {
        PNS,
        ALPHABETA,
        MINMAX,
        NEGASCOUT,
        RNG,
        FOXCS,
        UNDEFINED
    }

    /// <summary>
    /// Enumération caractérisant le type de pièces.
    /// </summary>
    public enum TokenType
    {
        GOLD,
        SILVER,
        ROOK,
        BISHOP,
        KNIGHT,
        LANCE,
        PAWN,
        KING
    }

    /// <summary>
    /// Classe représentative d'un joueur.
    /// </summary>
    [Serializable]
    public class Player
    {
        /// <summary>
        /// Nom du joueur.
        /// </summary>
        public string name;

        /// <summary>
        /// Type de joueur.
        /// </summary>
        public PlayerType type;

        /// <summary>
        /// Couleur du joueur
        /// </summary>
        public GameColor color;

        /// <summary>
        /// Compteur de mouvements.
        /// </summary>
        public int moveCount;

        /// <summary>
        /// Score.
        /// </summary>
        public int score;

        /// <summary>
        /// Compteur de promotions.
        /// </summary>
        public int promCount;

        /// <summary>
        /// Compteur de captures.
        /// </summary>
        public int captCount;

        /// <summary>
        /// Compteur de redéploiements.
        /// </summary>
        public int dropCount;

        /// <summary>
        /// Compteur de pièces prises par l'adversaire.
        /// </summary>
        public int lostCount;


        /// <summary>
        /// Indicateur de promotion par le biais de la pièce à promouvoir, servi pour le choix de la promotion par l'IA.
        /// </summary>
        public Token toPromoteToken { get; set; }

        /// <summary>
        /// Banc de capture.
        /// </summary>
        [NonSerialized]
        public CaptureBench captureBench;

        public Player(string name, PlayerType type, GameColor color)
        {
            this.name = name;
            this.type = type;
            this.color = color;
            this.moveCount = 0;
            this.score = 0;
            this.promCount = 0;
            this.captCount = 0;
            this.dropCount = 0;
            this.lostCount = 0;
        }

        /// <summary>
        /// Zone de promotion du joueur.
        /// </summary>
        /// <returns>Liste des coordonnées correspondant à la zone de promotion du joueur.</returns>
        public List<Coordinates> getPromotionZone()
        {
            List<Coordinates> coordinates = new List<Coordinates>();

            if (color == GameColor.SENTE)
            {
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 6; j < 9; j++)
                    {
                        coordinates.Add(new Coordinates(i, j));
                    }
                }
            }
            else
            {
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        coordinates.Add(new Coordinates(i, j));
                    }
                }
            }
            return coordinates;
        }

        public bool equals(Player p)
        {
            return this != null &&
                p != null &&
                color == p.color;
        }
    }

    /// <summary>
    /// Classe de coordonnées bidimentionnelles.
    /// </summary>
    [Serializable]
    public class Coordinates
    {
        /// <summary>
        /// Coordonnée x.
        /// </summary>
        public int x;

        /// <summary>
        /// Coordonnée y.
        /// </summary>
        public int y;

        public Coordinates()
        {
            this.x = 0;
            this.y = 0;
        }

        public Coordinates(Coordinates coord)
        {
            this.x = coord.x;
            this.y = coord.y;
        }

        public Coordinates(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Définit un nouveau couple x et y.
        /// </summary>
        /// <param name="x">Coordonée x.</param>
        /// <param name="y">Coordonnée y.</param>
        public void set(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        /// <summary>
        /// Définit un nouveau couple x et y en fonction d'autres coordonnées.
        /// </summary>
        /// <param name="coord">Coordonées.</param>
        public void set(Coordinates coord)
        {
            this.x = coord.x;
            this.y = coord.y;
        }

        /// <summary>
        /// Vérifie si les coordonnées sont comprises dans une surface.
        /// </summary>
        /// <returns>Vrai si comprises, faux sinon.</returns>
        /// <param name="top">Y max.</param>
        /// <param name="left">X Min.</param>
        /// <param name="bottom">Y Min.</param>
        /// <param name="right">X Max.</param>
        public bool isInsideBorders(int top = 8, int left = 0, int bottom = 0, int right = 8)
        {
            return (y <= top) && (x >= left) && (y >= bottom) && (x <= right);
        }

        /// <summary>
        /// Teste si deux coordonnées sont égales.
        /// </summary>
        /// <param name="coord">Coordonnées à tester.</param>
        public bool equals(Coordinates coord)
        {
            return (coord.x == x) && (coord.y == y);
        }

        /// <summary>
        /// Coordonnées voisines.
        /// </summary>
        /// <returns>Les coordonées voisines.</returns>
        /// <param name="neighbor">Sens relatif vers où regarder.</param>
        /// <param name="color">Couleur du joueur.</param>
        public Coordinates getNeighbor(Neighbor neighbor, GameColor color)
        {
            int a = (color == GameColor.SENTE) ? 1 : -1;

            switch (neighbor)
            {
                case Neighbor.TOP:
                    return new Coordinates(x, y + a);
                case Neighbor.LEFT:
                    return new Coordinates(x - 1, y);
                case Neighbor.BOTTOM:
                    return new Coordinates(x, y - a);
                case Neighbor.RIGHT:
                    return new Coordinates(x + 1, y);
                case Neighbor.TOP_LEFT:
                    return new Coordinates(x - 1, y + a);
                case Neighbor.TOP_RIGHT:
                    return new Coordinates(x + 1, y + a);
                case Neighbor.BOTTOM_RIGHT:
                    return new Coordinates(x + 1, y - a);
                case Neighbor.BOTTOM_LEFT:
                    return new Coordinates(x - 1, y - a);
                default:
                    return new Coordinates(0, 0);
            }
        }

        /// <summary>
        /// Retourne une String qui représente le sens courant.
        /// </summary>
        /// <returns>Une String qui représente le sens courant.</returns>
        public override string ToString()
        {
            return string.Format("(x=" + x + ", y=" + y + ")");
        }

        /// <summary>
        /// L'index unidimentionnel en fonction de ses coordonnées.
        /// </summary>
        /// <returns>L'index.</returns>
        public int getIndex()
        {
            return 9 * y + x;
        }

        /// <summary>
        /// Supprime les doublons d'une liste de coordonnées.
        /// </summary>
        /// <returns>Une liste de coordonnées sans doublons.</returns>
        /// <param name="coordinates">Une liste de coordonnées.</param>
        public static List<Coordinates> removeDuplicates(List<Coordinates> coordinates)
        {
            var d = coordinates.GroupBy(a => new { a.x, a.y }).ToList();
            return d.Select(group => group.First()).ToList();
        }

        /// <summary>
        /// Affichage dans la console d'une liste de coordonnées.
        /// </summary>
        /// <param name="coordinates">Liste de coordonnées.</param>
        public static void debugList(List<Coordinates> coordinates)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Coordinates c in coordinates)
            {
                sb.Append(c.ToString() + "\n");
            }
        }


        public string ToPrologCode()
            => getIndex().ToString();

        public Coordinates cloneThis()
        {
            return new Coordinates(this.x, this.y);
        }
    }

    /// <summary>
    /// Classe caractérisant un mouvement.
    /// </summary>
    [Serializable]
    public class Move
    {
        /// <summary>
        /// Nom du joueur.
        /// </summary>
        [XmlAttribute]
        public string playerName;

        /// <summary>
        /// Identifiant de la pièce.
        /// </summary>
        [XmlAttribute]
        public int tokenID;

        /// <summary>
        /// Nom de la pièce.
        /// </summary>
        [XmlAttribute]
        public String tokenName;

        /// <summary>
        /// Nombre de mouvements de la pièce.
        /// </summary>
        [XmlAttribute]
        public int tokenMoveCount;

        /// <summary>
        /// Coordonnée de départ x.
        /// </summary>
        [XmlAttribute]
        public int startCoordX;

        /// <summary>
        /// Coordonnée de départ y.
        /// </summary>
        [XmlAttribute]
        public int startCoordY;

        /// <summary>
        /// Coordonnée de destination x.
        /// </summary>
        [XmlAttribute]
        public int destinationCoordX;

        /// <summary>
        /// Coordonnée de destination y.
        /// </summary>
        [XmlAttribute]
        public int destinationCoordY;

        /// <summary>
        /// Etat de promotion de la pièce.
        /// </summary>
        [XmlAttribute]
        public bool isPromoted;

        public Move()
        {

        }

        public Move(Token token, Player player, Box startBox, Box destinationBox)
        {
            tokenID = token.id;
            playerName = player.name;
            startCoordX = startBox.coord.x;
            startCoordY = startBox.coord.y;
            destinationCoordX = destinationBox.coord.x;
            destinationCoordY = destinationBox.coord.y;
        }

        public Move(Token token, Player player, Coordinates startCoord, Coordinates destinationCoord)
        {
            tokenID = token.id;
            tokenMoveCount = token.moves.Count;
            tokenName = token.getLetter().ToString();
            playerName = player.name;
            startCoordX = startCoord.x;
            startCoordY = startCoord.y;
            destinationCoordX = destinationCoord.x;
            destinationCoordY = destinationCoord.y;
        }

        public Move(string playerName, bool isPromoted, int tokenID, Coordinates startCoord, Coordinates destinationCoord)
        {
            this.playerName = playerName;
            this.tokenID = tokenID;
            this.isPromoted = isPromoted;
            startCoordX = startCoord.x;
            startCoordY = startCoord.y;
            destinationCoordX = destinationCoord.x;
            destinationCoordY = destinationCoord.y;
        }

        /// <summary>
        /// Joue le mouvement sur la copie du board.
        /// </summary>
        public void play()
        {
            List<Token> tokens = _GameManager.tokens;
            foreach (Token t in tokens)
            {
                if (t.id == tokenID)
                {
                    if (this.startCoordX == destinationCoordX && startCoordY == destinationCoordY && isPromoted)
                    {
                        t.promote();
                    }
                    else if (this.startCoordX == destinationCoordX && this.startCoordY == destinationCoordY && isPromoted == false)
                    {
                    }
                    else if (this.destinationCoordX == (-100) && this.destinationCoordY == (-100))
                    {
                    }
                    else
                    {
                        t.selected = true;
                        _GameManager.selectedToken = t;
                        foreach (Box b in _GameManager.boxes)
                        {
                            if (b.coord.x == destinationCoordX && b.coord.y == destinationCoordY)
                            {
                                _GameManager.moveToken(b);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Supprime les doublons d'une liste de mouvements.
        /// </summary>
        /// <returns>Une liste de mouvements sans doublons.</returns>
        /// <param name="coordinates">Une liste de mouvements.</param>
        public static List<Move> removeDuplicates(List<Move> moves)
        {
            var d = moves.GroupBy(a => new { a.tokenID, a.startCoordX, a.startCoordY, a.destinationCoordX, a.destinationCoordY }).ToList();
            return d.Select(group => group.First()).ToList();
        }
        /// <summary>
        /// Trie par propriétaire d'une liste de mouvements.
        /// </summary>
        /// <returns>Une liste de mouvements triée par propriétaire.</returns>
        /// <param name="coordinates">Une liste de mouvements.</param>
        public static List<Move> sortMoves(List<Move> moves)
        {
            //var d = moves.GroupBy(a => new { a.tokenID }).ToList();
            //var d = moves.Sort(a => new { a.tokenID }, a => new { a.tokenID }).ToList();
            List<Move> sort = moves.OrderBy(o => o.tokenID).ToList();
            var d = sort.OrderBy(o => o.playerName).ToList();
            return d;
        }

        /// <summary>
        /// Retourne une String qui représente le Move courant.
        /// </summary>
        /// <returns>Une String qui représente le  Move courant.</returns>
        public override string ToString()
        {

            string str = "is tostring from Move id : " + tokenID.ToString();
            str += "\n playerName: " + playerName;
            str += "\n startCoordX: " + startCoordX.ToString();
            str += "\n startCoordY: " + startCoordY.ToString();
            str += "\n destCoordX: " + destinationCoordX.ToString();
            str += "\n destCoordY: " + destinationCoordY.ToString();

            return str;
        }

        /// <summary>
        /// Jouer le move sur le board de la node donnée.
        /// </summary>
        /// <param name="node">La node donnée.</param>
        public void play(Node node)
        {
            List<Token> tokens = node.board.tokenList;
            foreach (Token t in tokens)
            {
                if (t.id == tokenID)
                {
                    if (this.startCoordX == destinationCoordX && startCoordY == destinationCoordY && isPromoted)
                    {
                        t.promote();
                    }
                    else
                    {
                        foreach (Box b in node.board.boxes)
                        {
                            if (b.coord.x == destinationCoordX && b.coord.y == destinationCoordY)
                            {
                                _GameManager.AImoveToken(node, b, tokenID);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Un plateau de jeu.
    /// </summary>
    [Serializable]
    public class Board
    {
        /// <summary>
        /// Liste des tokens du plateau de jeu.
        /// </summary>
        public List<Token> tokenList;

        /// <summary>
        /// Liste des tokens du plateau de jeu à chaque tour.
        /// </summary>
        public List<Token> tokenListUpdate;

        /// <summary>
        /// Liste des cases du plateau de jeu.
        /// </summary>
        public List<Box> boxes;

        public Board(List<Token> tokenList, List<Box> boxes)
        {
            this.tokenList = tokenList;
            this.boxes = boxes;
            tokenListUpdate = new List<Token>();
        }

        public Board(List<Token> tokenList)
        {
            this.tokenList = tokenList;
            boxes = _GameManager.boxes;
        }

        /// <summary>
        /// Récuperation d'un token par ID.
        /// </summary>
        /// <returns>Le token voulu..</returns>
        /// <param name="tokenID">Un ID de token.</param>
        public Token getTokenByID(int tokenID)
        {
            Token thisToken = null;

            foreach (Token t in tokenList)
            {
                if (t.id == tokenID)
                {
                    thisToken = t;
                }
            }

            return thisToken;
        }

        /// <summary>
        /// Récuperation de tous les mouvements possibles pour un joueur donné.
        /// </summary>
        /// <returns>Les mouvements possibles pour ce joueur.</returns>
        /// <param name="player">Le joueur en question.</param>
        public List<Move> getPlayerMoves(Player player)
        {
            List<Move> movesList = new List<Move>();
            foreach (Token t in tokenList)
            {
                List<Coordinates> legalMoves;
                Player owner = t.owner;
                Box startBox = t.box;
                Box destinationBox;
                if (t.moves != null)
                {
                    if (t.owner.name == player.name)
                    {
                        legalMoves = t.moves;
                        foreach (Coordinates destination in legalMoves)
                        {
                            foreach (Box b in boxes)
                            {
                                if ((b.coord.x == destination.x) && (b.coord.y == destination.y))
                                {
                                    destinationBox = b;
                                    Move move = new Move(
                                                    owner.name,
                                                    t.isPromoted,
                                                    t.id,
                                                    startBox.coord,
                                                    destinationBox.coord
                                                );
                                    movesList.Add(move);
                                }
                            }
                        }
                    }
                }
            }
            return movesList;
        }

        /// <summary>
        /// Met à jour la liste des tokens sur le plateau de jeu.
        /// </summary>
        public void updateBoard()
        {
            tokenListUpdate.Clear();
            for (int i = 0; i < 81; i++)
            {
                if (boxes[i].token != null)
                {
                    tokenListUpdate.Add(boxes[i].token);
                }
            }
        }

        public List<Token> getTokenListUpdate()
        {
            return tokenListUpdate;
        }

    }

    public static class ShogiUtils
    {
        public static string getColorLetter(GameColor color)
            => color == GameColor.SENTE ? "b" : "w";

        public static List<T> DisjointList<T>(List<T> ts1, List<T> ts2)
        {
            List<T> ts = new List<T>();

            foreach (T s1 in ts1)
            {
                foreach (T s2 in ts2)
                {
                    if (!s1.Equals(s2))
                        ts.Add(s1);
                }
            }

            return ts;
        }
    }

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
