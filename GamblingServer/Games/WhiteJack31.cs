using System.Diagnostics;
using System.Net.WebSockets;
using System.Security.Cryptography.Xml;

namespace GamblingServer.Games
{
    public class WhiteJack31:BaseCardgame
    {
        public WhiteJack31(string[] ids, Guid guid): base(ids,guid) {
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
        public async Task<bool> setKnock(string knocker) {
            if(knock_indicator == "") {
                knock_indicator = knocker;
                await SendAll("knock",knocker);
                return true;
            }
            return false;
        }
        public int EvaluateHand(List<string> hand) {
            //three of a kind check
            if(hand.DistinctBy(x => x[0]).Count() == 1)
            {
                return 30;
            }
            // normal eval
            Dictionary<string, int> suit_values = new Dictionary<string, int>() { 
                { "D",0},
                { "H",0},
                { "S",0},
                { "C",0},
            };
            foreach (string card in hand) {
                suit_values[card[1].ToString()] += cardValues[card[0].ToString()];
            }

            return suit_values.MaxBy(kv=>kv.Value).Value;
        }
        public async void CheckWincons() {
            var handVals = new Dictionary<string, int>();
            foreach (var hand in PlayerHands)
            {
                handVals[hand.Key] = EvaluateHand(hand.Value);
            }

            if (handVals[PlayerHands.Keys.ElementAt(turnMarker)] == 31)
            {
                await SendAll("winner", PlayerHands.Keys.ElementAt(turnMarker));
                
                return;
            }
            IncrementTurn();
            if (knock_indicator == PlayerHands.Keys.ElementAt(turnMarker)) {
                await SendAll("winner",handVals.MaxBy(kv=>kv.Value).Key);
                return;
            }
        }
    }
}
