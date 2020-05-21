using System;
using System.Collections.Generic;
using ShogiUtils;

/// <summary>
/// Gestion d'une IA de type MiniMax.
/// </summary>
public class RandomGame : AIHandler
{

    /// <summary>
    /// Player = Le joueur, OpponentPlayer = Son adversaire.
    /// </summary>
    public Player player, opponentPlayer;

    public List<Coordinates> list = new List<Coordinates>();
    public List<Move> movesList = new List<Move>();

    public RandomGame(Player me, Player him)
    {
        this.player = me;
        this.opponentPlayer = him;
    }

    /// <summary>
    /// Execution de l'algorithme de recherche.
    /// </summary>
    public override void Run()
    {
        Console.WriteLine(_GameManager.currentPlayerIndex == 0 ? "SENTE turn n° " + _GameManager.turnCount : "GOTE turn n° " + _GameManager.turnCount);
        _GameManager.workFlow.Append(_GameManager.currentPlayerIndex == 0 ? "SENTE turn n° " + _GameManager.turnCount : "GOTE turn n° " + _GameManager.turnCount);
        _GameManager.moveFlow.Append("\nMOVES : \n");
        var watch = System.Diagnostics.Stopwatch.StartNew();

        Node startingNode = new Node(player, opponentPlayer);
        GetAllMovesPossible(startingNode);
        moveToPlay = SelectMove(startingNode);

        watch.Stop();

        Console.WriteLine(",\nTemps écoulé : " + watch.ElapsedMilliseconds + "ms");
        _GameManager.workFlow.Append("\nTemps écoulé : " + watch.ElapsedMilliseconds + "ms");
        writeMoveFlow();
        _GameManager.PutMoveList(movesList);
        isDone = true;

    }
    //Ajoute le mouvements dans la chaine moveFlow pour écriture dans les fichiers Logs
    public void writeMoveFlow()
    {
        foreach (Move m in movesList)
        {
            if (m.ToString().Equals(null))
            {
                break;
            }
            _GameManager.moveFlow.Append(m.ToString());
        }
    }
    //Récupère la liste des mouvements possibles de l'état
    public void GetAllMovesPossible(Node n)
    {
        List<Move> m = n.GetAllMoves();
        Console.WriteLine(" NB ACTIONS UNIQUES = " + m.Count);
        _GameManager.workFlow.Append(" NB ACTIONS UNIQUES = " + m.Count);
        foreach (Move c in m)
        {
            movesList.Add(c);
        }
        movesList = Move.removeDuplicates(movesList);
        Console.WriteLine(" NB ACTIONS ToTAL = " + movesList.Count);
        _GameManager.workFlow.Append(" NB ACTIONS TOTAL = " + movesList.Count);
    }
    /*
    public void GetAllCoordinatesPossible(Node n)
    {
        List<Coordinates> a = n.GetAllCoordinates();
        a = Coordinates.removeDuplicates(a);
        Console.WriteLine("NB ACTIONS UNIQUES = " + a.Count);
        _GameManager.workFlow.Append("NB ACTIONS UNIQUES = " + a.Count);
        foreach (Coordinates c in a)
        {
            list.Add(c);
        }
        list = Coordinates.removeDuplicates(list);
        Console.WriteLine("NB ACTIONS ToTAL = " + list.Count);
        _GameManager.workFlow.Append("NB ACTIONS ToTAL = " + list.Count);
    }
    */

    /// <summary>
    /// Sélectionne un mouvement aléatoirement parmis une liste de mouvements possibles
    /// </summary>
    /// <param name="currentNode">Node courant.</param>
    /// <returns>Mouvement légal aléatoire.</returns>
    private Move SelectMove(Node currentNode)
    {
        Random rand = new Random();

        if (currentNode.endOfGame())
        {
            Console.WriteLine("VICTOIRE ! joueur : " + currentNode.player);
            System.Threading.Thread.Sleep(1500);
            return new Move();
        }

        int index = 0;
        List<Move> possibleMoves = currentNode.board.getPlayerMoves(currentNode.player);
        int c = possibleMoves.Count;

        if (c < 1)
        {
            Node nextNode = new Node(currentNode, possibleMoves[index], currentNode.player, currentNode.opponentPlayer);
            return possibleMoves[index];
        }
        else //(c >= 1)
        {
            index = rand.Next(0, c - 1);
            Node nextNode = new Node(currentNode, possibleMoves[index], currentNode.player, currentNode.opponentPlayer);
            Console.WriteLine("NB POSSIBLE : " + c);
            return possibleMoves[index];
        }

    }

}
