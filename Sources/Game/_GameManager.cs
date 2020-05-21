using System.Collections;
using System.Collections.Generic;
using ShogiUtils;
using System.IO;
using System;
using System.Text;

/// <summary>
/// Gestion du jeu.
/// </summary>
public static class _GameManager
{

    /// <summary>
    /// Bancs de capture des joueurs.
    /// </summary>
    public static CaptureBench captureBench1, captureBench2;

    /// <summary>
    /// Tableau des joueurs.
    /// </summary>
    public static Player[] players;

    /// <summary>
    /// Tableau Contenant les IA
    /// </summary>
    public static AIHandler[] AI;

    /// <summary>
    /// Liste des cases.
    /// </summary>
    public static List<Box> boxes;

    /// <summary>
    /// Liste des pièces.
    /// </summary>
    public static List<Token> tokens;

    /// <summary>
    /// Index du joueur courant.
    /// </summary>
    public static int currentPlayerIndex = 0;

    /// <summary>
    /// Pièce sélectionnée actuellement.
    /// </summary>
    public static Token selectedToken = null;

    /// <summary>
    /// Table de jeu.
    /// </summary>
    public static Board board;

    /// <summary>
    /// Booleen de fin de partie.
    /// </summary>
    public static bool endOfGame = false;

    /// <summary>
    /// current turn number
    /// </summary>
    public static int turnCount;

    /// <summary>
    /// List of possible moves
    /// </summary>
    public static List<Move> movesList;

	/// <summary>
	/// Liste des toutes les pièces du jeu.
	/// </summary>
	public static List<Token> allTokens;

    public static double[] lastRewardPlayer;

    /// <summary>
    /// List of possible moves after removed duplicates and Sorting
    /// </summary>
    public static void PutMoveList(List<Move> m)
    {
        movesList = Move.removeDuplicates(m);
        movesList = Move.sortMoves(movesList);
    }

    /// <summary>
    /// The work flow (Text that is saved to the log file).
    /// </summary>
    public static StringBuilder workFlow;

    /// <summary>
    /// The move flow (Text that is saved to the log file).
    /// </summary>
    public static StringBuilder moveFlow;

    public static Token toPromoteToken;


    /// <summary>
    /// Initialisation.
    /// </summary>
    public static void initGameManager()
    {

		endOfGame = false; // Pour une nouvelle séquence
		currentPlayerIndex = 0; //Pour une nouvelle séquence

        captureBench1 = _init.cb1;
        captureBench2 = _init.cb2;

        captureBench1.player = players[0];
        captureBench2.player = players[1];

        players[0].captureBench = captureBench1;
        players[1].captureBench = captureBench2;
        board = new Board(tokens, boxes);
        // Première update des moves disponibles pour tous les tokens
        updateAllTokenMoves();

		allTokens = new List<Token> ();

		updateAllTokens ();

        lastRewardPlayer = new double[2] { 0, 0 };

		players [0].score = 0;
		players [1].score = 0;

		turnCount = 0;

    }
		

    /// <summary>
    /// Booleen de tour joué.
    /// </summary>
    public static bool awaitMakingAiMove = false;

    /// <summary>
    /// Boucle de jeu.
    /// </summary>
    public static void gameLoop()
    {
        awaitMakingAiMove = true;
        turnCount = 1;
        //updateTokensToEatList();
        AI[currentPlayerIndex].SearchBestMove();
        AI[currentPlayerIndex].WaitFor();
        while (!endOfGame)
        {
            if (!awaitMakingAiMove)
            {
                awaitMakingAiMove = true;  // En prévision que l'IA va jouer suite au NextTurn ()
                NextTurn();
            }
            System.Threading.Thread.Sleep(50);
        }

        lastUpdate();

		removeTokenSelection();
        updateAllTokenMoves();

    }

    public static void lastUpdate()
    {
        if (players[currentPlayerIndex].type == PlayerType.FOXCS)
        {
            swapPlayerIndexes();
            if (AI[currentPlayerIndex].GetType() == typeof(Sandbox.Sources.Game.AI.FOXCS.FOXCS))
                AI[currentPlayerIndex].Run();
            lastRewardPlayer[currentPlayerIndex] = 0;
            swapPlayerIndexes();
            lastRewardPlayer[currentPlayerIndex] = 0;
        }
    }

