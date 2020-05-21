using System;
using System.Collections.Generic;
using ShogiUtils;

public class Node
{

    /// <summary>
    /// Le tableau de jeu.
    /// </summary>
    public Board board;

    /// <summary>
    /// Player = Le joueur, OpponentPlayer = Son adversaire.
    /// </summary>
    public Player player, opponentPlayer;

    /// <summary>
    /// CaptureBoxesPlayer = Les boites de capture du joueur, CaptureBoxesOpponentPlayer = Les boites de capture de l'adversaire.
    /// </summary>
    public List<CaptureBox> captureBoxesPlayer, captureBoxesOpponentPlayer;

    /// <summary>
    /// MyKing = Les coordonnées du roi du joueur, EnemyKing = Les coordonnées du roi adverse.
    /// </summary>
    public Coordinates myKing, enemyKing;

    /// <summary>
    /// Bool indiquant si le joueur qui joue à cette node est le joueur de la node racine.
    /// </summary>
    public bool isInitialPlayer;

    /// <summary>
    /// the score of the node, set at the evaluation
    /// </summary>
    public int nodeScore;

    /// <summary>
    /// Create root Node
    /// </summary>
    /// <param name="player"></param>
    /// <param name="opponentPlayer"></param>
    public Node(Player player, Player opponentPlayer)
    {
        board = AIHandler.CloneBoard(_GameManager.board);
        this.player = AIHandler.ClonePlayer(player);
        this.opponentPlayer = AIHandler.ClonePlayer(opponentPlayer);
        captureBoxesPlayer = (player == _GameManager.players[0] ? AIHandler.CloneCaptureBoxesInitial(_GameManager.players[0].captureBench) : AIHandler.CloneCaptureBoxesInitial(_GameManager.players[1].captureBench));
        captureBoxesOpponentPlayer = (opponentPlayer == _GameManager.players[0] ? AIHandler.CloneCaptureBoxesInitial(_GameManager.players[0].captureBench) : AIHandler.CloneCaptureBoxesInitial(_GameManager.players[1].captureBench));
        isInitialPlayer = true;
    }

    /// <summary>
    /// Create a children node, and apply the move given to get to the new state of the game
    /// </summary>
    /// <param name="parentNode">the parent node of this node</param>
    /// <param name="move">the move to get to the new state from parentNode</param>
    /// <param name="player">the current player in this node</param>
    /// <param name="opponentPlayer">the opponent player</param>
    ///  <param name="makeTree"> will parent and children be insitialised</param>
    public Node(Node parentNode, Move move, Player player, Player opponentPlayer)
    {
        board = AIHandler.CloneBoard(parentNode.board);
        this.player = AIHandler.ClonePlayer(player);
        this.opponentPlayer = AIHandler.ClonePlayer(opponentPlayer);
        captureBoxesPlayer = AIHandler.CloneCaptureBoxesNode(parentNode.captureBoxesPlayer);
        captureBoxesOpponentPlayer = AIHandler.CloneCaptureBoxesNode(parentNode.captureBoxesOpponentPlayer);
        isInitialPlayer = parentNode.isInitialPlayer;
        Move m = AIHandler.CloneMove(move);
        m.play(this);
        _GameManager.AIupdateAllTokenMoves(this);
        isInitialPlayer = isInitialPlayer == true ? false : true;

    }


    /// <summary>
    /// Vérifie si la partie est terminée à ce node.
    /// </summary>
    /// <returns>Vrai si la partie est terminée, faux sinon.</returns>
    public bool endOfGame()
    {

        foreach (Token t in board.tokenList)
        {
            if (t.getTokenType() == TokenType.KING && t.isCaptured)
            {
                return true;
            }
        }

        return false;

    }

    /// <summary>
    /// Récupération du score du joueur.
    /// </summary>
    /// <returns>Le score du joueur.</returns>
    public int getScore()
    {

        return player.score; // Should in theory never be null aside from the root node

    }

