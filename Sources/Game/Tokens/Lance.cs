using System.Collections;
using System.Collections.Generic;
using ShogiUtils;
using System.Linq;
using System;

/// <summary>
/// Classe du Lancier.
/// </summary>
[Serializable]
public class Lance : Token {

    /// <summary>
    /// Mouvements autorisés par la pièce.
    /// </summary>
    /// <returns>Une liste de coordonnées autorisées lors des déplacements.</returns>
    /// <param name="board">Le plateau de jeu.</param>
    public override List<Coordinates> legalMoves(Board board)
    {
        List<Coordinates> coordinates = new List<Coordinates>();
        CheckNeighborIsLegalRepeat(Neighbor.TOP, coordinates, board);
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

		int firstRow = (owner.color == GameColor.SENTE) ? 0 : 1;
		int lastRow = (owner.color == GameColor.SENTE) ? 8 : 9;

		for (int i = 0; i < 9; i++) {
			for (int j = firstRow; j < lastRow; j++) {
				c = new Coordinates (i, j);
				b = board.boxes [c.getIndex ()];
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
		
		return TokenType.LANCE;

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

		} else {
			
			if (box.coord.x != king.x) {
				
				return 0;

			} else {
				
				Coordinates coordTmp = box.coord;
				Boolean obstacle = false;
				for (int i = 1; i < Math.Abs (box.coord.y - king.y) - 1; i++) {
					coordTmp = coordTmp.getNeighbor (Neighbor.TOP, owner.color);
					if (board.boxes [coordTmp.getIndex ()].token != null) {
						obstacle = true;
					}
				}
				int bonus = 0;
				if(Math.Abs(box.coord.y - king.y) >= 2) {
					bonus += 200;
				}
				return obstacle ? 0 : 200 + bonus;

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
            
            bool done = false;
            int i = 1;
            while(done == false)
            {
                tmp.set(this.box.coord.x, this.box.coord.y + i);
                if (tmp.isInsideBorders() && _GameManager.getBoxFromCoordinates(tmp.x, tmp.y).getToken() != null )
                {
                    done = true;
                    coordToEat.Add(tmp.cloneThis());
                }
                i++;
                if(i == 9)
                {
                    done = true;
                }
            }
        }
        if (coordToEat.Count != 0)
        {
            possibleEats.AddRange(coordToEat);
        }
    }
		
}
