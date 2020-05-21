using System.Collections.Generic;
using ShogiUtils;
using System;

/// <summary>
/// Classe définissant une pièce du jeu.
/// </summary>
[Serializable]
public abstract class Token : IEquatable<Token>
{

	/// <summary>
	/// Identifiant de la pièce.
	/// </summary>
	public int id;

	/// <summary>
	/// Case de capture si la pièce est capturée prise.
	/// </summary>
	public CaptureBox captureLocation;

	/// <summary>
	/// Case sur laquelle est la pièce.
	/// </summary>
	public Box box;

	/// <summary>
	/// Type de pièce.
	/// </summary>
	public TokenType type;

	///<summary>
	/// Valeur de la pièce.
	/// </summary>
	public int value = 0;

	/// <summary>
	/// Etat de promotion de la pièce.
	/// </summary>
	public bool isPromoted;

	/// <summary>
	/// Joueur possesseur de la pièce.
	/// </summary>
	public Player owner;

	/// <summary>
	/// Etat de sélection de la pièce.
	/// </summary>
	public bool selected;

	/// <summary>
	/// Etat de capture de la pièce.
	/// </summary>
	public bool isCaptured;

	/// <summary>
	/// Liste des mouvements autorisés de la pièce.
	/// </summary>
	public List<Coordinates> moves = null;

    /// <summary>
    /// ID interne de la pièce par rapport à son type et au camp auquelle elle appartient.
    /// </summary>
    public int internalID;

	/// <summary>
	/// Initialisation de la pièce.
	/// </summary>
	public Token () {
		this.type = getTokenType ();
		this.isPromoted = false;
		this.selected = false;
		this.isCaptured = false;
        this.possibleEats = new List<Coordinates>();
	}
            
    public Token(bool isCaptured)
    {
        this.type = getTokenType();
        this.isPromoted = false;
        this.selected = false;
        this.isCaptured = isCaptured;
        this.possibleEats = new List<Coordinates>();
    }

    /// <summary>
    /// Mouvements autorisés par la pièce.
    /// </summary>
    /// <returns>Une liste de coordonnées autorisées lors des déplacements.</returns>
    /// <param name="board">Le plateau de jeu.</param>
    public abstract List<Coordinates> legalMoves(Board board);
    public abstract List<Coordinates> possibleMoves(Board board);
    public List<Coordinates> possibleEats;

    /// <summary>
    /// Liste des mouvements possibles de la pièce.
    /// </summary>
    /// <returns>Une liste déplacements possibles.</returns>
    /// <param name="board">Le plateau de jeu.</param>
    //public abstract List<Move> GetTokenMoves2(Board board);
    public virtual List<Move> GetTokenMoves(Board board)
    {
        List<Move> m = new List<Move>();

        foreach (Coordinates c in possibleMoves(board))
        {
            m.Add(new Move(this, owner, box.coord, c));
        }
        foreach (Coordinates c in possibleMovesPlus(board))
        {
            m.Add(new Move(this, owner, box.coord, c));
        }
        return m;
    }

    /// <summary>
    /// Ajout d'un mouvement autorisé par la pièce, après vérification.
    /// </summary>
    /// <param name="board">Le plateau de jeu.</param>
    /// <param name="neighbor">Enumeration d'une case voisine.</param>
    /// <param name="legalMoves">Une liste de coordonnees</param>
    public virtual void CheckNeighborIsLegal(Neighbor neighbor, List<Coordinates> legalMoves, Board board)
    {
        GameColor ownerColor = owner.color;
        Coordinates c = box.coord.getNeighbor(neighbor, ownerColor);
        Box b;
        if (c.isInsideBorders())
        {
            b = board.boxes[c.getIndex()];
            if ((b.token == null) || (b.token.owner.color != owner.color))
            {
                legalMoves.Add(c);
            }
        }
    }

    /// <summary>
    /// Vérifie si une pièce ennemie est présente à cette endroit
    /// </summary>
    /// <param name="board">Le plateau de jeu.</param>
    /// <param name="neighbor">Enumeration d'une case voisine.</param>
    /// <param name="legalMoves">Une liste de coordonnees</param>
    public virtual void CheckNeighborIsEatable(Neighbor neighbor, List<Coordinates> legalMoves, Board board)
    {
        GameColor ownerColor = owner.color;
        Coordinates c = box.coord.getNeighbor(neighbor, ownerColor);
        Box b;
        if (c.isInsideBorders())
        {
            b = board.boxes[c.getIndex()];
            if ((b.token != null) && (b.token.owner.color != owner.color))
            {
                legalMoves.Add(c);
            }
        }
    }