    /// <summary>
    /// Passage au tour suivant.
    /// </summary>
    public static void NextTurn()
    {
        if (!endOfGame)
        {
            swapPlayerIndexes();
            turnCount++;

            removeTokenSelection();
            updateAllTokenMoves();
			updateAllTokens ();
            updateTokensToEatList();
            AI[currentPlayerIndex].SearchBestMove();
            AI[currentPlayerIndex].WaitFor();
		
        }
    }

    /// <summary>
    /// Inversion de l'index du joueur actuel.
    /// </summary>
    public static void swapPlayerIndexes()
    {
        if (currentPlayerIndex < players.Length - 1)
        {
            currentPlayerIndex++;
        }
        else
        {
            currentPlayerIndex = 0;
        }
    }

    /// <summary>
    /// Déplacer une pièce vers une case lors de la recherche de coup par l'IA.
    /// </summary>
    /// <param name="node">Une node.</param>
    /// <param name="box">Une case.</param>
    /// <param name="currentTokenID">ID du token courant.</param>
    public static void AImoveToken(Node node, Box box, int currentTokenID)
    {
        Player currentPlayer = null;
        Player opponentPlayer = null;

        if (node.isInitialPlayer == true)
        {
            currentPlayer = node.player;
            opponentPlayer = node.opponentPlayer;
        }
        else
        {
            currentPlayer = node.opponentPlayer;
            opponentPlayer = node.player;
        }

        Token currentToken = node.board.getTokenByID(currentTokenID);

        if (currentToken.isCaptured == false)
        {
            List<Coordinates> legalMoves;
            legalMoves = currentToken.moves;

            foreach (Coordinates move in legalMoves)
            {
                if (move.equals(box.coord))
                {
                    if (box.token != null)
                    {
                        AImoveTokenToBusyBox(
                            node,
                            box,
                            currentPlayer,
                            currentToken
                        );
                    }
                    else
                    {
                        AImoveTokenToEmptyBox(box, currentPlayer, currentToken);
                    }
                }
            }
        }
        else
        {
            AIdropMove(node, box, currentToken);
        }
    }

    /// <summary>
    /// Déplacement de la pièce vers une case vide lors de la recherche de coup par l'IA.
    /// </summary>
    /// <param name="box">Une case.</param>
    /// <param name="currentPlayer">Joueur courant.</param>
    /// <param name="currentToken">Pièce courante.</param>
    public static void AImoveTokenToEmptyBox(Box box, Player currentPlayer, Token currentToken)
    {
        unbindBoxAndToken(currentToken.box, currentToken);
        bindBoxAndToken(box, currentToken);

        if (currentToken.type != TokenType.GOLD && currentToken.type != TokenType.KING && currentToken.isPromoted == false)
        {
            if (currentToken.isInsideZone(currentToken.box, currentPlayer.getPromotionZone()))
            {
                AIPromote(currentToken);
            }

        }
    }

    /// <summary>
    /// Déplacement d'une pièce vers une case non vide lors de la recherche de coup par l'IA.
    /// </summary>
    /// <param name="node">Une node.</param>
    /// <param name="box">Une case.</param>
    /// <param name="currentPlayer">Joueur courant.</param>
    /// <param name="currentToken">Pièce courante.</param>
    public static void AImoveTokenToBusyBox(Node node, Box box, Player currentPlayer, Token currentToken)
    {
        Token tokenInBox = box.token;
        if (tokenInBox.owner != currentPlayer)
        {
            currentPlayer.score += tokenInBox.value;
            tokenInBox.owner = currentPlayer;

            if (tokenInBox.type != TokenType.KING)
            {
                if (tokenInBox.isPromoted == true)
                {
                    tokenInBox.removePromotion();
                }

                CaptureBox cb = tokenInBox.getCaptureLocation(node);
                tokenInBox.box = cb;
                cb.addToken(tokenInBox);
                tokenInBox.isCaptured = true;
                unbindBoxAndToken(currentToken.box, currentToken);
                bindBoxAndToken(box, currentToken);

                if (currentToken.type != TokenType.GOLD && currentToken.type != TokenType.KING && currentToken.isPromoted == false)
                {
                    if (currentToken.isInsideZone(currentToken.box, currentPlayer.getPromotionZone()))
                    {
                        AIPromote(currentToken);
                    }
                }
            }
            else
            {
                // Permet à endOfGame() de detecter une fin de partie
                tokenInBox.isCaptured = true;
            }
        }
    }

