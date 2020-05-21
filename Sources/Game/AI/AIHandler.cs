using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using ShogiUtils;
using System.Text;
using System.Collections;

/// <summary>
/// Gestion d'une IA.
/// </summary>
public class AIHandler {

	/// <summary>
	/// Le coup trouvé.
	/// </summary>
	public Move moveToPlay = null;

    /// <summary>
    /// 
    /// </summary>
    public bool toPromote = false;

	/// <summary>
	/// Vrai si le coup est trouvé, faux sinon.
	/// </summary>
	private bool _isDone = false;

	/// <summary>
	/// Permet d'employer un verrou.
	/// </summary>
	private object _handle = new object ();

	/// <summary>
	/// Le thread de recherche.
	/// </summary>
	private System.Threading.Thread _thread = null;

    private Boolean promotionBool;

    public Boolean getPromotionBool()
    {
        return promotionBool;
    }

    public void setPromotionBool(Boolean value)
    {
        promotionBool = value;
    }

	/// <summary>
	/// Vrai si le coup est trouvé, faux sinon.
	/// </summary>
	/// <returns>Un bool vrai ou faux.</returns>
	public bool isDone {
		
		get {
			bool temp;
			lock (_handle) {
				temp = _isDone;
			}
			return temp;
		}

		set {
			lock (_handle) {
				_isDone = value;
			}
		}

	}

	/// <summary>
	/// Permet de chercher le meilleur coup déduit par l'algorithme.
	/// </summary>
	public void SearchBestMove () {
		
		if (_thread == null) {
			
			isDone = false;
            Start();

		} else {
			
			if (Update ()) {
				_thread = null;
				isDone = false;
				Start ();
			} else {
				Console.WriteLine ("ERROR : Can't start search thread while another is active!");
				Environment.Exit (1);
			}

		}

    }

	/// <summary>
	/// Vérifie si la recherche de mouvement est terminée.
	/// </summary>
	/// <returns>Un bool vrai ou faux.</returns>
	public bool Update () {
		
		if (isDone) {
			return true;
		} else {
			return false;
		}

	}

	/// <summary>
	/// Lance le thread de recherche.
	/// </summary>
	public void Start () {
		
		_thread = new System.Threading.Thread (() => Run ());
		_thread.Start ();

	}

	/// <summary>
	/// Execution du thread de recherche.
	/// </summary>
	public virtual void Run () {
		
		Player currentPlayer = _GameManager.players [_GameManager.currentPlayerIndex];

		List<Move> moves = _GameManager.board.getPlayerMoves (currentPlayer);

		System.Random rng = new System.Random ();
		int r = rng.Next (moves.Count);

		moveToPlay = moves [r];

		System.Threading.Thread.Sleep (500); // Pauses the algorithm for this gamemode as it's too fast
		isDone = true; // Makes use of the lock

	}

	/// <summary>
	/// Attente d'un coup à jouer puis execution de ce coup par l'IA.
	/// </summary>
	public void WaitFor () {
		
		while (!Update ()) {

			System.Threading.Thread.Sleep (100);

		}

		try {

			moveToPlay.play ();

			Console.Write (StateToString (_GameManager.board));
			Console.Write (CapturesToString ());
			Console.Write ("\n\n");

			_GameManager.workFlow.Append (StateToString (_GameManager.board));
			_GameManager.workFlow.Append (CapturesToString ());
			_GameManager.workFlow.Append ("\n\n");

			_GameManager.awaitMakingAiMove = false;

		} catch (NullReferenceException) {
			
			Console.WriteLine ("ERROR : Trying to play a null move!");
			Environment.Exit (1);

		}

	}

