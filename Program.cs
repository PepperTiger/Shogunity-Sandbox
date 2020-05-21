
using Sandbox.Sources.Game.AI;
using ShogiUtils;
using System;
using System.Collections.Generic;
using System.IO;

namespace Sandbox {

    class MainClass
    {
        //Création de Fichier de Logs contenant les mouvements possibles des pieces
        public static void SetMoveListsLogs()
        {
            string file2 = "../../Logs/MOVES-" + (new DirectoryInfo("../../Logs").GetFiles().Length - 1) + "---" + _GameManager.players[0].name + "-" + _GameManager.players[0].type + "-" + _GameConfig.player1Difficulty + "---" + _GameManager.players[1].name + "-" + _GameManager.players[1].type + "-" + _GameConfig.player2Difficulty + ".txt";
            System.IO.File.WriteAllText(file2, _GameManager.moveFlow.ToString());

            _GameManager.moveFlow.Clear();
            foreach (Move m in _GameManager.movesList)
            {
                _GameManager.moveFlow.Append(m.ToString());
            }
            int total = 0;
            foreach (Token t in _GameManager.tokens)
            {
                int acc = 0;

                foreach (Move n in _GameManager.movesList)
                {
                    if (n.tokenID == t.id)
                    {
                        acc++;
                        total++;
                        _GameManager.moveFlow.Append(t.ToString() + acc + "\n");
                    }

                }
            }
            _GameManager.moveFlow.Append("\n TOTAL = " + total);

            string file3 = "../../Logs/LIST_OF_MOVES-" + (new DirectoryInfo("../../Logs").GetFiles().Length - 2) + "---" + _GameManager.players[0].name + "-" + _GameManager.players[0].type + "-" + _GameConfig.player1Difficulty + "---" + _GameManager.players[1].name + "-" + _GameManager.players[1].type + "-" + _GameConfig.player2Difficulty + ".txt";
            System.IO.File.WriteAllText(file3, _GameManager.moveFlow.ToString());
        }

        public static void Main(string[] args)
        {

			// Ask the user what game to initialize
			_init.setGameConfig();

			// Start the total time spent timer
			var totalTime = System.Diagnostics.Stopwatch.StartNew();

			int winner1 = 0;
			int winner2 = 0;

			_init.initGame();

			for (int i = 0; i < _init.parties; i++) {
				_GameManager.initGameManager();
				Console.WriteLine("Lancement du GameLoop n°" + (i+1) + "\n\n");
				_GameManager.gameLoop();
				if (_GameManager.currentPlayerIndex == 0)
				{
					winner1++;
				} else
				{
					winner2++;
				}
				Console.WriteLine("Fin du GameLoop n°" + (i+1));
				_init.reinitBoard();
			}

			// Le StringBuilder workFlow est initialisé dans initGameManager()
			if (_GameManager.currentPlayerIndex == 0) {
				Console.WriteLine ("\n\nSENTE WON!\n");
				_GameManager.workFlow.Append ("SENTE WON!\n\n");
			} else {
				Console.WriteLine ("\n\nGOTE WON!\n");
				_GameManager.workFlow.Append ("GOTE WON!\n\n");
			}

			// Ouverture d'un fichier
			string file = "../../Logs/Game-" + new DirectoryInfo ("../../Logs").GetFiles ().Length + "---" + _GameManager.players [0].name + "-" + _GameManager.players [0].type + "-" + _GameConfig.player1Difficulty + "---" + _GameManager.players [1].name + "-" + _GameManager.players [1].type + "-" + _GameConfig.player2Difficulty + ".txt";

			// Stop the total time spent timer
			totalTime.Stop ();

			Console.WriteLine ("\nTemps total écoulé : " + totalTime.ElapsedMilliseconds + "ms\n\n");
			_GameManager.workFlow.Append ("\nTemps total écoulé : " + totalTime.ElapsedMilliseconds + "ms\n\n");

			// Ecriture du contenu du StringBuilder dans le fichier
			System.IO.File.WriteAllText (file, _GameManager.workFlow.ToString ());
                /*
			Console.WriteLine("Press Any Key to Exit");
                */
			Console.ReadLine();

			Environment.Exit (1);

        }
    }

}