    /// <summary>
    /// Redéploiement d'une pièce vers une case lors de la recherche de coup par l'IA.
    /// </summary>
    /// <param name="node">Une node.</param>
    /// <param name="box">Case de destination.</param>
    /// <param name="currentToken">Piece courante.</param>
    public static void AIdropMove(Node node, Box box, Token currentToken)
    {
        List<Coordinates> legalDrops = currentToken.legalDrops(node.board);
        foreach (Coordinates move in legalDrops)
        {
            if (move.equals(box.coord))
            {
                currentToken.getCaptureLocation(node).removeToken();
                currentToken.captureLocation = null;
                bindBoxAndToken(box, currentToken);
                currentToken.isCaptured = false;
            }
        }
    }

    /// <summary>
    /// Promotion d'une pièce suite à un mouvement lors de la recherche de coup par l'IA.
    /// </summary>
    /// <param name="token">Token à promouvoir.</param>

    public static void AIPromote(Token currentToken) {

        
        toPromoteToken = currentToken;
        
        players[currentPlayerIndex].toPromoteToken = currentToken;

        if (players[currentPlayerIndex].type == PlayerType.UNDEFINED || players[currentPlayerIndex].type == PlayerType.ALPHABETA ||
            players[currentPlayerIndex].type == PlayerType.MINMAX || players[currentPlayerIndex].type == PlayerType.NEGASCOUT ||
            players[currentPlayerIndex].type == PlayerType.PNS || players[currentPlayerIndex].type == PlayerType.RNG)

            currentToken.promote();

        else
        {
            if (players[currentPlayerIndex].type == PlayerType.FOXCS)
            {
                AI[currentPlayerIndex].Run();
            }
            else
            {
                AI[currentPlayerIndex].SearchBestMove();
            }
        }

        if (AI[currentPlayerIndex].toPromote)
            currentToken.promote();
        AI[currentPlayerIndex].toPromote = false;
        toPromoteToken = null;
        
    }

    /// <summary>
    /// Déplacer une pièce vers une case.
    /// </summary>
    /// <param name="box">Une case.</param>
    public static void moveToken(Box box)
    {
        Player currentPlayer = players[currentPlayerIndex];
        Token currentToken = selectedToken;
        Player opponentPlayer = (currentPlayerIndex == 0) ? players[1] : players[0];
        if (!currentToken.isCaptured)
        {
            List<Coordinates> legalMoves;
            legalMoves = currentToken.moves;

            foreach (Coordinates move in legalMoves)
            {
                if (move.equals(box.coord))
                {
                    currentPlayer.moveCount++;
                    Coordinates srcCoord = currentToken.box.coord;
                    if (box.token != null)
                    {
                        moveTokenToBusyBox(box, currentPlayer, opponentPlayer, currentToken);
                    }
                    else
                    {
                        moveTokenToEmptyBox(box, currentPlayer, currentToken);
                    }
                }
            }
        }
        else
        {
            List<Coordinates> legalMoves;
            legalMoves = currentToken.moves;
            foreach (Coordinates move in legalMoves)
            {
                if (move.equals(box.coord))
                {
                    currentPlayer.dropCount++;
                    dropMove(box);
                }
            }
        }
    }

    /// <summary>
    /// Déplacement de la pièce vers une case vide.
    /// </summary>
    /// <param name="box">Une case.</param>
    /// <param name="currentPlayer">Joueur courant.</param>
    /// <param name="currentToken">Pièce courante.</param>
    public static void moveTokenToEmptyBox(Box box, Player currentPlayer, Token currentToken)
    {
        Coordinates srcCoord = new Coordinates(currentToken.box.coord);
        unbindBoxAndToken(currentToken.box, currentToken);
        bindBoxAndToken(box, currentToken);

        if (currentToken.type != TokenType.GOLD && currentToken.type != TokenType.KING && currentToken.isPromoted == false)
        {
            if (currentToken.isInsideZone(currentToken.box, currentToken.owner.getPromotionZone()))
            {
                AIPromote(currentToken);
                toPromoteToken = null;
            }
        }
    }