	/// <summary>
	/// Permet de cloner un objet sérializable.
	/// </summary>
	/// <param name="board">Un tableau de jeu.</param>
	/// <returns>Une copie de l'objet donné.</returns>
	public static Board CloneBoard (Board board) {
		if (!board.GetType ().IsSerializable) {
			throw new Exception ("Object must be serializable");
		}
		MemoryStream ms = new MemoryStream ();
		BinaryFormatter bf = new BinaryFormatter ();
		bf.Serialize (ms, board);
		ms.Position = 0;
		Board newBoard = (Board) bf.Deserialize (ms);
		ms.Close ();
		return newBoard;
	}

	/// <summary>
	/// Permet de cloner un objet sérializable.
	/// </summary>
	/// <param name="player">Un joueur.</param>
	/// <returns>Une copie de l'objet donné.</returns>
	public static Player ClonePlayer (Player player) {
		if (!player.GetType ().IsSerializable) {
			throw new Exception ("Object must be serializable");
		}
		MemoryStream ms = new MemoryStream ();
		BinaryFormatter bf = new BinaryFormatter ();
		bf.Serialize (ms, player);
		ms.Position = 0;
		Player newPlayer = (Player) bf.Deserialize (ms);
		ms.Close ();
		return newPlayer;
	}

	/// <summary>
	/// Permet de cloner un objet sérializable.
	/// </summary>
	/// <param name="move">Un mouvement.</param>
	/// <returns>Une copie de l'objet donné.</returns>
	public static Move CloneMove (Move move) {
		if (!move.GetType ().IsSerializable) {
			throw new Exception ("Object must be serializable");
		}
		MemoryStream ms = new MemoryStream ();
		BinaryFormatter bf = new BinaryFormatter ();
		bf.Serialize (ms, move);
		ms.Position = 0;
		Move newMove = (Move) bf.Deserialize (ms);
		ms.Close ();
		return newMove;
	}

	/// <summary>
	/// Permet de cloner un objet sérializable.
	/// </summary>
	/// <param name="list">Une liste de cases de capture.</param>
	/// <returns>Une copie de l'objet donné.</returns>
	public static List<CaptureBox> CloneCaptureBoxesNode (List<CaptureBox> list) {
		
		List<CaptureBox> newList = new List<CaptureBox> ();

		foreach (CaptureBox box in list) {
			if (!box.GetType ().IsSerializable) {
				throw new Exception ("Object must be serializable");
			}
			MemoryStream ms = new MemoryStream ();
			BinaryFormatter bf = new BinaryFormatter ();
			bf.Serialize (ms, box);
			ms.Position = 0;
			CaptureBox newBox = (CaptureBox) bf.Deserialize (ms);
			ms.Close ();
			newList.Add (newBox);
		}

		return newList;

	}