    /// <summary>
    /// Ajout des mouvements dans une direction autorisés par la pièce, après vérification.
    /// </summary>
    /// <param name="board">Le plateau de jeu.</param>
    /// <param name="neighbor">Enumeration d'une case voisine.</param>
    /// <param name="legalMoves">Une liste de coordonnees</param>
    public virtual void CheckNeighborIsLegalRepeat(Neighbor neighbor, List<Coordinates> legalMoves, Board board)
    {
        bool ok = true;
        GameColor ownerColor = owner.color;
        Box b;
        Coordinates c = box.coord;
        while (ok)
        {
            c = c.getNeighbor(neighbor, ownerColor);
            if (c.isInsideBorders())
            {
                b = board.boxes[c.getIndex()];
                if (b.token == null)
                {
                    legalMoves.Add(c);
                }
                else if (b.token.owner.color != owner.color)
                {
                    legalMoves.Add(c);
                    ok = false;
                }
                else
                {
                    ok = false;
                }
            }
            else
            {
                ok = false;
            }
        }
    }

    /// <summary>
    /// Ajout des mouvements dans une direction possibles par la pièce, après vérification.
    /// </summary>
    /// <param name="board">Le plateau de jeu.</param>
    /// <param name="neighbor">Enumeration d'une case voisine.</param>
    /// <param name="legalMoves">Une liste de coordonnees</param>
    public virtual void CheckNeighborIsPossible(Neighbor neighbor, List<Coordinates> coordinates, Board board)
    {
        GameColor ownerColor = owner.color;
        Coordinates c = box.coord.getNeighbor(neighbor, ownerColor);
        Box b;
        if (c.isInsideBorders())
        {
            b = board.boxes[c.getIndex()];
            coordinates.Add(c);
        }
    }

    /// <summary>
    /// Ajout des mouvements possibles par la pièce, après vérification.
    /// </summary>
    /// <param name="board">Le plateau de jeu.</param>
    /// <param name="neighbor">Enumeration d'une case voisine.</param>
    /// <param name="legalMoves">Une liste de coordonnees</param>
    public virtual void CheckNeighborIsPossibleRepeat(Neighbor neighbor, List<Coordinates> coordinates, Board board)
    {
        int i = 1;
        GameColor ownerColor = owner.color;
        Box b;
        Coordinates c = box.coord;
        while (i < 9)
        {
            c = c.getNeighbor(neighbor, ownerColor);
            if (c.isInsideBorders())
            {
                b = board.boxes[c.getIndex()];

                coordinates.Add(c);
            }
            i++;
        }
    }

    /// <summary>
    /// Mouvements complémentaires (de promotion) autorisés par la pièce.
    /// </summary>
    /// <returns>Une liste de coordonnées complémentaires (de promotion) autorisées lors des déplacements.</returns>
    /// <param name="board">Le plateau de jeu.</param>
    //Général d'or, Général d'argent, Cavalier, Lancier, Pion
    public virtual List<Coordinates> legalMovesPlus(Board board)
    {
        List<Coordinates> coordinates = new List<Coordinates>();
        CheckNeighborIsLegal(Neighbor.RIGHT, coordinates, board);
        CheckNeighborIsLegal(Neighbor.LEFT, coordinates, board);
        CheckNeighborIsLegal(Neighbor.TOP, coordinates, board);
        CheckNeighborIsLegal(Neighbor.TOP_RIGHT, coordinates, board);
        CheckNeighborIsLegal(Neighbor.TOP_LEFT, coordinates, board);
        CheckNeighborIsLegal(Neighbor.BOTTOM, coordinates, board);
        coordinates = Coordinates.removeDuplicates(coordinates);
        return coordinates;
    }

