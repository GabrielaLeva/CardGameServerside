using System.Diagnostics;
using System.Security.Cryptography.Xml;

namespace GamblingServer.Games
{
    public class WhiteJack31:BaseCardgame
    {
        public WhiteJack31(int[] ids): base(ids) {
            foreach (int id in ids) {
                DrawCards(id, 3);
            }
            knock_indicator = -1;
        }
        protected int knock_indicator;
        public override bool ValidateTurn(int id)
        {
            if (knock_indicator == id) {
                return false;
            }
            return base.ValidateTurn(id);
        }
        public void setKnock(int knocker) {
            if(knock_indicator == -1) knock_indicator = knocker;
        }
        public int EvaluateHand(List<string> hand) {
            int handvalue=0;
            //three of a kind check
            if(hand.DistinctBy(x => x[0]).Count() == 1)
            {
                return 30;
            }
            // normal eval
            foreach (string card in hand) {
                handvalue += cardValues[card[0].ToString()];
            }
            return handvalue;
        }
        public int CheckWincons(int id) {
            if (knock_indicator == id) {
                //TODO: drawbreakers 
                Dictionary<int,int> handVals = new Dictionary<int,int>();
                foreach (var hand in PlayerHands)
                {
                    handVals[hand.Key]=EvaluateHand(hand.Value);
                }
                return handVals.Max().Key;
            } else if (EvaluateHand(PlayerHands[id]) == 31) {
                return id;
            }
            return -1;
        }
    }
}