    /// <summary>
    /// Déplacement d'une pièce vers une case non vide.
    /// </summary>
    /// <param name="box">Une case.</param>
    /// <param name="currentPlayer">Joueur courant.</param>
    /// <param name="opponentPlayer">Joueur adverse.</param>
    /// <param name="currentToken">Pièce courante.</param>
    public static void moveTokenToBusyBox(Box box, Player currentPlayer, Player opponentPlayer, Token currentToken)
    {
        Token tokenInBox = box.token;
        if (tokenInBox.owner != currentPlayer)
        {

            opponentPlayer.lostCount++;
            currentPlayer.score += tokenInBox.value;
            currentPlayer.captCount++;
            tokenInBox.owner = currentPlayer;
            Move captureMove = new Move();
            captureMove.tokenID = tokenInBox.id;
            captureMove.playerName = opponentPlayer.name;
            captureMove.startCoordX = tokenInBox.box.coord.x;
            captureMove.startCoordY = tokenInBox.box.coord.y;
            captureMove.isPromoted = tokenInBox.isPromoted;

            if (tokenInBox.type == TokenType.KING)
            {
                // Capture du roi
                endOfGame = true;
            }
            else
            {
                // Capture d'une autre pièce
                if (tokenInBox.isPromoted == true)
                {
                    tokenInBox.removePromotion();
                }

                CaptureBox cb = tokenInBox.getCaptureLocation();
                tokenInBox.box = cb;
                captureMove.destinationCoordX = cb.coord.x;
                captureMove.destinationCoordY = cb.coord.y;
                cb.addToken(tokenInBox);
                tokenInBox.isCaptured = true;
                unbindBoxAndToken(currentToken.box, currentToken);
                bindBoxAndToken(box, currentToken);

                if (currentToken.type != TokenType.GOLD && currentToken.type != TokenType.KING && currentToken.isPromoted == false)
                {
                    if (currentToken.isInsideZone(currentToken.box, currentToken.owner.getPromotionZone()))
                    {
                        AIPromote(currentToken);
                        toPromoteToken = null;
                    }

                }
            }
        }
    }

    /// <summary>
    /// Redéploiement d'une pièce vers une case.
    /// </summary>
    /// <param name="box">Case de destination.</param>
    public static void dropMove(Box box)
    {
        Player currentPlayer = players[currentPlayerIndex];
        Token currentToken = selectedToken;

        if (currentToken.owner == currentPlayer)
        {
            List<Coordinates> legalDrops = currentToken.legalDrops(board);
            foreach (Coordinates move in legalDrops)
            {
                if (move.equals(box.coord))
                {
                    currentPlayer.dropCount++;
                    currentToken.getCaptureLocation().removeToken();
                    currentToken.captureLocation = null;
                    bindBoxAndToken(box, currentToken);
                    currentToken.isCaptured = false;
                }
            }
        }
    }

    /// <summary>
    /// Autopromotion d'une pièce suite à un mouvement.
    /// </summary>
    public static void Promote()
    {
        selectedToken.promote();
        toPromoteToken = null;
    }

    /// <summary>
    /// Affectation d'une pièce à une case.
    /// </summary>
    /// <param name="box">Case</param>
    /// <param name="token">Pièce.</param>
    public static void bindBoxAndToken(Box box, Token token)
    {
        box.token = token;
        token.box = box;
    }

    /// <summary>
    /// Retrait de l'affectation d'une pièce à une case.
    /// </summary>
    /// <param name="box">Case</param>
    /// <param name="token">Pièce.</param>
    public static void unbindBoxAndToken(Box box, Token token)
    {
        token.box = null;
        box.token = null;
    }

    /// <summary>
    /// Suppression de la selection de pièce.
    /// </summary>
    public static void removeTokenSelection()
    {
        selectedToken.selected = false;
        selectedToken = null;
    }