    /// <summary>
    /// Mouvements complémentaires (de promotion) possibles par la pièce.
    /// </summary>
    /// <returns>Une liste de coordonnées complémentaires (de promotion) possibles lors des déplacements.</returns>
    /// <param name="board">Le plateau de jeu.</param>
    //Général d'or, Général d'argent, Cavalier, Lancier, Pion
    public virtual List<Coordinates> possibleMovesPlus(Board board)
    {
        List<Coordinates> coordinates = new List<Coordinates>();
        CheckNeighborIsPossible(Neighbor.RIGHT, coordinates, board);
        CheckNeighborIsPossible(Neighbor.LEFT, coordinates, board);
        CheckNeighborIsPossible(Neighbor.TOP, coordinates, board);
        CheckNeighborIsPossible(Neighbor.TOP_RIGHT, coordinates, board);
        CheckNeighborIsPossible(Neighbor.TOP_LEFT, coordinates, board);
        CheckNeighborIsPossible(Neighbor.BOTTOM, coordinates, board);
        coordinates = Coordinates.removeDuplicates(coordinates);
        return coordinates;
    }

    /// <summary>
    /// Promotion la pièce actuelle.
    /// </summary>
    public virtual void promote () {

		isPromoted = true;
		switch (type) {
			case TokenType.PAWN:
				value = 700;
				break;
			case TokenType.LANCE:
				value = 600;
				break;
			case TokenType.KNIGHT:
				value = 600;
				break;
			case TokenType.SILVER:
				value = 600;
				break;
			case TokenType.BISHOP:
				value = 1000;
				break;
			case TokenType.ROOK:
				value = 1200;
				break;
		}

		_GameManager.players [_GameManager.currentPlayerIndex].promCount++;

	}

	/// <summary>
	/// Retrait de la promotion.
	/// </summary>
	public virtual void removePromotion () {

		isPromoted = false;
		switch (type) {
			case TokenType.PAWN:
				value = 100;
				break;
			case TokenType.LANCE:
				value = 300;
				break;
			case TokenType.KNIGHT:
				value = 400;
				break;
			case TokenType.SILVER:
				value = 500;
				break;
			case TokenType.BISHOP:
				value = 800;
				break;
			case TokenType.ROOK:
				value = 1000;
				break;
		}

	}

	/// <summary>
	/// Listes les rédéploiements autorisés par la pièce.
	/// </summary>
	/// <returns>Une liste de coordonnées autorisées lors des rédeploiements.</returns>
	/// <param name="board">Le plateau de jeu.</param>
	public virtual List<Coordinates> legalDrops (Board board) {
		List<Coordinates> coordinates = new List<Coordinates> ();
		foreach (Box b in board.boxes) {
			if (b.token == null) {
				coordinates.Add (b.coord);
			}
		}

		return coordinates;

	}

	/// <summary>
	/// Retourne l'emplacement de la case de capture de la pièce.
	/// </summary>
	/// <returns>Une case de capture.</returns>
	public CaptureBox getCaptureLocation () {

		if (isCaptured) {
			return captureLocation;
		} else {
			switch (type) {
				case TokenType.BISHOP:
					return owner.captureBench.getBoxBishop ();
				case TokenType.GOLD:
					return owner.captureBench.getBoxGold ();
				case TokenType.KNIGHT:
					return owner.captureBench.getBoxKnight ();
				case TokenType.LANCE:
					return owner.captureBench.getBoxLance ();
				case TokenType.PAWN:
					return owner.captureBench.getBoxPawn ();
				case TokenType.ROOK:
					return owner.captureBench.getBoxRook ();
				case TokenType.SILVER:
					return owner.captureBench.getBoxSilver ();
				default:
					return null;
			}
		}

	}

	/// <summary>
	/// Récuperation de la zone de capture pour une node.
	/// </summary>
	/// <returns>La zone de capture.</returns>
	/// <param name="node">Une node.</param>
	public CaptureBox getCaptureLocation (Node node) {

		if (isCaptured) {
			return captureLocation;
		} else {
			switch (type) {
				case TokenType.BISHOP:
					return node.getBoxBishop ();
				case TokenType.GOLD:
					return node.getBoxGold ();
				case TokenType.KNIGHT:
					return node.getBoxKnight ();
				case TokenType.LANCE:
					return node.getBoxLance ();
				case TokenType.PAWN:
					return node.getBoxPawn ();
				case TokenType.ROOK:
					return node.getBoxRook ();
				case TokenType.SILVER:
					return node.getBoxSilver ();
			}
		}

		Console.WriteLine ("ERROR : Capture location not found!");
		Environment.Exit (1);

		return null; // Should never occur

	}