    /// <summary>
    /// Récupération du score du joueur ennemi.
    /// </summary>
    /// <returns>Le score du joueur ennemi.</returns>
    public int getOpponentScore()
    {

        return opponentPlayer.score; // Should in theory never be null aside from the root node

    }

    /// <summary>
    /// Récupération de la boite de capture pour Bishop.
    /// </summary>
    /// <returns>La boite de capture pour Bishop.</returns>
    public CaptureBox getBoxBishop()
    {

        List<CaptureBox> listOfCurrentPlayer = (isInitialPlayer ? captureBoxesPlayer : captureBoxesOpponentPlayer);

        if (listOfCurrentPlayer[0].tokens.Count < listOfCurrentPlayer[0].capacity)
        {
            return listOfCurrentPlayer[0];
        }

        Console.WriteLine("ERROR : No CaptureBox found! - Bishop");
        Environment.Exit(1);

        return null; // Should never occur

    }

    /// <summary>
    /// Récupération de la boite de capture pour Gold.
    /// </summary>
    /// <returns>La boite de capture pour Gold.</returns>
    public CaptureBox getBoxGold()
    {

        List<CaptureBox> listOfCurrentPlayer = (isInitialPlayer ? captureBoxesPlayer : captureBoxesOpponentPlayer);

        if (listOfCurrentPlayer[1].tokens.Count < listOfCurrentPlayer[1].capacity)
        {
            return listOfCurrentPlayer[1];
        }

        Console.WriteLine("ERROR : No CaptureBox found! - Gold");
        Environment.Exit(1);

        return null; // Should never occur

    }

    /// <summary>
    /// Récupération de la boite de capture pour Knight.
    /// </summary>
    /// <returns>La boite de capture pour Knight.</returns>
    public CaptureBox getBoxKnight()
    {

        List<CaptureBox> listOfCurrentPlayer = (isInitialPlayer ? captureBoxesPlayer : captureBoxesOpponentPlayer);

        if (listOfCurrentPlayer[2].tokens.Count < listOfCurrentPlayer[2].capacity)
        {
            return listOfCurrentPlayer[2];
        }

        Console.WriteLine("ERROR : No CaptureBox found! - Knight");
        Environment.Exit(1);

        return null; // Should never occur

    }

    /// <summary>
    /// Récupération de la boite de capture pour Lance.
    /// </summary>
    /// <returns>La boite de capture pour Lance.</returns>
    public CaptureBox getBoxLance()
    {

        List<CaptureBox> listOfCurrentPlayer = (isInitialPlayer ? captureBoxesPlayer : captureBoxesOpponentPlayer);

        if (listOfCurrentPlayer[3].tokens.Count < listOfCurrentPlayer[3].capacity)
        {
            return listOfCurrentPlayer[3];
        }

        Console.WriteLine("ERROR : No CaptureBox found! - Lance");
        Environment.Exit(1);

        return null; // Should never occur

    }

    /// <summary>
    /// Récupération de la boite de capture pour Pawn.
    /// </summary>
    /// <returns>La boite de capture pour Pawn.</returns>
    public CaptureBox getBoxPawn()
    {

        List<CaptureBox> listOfCurrentPlayer = (isInitialPlayer ? captureBoxesPlayer : captureBoxesOpponentPlayer);

        if (listOfCurrentPlayer[4].tokens.Count < listOfCurrentPlayer[4].capacity)
        {
            return listOfCurrentPlayer[4];
        }

        Console.WriteLine("ERROR : No CaptureBox found! - Pawn");
        Environment.Exit(1);

        return null; // Should never occur

    }

    /// <summary>
    /// Récupération de la boite de capture pour Rook.
    /// </summary>
    /// <returns>La boite de capture pour Rook.</returns>
    public CaptureBox getBoxRook()
    {

        List<CaptureBox> listOfCurrentPlayer = (isInitialPlayer ? captureBoxesPlayer : captureBoxesOpponentPlayer);

        if (listOfCurrentPlayer[5].tokens.Count < listOfCurrentPlayer[5].capacity)
        {
            return listOfCurrentPlayer[5];
        }

        Console.WriteLine("ERROR : No CaptureBox found! - Rook");
        Environment.Exit(1);

        return null; // Should never occur

    }