    /// <summary>
    /// Mise à jour des déplacements possibles des tokens.
    /// </summary>
    public static void updateAllTokenMoves()
    {
        foreach (Token t in tokens)
        {
            t.updateMoves(board);
        }
    }

    /// <summary>
    /// Mise à jour des déplacements possibles des tokens pour l'arbre de recherche de l'IA
    /// </summary>
    /// <param name="node">Node pour laquelle les coups sont mis à jour.</param>
    public static void AIupdateAllTokenMoves(Node node)
    {
        foreach (Token t in node.board.tokenList)
        {
            t.updateMoves(node.board);
        }
    }

    public static Board getBoard()
    {
        return board;
    }

	/// <summary>
	/// Mise à jour de toutes les pièces du jeu.
	/// </summary>
	public static void updateAllTokens() 
	{
		board.updateBoard ();
		captureBench1.updateCaptureBench ();
		captureBench2.updateCaptureBench ();
		List<int> idDouble = new List<int> ();

		while (allTokens.Count != 40) {
			allTokens.Clear ();
			idDouble.Clear ();
			foreach (Token t in captureBench1.allTokenListCaptured) {
				if(!idDouble.Contains(t.id)) {
					allTokens.Add (t);
					idDouble.Add (t.id);
				}
			}

			foreach (Token t in captureBench2.allTokenListCaptured) {
				if(!idDouble.Contains(t.id)) {
					allTokens.Add (t);
					idDouble.Add (t.id);
				}
			}

			foreach (Token t in board.getTokenListUpdate()) {
				if(!idDouble.Contains(t.id)) {
					allTokens.Add (t);
					idDouble.Add (t.id);
				}
			}
		}
	}

	public static List<Token> GetAllTokens() 
	{
		return allTokens;
	}

    public static double getRewardOfAction(object action, bool rewardMode)
    {
        if (action is Move) {
            Move move = action as Move;
            Box dest = getBoxFromCoordinates(move.destinationCoordX, move.destinationCoordY);
            if (dest.getToken() != null)
            {
                if (rewardMode)
                {
                    switch (dest.getToken().type)
                    {
                        case TokenType.KING:
                            lastRewardPlayer[currentPlayerIndex] = 1;
                            endOfGame = true;
                            break;
                        default:
                            lastRewardPlayer[currentPlayerIndex] = 0;
                            break;
                    }
                }
                else
                {
                    switch (dest.getToken().type)
                    {
                        case TokenType.PAWN: lastRewardPlayer[currentPlayerIndex] = !dest.getToken().isPromoted ? 1 : 7; break;
                        case TokenType.LANCE: lastRewardPlayer[currentPlayerIndex] = !dest.getToken().isPromoted ? 3 : 6; break;
                        case TokenType.KNIGHT: lastRewardPlayer[currentPlayerIndex] = !dest.getToken().isPromoted ? 4 : 6; break;
                        case TokenType.SILVER: lastRewardPlayer[currentPlayerIndex] = !dest.getToken().isPromoted ? 5 : 6; break;
                        case TokenType.GOLD: lastRewardPlayer[currentPlayerIndex] = 6; break;
                        case TokenType.BISHOP: lastRewardPlayer[currentPlayerIndex] = !dest.getToken().isPromoted ? 8 : 10; break;
                        case TokenType.ROOK: lastRewardPlayer[currentPlayerIndex] = !dest.getToken().isPromoted ? 10 : 12; break;
                        case TokenType.KING:
                            endOfGame = true;
                            lastRewardPlayer[currentPlayerIndex] = 504; break;
                    }
                }
            }
            else
                lastRewardPlayer[currentPlayerIndex] = 0;
            return lastRewardPlayer[currentPlayerIndex];
        }
        else
        {
            lastRewardPlayer[currentPlayerIndex] = 0;
            return lastRewardPlayer[currentPlayerIndex];
        }
    }

    public static Box getBoxFromCoordinates(int coordX, int coordY)
    {
        foreach(Box b in boxes)
        {
            if(b.coord.x == coordX && b.coord.y == coordY)
            {
                return b;
            }
        }
        return null;
    }

    public static void updateTokensToEatList()
    {
        foreach(Token t in tokens)
        {
            t.getTokensToEat();
        }
        
    }

}
