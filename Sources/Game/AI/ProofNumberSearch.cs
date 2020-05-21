using System;
using ShogiUtils;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// Gestion d'une IA de type ProofNumberSearch, algorithme : https://chessprogramming.wikispaces.com/Proof-number+search
/// </summary>
public class ProofNumberSearch : AIHandler {

	public enum PNS {
		UNKNOWN, PROVEN, DISPROVEN
	}

	/// <summary>
	/// Player = Le joueur, OpponentPlayer = Son adversaire.
	/// </summary>
	public Player player, opponentPlayer;

	/// <summary>
	/// Profondeur de recherche.
	/// </summary>
	public int depth;

	/// <summary>
	/// compteur de noeuds recherchés
	/// </summary>
	public static int searchCount;

	/// <summary>
	/// sauvegarde du score du noeud séléctionné
	/// </summary>
	public int selectedScore;

	/// <summary>
	/// Constructor for the PNS algorithm
	/// </summary>
	/// <param name="thisPlayer">the player represented by this instance</param>
	/// <param name="opponent">the opponent player</param>
	public ProofNumberSearch (Player thisPlayer, Player opponent) {
		player = thisPlayer;
		opponentPlayer = opponent;
		depth = thisPlayer.color == GameColor.SENTE ? _GameConfig.player1Difficulty : _GameConfig.player2Difficulty;
	}

	/// <summary>
	/// Execution de la recherche
	/// </summary>
	public override void Run () {
		Console.WriteLine (_GameManager.currentPlayerIndex == 0 ? "Sente turn n° " + _GameManager.turnCount + "\nDébut recherche PNS" : "Gote turn n°" + _GameManager.turnCount + "\nDébut recherche PNS");
		_GameManager.workFlow.Append (_GameManager.currentPlayerIndex == 0 ? "Sente turn n° " + _GameManager.turnCount + "\nDébut recherche PNS" : "Gote turn n°" + _GameManager.turnCount + "\nDébut recherche PNS");

		Stopwatch watch = Stopwatch.StartNew (); //Start the timer

		NodeTree startingNode = new NodeTree (player, opponentPlayer);

		moveToPlay = RunPNS (startingNode, depth, true, true, watch).Key;

		watch.Stop ();
		Console.WriteLine ("Nombre de noeuds visités : " + searchCount + ",\nTemps écoulé : " + watch.ElapsedMilliseconds + "ms,\nScore du noeud séléctionné (Pour l'IA actuelle, plus est mieux) : " + selectedScore + "\nFin recherche PNS");
		_GameManager.workFlow.Append ("\nNombre de noeuds visités : " + searchCount + ",\nTemps écoulé : " + watch.ElapsedMilliseconds + "ms,\nScore du noeud séléctionné (Pour l'IA actuelle, plus est mieux) : " + selectedScore + "\nFin recherche PNS");

		isDone = true;
	}

	public KeyValuePair<Move, int> RunPNS (NodeTree root, int depht, bool isInitial, bool isMaximizing, Stopwatch timer) {

		searchCount = 0;
		root.nodeScore = root.Evaluation ();
		root.setPNS ();
		//ExpandNode (root/*, depht*/);
		SetProofAndDisproof (root);
		NodeTree currentNode = root;
		//int depthTmp = depth;
		while (root.proof != 0 && root.disproof != 0 && timer.ElapsedMilliseconds < 2500 * depth) {

			NodeTree mostProving = SelectMostProvingNode (currentNode, depht);
			ExpandNode (mostProving, depth);
			//Console.WriteLine ("expanded");
			//searchCount += mostProving.children.Count;
			currentNode = UpdateAncestors (root, currentNode);
			//Console.WriteLine ("Ancestor update");
		}
		NodeTree selectedChild = null;
		int mostProvingValue = int.MaxValue; //You want the lowest proof number  
		List<Move> legalMoves = root.board.getPlayerMoves (player);
		Console.WriteLine ("nb de coup pour root : " + legalMoves.Count + "\nnb root children : " + root.children.Count);
		SetProofAndDisproof (root);
		for (int i = 0; i < root.children.Count; i++) {
			if (root.children[i].pns == PNS.PROVEN) {
				selectedChild = root.children[i];
				Console.WriteLine ("THIS should be winning move");
				break;
			} else {
				if (mostProvingValue > root.children[i].proof) {
					//Console.Write ("new best proving assignation");
					mostProvingValue = root.children[i].proof;
					selectedChild = root.children[i];
				}
			}
		}
		Console.WriteLine ("score : " + mostProvingValue);
		//root.PrintTree ("", true);
		return new KeyValuePair<Move, int> (selectedChild.move, selectedChild.nodeScore);
	}

