using System.Collections.Generic;
using ShogiUtils;
using System;

/// <summary>
/// Classe du Chevalier.
/// </summary>
[Serializable]
public class Knight : Token {

	/// <summary>
	/// Mouvements autorisés par la pièce.
	/// </summary>
	/// <returns>Une liste de coordonnées autorisées lors des déplacements.</returns>
	/// <param name="board">Le plateau de jeu.</param>
	public override List<Coordinates> legalMoves (Board board) {

		List<Coordinates> coordinates = new List<Coordinates> ();
		Coordinates c;

		int a = (owner.color == GameColor.SENTE) ? 1 : -1;

		c = new Coordinates (box.coord.x - 1, box.coord.y + 2 * a);
		if (c.isInsideBorders ()) {
			Box b = board.boxes[c.getIndex ()];
			if (b.token == null || b.token.owner.color != owner.color) {
				coordinates.Add (c);
			}
		}

		c = new Coordinates (box.coord.x + 1, box.coord.y + 2 * a);
		if (c.isInsideBorders ()) {
			Box b = board.boxes[c.getIndex ()];
			if (b.token == null || b.token.owner.color != owner.color) {
				coordinates.Add (c);
			}
		}

        coordinates = Coordinates.removeDuplicates(coordinates);

        return coordinates;
	}

    /// <summary>
    /// Liste de tous les mouvements possibles par la pièce.
    /// </summary>
    /// <returns>Une liste de coordonnées possibles lors des déplacements.</returns>
    /// <param name="board">Le plateau de jeu.</param>
    public override List<Coordinates> possibleMoves(Board board)
    {
        List<Coordinates> coordinates = new List<Coordinates>();
        Coordinates c;

        int a = (owner.color == GameColor.SENTE) ? 1 : -1;

        c = new Coordinates(box.coord.x - 1, box.coord.y + 2 * a);
        if (c.isInsideBorders())
        {
            Box b = board.boxes[c.getIndex()];
            coordinates.Add(c);
        }

        c = new Coordinates(box.coord.x + 1, box.coord.y + 2 * a);
        if (c.isInsideBorders())
        {
            Box b = board.boxes[c.getIndex()];
            coordinates.Add(c);
        }

        coordinates = Coordinates.removeDuplicates(coordinates);

        return coordinates;
    }

    /// <summary>
    /// Mouvements complémentaires (de promotion) autorisés par la pièce.
    /// </summary>
    /// -> Correspond aux mouvements de "public virtual List<Coordinates> legalMovesPlus(Board board)" de la Classe Token

    /// <summary>
    /// Listes les rédéploiements autorisés par la pièce.
    /// </summary>
    /// <returns>Une liste de coordonnées autorisées lors des rédeploiements.</returns>
    /// <param name="board">Le plateau de jeu.</param>
    public override List<Coordinates> legalDrops (Board board) {

		List<Coordinates> coordinates = new List<Coordinates> ();
		Coordinates c;
		Box b;

		int firstRow = (owner.color == GameColor.SENTE) ? 0 : 2;
		int lastRow = (owner.color == GameColor.SENTE) ? 7 : 9;

		for (int i = 0; i < 9; i++) {
			for (int j = firstRow; j < lastRow; j++) {
				c = new Coordinates (i, j);
				b = board.boxes[c.getIndex ()];
				if (b.token == null) coordinates.Add (c);
			}
		}

		return coordinates;

	}

	/// <summary>
	/// Retourne le type de la pièce.
	/// </summary>
	/// <returns>Retour du type de la pièce.</returns>
	public override TokenType getTokenType () {

		return TokenType.KNIGHT;

	}

	/// <summary>
	/// Valeur de la position de la pièce.
	/// </summary>
	/// <returns>La valeur.</returns>
	/// <param name="king">Coordonnées du roi ennemi.</param>
	/// <param name="board">Tableau de jeu.</param>
	public override int positionValue (Coordinates king, Board board) {

		if (isPromoted) {

			return positionValueGoldPattern (king);

		}

		if (owner.color == GameColor.GOTE && box.coord.y > king.y || owner.color != GameColor.GOTE && box.coord.y < king.y) {

			return 0;

		} else {
			if (Math.Abs (box.coord.x - king.x) == 1 && Math.Abs (box.coord.y - king.y) == 2) {
				return 300;
			} else {
				return Math.Max (0, 200 - (25 * Math.Min (Math.Abs (king.y - box.coord.y), Math.Abs (king.x - box.coord.x))));
			}
		}

	}

    public override void getTokensToEat()
    {
        possibleEats = new List<Coordinates>();
        List<Coordinates> coordToEat = new List<Coordinates>();
        Coordinates tmp = new Coordinates();
        if (this.isPromoted)
        {
            tmp.set(this.box.coord.x, this.box.coord.y + 1);
            if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
            {
                coordToEat.Add(tmp.cloneThis());
            }
            tmp.set(this.box.coord.x + 1, this.box.coord.y + 1);
            if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
            {
                coordToEat.Add(tmp.cloneThis());
            }
            tmp.set(this.box.coord.x - 1, this.box.coord.y + 1);
            if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
            {
                coordToEat.Add(tmp.cloneThis());
            }
            tmp.set(this.box.coord.x + 1, this.box.coord.y);
            if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
            {
                coordToEat.Add(tmp.cloneThis());
            }
            tmp.set(this.box.coord.x - 1, this.box.coord.y);
            if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
            {
                coordToEat.Add(tmp.cloneThis());
            }
            tmp.set(this.box.coord.x, this.box.coord.y - 1);
            if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
            {
                coordToEat.Add(tmp.cloneThis());
            }
        }
        else
        {
           
            tmp.set(this.box.coord.x + 1, this.box.coord.y + 2);
            if (tmp.isInsideBorders())
            {
                if (_GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
                {
                    coordToEat.Add(tmp.cloneThis());
                }
            }
            tmp.set(this.box.coord.x - 1, this.box.coord.y + 2);
            if (tmp.isInsideBorders())
            {
                if (_GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
                {
                    coordToEat.Add(tmp.cloneThis());
                }
            }
        }
        if (coordToEat.Count != 0)
        {
            possibleEats.AddRange(coordToEat);
        }
    }
}
