using System;
using System.Collections.Generic;
using ShogiUtils;

public class NodeTree : Node {

	/// <summary>
	/// Parent of this node, only root have a null parent;
	/// </summary>
	public NodeTree parent;

	/// <summary>
	/// childrens of this node
	/// </summary>
	public List<NodeTree> children;

	/// <summary>
	/// are the children created ?
	/// </summary>
	public bool isExpanded;

	/// <summary>
	/// for PNS, proof of this node
	/// </summary>
	public int proof;

	/// <summary>
	/// for PNS, disproof of this node
	/// </summary>
	public int disproof;

	/// <summary>
	/// for Proof Number Search
	/// </summary>
	public ProofNumberSearch.PNS pns;

	public bool isMaximizing;

	public Move move;
	/// <summary>
	/// Create root Node
	/// </summary>
	/// <param name="player"></param>
	/// <param name="opponentPlayer"></param>
	public NodeTree (Player player, Player opponentPlayer) : base (player, opponentPlayer) {
		isInitialPlayer = true;
		parent = null;
		children = new List<NodeTree> ();
		setPNS ();
		isMaximizing = true;
		isExpanded = false;
		move = null;
	}

	/// <summary>
	/// Create a children node, and apply the move given to get to the new state of the game
	/// </summary>
	/// <param name="parentNode">the parent node of this node</param>
	/// <param name="move">the move to get to the new state from parentNode</param>
	/// <param name="player">the current player in this node</param>
	/// <param name="opponentPlayer">the opponent player</param>
	///  <param name="makeTree"> will parent and children be insitialised</param>
	public NodeTree (NodeTree parentNode, Move move, Player player, Player opponentPlayer) : base (parentNode, move, player, opponentPlayer) {
		this.move = move;
		isInitialPlayer = !parentNode.isInitialPlayer;
		parent = parentNode;
		children = new List<NodeTree> ();
		isExpanded = false;
		isMaximizing = parent.isMaximizing ? false : true;
		proof = 1;
		disproof = 1;
		setPNS ();
		nodeScore = 0;
	}

	public void initChildren () {
		foreach (Move move in board.getPlayerMoves (player)) {
			children.Add (new NodeTree (this, move, player, opponentPlayer));
		}
	}

	public void clearChildren () {
		children.Clear ();
	}

	public void setPNS () {
		if (endOfGame ()) {
			if (isMaximizing) {
				pns = ProofNumberSearch.PNS.PROVEN;
				proof = 0;
				disproof = int.MaxValue;
			} else {
				pns = ProofNumberSearch.PNS.DISPROVEN;
				proof = int.MaxValue;
				disproof = 0;
			}
		} else {
			pns = ProofNumberSearch.PNS.UNKNOWN;
			//proof = 1;
			//disproof = 1;
		}
	}
	public void PrintTree (string indentation, bool last) {
		System.Threading.Thread.Sleep(10);
		Console.Write (indentation);

		if(last) {
			Console.Write ("\\-");
			indentation += "  ";
		} else {
			Console.Write ("| ");
			indentation += "| ";
		}
		Console.WriteLine (proof+ "," + disproof+ "," + nodeScore);
		for(int i = 0; i< children.Count; i++) {
			children[i].PrintTree (indentation, i == children.Count - 1);
		}
	}
}