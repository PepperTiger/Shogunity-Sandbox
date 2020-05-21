using System;
using System.Collections;
using System.Collections.Generic;
using ShogiUtils;
using System.Text;
using Sandbox.Sources.Game.AI.FOXCS;
using YieldProlog;

/// <summary>
/// Initialisation du jeu.
/// </summary>
public static class _init {

	/// <summary>
	/// ID employé pour init les tokens.
	/// </summary>
	public static int tokenId;

	/// <summary>
	/// Nombre de lignes dans le board.
	/// </summary>
	public static int nrow = 9;

	/// <summary>
	/// Nombre de colonnes dans le board.
	/// </summary>
	public static int ncol = 9;

	/// <summary>
	/// Boxes du board.
	/// </summary>
	public static List<Box> boxes = new List<Box> ();

	/// <summary>
	/// Tokens du board.
	/// </summary>
	public static List<Token> tokens = new List<Token> ();

	/// <summary>
	/// Joueurs 1=BLACK-SENTE, 2=WHITE-GOTE
	/// </summary>
	public static Player player1, player2;

	/// <summary>
	/// Les deux IA du jeu.
	/// </summary>
	public static AIHandler AI1, AI2;

	/// <summary>
	/// Les deux bancs de capture du board
	/// </summary>
	public static CaptureBench cb1, cb2;

    public static FOXCSOptions FOXCSopt;
    public static FOXCSOptions FOXCSopt2;

    /// <summary>
    /// Le mode agressif du joueur 1 Q-Learning
    /// </summary>
    public static bool agressifMode1;

	/// <summary>
	/// Le mode agressif du joueur 2 Q-Learning
	/// </summary>
	public static bool agressifMode2;

	/// <summary>
	/// Choix pour le mode agressif (Oui : o || O, Non : n || N)
	/// </summary>
	public static char choix;

	/// <summary>
	/// Nombre de parties d'apprentissage
	/// </summary>
	public static int parties;

