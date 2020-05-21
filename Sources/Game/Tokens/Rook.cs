using System.Collections;
using System.Collections.Generic;
using ShogiUtils;
using System.Linq;
using System;

/// <summary>
/// Classe de la Tour.
/// </summary>
[Serializable]
public class Rook : Token {

	/// <summary>
	/// Retourne le type de la pièce.
	/// </summary>
	/// <returns>Retour du type de la pièce.</returns>
	public override TokenType getTokenType () {
		
		return TokenType.ROOK;

	}

    /// <summary>
    /// Mouvements autorisés par la pièce.
    /// </summary>
    /// <returns>Une liste de coordonnées autorisées lors des déplacements.</returns>
    /// <param name="board">Le plateau de jeu.</param>
    public override List<Coordinates> legalMoves(Board board)
    {
        List<Coordinates> coordinates = new List<Coordinates>();
        CheckNeighborIsLegalRepeat(Neighbor.BOTTOM, coordinates, board);
        CheckNeighborIsLegalRepeat(Neighbor.LEFT, coordinates, board);
        CheckNeighborIsLegalRepeat(Neighbor.TOP, coordinates, board);
        CheckNeighborIsLegalRepeat(Neighbor.RIGHT, coordinates, board);
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
        CheckNeighborIsPossibleRepeat(Neighbor.TOP, coordinates, board);
        CheckNeighborIsPossibleRepeat(Neighbor.BOTTOM, coordinates, board);
        CheckNeighborIsPossibleRepeat(Neighbor.RIGHT, coordinates, board);
        CheckNeighborIsPossibleRepeat(Neighbor.LEFT, coordinates, board);
        coordinates = Coordinates.removeDuplicates(coordinates);
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
        CheckNeighborIsPossible(Neighbor.BOTTOM_LEFT, coordinates, board);
        CheckNeighborIsPossible(Neighbor.TOP_LEFT, coordinates, board);
        CheckNeighborIsPossible(Neighbor.TOP_RIGHT, coordinates, board);
        CheckNeighborIsPossible(Neighbor.BOTTOM_RIGHT, coordinates, board);
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
        CheckNeighborIsLegal(Neighbor.BOTTOM_LEFT, coordinates, board);
        CheckNeighborIsLegal(Neighbor.TOP_LEFT, coordinates, board);
        CheckNeighborIsLegal(Neighbor.TOP_RIGHT, coordinates, board);
        CheckNeighborIsLegal(Neighbor.BOTTOM_RIGHT, coordinates, board);
        return coordinates;
    }
    
    /// <summary>
    /// Valeur de la position de la pièce.
    /// </summary>
    /// <returns>La valeur.</returns>
    /// <param name="king">Coordonnées du roi ennemi.</param>
    /// <param name="board">Tableau de jeu.</param>
    public override int positionValue (Coordinates king, Board board) {
		
		if (box.coord.x != king.x && box.coord.y != king.y) {
			
			return 0; // Ni sur la meme ligne ni la meme colonne

		} else {
			
			Boolean obstacle = false;
			if (king.x == box.coord.x) { // Meme colonne
				int yDir = king.y > box.coord.y ? 1 : -1;
				for (int i = 1; i < Math.Abs (box.coord.y - king.y); i++) {
					if (board.boxes [box.coord.getIndex () + 9 * i * yDir].token != null) {
						obstacle = true;
						break;
					}
				}
			} else { // Meme ligne
				int xDir = king.x > box.coord.x ? 1 : -1;
				for (int i = 1; i < box.coord.y - king.y; i += xDir) {
					if (board.boxes [box.coord.getIndex () + 9 * i * xDir].token != null) {
						obstacle = true;
						break;
					}
				}
			}
			if (Math.Abs (box.coord.y - king.y) >= 2 && Math.Abs (box.coord.y - king.y) < 6) {
				return obstacle ? 0 : 200;
			}

			return obstacle ? 0 : 100;

		}
	}

    public override void getTokensToEat()
    {
        possibleEats = new List<Coordinates>();
        List<Coordinates> coordToEat = new List<Coordinates>();
        /*CheckNeighborIsEatable(Neighbor.BOTTOM_LEFT, coordToEat, _GameManager.board);
        CheckNeighborIsEatable(Neighbor.TOP_LEFT, coordToEat, _GameManager.board);
        CheckNeighborIsEatable(Neighbor.TOP_RIGHT, coordToEat, _GameManager.board);
        CheckNeighborIsEatable(Neighbor.BOTTOM_RIGHT, coordToEat, _GameManager.board);*/
        bool leftdone = false;
        bool rightdone = false;
        bool topdone = false;
        bool bottomdone = false;
        int i = 1;
        Coordinates tmp = new Coordinates();
        while (leftdone == false)
        {
            tmp.set(this.box.coord.x - i, this.box.coord.y);
            if (tmp.isInsideBorders())
            {
                if (_GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
                {
                    coordToEat.Add(tmp.cloneThis());
                    leftdone = true;
                }
                i++;
            }
            else
            {
                leftdone = true;
            }
            if (i == 9)
            {
                leftdone = true;
            }
        }
        i = 1;
        while (rightdone == false)
        {
            tmp.set(this.box.coord.x + i, this.box.coord.y);
            if (tmp.isInsideBorders())
            {
                if (_GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
                {
                    coordToEat.Add(tmp.cloneThis());
                    rightdone = true;
                }
                i++;
            }
            else
            {
                rightdone = true;
            }
            if (i == 9)
            {
                rightdone = true;
            }
        }
        i = 1;
        while (topdone == false)
        {
            tmp.set(this.box.coord.x, this.box.coord.y + i);
            if (tmp.isInsideBorders())
            {
                if (_GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
                {
                    coordToEat.Add(tmp.cloneThis());
                    topdone = true;
                }
                i++;
            }
            else
            {
                topdone = true;
            }
            if (i == 9)
            {
                topdone = true;
            }
        }
        i = 0;
        while (bottomdone == false)
        {
            tmp.set(this.box.coord.x, this.box.coord.y - i);
            if (tmp.isInsideBorders())
            {
                if (_GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
                {
                    coordToEat.Add(tmp.cloneThis());
                    bottomdone = true;
                }
                i++;
            }
            else
            {
                bottomdone = true;
            }
            if (i == 9)
            {
                bottomdone = true;
            }
        }
        coordToEat = Coordinates.removeDuplicates(coordToEat);
        if (this.isPromoted)
        {
            CheckNeighborIsEatable(Neighbor.BOTTOM_RIGHT, coordToEat, _GameManager.board);
            CheckNeighborIsEatable(Neighbor.TOP_RIGHT, coordToEat, _GameManager.board);
            CheckNeighborIsEatable(Neighbor.TOP_RIGHT, coordToEat, _GameManager.board);
            CheckNeighborIsEatable(Neighbor.BOTTOM_LEFT, coordToEat, _GameManager.board);
            coordToEat = Coordinates.removeDuplicates(coordToEat);
        }
        if (coordToEat.Count != 0)
        {
            possibleEats.AddRange(coordToEat);
        }
    }
		
}