	/// <summary>
	/// Permet de cloner un objet sérializable.
	/// </summary>
	/// <param name="cb">Un banc de capture.</param>
	/// <returns>Une copie de l'objet donné.</returns>
	public static List<CaptureBox> CloneCaptureBoxesInitial (CaptureBench cb) {

		List<CaptureBox> listBoxes = new List<CaptureBox> ();

		if (!cb.boxBishop.GetType ().IsSerializable) {
			throw new Exception ("Object must be serializable");
		}
		MemoryStream ms1 = new MemoryStream ();
		BinaryFormatter bf1 = new BinaryFormatter ();
		bf1.Serialize (ms1, cb.boxBishop);
		ms1.Position = 0;
		listBoxes.Add ((CaptureBox) bf1.Deserialize (ms1));
		ms1.Close ();

		if (!cb.boxGold.GetType ().IsSerializable) {
			throw new Exception ("Object must be serializable");
		}
		MemoryStream ms2 = new MemoryStream ();
		BinaryFormatter bf2 = new BinaryFormatter ();
		bf2.Serialize (ms2, cb.boxGold);
		ms2.Position = 0;
		listBoxes.Add ((CaptureBox) bf2.Deserialize (ms2));
		ms2.Close ();

		if (!cb.boxKnight.GetType ().IsSerializable) {
			throw new Exception ("Object must be serializable");
		}
		MemoryStream ms3 = new MemoryStream ();
		BinaryFormatter bf3 = new BinaryFormatter ();
		bf3.Serialize (ms3, cb.boxKnight);
		ms3.Position = 0;
		listBoxes.Add ((CaptureBox) bf3.Deserialize (ms3));
		ms3.Close ();

		if (!cb.boxLance.GetType ().IsSerializable) {
			throw new Exception ("Object must be serializable");
		}
		MemoryStream ms4 = new MemoryStream ();
		BinaryFormatter bf4 = new BinaryFormatter ();
		bf4.Serialize (ms4, cb.boxLance);
		ms4.Position = 0;
		listBoxes.Add ((CaptureBox) bf4.Deserialize (ms4));
		ms4.Close ();

		if (!cb.boxPawn.GetType ().IsSerializable) {
			throw new Exception ("Object must be serializable");
		}
		MemoryStream ms5 = new MemoryStream ();
		BinaryFormatter bf5 = new BinaryFormatter ();
		bf5.Serialize (ms5, cb.boxPawn);
		ms5.Position = 0;
		listBoxes.Add ((CaptureBox) bf5.Deserialize (ms5));
		ms5.Close ();

		if (!cb.boxRook.GetType ().IsSerializable) {
			throw new Exception ("Object must be serializable");
		}
		MemoryStream ms6 = new MemoryStream ();
		BinaryFormatter bf6 = new BinaryFormatter ();
		bf6.Serialize (ms6, cb.boxRook);
		ms6.Position = 0;
		listBoxes.Add ((CaptureBox) bf6.Deserialize (ms6));
		ms6.Close ();

		if (!cb.boxSilver.GetType ().IsSerializable) {
			throw new Exception ("Object must be serializable");
		}
		MemoryStream ms7 = new MemoryStream ();
		BinaryFormatter bf7 = new BinaryFormatter ();
		bf7.Serialize (ms7, cb.boxSilver);
		ms7.Position = 0;
		listBoxes.Add ((CaptureBox) bf7.Deserialize (ms7));
		ms7.Close ();

		return listBoxes;

	}

	/// <summary>
	/// Create a string from a board.
	/// </summary>
	/// <param name="board">The board.</param>
	/// <returns>A representation of the board.</returns>
	public static string StateToString (Board board) {
		StringBuilder sb = new StringBuilder ();
		for (int i = 0; i < board.boxes.Count; i++) {
			if (i % 9 == 0) {
				sb.Append ("|\n");
			}
			if (board.boxes [i].token != null) {
				sb.Append ("|");
				sb.Append (board.boxes [i].token.owner.color == GameColor.SENTE ? "S" : "G");
				sb.Append (board.boxes [i].token.getTokenType () == TokenType.KNIGHT ? "N" : board.boxes [i].token.getTokenType ().ToString ().Substring (
					0,
					1
				));
				if (board.boxes [i].token.isPromoted) {
					sb.Append ("+");
				} else {
					sb.Append (" ");
				}
			} else {
				sb.Append ("|   ");
			}
		}
		sb.Append ("|\n");
		return sb.ToString ();
	}

	/// <summary>
	/// Create a string from a the captured tokens.
	/// </summary>
	/// <returns>A representation of the captured tokens.</returns>
	public static string CapturesToString () {
		StringBuilder sb = new StringBuilder ();
		sb.Append ("|");
		foreach (Token token in _GameManager.tokens) {
			if (token.isCaptured) {
				if (token.owner.color == GameColor.SENTE) {
					sb.Insert (0, token.getLetter ());
				} else {
					sb.Append (token.getLetter ());
				}
			}
		}
		sb.Replace ("|", "\nGote captured tokens : ");
		sb.Insert (0, "Sente captured tokens : ");
		sb.Append ("\n");
		return (sb.ToString ());
	}

}
