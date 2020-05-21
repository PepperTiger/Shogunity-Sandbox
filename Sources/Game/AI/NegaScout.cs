using System;
using System.Collections.Generic;
using ShogiUtils;

/// <summary>
/// Gestion d'une IA de type NegaScout.
/// </summary>
public class NegaScout : AIHandler {

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

	public NegaScout (Player me, Player him) {

		this.player = me;
		this.opponentPlayer = him;
		depth = me.color == GameColor.SENTE ? _GameConfig.player1Difficulty : _GameConfig.player2Difficulty;

	}

	/// <summary>
	/// Execution de l'algorithme de recherche.
	/// </summary>
	public override void Run () {

		this.searchCount = 0;

		Console.WriteLine (_GameManager.currentPlayerIndex == 0 ? "SENTE turn n° " + _GameManager.turnCount + "\nDébut recherche NegaScout" : "GOTE turn n° " + _GameManager.turnCount + "\nDébut recherche NegaScout");
		_GameManager.workFlow.Append (_GameManager.currentPlayerIndex == 0 ? "SENTE turn n° " + _GameManager.turnCount + "\nDébut recherche NegaScout" : "GOTE turn n° " + _GameManager.turnCount + "\nDébut recherche NegaScout");

		var watch = System.Diagnostics.Stopwatch.StartNew ();

		Node startingNode = new Node (player, opponentPlayer);
		moveToPlay = negaScout (startingNode, depth, int.MinValue / 2, int.MaxValue / 2, player.color, true).Key;

		watch.Stop ();

		Console.WriteLine ("Fin recherche NegaScout\nNombre de noeuds visités : " + searchCount + ",\nTemps écoulé : " + watch.ElapsedMilliseconds + "ms,\nScore du noeud séléctionné (Pour l'IA actuelle, plus est mieux) : " + this.selectedScore);
		_GameManager.workFlow.Append ("\nFin recherche NegaScout\nNombre de noeuds visités : " + searchCount + ",\nTemps écoulé : " + watch.ElapsedMilliseconds + "ms,\nScore du noeud séléctionné (Pour l'IA actuelle, plus est mieux) : " + this.selectedScore);

		isDone = true;

	}

	/// <summary>
	/// NegaScout recursive call.
	/// </summary>
	/// <param name="currentNode">Current node.</param>
	/// <param name="depth">Depth.</param>
	/// <param name="alpha">Alpha.</param>
	/// <param name="beta">Beta.</param>
	/// <param name="color">Color of the current player.</param>
	private KeyValuePair<Move, int> negaScout (
		Node currentNode,
		int depth,
		int alpha,
		int beta,
		GameColor color,
		bool isInitial
	) {

		searchCount++;

		if (currentNode.endOfGame ()) {
			return new KeyValuePair<Move, int> (null, -9999999);
		}

		if (depth == 0) {
			int eval = currentNode.Evaluation ();
			if (color == player.color) {
				return new KeyValuePair<Move, int> (null, eval);
			} else {
				return new KeyValuePair<Move, int> (null, -eval);
			}
		}

		Player p;

		if (color == this.player.color) {
			p = currentNode.player;
		} else {
			p = currentNode.opponentPlayer;
		}

		List<Move> listOfMoves = currentNode.board.getPlayerMoves (p);

		List<PairNodeMove> listOfPairs = new List<PairNodeMove> ();

		foreach (Move move in listOfMoves) {
			listOfPairs.Add (new PairNodeMove (
				new Node (currentNode, move, currentNode.player, currentNode.opponentPlayer),
				move,
				color,
				player.color
			));
		}

		listOfPairs.Sort ();

		int score = 0;
		Move selectedMove = null;
		int window = beta;

		GameColor nextColor = color == GameColor.SENTE ? GameColor.GOTE : GameColor.SENTE;

		for (int i = 0; i < listOfPairs.Count; i++) {

			Move m = listOfPairs [i].move;

			Node nextNode = listOfPairs [i].node;

			score = -negaScout (nextNode, depth - 1, -window, -alpha, nextColor, false).Value;
			if (alpha < score && score < beta && i > 0) {
				score = -negaScout (nextNode, depth - 1, -beta, -alpha, nextColor, false).Value;
			}

			if (score > alpha) {
				alpha = score;
				selectedMove = m;
				if (isInitial) {
					this.selectedScore = score;
				}
			}

			if (beta <= alpha) {
				break;
			}

			window = alpha + 1;

		}

		return new KeyValuePair<Move, int> (selectedMove, alpha);

	}

}

public class PairNodeMove : IComparable<PairNodeMove> {

	public Node node;
	public Move move;

	public int eval;

	public PairNodeMove (Node n, Move m, GameColor color, GameColor playerColor) {
		this.node = n;
		this.move = m;
		if (node.endOfGame ()) {
			this.eval = 9999999;
		} else {
			if (color == playerColor) {
				this.eval = node.Evaluation ();
			} else {
				this.eval = -node.Evaluation ();
			}
		}
	}

	public int CompareTo (PairNodeMove otherPair) {
		return this.eval > otherPair.eval ? -1 : this.eval == otherPair.eval ? 0 : 1;
	}

}