	/// <summary>
	/// Affectation d'une case de capture.
	/// </summary>
	/// <param name="cb">Une case de capture.</param>
	public void setCaptureLocation (CaptureBox cb) {

		captureLocation = cb;

	}

	/// <summary>
	/// Retourne le type de la pièce.
	/// </summary>
	/// <returns>Retour du type de la pièce.</returns>
	public abstract TokenType getTokenType ();

    public abstract void getTokensToEat();

	/// <summary>
	///	Mise à jour des mouvements, qui est aussi utilisée par l'IA.
	/// </summary>
	public void updateMoves (Board board) {

		if (isCaptured) {
			moves = legalDrops (board);
		} else {
			if (!isPromoted) moves = legalMoves (board);
			else moves = legalMovesPlus (board);
		}

	}

	/// <summary>
	/// Vérifie si la pièce fait bien partie de la zone passée en paramètre.
	/// </summary>
	/// <returns>Vrai si dans la zone, faux sinon.</returns>
	/// <param name="zone">Liste de coordonnées définissant une zone.</param>
	/// <param name="box">Boite contenant le token.</param>
	public bool isInsideZone (Box box, List<Coordinates> zone) {

		bool isInside = false;
		foreach (Coordinates c in zone) {
			if (box.coord.equals (c)) return true;
		}

		return isInside;

	}

    /// <summary>
    /// Retourne une String qui représente le Token courant.
    /// </summary>
    /// <returns>Une String qui représente le Token courant.</returns>
    public override string ToString()
    {

        string str = "\nis tostring from token id : " + this.id;
        str += "\n tokentype: " + getTokenType().ToString();
        str += "\n NBACtion = ";

        return str;

    }
    /*
    public override string ToString () {

		string str = "\nis tostring from token id : " + this.id;
		str += "\n value: " + value;
		str += "\n coord: " + box.coord.ToString ();
		str += "\n tokentype: " + getTokenType ().ToString ();
		str += "\n value: " + value;
		str += "\n owner: " + owner.name;
		str += "\n isPromoted: " + isPromoted;
		str += "\n isCaptured: " + isCaptured;

		return str;

	}
    */

    /// <summary>
    /// Valeur de la position de la pièce.
    /// </summary>
    /// <returns>La valeur.</returns>
    /// <param name="king">Coordonnées du roi ennemi.</param>
    /// <param name="board">Tableau de jeu.</param>
    public abstract int positionValue (Coordinates king, Board board);

	/// <summary>
	/// Valeur de la position de la piece si promue en Gold.
	/// </summary>
	/// <returns>La valeur.</returns>
	/// <param name="king">Coordonnées du roi ennemi.</param>
	public int positionValueGoldPattern (Coordinates king) {

		return Math.Max (0, 200 - 50 * Math.Max (Math.Abs (king.y - box.coord.y), Math.Abs (king.x - box.coord.x)));

	}

	public string getLetter () {
		switch (getTokenType ()) {
			case TokenType.BISHOP:
				return "b";
			case TokenType.SILVER:
				return "s";
			case TokenType.KING:
				return "k";
			case TokenType.PAWN:
				return "p";
			case TokenType.ROOK:
				return "r";
			case TokenType.LANCE:
				return "l";
			case TokenType.GOLD:
				return "g";
			case TokenType.KNIGHT:
				return "n";
			default:
				return "e"; //should never happen 
		}
	}

    public string ToPrologCode()
        => isPromoted ?
        ShogiUtils.ShogiUtils.getColorLetter(owner.color) + getLetter() + internalID + "p"
      : ShogiUtils.ShogiUtils.getColorLetter(owner.color) + getLetter() + internalID;

    public Box getBox()
    {
        return box;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Token);
    }

    public bool Equals(Token other)
    {
        return other != null &&
               type == other.type &&
               EqualityComparer<Player>.Default.Equals(owner, other.owner) &&
               internalID == other.internalID;
    }

    public static bool operator ==(Token token1, Token token2)
    {
        return EqualityComparer<Token>.Default.Equals(token1, token2);
    }

    public static bool operator !=(Token token1, Token token2)
    {
        return !(token1 == token2);
    }
}
