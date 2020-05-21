    using System.Collections;
using System.Collections.Generic;
using ShogiUtils;
using System.Linq;
using System;

/// <summary>
/// Classe du Pion.
/// </summary>
[Serializable]
public class Pawn : Token {
	
	/// <summary>
	/// Mouvements autorisés par la pièce.
	/// </summary>
	/// <returns>Une liste de coordonnées autorisées lors des déplacements.</returns>
	/// <param name="board">Le plateau de jeu.</param>
	public override List<Coordinates> legalMoves (Board board) {
		
		List<Coordinates> coordinates = new List<Coordinates> ();
        CheckNeighborIsLegal(Neighbor.TOP, coordinates, board);
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
        CheckNeighborIsPossible(Neighbor.TOP, coordinates, board);
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

		int firstRow = (owner.color == GameColor.SENTE) ? 0 : 1;
		int lastRow = (owner.color == GameColor.SENTE) ? 8 : 9;
		for (int i = 0; i < 9; i++) {
			bool columnOK = true;
			List<Coordinates> columnCoord = new List<Coordinates> ();
			for (int j = firstRow; j < lastRow && columnOK; j++) {
				c = new Coordinates (i, j);
				Box b = board.boxes [c.getIndex ()];
				if (b.token == null) {
					columnCoord.Add (c);
				} else if (b.token.type == TokenType.PAWN && b.token.isPromoted == false && this.owner == b.token.owner) {
					columnOK = false;
				}
			}
			if (columnOK) coordinates.AddRange (columnCoord);
		}

		return coordinates;

	}

	/// <summary>
	/// Retourne le type de la pièce.
	/// </summary>
	/// <returns>Retour du type de la pièce.</returns>
	public override TokenType getTokenType () {
		
		return TokenType.PAWN;

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
			return 1;
		}

		return Math.Max (100 - 13 * Math.Abs (box.coord.y - king.y), 1);
        
	}

    public override void getTokensToEat()
    {
        possibleEats = new List<Coordinates>();
        List<Coordinates> coordToEat = new List<Coordinates>();
        Coordinates tmp = new Coordinates();
        if (this.isPromoted)
        {
            
            tmp.set(this.box.coord.x + 1, this.box.coord.y + 1);
            if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken().owner.color != this.owner.color)
            {
                coordToEat.Add(tmp.cloneThis());
            }
            tmp.set(this.box.coord.x - 1, this.box.coord.y + 1);
            if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken().owner.color != this.owner.color)
            {
                coordToEat.Add(tmp.cloneThis());
            }
            tmp.set(this.box.coord.x + 1, this.box.coord.y);
            if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken().owner.color != this.owner.color)
            {
                coordToEat.Add(tmp.cloneThis());
            }
            tmp.set(this.box.coord.x - 1, this.box.coord.y);
            if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken().owner.color != this.owner.color)
            {
                coordToEat.Add(tmp.cloneThis());
            }
            tmp.set(this.box.coord.x, this.box.coord.y - 1);
            if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken().owner.color != this.owner.color)
            {
                coordToEat.Add(tmp.cloneThis());
            }

        }
        tmp.set(this.box.coord.x, this.box.coord.y + 1);
        if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken().owner.color != this.owner.color)
        {
            coordToEat.Add(tmp.cloneThis());
        }
        coordToEat = Coordinates.removeDuplicates(coordToEat);
        if(coordToEat.Count != 0)
        {
            possibleEats.AddRange(coordToEat);
        }
        
    }
		
}
