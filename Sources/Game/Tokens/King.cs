using System.Collections;
using System.Collections.Generic;
using ShogiUtils;
using System.Linq;
using System;

/// <summary>
/// Classe du Roi.
/// </summary>
[Serializable]
public class King : Token {

    /// <summary>
    /// Mouvements autorisés par la pièce.
    /// </summary>
    /// <returns>Une liste de coordonnées autorisées lors des déplacements.</returns>
    /// <param name="board">Le plateau de jeu.</param>
    public override List<Coordinates> legalMoves(Board board)
    {
        List<Coordinates> coordinates = new List<Coordinates>();
        CheckNeighborIsLegal(Neighbor.RIGHT, coordinates, board);
        CheckNeighborIsLegal(Neighbor.LEFT, coordinates, board);
        CheckNeighborIsLegal(Neighbor.TOP, coordinates, board);
        CheckNeighborIsLegal(Neighbor.TOP_RIGHT, coordinates, board);
        CheckNeighborIsLegal(Neighbor.TOP_LEFT, coordinates, board);
        CheckNeighborIsLegal(Neighbor.BOTTOM, coordinates, board);
        CheckNeighborIsLegal(Neighbor.BOTTOM_RIGHT, coordinates, board);
        CheckNeighborIsLegal(Neighbor.BOTTOM_LEFT, coordinates, board);
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
        CheckNeighborIsPossible(Neighbor.BOTTOM, coordinates, board);
        CheckNeighborIsPossible(Neighbor.BOTTOM_LEFT, coordinates, board);
        CheckNeighborIsPossible(Neighbor.BOTTOM_RIGHT, coordinates, board);
        CheckNeighborIsPossible(Neighbor.RIGHT, coordinates, board);
        CheckNeighborIsPossible(Neighbor.LEFT, coordinates, board);
        coordinates = Coordinates.removeDuplicates(coordinates);
        return coordinates;
    }

/// <summary>
/// Retourne le type de la pièce.
/// </summary>
/// <returns>Retour du type de la pièce.</returns>
public override TokenType getTokenType () {
		
		return TokenType.KING;

	}

	public override List<Coordinates> legalDrops (Board board) {
		return new List<Coordinates> ();
	}

	/// <summary>
	/// Valeur de la position de la pièce.
	/// </summary>
	/// <returns>La valeur.</returns>
	/// <param name="king">Coordonnées du roi ennemi.</param>
	/// <param name="board">Tableau de jeu.</param>
	public override int positionValue (Coordinates king, Board board) {
		
		return 0;

	}

    public override void getTokensToEat()
    {
        possibleEats = new List<Coordinates>();
        List<Coordinates> coordToEat = new List<Coordinates>();
        Coordinates tmp = new Coordinates();
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
        if (coordToEat.Count != 0)
        {
            possibleEats.AddRange(coordToEat);
        }
    }

}
