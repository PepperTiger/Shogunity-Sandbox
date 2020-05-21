using System.Collections;
using System.Collections.Generic;
using ShogiUtils;

/// <summary>
/// Banc de capture.
/// </summary>
public class CaptureBench {
	
	/// <summary>
	/// Tableau de cases de capture pion.
	/// </summary>
	public CaptureBox boxPawn;

	/// <summary>
	/// Tableau de cases de capture tour.
	/// </summary>
	public CaptureBox boxRook;

	/// <summary>
	/// Tableau de cases de capture fou.
	/// </summary>
	public CaptureBox boxBishop;

	/// <summary>
	/// Tableau de cases de capture lancier.
	/// </summary>
	public CaptureBox boxLance;

	/// <summary>
	/// Tableau de cases de capture chevalier.
	/// </summary>
	public CaptureBox boxKnight;

	/// <summary>
	/// Tableau de cases de capture général d'argent.
	/// </summary>
	public CaptureBox boxSilver;

	/// <summary>
	/// Tableau de cases de capture général d'or.
	/// </summary>
	public CaptureBox boxGold;

	/// <summary>
	/// Joueur à qui appartient le banc de capture.
	/// </summary>
	public Player player;

	/// <summary>
	/// Liste des toutes les pièces sur le banc de capture.
	/// </summary>
	public List<Token> allTokenListCaptured;

	public CaptureBench () {
		this.boxPawn = new CaptureBox (TokenType.PAWN, 18); // 18
		this.boxRook = new CaptureBox (TokenType.ROOK, 2); // 2
		this.boxBishop = new CaptureBox (TokenType.BISHOP, 2); // 2
		this.boxLance = new CaptureBox (TokenType.LANCE, 4); // 4
		this.boxKnight = new CaptureBox (TokenType.KNIGHT, 4); // 4
		this.boxSilver = new CaptureBox (TokenType.SILVER, 4); // 4
		this.boxGold = new CaptureBox (TokenType.GOLD, 4); // 4
		allTokenListCaptured = new List<Token>();
	}

	/// <summary>
	/// Case de capture pion.
	/// </summary>
	/// <returns>Retourne une case de capture pion.</returns>
	public CaptureBox getBoxPawn () {
		
		if (boxPawn.tokens.Count < boxPawn.capacity) {
			return boxPawn;
		}

		return null;

	}

	/// <summary>
	/// Case de capture tour.
	/// </summary>
	/// <returns>Retourne une case de capture tour.</returns>
	public CaptureBox getBoxRook () {
		
		if (boxRook.tokens.Count < boxRook.capacity) {
			return boxRook;
		}

		return null;

	}

	/// <summary>
	/// Case de capture fou.
	/// </summary>
	/// <returns>Retourne une case de capture fou.</returns>
	public CaptureBox getBoxBishop () {
		
		if (boxBishop.tokens.Count < boxBishop.capacity) {
			return boxBishop;
		}

		return null;

	}

	/// <summary>
	/// Case de capture lancier.
	/// </summary>
	/// <returns>Retourne une case de capture lancier.</returns>
	public CaptureBox getBoxLance () {
		
		if (boxLance.tokens.Count < boxLance.capacity) {
			return boxLance;
		}

		return null;

	}

	/// <summary>
	/// Case de capture chevalier.
	/// </summary>
	/// <returns>Retourne une case de capture chevalier.</returns>
	public CaptureBox getBoxKnight () {
		
		if (boxKnight.tokens.Count < boxKnight.capacity) {
			return boxKnight;
		}

		return null;

	}

	/// <summary>
	/// Case de capture général d'argent.
	/// </summary>
	/// <returns>Retourne une case de capture général d'argent.</returns>
	public CaptureBox getBoxSilver () {
		
		if (boxSilver.tokens.Count < boxSilver.capacity) {
			return boxSilver;
		}

		return null;

	}

	/// <summary>
	/// Case de capture général d'or.
	/// </summary>
	/// <returns>Retourne une case de capture général d'or.</returns>
	public CaptureBox getBoxGold () {
		
		if (boxGold.tokens.Count < boxGold.capacity) {
			return boxGold;
		}

		return null;

	}

	/// <summary>
	/// Met à jour la liste des tokens sur le banc de capture.
	/// </summary>
	public void updateCaptureBench() 
	{

		boxPawn.UpdateTokenListCaptured ();
		boxGold.UpdateTokenListCaptured ();
		boxKnight.UpdateTokenListCaptured ();
		boxLance.UpdateTokenListCaptured ();
		boxBishop.UpdateTokenListCaptured ();
		boxRook.UpdateTokenListCaptured ();
		boxSilver.UpdateTokenListCaptured ();

		allTokenListCaptured.Clear ();
		foreach (Token t in boxPawn.getTokenListCaptured()) {
			allTokenListCaptured.Add (t);
		}
		foreach (Token t in boxRook.getTokenListCaptured()) {
			allTokenListCaptured.Add (t);
		}
		foreach (Token t in boxBishop.getTokenListCaptured()) {
			allTokenListCaptured.Add (t);
		}
		foreach (Token t in boxLance.getTokenListCaptured()) {
			allTokenListCaptured.Add (t);
		}
		foreach (Token t in boxKnight.getTokenListCaptured()) {
			allTokenListCaptured.Add (t);
		}
		foreach (Token t in boxSilver.getTokenListCaptured()) {
			allTokenListCaptured.Add (t);
		}
		foreach (Token t in boxGold.getTokenListCaptured()) {
			allTokenListCaptured.Add (t);
		}

	}

	public List<Token> getAllTokenListCaptured() 
	{
		return allTokenListCaptured;
	}

}