	public static void SetProofAndDisproof (NodeTree current) {
		if (current.children.Count > 0 && current.isExpanded) {
			if (/* ! */current.isMaximizing) { // AND Node
				current.proof = 0;
				current.disproof = 99999;
				foreach (NodeTree child in current.children) {
					current.proof += child.proof;
					current.disproof = Math.Min (child.disproof, current.disproof);
				}
			} else { // OR Node
				current.proof = 99999;
				current.disproof = 0;
				foreach (NodeTree child in current.children) {
					current.disproof += child.disproof;
					current.proof = Math.Min(current.proof, child.proof);
				}
			}
		} else {
			//SetPNSbyScore (current);
			switch (current.pns) {
				case PNS.PROVEN:
					current.proof = 99999;
					current.disproof = 0;
					break;
				case PNS.DISPROVEN:
					current.proof = 0;
					current.disproof = 99999;
					break;
				case PNS.UNKNOWN:
					current.proof = 1;
					current.disproof = 1;
					break;
				default:
					break;
			}
		}
	}

	public static NodeTree SelectMostProvingNode (NodeTree current, int depth) {
		//Console.WriteLine ("SMPN");
		int depthTmp = depth;
		while (current.isExpanded && current.children.Count > 0 && depthTmp > 0) {
			int value = int.MaxValue;
			NodeTree best = null;
			//Console.WriteLine ("value of best child " + value + " child count " + current.children.Count);

			if (!current.isMaximizing) { // AND
				foreach (NodeTree child in current.children) {
					if (value > child.disproof) {
						best = child;
						value = child.disproof;
					}

				}
			} else { //OR
				foreach (NodeTree child in current.children) {
					if (value > child.proof) {
						best = child;
						value = child.proof;
					}
				}
			}
			depthTmp--;
			current = best;
			//Console.WriteLine ("current child ?" + current.children.Count);
		}
		ExpandNode (current, depthTmp);

		return current;
	}

	public static void ExpandNode (NodeTree current, int depth) {
		if ( depth == 0) {
			current.Evaluation ();
			SetProofAndDisproof (current);
			SetPNSbyScore (current);
			return;
		}
		if (!current.isExpanded) {
			current.initChildren ();
		}
		foreach (NodeTree child in current.children) {
			searchCount++;
			child.nodeScore = child.Evaluation ();
			SetProofAndDisproof (child);
			//SetPNSbyScore (child);
			if (child.isMaximizing) { //OR Node
				if (child.proof == 0) break;
			} else { //AND Node
				if (child.disproof == 0) break;
			}
		}
		current.isExpanded = true;
	}

	public NodeTree UpdateAncestors (NodeTree root, NodeTree current) {
		while (current != root) {
			int oldProof = current.proof;
			int oldDisproof = current.disproof;
			SetProofAndDisproof (current);
			Console.Write ("P = " + current.proof + "D = " + current.disproof);
			if (current.proof == oldDisproof && current.disproof == oldDisproof) {
				//ExpandNode (current/*, depth*/);
				return current;
			}
			current = current.parent;
		}
		SetProofAndDisproof (root);
		return root;
	}

	public static void SetPNSbyScore (NodeTree current) {

		if (current.endOfGame ()) {
			if (current.isMaximizing) {
				current.pns = PNS.PROVEN;
				return;
			} else {
				current.pns = PNS.DISPROVEN;
				return;
			}
		} else {
			int parentScore = current.parent == null ? 0 : current.parent.nodeScore;
			if (current.nodeScore - parentScore > 0) {
				//current.pns = PNS.PROVEN;
			} else {
				current.pns = PNS.DISPROVEN;
			}

		}
		//current.pns = PNS.UNKNOWN;
	}

	
}