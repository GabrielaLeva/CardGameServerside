using System.Diagnostics;
using System.Security.Cryptography.Xml;

namespace GamblingServer.Games
{
    public class WhiteJack31:BaseCardgame
    {
        public WhiteJack31(string[] ids): base(ids) {
            foreach (var id in ids) {
                DrawCards(id, 3);
            }
            knock_indicator = "";
        }
        protected string knock_indicator;
        public override bool ValidateTurn(string uname)
        {
            if (knock_indicator == uname) {
                return false;
            }
            return base.ValidateTurn(uname);
        }
        public void setKnock(string knocker) {
            if(knock_indicator == "") knock_indicator = knocker;
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
        public void CheckWincons() {
            var handVals = new Dictionary<string, int>();
            foreach (var hand in PlayerHands)
            {
                handVals[hand.Key] = EvaluateHand(hand.Value);
            }

            if (handVals[PlayerHands.Keys.ElementAt(turnMarker)] == 31)
            {
                return;
            }
            IncrementTurn();
            if (knock_indicator == PlayerHands.Keys.ElementAt(turnMarker)) {
                return;
            }
        }
    }
}
