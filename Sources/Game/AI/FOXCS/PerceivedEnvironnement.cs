    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShogiUtils;

namespace Sandbox.Sources.Game.AI.FOXCS
{
    /// <summary>
    /// Listes des etats et des actions sous forme de predicats.
    /// </summary>
    [Serializable]
    public class PerceivedEnvironnement
    {
        /// <summary>
        /// Liste des états.
        /// </summary>
        public List<Attribute> states { get; }

        /// <summary>
        /// Liste des actions.
        /// </summary>
        public List<Attribute> actions { get; }

        public List<Move> legalMoves { get; }

        /// <summary>
        /// Variable de copie du board.
        /// </summary>
        public Board board { get; private set; }

        public List<string> tokenOnBoard { get; set; }
            

        /// <summary>
        /// Constructeur, initialisant les variables de <see cref="PerceivedEnvironnement"/>.
        /// </summary>
        public PerceivedEnvironnement()
        {
            this.board = null;
            this.states = new List<Attribute>();
            this.actions = new List<Attribute>();
            this.tokenOnBoard = new List<string>();
        }

        /// <summary>
        /// Met à jour les informations d'états et d'actions à partir du tableau courant.
        /// </summary>  
        public void UpdateSensors(Board board, FOXCSOptions fo)
        {
            this.states.Clear();
            this.actions.Clear();
            this.board = AIHandler.CloneBoard(board);
            this.tokenOnBoard.Clear();
            List<Token> tokenList = _GameManager.GetAllTokens();

            if (_GameManager.toPromoteToken != null)
            {
                this.actions.Add(new Attribute("promote", new string[] { _GameManager.toPromoteToken.ToPrologCode(), FOXCS.ToPrologCode(true) }, fo.actionPredicateOptions["promote"]));
                this.actions.Add(new Attribute("promote", new string[] { _GameManager.toPromoteToken.ToPrologCode(), FOXCS.ToPrologCode(false) }, fo.actionPredicateOptions["promote"]));
            }

            foreach (Token tok in tokenList)
            {
                Attribute acc;

                if (!tok.isCaptured)
                {
                    tokenOnBoard.Add(tok.ToPrologCode());
                    this.states.Add(new Attribute("onTile", new string[] { tok.ToPrologCode(), "c"+tok.box.getCoord().x.ToString(), "l"+tok.box.getCoord().y.ToString() }, fo.statePredicateOptions["onTile"]));
                    List<Coordinates> tokLegalMoves = !tok.isPromoted ? tok.legalMoves(this.board) : tok.legalMovesPlus(this.board);
                    List<Coordinates> tokPossibleMoves = !tok.isPromoted ? tok.possibleMoves(this.board) : tok.possibleMovesPlus(this.board);

                    foreach (Coordinates cord in tokLegalMoves)
                    {
                        
                        acc = new Attribute("legalMove", new string[] { tok.ToPrologCode(), "c" + cord.x.ToString(), "l" + cord.y.ToString() }, fo.statePredicateOptions["legalMove"]);
                        if(!states.Contains(acc))
                            this.states.Add(acc);
                            
                        

                        if (tok.owner.Equals(_GameManager.players[_GameManager.currentPlayerIndex]) && _GameManager.toPromoteToken == null)
                        {
                            acc = new Attribute("move", new string[] { tok.ToPrologCode(), "c" + cord.x.ToString(), "l" + cord.y.ToString() }, fo.actionPredicateOptions["move"]);
                            if (!actions.Contains(acc))
                                this.actions.Add(acc);
                        }
                    }

                    /*
                    tok.getTokensToEat();
                    foreach (Coordinates cord in tok.possibleEats)
                    {
                        acc = new Attribute("inRange", new string[] { tok.ToPrologCode(), this.board.boxes[cord.getIndex()].getToken().ToPrologCode(), "c" + cord.x.ToString(), "l" + cord.y.ToString() }, fo.statePredicateOptions["inRange"]);
                        if (!states.Contains(acc))
                            this.states.Add(acc);
                    }
                    */
                           
                }

                else
                {
                    
                    foreach(Coordinates cord in tok.legalDrops(board))
                    {
                        if (tok.owner.Equals(_GameManager.players[_GameManager.currentPlayerIndex]) && _GameManager.toPromoteToken == null)
                        {
                            acc = new Attribute("drop", new string[] { tok.ToPrologCode(), "c" + cord.x.ToString(), "l" + cord.y.ToString() }, fo.actionPredicateOptions["drop"]);
                            if (!actions.Contains(acc))
                                this.actions.Add(acc);
                        }
                            
                        acc = new Attribute("legalDrop", new string[] { tok.ToPrologCode(), "c" + cord.x.ToString(), "l" + cord.y.ToString() }, fo.statePredicateOptions["legalDrop"]);
                        if (!states.Contains(acc))
                            this.states.Add(acc);
                    }
                    
                    
                }
            }
        }
    }
}
