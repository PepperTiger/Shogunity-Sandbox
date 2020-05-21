using System.Collections;
using ShogiUtils;
using System.Collections.Generic;
using System.Linq;
using System;

/// <summary>
/// Case de capture où vont les pièces capturées.
/// </summary>
[Serializable]
public class CaptureBox : Box {

	/// <summary>
	/// Capacité.
	/// </summary>
	public int capacity;

	/// <summary>
	/// Pile de pièces.
	/// </summary>
	public Stack<Token> tokens;

	/// <summary>
	/// Type de pièce.
	/// </summary>
	public TokenType type;

	/// <summary>
	/// Liste des pièces de la case de capture.
	/// </summary>
	public List<Token> tokenListCaptured;

	public CaptureBox (TokenType tokenType, int capacity) : base () {
		this.capacity = capacity;
		this.type = tokenType;
		this.tokens = new Stack<Token> ();
		this.coord = new Coordinates (-100, -100);
		tokenListCaptured = new List<Token> ();
	}

	/// <summary>
	/// Ajoute une pièce à la pile.
	/// </summary>
	/// <param name="token">Une pièce.</param>
	public void addToken (Token token) {
		
		token.setCaptureLocation (this);
		tokens.Push (token);

	}

	/// <summary>
	/// Dépile une pièce.
	/// </summary>
	public void removeToken () {
		
		tokens.Pop ();

	}

	/// <summary>
	/// Met à jour la liste des tokens sur la case de capture
	/// </summary>
	public void UpdateTokenListCaptured () 
	{
		tokenListCaptured.Clear ();
		foreach (Token t in tokens) {
			tokenListCaptured.Add (t);
		}
	}

	public List<Token> getTokenListCaptured() 
	{
		return tokenListCaptured;
	}
}
