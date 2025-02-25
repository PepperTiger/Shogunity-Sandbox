using System;
using System.Collections.Generic;

//using Unity.Engine;

using ShogiUtils;


namespace Sandbox.Sources.Game.AI
{

	public class MarkovianDecisionProcessus
	{

		/// <summary>
		/// Tableau de la progression d'un état à un autre
		/// </summary>
		public Progression [] progression;

		/// <summary>
		/// Tableau des récompenses d'un état
		/// </summary>
		public Recompense [] recompenses;

		/// <summary>
		/// Facteur d'escompte (compris entre 0 et 1 inclus)
		/// Valeur à modifier par la suite
		/// </summary>
		public double gamma = 0.5; 

		/// <summary>
		/// Possibilité d'erreur (compris en 0% et 100%)
		/// Valeur à modifier par la suite
		/// </summary>
		public double error = 0.1; //Taux à 10% d'erreurs

		/// <summary>
		/// Valeur obtenue pour chaque état 
		/// </summary>
		Dictionary<qState, Double> valeur = new Dictionary<qState, Double> ();


		/// <summary>
		/// Meilleur valeur de l'actuel dictionnaire de chaque état
		/// </summary>
		public Dictionary<qState, Double> currentValue = new Dictionary<qState, Double> ();

		/// <summary>
		/// Meilleur valeur (valeur optimale) de chaque état
		/// </summary>
		public Dictionary<qState, Double> optiValue = new Dictionary<qState, Double> ();


		/// <summary>
		/// Meilleur mouvement de l'actuel dictionnaire de chaque état
		/// </summary>
		public Dictionary<qState, Move> currentMove = new Dictionary<qState, Move> ();

		/// <summary>
		/// Le mouvement liée à la valeur optimale de chaque état
		/// </summary>
		public Dictionary<qState, Move> optiMove = new Dictionary<qState, Move> ();


		/// <summary>
		/// Degré d'importance aux nouvelles recherches
		/// </summary>
		public double learningRate;


	
		public void read_files() { //lecture et importation des valeurs dans les fichiers dans une variable (ici un tableau)

			//Sauvegarder dans la variable --> progression

			//Sauvegarder dans la variable --> recompenses

		}


		public MarkovianDecisionProcessus(Progression [] progression, Recompense [] recompenses)
		{
			this.progression = progression;
			this.recompenses = recompenses;
		}


		/// <summary>
		/// Récupère toutes les actions possibles à partir d'un état donné.
		/// </summary>
		/// <returns>Une liste de toutes les actions possibles de cet état donné.</returns>
		/// <param name="state">L'état auquel on veut connaître les différentes actions possibles.</param>
		public List<Move> getActions(qState state) 
		{
			List<Move> actions = new List<Move> ();
			foreach(qState q in progression.getQStartState()) {
				if(q == state) {
					actions.Add(progression.getAction());
				}
			}
			return actions;
		}


		/// <summary>
		/// Récupère pour un état de départ donné et l'action réalisée, une paire <état d'arrivé, la probabilité>
		/// </summary>
		/// <returns>Un dictionnaire de cette paire</returns>
		/// <param name="state">L'état de départ.</param>
		/// /// <param name="action">L'action réalisée.</param>
		public Dictionary<qState, Double> getPair(qState state, Move action)
		{
			Dictionary<qState, Double> pair = new Dictionary<qState, Double> ();
			foreach (qState q in progression.getQStartState()) {
				if (q == state) {
					foreach (Move m in getActions(q) {
						if(m == action) {
							pair.Add (progression.getNewState (), progression.getProbabilite ());
						}
					}
				}
			}
			return pair;
		}


		/// <summary>
		/// Calcul les valeurs de chaque état via l'équation de Bellman, ainsi que la convergence des valeurs
		/// Sauvegarde la valeure optimale/le mouvement optimale pour chaque état
		/// </summary>
		/// <returns>Un dictionnaire d'une paire <état, valeur de cet état></returns>
		public Dictionary<qState, Double> solvingValue() {

			Dictionary<qState, Double> copy = new Dictionary<qState, Double> ();

			//Initialisation de tous les états avec une valeur de 0
			foreach (qState q in progression.getQStartState()) { 
				valeur.Add (q, 0);
				currentValue.Add (q, 0);
				optiValue.Add (q, 0);
			}
			
			while (true) {
				
				foreach (KeyValuePair<qState, Double> v in valeur) {
					copy.Add (v.Key, v.Value); //copie du dictionnaire "valeur" 
				}

				double checkup = 0.0;
			
				foreach (qState q in progression.getQStartState()) {
					double somme = 0.0;
					
					foreach (Move m in getActions(q)) {
						double nombre = 0.0;
						nombre = getPair (q, m).Values * valeur[getPair(q, m).Keys]; //Somme de : P(s'|q, m) * V(s')
						somme += nombre;
						if (currentValue [q] < nombre) {
							currentValue [q] = nombre; //Valeur maximale pour chaque état
							if (currentMove.ContainsKey (q)) {
								currentMove [q] = m;
							} else {
								currentMove.Add (q, m); //Mouvement lié à cette valeur maximale
							}
						}
					}
					
					valeur [q] = recompenses.q.getReward () + gamma * somme; //L'équation de Bellman 
					checkup = Mathf.Max (checkup, Mathf.Abs (valeur [q] - copy [q]));
				}
				
				//Test de convergence (sur les valeurs d'utilité des états)
				if (checkup < error * (1 - gamma) / gamma) {
					return copy;
				} else {
					
					foreach (KeyValuePair<qState, Double> v in copy) {
						copy.Remove (v.Key); //dictionnaire copy vide
					}
					foreach (qState q in optiValeur) {
						optiValue [q] = recompenses.q.getReward () + gamma * currentValue [q]; //Sauvegarde de la valeur optimale pour chaque état
						optiMove [q] = currentMove[q]; // Sauvegarde du mouvement optimale pour chaque état
					}
				}

			}
		}


		public Dictionary<qState, Double> getOptiValue() {
			return optiValue;
		}

		public Dictionary<qState, Double> getOptiMove() {
			return optiMove;
		}

		public void updateOptiValue(qState q) {
			double oldValue = optiValue [q];
			optiValue[q] = (1 - learningRate) * oldValue + learningRate * (recompenses.q.getReward () + gamma * Mathf.Max( 
		}

	
		/* Suite à cette nouvelle fonction, le script doit s'adapter. Il faut mettre en place une valeur pour un état avec une action donnée (et
		 * non juste un état) --> Q(s, a) = V(s).
		 * Faire une fonction qui détermine la valeur max de Q(s', a')
		 * Il faut donc décomposer la grosse fonction solvingValue.
		 * Voir à utiliser une structure de type : qState q, Action a, Valeur de (q, a), qState Arrivée q', probabilité de P(q'|q, a)
		 * Réaliser une liste de cette structure
		 * Peut-être supprimer les 2 autres scripts */


	}
}

