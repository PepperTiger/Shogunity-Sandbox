using System.Collections;
using System.Collections.Generic;
using ShogiUtils;
using System.Linq;
using System;

/// <summary>
/// Classe du Fou.
/// </summary>
[Serializable]
public class Bishop : Token {
	
    /// <summary>
    /// Mouvements autorisés par la pièce.
    /// </summary>
    /// <returns>Une liste de coordonnées autorisées lors des déplacements.</returns>
    /// <param name="board">Le plateau de jeu.</param>
    public override List<Coordinates> legalMoves(Board board)
    {
        List<Coordinates> coordinates = new List<Coordinates>();
        CheckNeighborIsLegalRepeat(Neighbor.BOTTOM_LEFT, coordinates, board);
        CheckNeighborIsLegalRepeat(Neighbor.TOP_LEFT, coordinates, board);
        CheckNeighborIsLegalRepeat(Neighbor.TOP_RIGHT, coordinates, board);
        CheckNeighborIsLegalRepeat(Neighbor.BOTTOM_RIGHT, coordinates, board);
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
        CheckNeighborIsPossibleRepeat(Neighbor.BOTTOM_LEFT, coordinates, board);
        CheckNeighborIsPossibleRepeat(Neighbor.TOP_LEFT, coordinates, board);
        CheckNeighborIsPossibleRepeat(Neighbor.TOP_RIGHT, coordinates, board);
        CheckNeighborIsPossibleRepeat(Neighbor.BOTTOM_RIGHT, coordinates, board);
        coordinates = Coordinates.removeDuplicates(coordinates);
        return coordinates;
    }

    /// <summary>
    /// Mouvements complémentaires (de promotion) autorisés par la pièce.
    /// </summary>
    /// <returns>Une liste de coordonnées complémentaires (de promotion) autorisées lors des déplacements.</returns>
    /// <param name="board">Le plateau de jeu.</param>
    public override List<Coordinates> legalMovesPlus(Board board)
    {
        List<Coordinates> coordinates = new List<Coordinates>();
        coordinates = legalMoves(board);
        CheckNeighborIsLegal(Neighbor.BOTTOM, coordinates, board);
        CheckNeighborIsLegal(Neighbor.LEFT, coordinates, board);
        CheckNeighborIsLegal(Neighbor.RIGHT, coordinates, board);
        CheckNeighborIsLegal(Neighbor.TOP, coordinates, board);
        return coordinates;
    }

    /// <summary>
    /// Liste de tous les mouvements de promotion possibles par la pièce.
    /// </summary>
    /// <returns>Une liste de coordonnées possibles lors des déplacements de promotion.</returns>
    /// <param name="board">Le plateau de jeu.</param>
    public override List<Coordinates> possibleMovesPlus(Board board)
    {
        List<Coordinates> coordinates = new List<Coordinates>();
        coordinates = legalMoves(board);
        CheckNeighborIsPossible(Neighbor.TOP, coordinates, board);
        CheckNeighborIsPossible(Neighbor.BOTTOM, coordinates, board);
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
		
		return TokenType.BISHOP;

	}

	/// <summary>
	/// Valeur de la position de la pièce.
	/// </summary>
	/// <returns>La valeur.</returns>
	/// <param name="king">Coordonnées du roi ennemi ennemi.</param>
	/// <param name="board">Tableau de jeu.</param>
	public override int positionValue (Coordinates king, Board board) {
		
		int diffX = box.coord.x - king.x;
		int diffY = box.coord.y - king.y;
		int sumX = box.coord.x + king.x;
		int sumY = box.coord.y + king.y;

		if (king.x == box.coord.x || king.y == box.coord.y || (Math.Abs (diffX) != Math.Abs (diffY) && sumX != sumY)) {
			
			return 0;

		}

		Boolean obstacle = false;
		int yDir = king.y > box.coord.y ? 1 : -1;
		int xDir = king.x > box.coord.x ? 1 : -1;

		for (int i = 1; i < Math.Abs (diffY); i++) {
			if (board.boxes [box.coord.getIndex () + (9 * i * yDir) + i * xDir].token != null) {
				obstacle = true;
				break;
			}
		}

		return obstacle ? 10 : 400;

	}

    public override void getTokensToEat()
    {
        possibleEats = new List<Coordinates>();
        List<Coordinates> coordToEat = new List<Coordinates>();
        /*CheckNeighborIsEatable(Neighbor.BOTTOM_LEFT, coordToEat, _GameManager.board);
        CheckNeighborIsEatable(Neighbor.TOP_LEFT, coordToEat, _GameManager.board);
        CheckNeighborIsEatable(Neighbor.TOP_RIGHT, coordToEat, _GameManager.board);
        CheckNeighborIsEatable(Neighbor.BOTTOM_RIGHT, coordToEat, _GameManager.board);*/
        bool bottomleftdone = false;
        bool bottomrightdone = false;
        bool toprightdone = false;
        bool topleftdone = false;
        int i = 1;
        Coordinates tmp = new Coordinates();
        while(bottomleftdone == false)
        {
            tmp.set(this.box.coord.x - i, this.box.coord.y - i);
            if(tmp.isInsideBorders())
            {
                if(_GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
                {
                    coordToEat.Add(tmp.cloneThis());
                    bottomleftdone = true;
                }
                i++;
            }
            else
            {
                bottomleftdone = true;
            }
            if(i == 9)
            {
                bottomleftdone = true;
            }
        }
        i = 1;
        while (bottomrightdone == false)
        {
            tmp.set(this.box.coord.x + i, this.box.coord.y - i);
            if (tmp.isInsideBorders())
            {
                if (_GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
                {
                    coordToEat.Add(tmp.cloneThis());
                    bottomrightdone = true;
                }
                i++;
            }
            else
            {
                bottomrightdone = true;
            }
            if (i == 9)
            {
                bottomrightdone = true;
            }
        }
        i = 1;
        while (toprightdone == false)
        {
            tmp.set(this.box.coord.x + i, this.box.coord.y + i);
            if (tmp.isInsideBorders())
            {
                if (_GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
                {
                    coordToEat.Add(tmp.cloneThis());
                    toprightdone = true;
                }
                i++;
            }
            else
            {
                toprightdone = true;
            }
            if (i == 9)
            {
                toprightdone = true;
            }
        }
        i = 1;
        while (topleftdone == false)
        {
            tmp.set(this.box.coord.x - i, this.box.coord.y + i);
            if (tmp.isInsideBorders())
            {
                if (_GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
                {
                    coordToEat.Add(tmp.cloneThis());
                    topleftdone = true;
                }
                i++;
            }
            else
            {
                topleftdone = true;
            }
            if (i == 9)
            {
                topleftdone = true;
            }
        }
        coordToEat = Coordinates.removeDuplicates(coordToEat);
        if (this.isPromoted)
        {
            tmp.set(this.box.coord.x, this.box.coord.y + 1);
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
        if (coordToEat.Count != 0)
        {
            possibleEats.AddRange(coordToEat);
        }
    }

}