    /// <summary>
    /// Initialisation des options de jeu.
    /// </summary>
    public static void setGameConfig() {

        Console.Out.WriteLine("*********************\n" +
                              "*Shogunity - Sandbox*\n" +
                              "*********************\n");

        Console.Out.WriteLine("Bienvenue sur la sandbox de Shogunity, permettant de tester des IAs Basiques et de lancer une séquence d'apprentissage pour FOXCS.");

        do
        {
            Console.WriteLine("\nNombre de parties [1, " + Int32.MaxValue + "] ?\n");
            try {
                parties = int.Parse(Console.ReadLine());
            } catch (FormatException) {
                Console.WriteLine("Veuillez entrer un nombre dans l'intervalle cité.");
            }
        } while (parties < 1 || parties > Int32.MaxValue);

        int tmp = 0;
        int scenario = 0;

        Dictionary<string, PredicateOptions> actionPredicateOptions = new Dictionary<string, PredicateOptions>();
        Dictionary<string, PredicateOptions> actionPredicateOptions2 = new Dictionary<string, PredicateOptions>();
        Dictionary<string, PredicateOptions> statePredicateOptions = new Dictionary<string, PredicateOptions>();
        Dictionary<string, PredicateOptions> statePredicateOptions2 = new Dictionary<string, PredicateOptions>();

        /*
        statePredicateOptions.Add("inRange", new PredicateOptions(
            0, 0, false,
            new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1)
            }));
            


        /*
        statePredicateOptions.Add("allyInRange", new PredicateOptions(
            0, 0, false,
            new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75)
            }));

        /*
        statePredicateOptions.Add("ennemyInRange", new PredicateOptions(
            0, 0, false,
            new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5),
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1)
            }));

        */
        List<HornClause> knowledgeBase = new List<HornClause>();
        /*
        
        knowledgeBase.Add(new HornClause(new Sandbox.Sources.Game.AI.FOXCS.Attribute("ennemyInRange", new string[] { "TOKEN1", "TOKEN2", "X", "Y" }, statePredicateOptions["ennemyInRange"]),
            "ennemyInRange(TOKEN1, TOKEN2, X, Y) :- \n" +
                "inRange(TOKEN1, TOKEN2, X, Y), \n" +
                "legalMove(TOKEN1, X, Y). \n"));
                */

        do {
			Console.WriteLine ("\nType AI1 ?\n 1 : MiniMax \n 2 : AlphaBeta \n 3 : NegaScout\n 4 : ProofNumberSearch\n 5 : PartieAleatoire\n 6 : FOXCS\n");
			try {
				tmp = int.Parse (Console.ReadLine ());
			} catch (FormatException) {
				Console.WriteLine ("\nVeuillez entrer un nombre, vous avez tapé :" + tmp);
			}
		} while (tmp > 6 || tmp < 0);

		switch (tmp) {
			case 1:
				_GameConfig.player1Type = PlayerType.MINMAX;
				break;
			case 2:
				_GameConfig.player1Type = PlayerType.ALPHABETA;
				break;
			case 3:
				_GameConfig.player1Type = PlayerType.NEGASCOUT;
				break;
			case 4:
				_GameConfig.player1Type = PlayerType.PNS;
				break;
            case 5:
                _GameConfig.player1Type = PlayerType.RNG;
                break;
            case 6:
                _GameConfig.player1Type = PlayerType.FOXCS;
                break;
        }

		tmp = 0;

        if (_GameConfig.player1Type != PlayerType.FOXCS)
        {
            do
            {
                Console.WriteLine("\nDepth AI 1 ? From 1 to 5\n");
                try
                {
                    tmp = int.Parse(Console.ReadLine());
                }
                catch (FormatException)
                {
                    Console.WriteLine("\nVeuillez entrer un nombre");
                }
            } while (tmp < 1 || tmp > 6);
            _GameConfig.player1Difficulty = tmp;
        }

        if (_GameConfig.player1Type == PlayerType.FOXCS)
        {

            Console.WriteLine("\nOptions prédefinies pour FOXCS ? \n"+
                              " 0. Scénario par défaut (général => spécifique)\n"+
                              " 1. Scénario par défaut simple (général => spécifique)\n" +
                              " 2. Scénario par défaut simple (spécifique => général)\n" +
                              " 3. Scénario par défaut simple (hybride)\n" +
                              " 4. Scénario personalisé.\n");
            switch (Int32.Parse(Console.ReadLine()))
            {
                case 0:
                    actionPredicateOptions.Add("move", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1)
                    }));


