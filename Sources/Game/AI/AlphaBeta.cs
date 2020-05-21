using System;
using System.Collections.Generic;
using ShogiUtils;

/// <summary>
/// Gestion d'une IA de type AlphaBeta.
/// </summary>
public class AlphaBeta : AIHandler {

	/// <summary>
	/// Player = Le joueur, OpponentPlayer = Son adversaire.
	/// </summary>
	public Player player, opponentPlayer;

	/// <summary>
	/// Profondeur de recherche.
	/// </summary>
	public int depth;

	/// <summary>
	/// Number of nodes visited.
	/// </summary>
	public int searchCount;

	/// <summary>
	/// The selected score.
	/// </summary>
	public int selectedScore;

	public AlphaBeta (Player me, Player him) {

		player = me;
		opponentPlayer = him;
		depth = me.color == GameColor.SENTE ? _GameConfig.player1Difficulty : _GameConfig.player2Difficulty;

	}

	/// <summary>
	/// Execution de l'algorithme de recherche.
	/// </summary>
	public override void Run () {
		
		moveToPlay = AlphaBetaBasic ();
		isDone = true;
		
	}

	/// <summary>
	/// Maximizer.
	/// </summary>
	/// <param name="currentNode">Node courant.</param>
	/// <param name="depth">Profondeur de recherche restante.</param>
	/// <param name="alpha">Valeur de Alpha.</param>
	/// <param name="beta">Valeur de Beta.</param>
	/// <returns>Paire mouvement/score.</returns>
	private KeyValuePair<Move, int> Maximize (Node currentNode, int depth, int alpha, int beta, bool isInitial) {
		
		searchCount++;
		
		if (currentNode.endOfGame ()) {
			return new KeyValuePair<Move, int> (null, -9999999);
		}

		if (depth == 0) { // Cannot occur at first call at thus cannot return a null move to Run ()
			int eval = currentNode.Evaluation ();
			return new KeyValuePair<Move, int> (null, eval); // Can only return the null move to Minimize (), which does not use it
		}

		int score = int.MinValue;
		Move selectedMove = null;
		foreach (Move m in currentNode.board.getPlayerMoves (currentNode.player)) {
			Node nextNode = new Node (currentNode, m, currentNode.player, currentNode.opponentPlayer);
			score = Math.Max (score, Minimize (nextNode, depth - 1, alpha, beta).Value);
			// Score must be STRICTLY SUPERIOR (Equal can happen but is pointless, Inferior cannot occur)
			if (score > alpha) {
				alpha = score;
				selectedMove = m;
				if (isInitial) {
					selectedScore = score;
				}
			}
			if (beta <= alpha) { // Cannot occur on first call
				break;
			}
		}
		
		// Has to return a move, so it can return one to Run ()
		return new KeyValuePair<Move, int> (selectedMove, score);

	}

	/// <summary>
	/// Minimizer.
	/// </summary>
	/// <param name="currentNode">Node courant.</param>
	/// <param name="depth">Profondeur de recherche restante.</param>
	/// <param name="alpha">Valeur de Alpha.</param>
	/// <param name="beta">Valeur de Beta.</param>
	/// <returns>Paire mouvement/score.</returns>
	private KeyValuePair<Move, int> Minimize (Node currentNode, int depth, int alpha, int beta) {
		
		searchCount++;

		if (currentNode.endOfGame ()) {
			return new KeyValuePair<Move, int> (null, 9999999);
		}

		if (depth == 0) { // Cannot occur at first call at thus cannot return a null move to Run ()
			int eval = currentNode.Evaluation ();
			return new KeyValuePair<Move, int> (null, eval);
		}

		int score = int.MaxValue;
		foreach (Move m in currentNode.board.getPlayerMoves (currentNode.opponentPlayer)) {
			Node nextNode = new Node (currentNode, m, currentNode.player, currentNode.opponentPlayer);
			score = Math.Min (score, Maximize (nextNode, depth - 1, alpha, beta, false).Value);
			// Score must be STRICTLY INFERIOR (Equal can happen but is pointless, Superior cannot occur)
			if (score < beta) {
				beta = score;
			}
			if (beta <= alpha) { // Can occur on first call
				break;
			}
		}

		// Does not have to return a move, as Maximize () is aware of it
		return new KeyValuePair<Move, int> (null, score);

	}

