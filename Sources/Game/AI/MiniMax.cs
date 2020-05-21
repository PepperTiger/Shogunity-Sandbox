using System;
using System.Collections.Generic;
using ShogiUtils;

/// <summary>
/// Gestion d'une IA de type MiniMax.
/// </summary>
public class MiniMax : AIHandler {

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

	public MiniMax (Player me, Player him) {

		this.player = me;
		this.opponentPlayer = him;
		depth = me.color == GameColor.SENTE ? _GameConfig.player1Difficulty : _GameConfig.player2Difficulty;

	}

	/// <summary>
	/// Execution de l'algorithme de recherche.
	/// </summary>
	public override void Run () {
		
		this.searchCount = 0;

		Console.WriteLine (_GameManager.currentPlayerIndex == 0 ? "SENTE turn n° " + _GameManager.turnCount + "\nDébut recherche MiniMax" : "GOTE turn n° " + _GameManager.turnCount + "\nDébut recherche MiniMax");
		_GameManager.workFlow.Append (_GameManager.currentPlayerIndex == 0 ? "SENTE turn n° " + _GameManager.turnCount + "\nDébut recherche MiniMax" : "GOTE turn n° " + _GameManager.turnCount + "\nDébut recherche MiniMax");

		var watch = System.Diagnostics.Stopwatch.StartNew ();

		Node startingNode = new Node (player, opponentPlayer);
		moveToPlay = Maximize (startingNode, depth, true).Key;

		watch.Stop ();

		Console.WriteLine ("Fin recherche MiniMax\nNombre de noeuds visités : " + searchCount + ",\nTemps écoulé : " + watch.ElapsedMilliseconds + "ms" + ",\nScore du noeud séléctionné (Pour l'IA actuelle, plus est mieux) : " + this.selectedScore);
		_GameManager.workFlow.Append ("\nFin recherche MiniMax\nNombre de noeuds visités : " + searchCount + ",\nTemps écoulé : " + watch.ElapsedMilliseconds + "ms" + ",\nScore du noeud séléctionné (Pour l'IA actuelle, plus est mieux) : " + this.selectedScore);

		isDone = true;

	}

	/// <summary>
	/// Maximizer.
	/// </summary>
	/// <param name="currentNode">Node courant.</param>
	/// <param name="depth">Profondeur de recherche restante.</param>
	/// <returns>Paire mouvement/score.</returns>
	private KeyValuePair<Move, int> Maximize (Node currentNode, int depth, bool isInitial) {

		this.searchCount++;

		if (currentNode.endOfGame ()) {
			return new KeyValuePair<Move, int> (null, -9999999);
		}

		if (depth == 0) { // Cannot occur at first call at thus cannot return a null move to Run ()
			int eval = currentNode.Evaluation ();
			return new KeyValuePair<Move, int> (null, eval); // Can only return the null move to Minimize (), which does not use it
		}

		int max = int.MinValue;
		Move selectedMove = null;
		foreach (Move m in currentNode.board.getPlayerMoves (currentNode.player)) {
			Node nextNode = new Node (currentNode, m, currentNode.player, currentNode.opponentPlayer);
			int score = Minimize (nextNode, depth - 1).Value;
			if (score > max) {
				max = score;
				selectedMove = m;
				if (isInitial) {
					this.selectedScore = score;
				}
			}
		}

		// Has to return a move, so it can return one to Run ()
		return new KeyValuePair<Move, int> (selectedMove, max);

	}

	/// <summary>
	/// Minimizer.
	/// </summary>
	/// <param name="currentNode">Node courant.</param>
	/// <param name="depth">Profondeur de recherche restante.</param>
	/// <returns>Paire mouvement/score.</returns>
	private KeyValuePair<Move, int> Minimize (Node currentNode, int depth) {

		this.searchCount++;
		
		if (currentNode.endOfGame ()) {
			return new KeyValuePair<Move, int> (null, 9999999);
		}

		if (depth == 0) { // Cannot occur at first call at thus cannot return a null move to Run ()
			int eval = currentNode.Evaluation ();
			return new KeyValuePair<Move, int> (null, eval);
		}

		int min = int.MaxValue;
		foreach (Move m in currentNode.board.getPlayerMoves (currentNode.opponentPlayer)) {
			Node nextNode = new Node (currentNode, m, currentNode.player, currentNode.opponentPlayer);
			int score = Maximize (nextNode, depth - 1, false).Value;
			if (score < min) {
				min = score;
			}
		}

		// Does not have to return a move, as Maximize () is aware of it
		return new KeyValuePair<Move, int> (null, min);

	}

}
