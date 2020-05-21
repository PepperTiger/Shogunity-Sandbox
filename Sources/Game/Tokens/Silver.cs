using System.Collections;
using System.Collections.Generic;
using ShogiUtils;
using System.Linq;
using System;

/// <summary>
/// Classe du Général d'argent.
/// </summary>
[Serializable]
public class Silver : Token {

    /// <summary>
    /// Mouvements autorisés par la pièce.
    /// </summary>
    /// <returns>Une liste de coordonnées autorisées lors des déplacements.</returns>
    /// <param name="board">Le plateau de jeu.</param>
    public override List<Coordinates> legalMoves(Board board)
    {
        List<Coordinates> coordinates = new List<Coordinates>();
        CheckNeighborIsLegal(Neighbor.BOTTOM_RIGHT, coordinates, board);
        CheckNeighborIsLegal(Neighbor.BOTTOM_LEFT, coordinates, board);
        CheckNeighborIsLegal(Neighbor.TOP, coordinates, board);
        CheckNeighborIsLegal(Neighbor.TOP_RIGHT, coordinates, board);
        CheckNeighborIsLegal(Neighbor.TOP_LEFT, coordinates, board);
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
        CheckNeighborIsPossible(Neighbor.TOP, coordinates, board);
        CheckNeighborIsPossible(Neighbor.TOP_LEFT, coordinates, board);
        CheckNeighborIsPossible(Neighbor.TOP_RIGHT, coordinates, board);
        CheckNeighborIsPossible(Neighbor.BOTTOM_LEFT, coordinates, board);
        CheckNeighborIsPossible(Neighbor.BOTTOM_RIGHT, coordinates, board);
        coordinates = Coordinates.removeDuplicates(coordinates);
        return coordinates;
    }

    /// <summary>
    /// Mouvements complémentaires (de promotion) autorisés par la pièce.
    /// </summary>
    /// -> Correspond aux mouvements de "public virtual List<Coordinates> legalMovesPlus(Board board)" de la Classe Token

    /// <summary>
    /// Retourne le type de la pièce.
    /// </summary>
    /// <returns>Retour du type de la pièce.</returns>
    public override TokenType getTokenType () {
		
		return TokenType.SILVER;

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

		return Math.Max (0, 200 - 33 * Math.Abs (king.y - box.coord.y));

	}

    public override void getTokensToEat()
    {
        possibleEats = new List<Coordinates>();
        List<Coordinates> coordToEat = new List<Coordinates>();
        Coordinates tmp = new Coordinates();
        if (this.isPromoted)
        {

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
            tmp.set(this.box.coord.x - 1, this.box.coord.y - 1);
            if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
            {
                coordToEat.Add(tmp.cloneThis());
            }
            tmp.set(this.box.coord.x + 1, this.box.coord.y - 1);
            if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
            {
                coordToEat.Add(tmp.cloneThis());
            }
        }
        tmp.set(this.box.coord.x, this.box.coord.y + 1);
        if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
        {
            coordToEat.Add(tmp.cloneThis());
        }
        tmp.set(this.box.coord.x - 1, this.box.coord.y + 1);
        if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
        {
            coordToEat.Add(tmp.cloneThis());
        }
        tmp.set(this.box.coord.x + 1, this.box.coord.y + 1);
        if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
        {
            coordToEat.Add(tmp.cloneThis());
        }
        coordToEat = Coordinates.removeDuplicates(coordToEat);
        if (coordToEat.Count != 0)
        {
            possibleEats.AddRange(coordToEat);
        }
    }
		
}
