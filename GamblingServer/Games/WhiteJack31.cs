using System.Diagnostics;
using System.Net.WebSockets;
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
        public override void IncrementTurn()
        {
            base.IncrementTurn();
            if (knock_indicator == turnMarker+1)
            {
                Dictionary<int, int> handVals = new Dictionary<int, int>();
                foreach (var hand in PlayerHands)
                {
                    handVals[hand.Key] = EvaluateHand(hand.Value);
                }
                
                SendAll("win", handVals.MaxBy(x => x.Value).Key.ToString());
            }
        }
        public string Discarddraw(int id, string card)
        {
            try
            {
                if (ValidateTurn(id))
                {
                    DiscardCard(id, card);
                    DrawCards(id, 1); 
                    SendAll("discard", card);
                    if (EvaluateHand(PlayerHands[id]) == 31)
                    {
                        SendAll("win", id.ToString());
                    }
                    return PlayerHands[id][2];
                }
            }
            catch (ArgumentException)
            {
                return "Invalid card or player";
            }
            return "turn violation";
        }
        public void setKnock(int knocker) {
            if (knock_indicator == -1 & ValidateTurn(knocker)) { 
                knock_indicator = knocker;
                SendAll("knock",knocker.ToString());
            }
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
    }
}