                    actionPredicateOptions.Add("drop", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1)
                        }));


                    actionPredicateOptions.Add("promote", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.BOOL, new ArgMode[]{
                        ArgMode.CONST}, -1)
                        }));

                    statePredicateOptions.Add("onTile", new PredicateOptions(
                        1, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1)
                        }));


                    statePredicateOptions.Add("legalMove", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1)
                        }));


                    statePredicateOptions.Add("legalDrop", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1)
                        }));

                    FOXCSopt = new FOXCSOptions(
                        "test", 300, 0.1, 0.1, 0.01 * 504, 5,
                         0.71, 25, 20, 0.1, 20, 0.33, false, false,
                         "FOXCStestBlackPopSet1",
                         statePredicateOptions, actionPredicateOptions, knowledgeBase.ToArray(),
                         new double[] { 1, 1, 0, 3, 3, 0, 1 }, 2, 4, true, true);
                    break;

                case 1:

                    actionPredicateOptions.Add("move", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75)
                        }));


                    actionPredicateOptions.Add("drop", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75)
                        }));


                    actionPredicateOptions.Add("promote", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.BOOL, new ArgMode[]{
                        ArgMode.CONST}, -1)
                        }));

                    statePredicateOptions.Add("onTile", new PredicateOptions(
                        1, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75)
                        }));

                    FOXCSopt = new FOXCSOptions(
                        "test", 200, 0.1, 0.1, 0.01 * 504, 5,
                         0.71, 25, 20, 0.1, 20, 0.33, false, false,
                         "FOXCStestBlackPopSet2",
                         statePredicateOptions, actionPredicateOptions, knowledgeBase.ToArray(),
                         new double[] { 1, 2, 0, 4, 4, 0, 1 }, 2, 6, true, true);
                    break;

                case 2:

                    actionPredicateOptions.Add("move", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    }));


                    actionPredicateOptions.Add("drop", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25)
                        }));


                    actionPredicateOptions.Add("promote", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.BOOL, new ArgMode[]{
                        ArgMode.CONST}, -1)
                        }));

                    statePredicateOptions.Add("onTile", new PredicateOptions(
                        1, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25)
                        }));


                    statePredicateOptions.Add("legalMove", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25)
                        }));


                    statePredicateOptions.Add("legalDrop", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25)
                        }));

                    FOXCSopt = new FOXCSOptions(
                        "test", 200, 0.1, 0.1, 0.01 * 504, 5,
                        0.71, 50, 20, 0.1, 20, 0.33, false, false,
                        "FOXCStestBlackPopSet3",
                        statePredicateOptions, actionPredicateOptions, knowledgeBase.ToArray(),
                        new double[] { 3, 3, 1, 1, 1, 1, 1 }, 2, 6, true, true);
                    break;

                case 3:
                    actionPredicateOptions.Add("move", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5),
                    }));


                    actionPredicateOptions.Add("drop", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75)
                        }));


                    actionPredicateOptions.Add("promote", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.BOOL, new ArgMode[]{
                        ArgMode.CONST}, -1)
                        }));

                    statePredicateOptions.Add("onTile", new PredicateOptions(
                        1, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75)
                        }));


                    statePredicateOptions.Add("legalMove", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75)
                        }));


                    statePredicateOptions.Add("legalDrop", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                        }));

                    FOXCSopt = new FOXCSOptions(
                        "test", 200, 0.1, 0.1, 0.01 * 504, 5,
                         0.71, 25, 20, 0.1, 20, 0.33, false, false,
                         "FOXCStestBlackPopSet4",
                         statePredicateOptions, actionPredicateOptions, knowledgeBase.ToArray(),
                         new double[] { 5, 5, 5, 5, 5, 1, 2 }, 2, 4, true, true);
                    break;

                case 4:
                    Console.WriteLine("Entrer un nom : ");
                    String nom = Console.ReadLine();
                    int popMax = 0;
                    while(popMax < 10 || popMax > 200)
                    {
                        Console.WriteLine("Quel maximum pour la population ? [10 à 200]");
                        popMax = Int32.Parse(Console.ReadLine());
                    }
                    double learnRate = 0;
                    while (learnRate < 0.1 || learnRate > 1)
                    {
                        Console.WriteLine("Entrer le taux d'apprentissage ? [0.1 à 1]");
                        learnRate = Double.Parse(Console.ReadLine());
                    }
                    double alph = 0;
                    while (alph < 0.1 || alph > 1)
                    {
                        Console.WriteLine("Entrer la valeur alpha ? [0.1 à 1]");
                        alph = Double.Parse(Console.ReadLine());
                    }
                    double errorThresh = 0.01 * 504;
                    int power = 5;
                    double discount = 0;
                    while (discount < 0.1 || discount > 1)
                    {
                        Console.WriteLine("Entrer la valeur de discount ? [0.1 à 1]");
                        discount = Double.Parse(Console.ReadLine());
                    }
                    double fitnessThresh = 0;
                    while (fitnessThresh < 0.1 || fitnessThresh > 1)
                    {
                        Console.WriteLine("Entrer le seuil de fitness? [0.1 à 1]");
                        fitnessThresh = Double.Parse(Console.ReadLine());
                    }
                    double exploPro = 0;
                    while (exploPro < 0.1 || exploPro > 1)
                    {
                        Console.WriteLine("Entrer la probabilité d'exploration? [0.1 à 1]");
                        exploPro = Double.Parse(Console.ReadLine());
                    }
                    String filePath = nom + "BlackPopFile";
                    FOXCSopt = new FOXCSOptions(
                        nom, popMax, learnRate, alph, 0.01 * 504, 5,
                         discount, 25, 20, fitnessThresh, 20, exploPro, false, false,
                         filePath,
                         statePredicateOptions, actionPredicateOptions, knowledgeBase.ToArray(),
                         new double[] { 1, 1, 0, 2, 2, 0, 1 }, 2, 4, true, true);
                    break;



                default:
                    throw new Exception();
            }
        }

		tmp = 0;

		do {
			Console.WriteLine ("\nType AI2 ?\n 1 : MiniMax \n 2 : AlphaBeta \n 3 : NegaScout\n 4 : ProofNumberSearch\n 5 : PartieAleatoire\n 6 : FOXCS\n");
			try {
				tmp = int.Parse (Console.ReadLine ());
			} catch (FormatException) {
				Console.WriteLine ("\nVeuillez entrer un nombre");
			}
		} while (tmp < 1 || tmp > 6);

		switch (tmp) {
			case 1:
				_GameConfig.player2Type = PlayerType.MINMAX;
				break;
			case 2:
				_GameConfig.player2Type = PlayerType.ALPHABETA;
				break;
			case 3:
				_GameConfig.player2Type = PlayerType.NEGASCOUT;
				break;
			case 4:
				_GameConfig.player2Type = PlayerType.PNS;
				break;
            case 5:
                _GameConfig.player2Type = PlayerType.RNG;
                break;
            case 6:
                _GameConfig.player2Type = PlayerType.FOXCS;
                break;
        }

		tmp = 0;


        if (_GameConfig.player2Type != PlayerType.FOXCS)
        {
            do
            {
                Console.WriteLine("\nDepth AI 2 ? From 1 to 5\n");
                try
                {
                    tmp = int.Parse(Console.ReadLine());
                }
                catch (FormatException)
                {
                    Console.WriteLine("\nVeuillez entrer un nombre");
                }
            } while (tmp < 1 || tmp > 5);
            _GameConfig.player2Difficulty = tmp;
        }

        if (_GameConfig.player2Type == PlayerType.FOXCS)
        {
            Console.WriteLine("\nOptions prédefinies pour FOXCS ? \n" +
                              " 0. Scénario par défaut (général => spécifique)\n" +
                              " 1. Scénario par défaut simple (général => spécifique)\n" +
                              " 2. Scénario par défaut simple (spécifique => général)\n" +
                              " 3. Scénario par défaut simple (hybride)\n" +
                              " 4. Scénario personalisé.\n");
            switch (Int32.Parse(Console.ReadLine()))
            {
                case 0:
                    actionPredicateOptions2.Add("move", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1)
                    }));


                    actionPredicateOptions2.Add("drop", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1)
                        }));


                    actionPredicateOptions2.Add("promote", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.BOOL, new ArgMode[]{
                        ArgMode.CONST}, -1)
                        }));

                    statePredicateOptions2.Add("onTile", new PredicateOptions(
                        1, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1)
                        }));


                    statePredicateOptions2.Add("legalMove", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1)
                        }));


                    statePredicateOptions2.Add("legalDrop", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 1)
                        }));

                    FOXCSopt2 = new FOXCSOptions(
                        "test", 300, 0.1, 0.1, 0.01 * 504, 5,
                         0.71, 25, 20, 0.1, 20, 0.33, false, false,
                         "FOXCStestWhitePopSet1",
                         statePredicateOptions2, actionPredicateOptions2, knowledgeBase.ToArray(),
                         new double[] { 1, 1, 0, 3, 3, 0, 1 }, 2, 4, true, true);
                    break;

                case 1:

                    actionPredicateOptions2.Add("move", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75)
                        }));


                    actionPredicateOptions2.Add("drop", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75)
                        }));


                    actionPredicateOptions2.Add("promote", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.BOOL, new ArgMode[]{
                        ArgMode.CONST}, -1)
                        }));

                    statePredicateOptions2.Add("onTile", new PredicateOptions(
                        1, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.75)
                        }));

                    FOXCSopt2 = new FOXCSOptions(
                        "test", 200, 0.1, 0.1, 0.01 * 504, 5,
                         0.71, 25, 20, 0.1, 20, 0.33, false, false,
                         "FOXCStestWhitePopSet2",
                         statePredicateOptions2, actionPredicateOptions2, knowledgeBase.ToArray(),
                         new double[] { 1, 2, 0, 4, 4, 0, 1 }, 2, 6, true, true);
                    break;

                case 2:

                    actionPredicateOptions2.Add("move", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    }));


                    actionPredicateOptions2.Add("drop", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25)
                        }));


                    actionPredicateOptions2.Add("promote", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.BOOL, new ArgMode[]{
                        ArgMode.CONST}, -1)
                        }));

                    statePredicateOptions2.Add("onTile", new PredicateOptions(
                        1, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25)
                        }));


                    statePredicateOptions2.Add("legalMove", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25)
                        }));


                    statePredicateOptions2.Add("legalDrop", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.25)
                        }));

                    FOXCSopt2 = new FOXCSOptions(
                        "test", 200, 0.1, 0.1, 0.01 * 504, 5,
                        0.71, 50, 20, 0.1, 20, 0.33, false, false,
                        "FOXCStestWhitePopSet3",
                        statePredicateOptions2, actionPredicateOptions2, knowledgeBase.ToArray(),
                        new double[] { 3, 3, 1, 1, 1, 1, 1 }, 2, 4, true, true);
                    break;

                case 3:
                    actionPredicateOptions2.Add("move", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5),
                    }));


                    actionPredicateOptions2.Add("drop", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5)
                        }));


                    actionPredicateOptions2.Add("promote", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5),
                    new ArgOptions(ArgType.BOOL, new ArgMode[]{
                        ArgMode.CONST}, -1)
                        }));

                    statePredicateOptions2.Add("onTile", new PredicateOptions(
                        1, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5)
                        }));


                    statePredicateOptions2.Add("legalMove", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5)
                        }));


                    statePredicateOptions2.Add("legalDrop", new PredicateOptions(
                        0, 0, false,
                        new ArgOptions[] {
                    new ArgOptions(ArgType.TOKEN, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5),
                    new ArgOptions(ArgType.X, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5),
                    new ArgOptions(ArgType.Y, new ArgMode[]{
                        ArgMode.CONST, ArgMode.VAR, ArgMode.BOUND, ArgMode.ANONYMOUS}, 0.5),
                        }));

                    FOXCSopt2 = new FOXCSOptions(
                        "test", 300, 0.1, 0.1, 0.01 * 504, 5,
                         0.71, 25, 20, 0.1, 20, 0.33, false, false,
                         "FOXCStestWhitePopSet4",
                         statePredicateOptions2, actionPredicateOptions2, knowledgeBase.ToArray(),
                         new double[] { 5, 5, 5, 5, 5, 1, 2 }, 2, 4, true, true);
                    break;

                case 4:
                    Console.WriteLine("Entrer un nom : ");
                    String nom = Console.ReadLine();
                    int popMax = 0;
                    while (popMax < 10 || popMax > 200)
                    {
                        Console.WriteLine("Quel maximum pour la population ? [10 à 200]");
                        popMax = Int32.Parse(Console.ReadLine());
                    }
                    double learnRate = 0;
                    while (learnRate < 0.1 || learnRate > 1)
                    {
                        Console.WriteLine("Entrer le taux d'apprentissage ? [0.1 à 1]");
                        learnRate = Double.Parse(Console.ReadLine());
                    }
                    double alph = 0;
                    while (alph < 0.1 || alph > 1)
                    {
                        Console.WriteLine("Entrer la valeur alpha ? [0.1 à 1]");
                        alph = Double.Parse(Console.ReadLine());
                    }
                    double errorThresh = 0.01 * 504;
                    int power = 5;
                    double discount = 0;
                    while (discount < 0.1 || discount > 1)
                    {
                        Console.WriteLine("Entrer la valeur de discount ? [0.1 à 1]");
                        discount = Double.Parse(Console.ReadLine());
                    }
                    double fitnessThresh = 0;
                    while (fitnessThresh < 0.1 || fitnessThresh > 1)
                    {
                        Console.WriteLine("Entrer le seuil de fitness? [0.1 à 1]");
                        fitnessThresh = Double.Parse(Console.ReadLine());
                    }
                    double exploPro = 0;
                    while (exploPro < 0.1 || exploPro > 1)
                    {
                        Console.WriteLine("Entrer la probabilité d'exploration? [0.1 à 1]");
                        exploPro = Double.Parse(Console.ReadLine());
                    }
                    String filePath = nom + "WhitePopFile";
                    FOXCSopt = new FOXCSOptions(
                        nom, popMax, learnRate, alph, 0.01 * 504, 5,
                         discount, 25, 20, fitnessThresh, 20, exploPro, false, false,
                         filePath,
                         statePredicateOptions, actionPredicateOptions, knowledgeBase.ToArray(),
                         new double[] { 1, 1, 0, 2, 2, 0, 1 }, 2, 4, true, true);
                    break;

                default:
                    throw new Exception();
            }
        }

    }

	/// <summary>
	/// Initialisation du jeu.
	/// </summary>
	public static void initGame () {

		tokenId = 0;

		_GameManager.workFlow = new StringBuilder ();

		// Création des cases
		generateBoxes ();

		// Création des joueurs
		generatePlayers ();

		// Création des pièces
		generatePlayer1Tokens (player1);
		generatePlayer2Tokens (player2);

		// Attribution de valeurs aux pièces
		generateTokenValues ();

		// Attribution des IA
		generateAI ();

		// Passage des références au game manager
		_GameManager.players = new Player[2] { player1, player2 };
		_GameManager.boxes = boxes;
		_GameManager.tokens = tokens;
		_GameManager.AI = new AIHandler[2] { AI1, AI2 };

		string tempo = "\nPlayer 1 : " + _GameManager.players [0].type + ", Player 2 : " + _GameManager.players [1].type + "\n";

		Console.WriteLine (tempo);
		_GameManager.workFlow.Append (tempo);

		tempo = "Depth Player 1 : " + _GameConfig.player1Difficulty + ", Depth Player 2 : " + _GameConfig.player2Difficulty + "\n\n\n";

		Console.WriteLine (tempo);
		_GameManager.workFlow.Append (tempo);

	}

	/// <summary>
	/// Réinitialisation du plateau de jeu après une partie terminée.
	/// Réalise plusieurs parties sans reconfigurer les joueurs à chacune d'elles.
	/// </summary>
	public static void reinitBoard() {
		tokenId = 0;
		boxes.Clear ();
		tokens.Clear ();

		generateBoxes ();
		generatePlayer1Tokens (player1);
		generatePlayer2Tokens (player2);
		generateTokenValues ();

		_GameManager.boxes = boxes;
		_GameManager.tokens = tokens;
	}

	/// <summary>
	/// Création des cases.
	/// </summary>
	static void generateBoxes () {

		for (int i = 0; i < nrow; i++) {
			for (int j = 0; j < nrow; j++) {

				// Création de la case
				Box box = new Box ();

				// Positionnement de la case dans l'espace
				box.coord = new Coordinates (j, i);

				// Ajout de la case à la liste
				boxes.Add (box);

			}
		}

		// Capture boxes - Player 1
		cb1 = new CaptureBench ();

		// Capture boxes - Player 2
		cb2 = new CaptureBench ();

	}

	/// <summary>
	///	Creation des joueurs.
	/// </summary>
	static void generatePlayers () {

		player1 = new Player ("AI_1", _GameConfig.player1Type, ShogiUtils.GameColor.SENTE);
		player2 = new Player ("AI_2", _GameConfig.player2Type, ShogiUtils.GameColor.GOTE);

	}

	/// <summary>
	/// Creation des pièces du joueur blanc.
	/// </summary>
	/// <param name="player">Un joueur.</param>
	static void generatePlayer1Tokens (Player player) {

		Box box;

		// Pions
		for (int i = 18; i < 27; i++) {

			Pawn pawn = new Pawn ();

			box = boxes [i];
			pawn.owner = player;
			pawn.id = tokenId;
            pawn.internalID = i - 17;
			tokenId++;
			tokens.Add (pawn);
			_GameManager.bindBoxAndToken (box, pawn);

		}

		// Tour
		Rook rook = new Rook ();

		box = boxes [16];
		rook.owner = player;
		rook.id = tokenId;
        rook.internalID = 0;
		tokenId++;
		tokens.Add (rook);
		_GameManager.bindBoxAndToken (box, rook);

		// Fou
		Bishop bishop = new Bishop ();

		box = boxes [10];
		bishop.owner = player;
		bishop.id = tokenId;
        bishop.internalID = 0;
        tokenId++;
		tokens.Add (bishop);
		_GameManager.bindBoxAndToken (box, bishop);

		// Or
		Gold gold;

		gold = new Gold ();
		box = boxes [3];
		gold.owner = player;
		gold.id = tokenId;
        gold.internalID = 1;
		tokenId++;
		tokens.Add (gold);
		_GameManager.bindBoxAndToken (box, gold);

		gold = new Gold ();
		box = boxes [5];
		gold.owner = player;
		gold.id = tokenId;
        gold.internalID = 2;
        tokenId++;
		tokens.Add (gold);
		_GameManager.bindBoxAndToken (box, gold);

		// Argent
		Silver silver;

		silver = new Silver ();
		box = boxes [2];
		silver.owner = player;
		silver.id = tokenId;
        silver.internalID = 1;
        tokenId++;
		tokens.Add (silver);
		_GameManager.bindBoxAndToken (box, silver);

		silver = new Silver ();
		box = boxes [6];
		silver.owner = player;
		silver.id = tokenId;
        silver.internalID = 2;
        tokenId++;
		tokens.Add (silver);
		_GameManager.bindBoxAndToken (box, silver);

		// Cavalier
		Knight knight;

		knight = new Knight ();
		box = boxes [1];
		knight.owner = player;
		knight.id = tokenId;
        knight.internalID = 1;
        tokenId++;
		tokens.Add (knight);
		_GameManager.bindBoxAndToken (box, knight);

		knight = new Knight ();
		box = boxes [7];
		knight.owner = player;
		knight.id = tokenId;
        knight.internalID = 2;
        tokenId++;
		tokens.Add (knight);
		_GameManager.bindBoxAndToken (box, knight);

		// Lancier
		Lance lance;

		lance = new Lance ();
		box = boxes [0];
		lance.owner = player;
		lance.id = tokenId;
        lance.internalID = 1;
		tokenId++;
		tokens.Add (lance);
		_GameManager.bindBoxAndToken (box, lance);

		lance = new Lance ();
		box = boxes [8];
		lance.owner = player;
		lance.id = tokenId;
        lance.internalID = 2;
        tokenId++;
		tokens.Add (lance);
		_GameManager.bindBoxAndToken (box, lance);


		// Roi
		King king = new King ();

		box = boxes [4];
		king.owner = player;
		king.id = tokenId;
        king.internalID = 0;
		tokenId++;
		tokens.Add (king);
		_GameManager.bindBoxAndToken (box, king);

	}

	/// <summary>
	/// Creation des pièces du joueur noir.
	/// </summary>
	/// <param name="player">Un joueur.</param>
	static void generatePlayer2Tokens (Player player) {

		Box box;

		// Pions
		for (int i = 54; i < 63; i++) {

			Pawn pawn = new Pawn ();

			box = boxes [i];
			pawn.owner = player2;
			pawn.id = tokenId;
            pawn.internalID = i - 53;
			tokenId++;
			tokens.Add (pawn);
			_GameManager.bindBoxAndToken (box, pawn);

		}

		// Tour
		Rook rook = new Rook ();

		box = boxes [64];
		rook.owner = player;
		rook.id = tokenId;
        rook.internalID = 0;
		tokenId++;
		tokens.Add (rook);
		_GameManager.bindBoxAndToken (box, rook);

		// Fou
		Bishop bishop = new Bishop ();

		box = boxes [70];
		bishop.owner = player;
		bishop.id = tokenId;
        bishop.internalID = 0;
		tokenId++;
		tokens.Add (bishop);
		_GameManager.bindBoxAndToken (box, bishop);

		// Or
		Gold gold;

		gold = new Gold ();
		box = boxes [77];
		gold.owner = player;
		gold.id = tokenId;
        gold.internalID = 1;
		tokenId++;
		tokens.Add (gold);
		_GameManager.bindBoxAndToken (box, gold);

		gold = new Gold ();
		box = boxes [75];
		gold.owner = player;
		gold.id = tokenId;
        gold.internalID = 2;
        tokenId++;
		tokens.Add (gold);
		_GameManager.bindBoxAndToken (box, gold);

		// Argent
		Silver silver;

		silver = new Silver ();
		box = boxes [78];
		silver.owner = player;
		silver.id = tokenId;
        silver.internalID = 1;
		tokenId++;
		tokens.Add (silver);
		_GameManager.bindBoxAndToken (box, silver);

		silver = new Silver ();
		box = boxes [74];
		silver.owner = player;
		silver.id = tokenId;
        silver.internalID = 2;
        tokenId++;
		tokens.Add (silver);
		_GameManager.bindBoxAndToken (box, silver);

		// Cavalier
		Knight knight;

		knight = new Knight ();
		box = boxes [79];
		knight.owner = player;
		knight.id = tokenId;
        knight.internalID = 1;
		tokenId++;
		tokens.Add (knight);
		_GameManager.bindBoxAndToken (box, knight);

		knight = new Knight ();
		box = boxes [73];
		knight.owner = player;
		knight.id = tokenId;
        knight.internalID = 2;
        tokenId++;
		tokens.Add (knight);
		_GameManager.bindBoxAndToken (box, knight);

		// Lancier
		Lance lance;

		lance = new Lance ();
		box = boxes [80];
		lance.owner = player;
		lance.id = tokenId;
        lance.internalID = 1;
		tokenId++;
		tokens.Add (lance);
		_GameManager.bindBoxAndToken (box, lance);

		lance = new Lance ();
		box = boxes [72];
		lance.owner = player;
		lance.id = tokenId;
        lance.internalID = 2;
        tokenId++;
		tokens.Add (lance);
		_GameManager.bindBoxAndToken (box, lance);

		// Roi
		Jewel jewel = new Jewel ();

		box = boxes [76];
		jewel.owner = player;
		jewel.id = tokenId;
        jewel.internalID = 0;
		tokenId++;
		tokens.Add (jewel);
		_GameManager.bindBoxAndToken (box, jewel);
	}

	/// <summary>
	/// Affectation des valeurs aux pièces créées.
	/// </summary>
	public static void generateTokenValues () {

		foreach (Token t in tokens) {
			switch (t.getTokenType ()) {
				case TokenType.PAWN:
					t.value = 100;
					break;
				case TokenType.LANCE:
					t.value = 300;
					break;
				case TokenType.KNIGHT:
					t.value = 400;
					break;
				case TokenType.SILVER:
					t.value = 500;
					break;
				case TokenType.BISHOP:
					t.value = 800;
					break;
				case TokenType.ROOK:
					t.value = 1000;
					break;
				case TokenType.GOLD:
					t.value = 600;
					break;
				case TokenType.KING:
					t.value = 10000;
					break;
			}
		}

	}

	/// <summary>
	/// Generation des IA.
	/// </summary>
	public static void generateAI () {

		AI1 = null;
		AI2 = null;

		switch (_GameConfig.player1Type) {

			case PlayerType.ALPHABETA:
				AI1 = new AlphaBeta (player1, player2);
				break;
			case PlayerType.MINMAX:
				AI1 = new MiniMax (player1, player2);
				break;
			case PlayerType.NEGASCOUT:
				AI1 = new NegaScout (player1, player2);
				break;
			case PlayerType.PNS:
				AI1 = new ProofNumberSearch (player1, player2);
				break;
            case PlayerType.RNG:
                AI1 = new RandomGame(player1, player2);
                break;
            case PlayerType.FOXCS:
                AI1 = new FOXCS(FOXCSopt);
                break;
        }

		switch (_GameConfig.player2Type) {

			case PlayerType.ALPHABETA:
				AI2 = new AlphaBeta (player2, player1);
				break;
			case PlayerType.MINMAX:
				AI2 = new MiniMax (player2, player1);
				break;
			case PlayerType.NEGASCOUT:
				AI2 = new NegaScout (player2, player1);
				break;
			case PlayerType.PNS:
				AI2 = new ProofNumberSearch (player2, player1);
				break;
            case PlayerType.RNG:
                AI2 = new RandomGame(player2, player1);
                break;
            case PlayerType.FOXCS:
                AI2 = new FOXCS(FOXCSopt2);
                break;
                /*
			case PlayerType.QLEARNING:
				AI2 = new QLearning (player2, player1);
				break;
                */
		}

	}

}
