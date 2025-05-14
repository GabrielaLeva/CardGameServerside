using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace GamblingServer.Games
{
    /// <summary>
    /// Base cardgame class including basic game functionality
    /// Will evolve as more base functions are needed
    /// </summary>
    public class BaseCardgame
    {
        /// <summary>
        /// The cards are stored as 2 character strings for now  with 0 as 10 and A as ace, and the house being marked by the first letter in the english name (like C for clubs)
        /// </summary>
        public List<string> Deck;
        public Dictionary<string, List<string>> PlayerHands;
        public Dictionary<string, WebSocket> PlayerSockets;
        public List<string> DiscardPile;
        protected Dictionary<string, int> cardValues;
        protected int turnMarker;
        protected int PlayerAmount;
        protected Guid guid;

        public BaseCardgame(string[] ids,Guid guid) {
            this.guid = guid;
            Deck = [];
            DiscardPile = [];
            Deckgen();
            CardValGen();
            PlayerHands = [];
            PlayerSockets = [];
            turnMarker = 0;
            PlayerAmount = ids.Length;
            foreach (string id in ids) {
                PlayerHands[id] = [];
            }
            
        }
        /// <summary>
        /// Generates a shuffled deck of cards, virtual so inherieted classes can use different decks
        /// </summary>
        protected virtual void Deckgen()
        {
            Random random = new Random();
            string[] cards = ["2", "3", "4", "5", "6", "7", "8", "9", "0", "J", "Q", "K", "A"];
            string[] houses = ["C", "S", "H", "D"];
            foreach (var card in cards) {
                foreach (var house in houses) {
                    Deck.Add(card + house);
                }
            }
            /// shuffle
            int n = Deck.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                string value = Deck[k];
                Deck[k] = Deck[n];
                Deck[n] = value;
            }

        }
        protected virtual void CardValGen() {
            cardValues = new Dictionary<string, int>();
            string[] cards = ["2", "3", "4", "5", "6", "7", "8", "9", "0", "J", "Q", "K", "A"];
            for (int i = 0; i < 8; i++) {
                cardValues[cards[i]]=Int16.Parse(cards[i]);
            }
            for (int i = 0; i < 4; i++)
            {
                cardValues[cards[i+8]] = 10;
            }
            cardValues.Add(cards.Last(), 11);
        }
        public virtual bool ValidateTurn(string uname) => uname == PlayerHands.Keys.ElementAt(turnMarker);
        public  void IncrementTurn()
        {
            turnMarker = (turnMarker + 1) % PlayerAmount;
            var uname = PlayerHands.ElementAt(turnMarker).Key;
            string message = "urturn";
            var bytes = Encoding.UTF8.GetBytes(message);
            PlayerSockets[uname].SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length),
                        WebSocketMessageType.Text, true, CancellationToken.None);
        }
        public string[] DrawCards(string id,int x) {
            // TODO: add a deck out exception when the player tries to draw more cards than there are in deck
            PlayerHands[id].AddRange(Deck.GetRange(0, x));
            Deck.RemoveRange(0, x);
            return PlayerHands[id].GetRange(PlayerHands[id].Count - x, x).ToArray();
        }
        public async void DiscardCard(string id, string card) {
            if (!PlayerHands[id].Remove(card))
            {
                throw new ArgumentException("ERROR: " + card + " not found in player hand");
            }
            DiscardPile.Add(card);
            await SendAll("discard",card,id);
        }
        public async Task SendAll(string action, string data) {
            string message = action + ":" + data;
            var bytes = Encoding.UTF8.GetBytes(message);
            foreach (var conn in PlayerSockets.Values) {
                if (conn.State == WebSocketState.Open) {
                    await conn.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), 
                        WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
        public async Task SendAll(string action, string data, string exclude_sender)
        {
            string message = action + ":" + data;
            var bytes = Encoding.UTF8.GetBytes(message);
            foreach (var conn in PlayerSockets)
            {
                if (conn.Key!= exclude_sender& conn.Value.State == WebSocketState.Open)
                {
                    await conn.Value.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length),
                        WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}