	private Move AlphaBetaBasic () {
		
		searchCount = 0;
		
		Console.WriteLine (_GameManager.currentPlayerIndex == 0 ? "SENTE turn n° " + _GameManager.turnCount + "\nDébut recherche AB" : "GOTE turn n° " + _GameManager.turnCount + "\nDébut recherche AB");
		_GameManager.workFlow.Append (_GameManager.currentPlayerIndex == 0 ? "SENTE turn n° " + _GameManager.turnCount + "\nDébut recherche AB" : "GOTE turn n° " + _GameManager.turnCount + "\nDébut recherche AB");

		var watch = System.Diagnostics.Stopwatch.StartNew ();
		
		Node startingNode = new Node (player, opponentPlayer);
		moveToPlay = Maximize (startingNode, depth, int.MinValue, int.MaxValue, true).Key;

		watch.Stop ();

		Console.WriteLine ("Fin recherche AB\nNombre de noeuds visités : " + searchCount + ",\nTemps écoulé : " + watch.ElapsedMilliseconds + "ms,\nScore du noeud séléctionné (Pour l'IA actuelle, plus est mieux) : " + this.selectedScore);
		_GameManager.workFlow.Append ("\nFin recherche AB\nNombre de noeuds visités : " + searchCount + ",\nTemps écoulé : " + watch.ElapsedMilliseconds + "ms,\nScore du noeud séléctionné (Pour l'IA actuelle, plus est mieux) : " + this.selectedScore);

		return moveToPlay;

	}

	private Move AlphaBetaIterativeAspiration () {
		Console.WriteLine (_GameManager.currentPlayerIndex == 0 ? "SENTE turn n° " + _GameManager.turnCount + "\nDébut recherche AB Iterative Deepening & Aspiration Search" : "GOTE turn n° " + _GameManager.turnCount + "\nDébut recherche AB Iterative Deepening & Aspiration Search");
		_GameManager.workFlow.Append (_GameManager.currentPlayerIndex == 0 ? "SENTE turn n° " + _GameManager.turnCount + "\nDébut recherche AB Iterative Deepening & Aspiration Search" : "GOTE turn n° " + _GameManager.turnCount + "\nDébut recherche ABIterative Deepening & Aspiration Search");
		
		var watch = System.Diagnostics.Stopwatch.StartNew ();

		searchCount = 0;
		
		int alpha = int.MinValue;
		int beta = int.MaxValue;
		int bestScore;
		
		KeyValuePair<Move, int> bestTupleMoveScore = new KeyValuePair<Move, int> (null, int.MinValue); // Max will be called first so it will replace minValue
		NodeTree startingNode = new NodeTree (player, opponentPlayer);

		for (int i = 1; i <= depth; i++) {
			Console.WriteLine ("Search at depth " + i);
			bestTupleMoveScore = Maximize (startingNode, depth, alpha, beta, true);
			bestScore = bestTupleMoveScore.Value;
			int window = 10;
			if (alpha >= bestScore || beta <= bestScore) {
				i--;
				Console.WriteLine ("Out of the window, research at depth " + i);
				alpha = int.MinValue; 
				beta = int.MaxValue;
			} else {
				alpha = bestScore - window;
				beta = bestScore + window;
			}
		}
		watch.Stop ();
		Console.WriteLine ("Fin recherche AB ID Aspiration\nNombre de noeuds visités : " + searchCount + ",\nTemps écoulé : " + watch.ElapsedMilliseconds + "ms,\nScore du noeud séléctionné (Pour l'IA actuelle, plus est mieux) : " + selectedScore);
		_GameManager.workFlow.Append ("\nFin recherche AB ID Aspiration\nNombre de noeuds visités : " + searchCount + ",\nTemps écoulé : " + watch.ElapsedMilliseconds + "ms,\nScore du noeud séléctionné (Pour l'IA actuelle, plus est mieux) : " + selectedScore);

		return bestTupleMoveScore.Key;
	}

}
