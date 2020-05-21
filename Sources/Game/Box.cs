using System.Collections;
using ShogiUtils;
using System;
using System.Collections.Generic;

/// <summary>
/// Une case du jeu.
/// </summary>
[Serializable]
public class Box {

	/// <summary>
    /// 
	/// Pièce qui est posée dessus.
	/// </summary>
	public Token token;

	/// <summary>
	/// Coordonnées logiques de la case en 2D.
	/// </summary>
	public Coordinates coord;

	public Box () {
		this.token = null;
		this.coord = null;
	}

    public Token getToken()
    {
        return token;
    }

    public Coordinates getCoord()
    {
        return coord;
    }

}
