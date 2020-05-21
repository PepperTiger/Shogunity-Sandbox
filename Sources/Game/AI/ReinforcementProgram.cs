using System;

using ShogiUtils;

namespace Sandbox.Sources.Game.AI
{
    public enum RPMode
    {
        INT,
        DOUBLE
    }

    public class ReinforcementProgam
    {
        private RPMode mode;

        public ReinforcementProgam(RPMode mode)
        {
            this.mode = mode;
        }
            //-> this.mode = mode;

        public object GetReward(Token tokCaptured, Player self)
        {

            switch (mode)
            {

                case RPMode.INT:
                    int r = 0;

                    if (tokCaptured == null){
                        return r;
                    }
                        //return r;

                    else
                    {
                        switch (tokCaptured.type)
                        {
                            case TokenType.PAWN: r = tokCaptured.isPromoted ? 7 : 1; break;
                            case TokenType.LANCE: r = tokCaptured.isPromoted ? 6 : 3; break;
                            case TokenType.KNIGHT: r = tokCaptured.isPromoted ? 6 : 4; break;
                            case TokenType.SILVER: r = tokCaptured.isPromoted ? 6 : 5; break;
                            case TokenType.GOLD: r = 6; break;
                            case TokenType.BISHOP: r = tokCaptured.isPromoted ? 10 : 8; break;
                            case TokenType.ROOK: r = tokCaptured.isPromoted ? 12 : 10; break;
                        }

                        return tokCaptured.owner.color.Equals(self.color) ? -r : r;

                    }



                case RPMode.DOUBLE:
                    throw new Exception("RPMode.DOUBLE not implemented\n");

                default:
                    throw new Exception("Wrong RPMode\n");
            }


        }
    }
}