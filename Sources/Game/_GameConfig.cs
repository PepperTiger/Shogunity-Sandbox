using System.Collections;
using ShogiUtils;

/// <summary>
/// Configuration du jeu.
/// </summary>
public static class _GameConfig {

	/// <summary>
	///	Identifiant de partie.
	/// </summary>
	public static int gameId;

	/// <summary>
	/// Player1Difficulty = Difficulté joueur 1, Player2Difficulty = Difficulté joueur 2
	/// </summary>
	public static int player1Difficulty, player2Difficulty;

	/// <summary>
	/// Mode de jeu.
	/// </summary>
	public static GameMode gameMode = GameMode.AI_VS_AI;

	/// <summary>
	/// player1Name = Nom joueur 1, player2Name = Nom joueur 2
	/// </summary>
	public static string player1Name, player2Name;

	/// <summary>
	/// player1Type = Type joueur 1, player2Type = Type joueur 2
	/// </summary>
	public static PlayerType player1Type, player2Type;

}