    /// <summary>
    /// Récupération de la boite de capture pour Silver.
    /// </summary>
    /// <returns>La boite de capture pour Silver.</returns>
    public CaptureBox getBoxSilver()
    {

        List<CaptureBox> listOfCurrentPlayer = (isInitialPlayer ? captureBoxesPlayer : captureBoxesOpponentPlayer);

        if (listOfCurrentPlayer[6].tokens.Count < listOfCurrentPlayer[6].capacity)
        {
            return listOfCurrentPlayer[6];
        }

        Console.WriteLine("ERROR : No CaptureBox found! - Silver");
        Environment.Exit(1);

        return null; // Should never occur

    }

    /// <summary>
    /// Evaluation du tableau de jeu pour cette node.
    /// </summary>
    public int Evaluation()
    {

        int deltaPieceValue = 0;
        int positionValue = 0;
        int mobilityValue = 0;

        setKingPosition();

        foreach (Box b in board.boxes)
        {

            if (b.token == null)
            {
                continue;
            }

            if (b.token.owner.name == player.name)
            {
                mobilityValue += b.token.moves.Count;
                deltaPieceValue += b.token.value;
                positionValue += b.token.positionValue(enemyKing, board);
            }
            else
            {
                deltaPieceValue -= b.token.value;
                positionValue -= b.token.positionValue(myKing, board);
                mobilityValue -= b.token.moves.Count;
            }

        }
        nodeScore = deltaPieceValue + positionValue + mobilityValue;
        return deltaPieceValue + positionValue + mobilityValue;
    }

    /// <summary>
    /// Mise à jour de la position des deux rois.
    /// </summary>
    public void setKingPosition()
    {

        foreach (Box box in board.boxes)
        {
            if (box.token != null)
            {
                if (box.token.type == TokenType.KING)
                {

                    if (box.token.owner.name == player.name)
                    {
                        myKing = box.coord;
                    }
                    else
                    {
                        enemyKing = box.coord;
                    }

                }
            }
        }
    }

    //Récupère la liste des mouvements possibles de l'état 
    public List<Move> GetAllMoves()
    {
        List<Move> moves = new List<Move>();
        List<Move> trans = new List<Move>();
        List<Token> tokens = new List<Token>();
        List<Token> tokensCPY = new List<Token>();
        tokensCPY = board.tokenList;
        foreach (Token t in tokensCPY)
        {
            if (t.owner.Equals(player))
            {
                tokens.Add(t);
            }
        }
        tokens = tokensCPY;
        foreach (Token t in tokens)
        {
            trans = t.GetTokenMoves(board);
            foreach (Move m in trans)
            {
                moves.Add(m);
            }
        }
        moves = Move.removeDuplicates(moves);
        return moves;
    }

    /*
    public List<Coordinates> GetAllCoordinates()
    {
        List<Coordinates> moves = new List<Coordinates>();
        List<Coordinates> trans = new List<Coordinates>();
        List<Token> tokens = new List<Token>();
        tokens = board.tokenList;
        foreach (Token t in tokens)
        {
            trans = t.legalMoves(board);
            //Console.WriteLine(tokens.IndexOf(t) + " nb LEGAL = " + trans.Count);
            foreach (Coordinates c in trans)
            {
                moves.Add(c);
            }
            /*
            trans = t.legalMovesPlus(board);
            Console.WriteLine(" nb LEGALPLUSS = " + trans.Count);
            foreach (Coordinates c in trans)
            {
                moves.Add(c);
            }
            trans = t.legalDrops(board);
            Console.WriteLine(" nb DrOP = " + trans.Count);
            foreach (Coordinates c in trans)
            {
                moves.Add(c);
            }
            
            moves = Coordinates.removeDuplicates(moves);
        }
        moves = Coordinates.removeDuplicates(moves);
        return moves;
    }
    */

}
