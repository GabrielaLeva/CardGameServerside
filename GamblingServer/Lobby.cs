using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks.Dataflow;
using GamblingServer.Games;
using Pomelo.EntityFrameworkCore.MySql.Storage.Internal;
namespace GamblingServer

{
    public enum GameType
    {
        Whitejack,
        War,
        Poker
    }
    public class Lobby
    {
        public GameType type;
        public Dictionary<string,WebSocket> userConnections;
        public int max_users;
        public Lobby(GameType type, string starter,WebSocket webSocket, int max_users)
        {
            this.type = type;
            userConnections = new Dictionary<string, WebSocket>();
            userConnections[starter] = webSocket;
            this.max_users = max_users;
            var bytes = Encoding.UTF8.GetBytes("Lobby created");
            webSocket.SendAsync( new ArraySegment<byte>(bytes,0,bytes.Length),WebSocketMessageType.Text,true,CancellationToken.None);   
        }
        public void PlayerJoin(string user, WebSocket webSocket)
        {
            userConnections.Add(user, webSocket);
            var bytes = Encoding.UTF8.GetBytes("User "+user+" joined the lobby.");
            foreach (var connection in userConnections.Values) {
                webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            }
            if (userConnections.Count >= max_users)
            {
                MakeGame();
            }
        }
        private static async Task GameDispatcher(WebSocket webSocket, string user, WhiteJack31 game)
        {
            var buffer = new byte[1024 * 4];
            var message = string.Join(",",game.PlayerHands[user]);
            var bytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes, 0, bytes.Length),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            while (webSocket.State != WebSocketState.Closed && webSocket.State != WebSocketState.Aborted)
            {
                var user_action = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                switch (Encoding.UTF8.GetString(buffer, 0, user_action.Count))
                {
                    case "take":
                        game.PlayerHands[user].Add(game.DiscardPile.Last());
                        message = game.PlayerHands[user][3];
                        bytes = Encoding.UTF8.GetBytes(message);
                        await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes, 0, bytes.Length),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
                        user_action = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                        game.DiscardCard(user, Encoding.UTF8.GetString(buffer, 0, user_action.Count));
                        game.CheckWincons();
                        break;
                    case "draw":
                        game.DrawCards(user, 1);
                        message = game.PlayerHands[user][3];
                        bytes = Encoding.UTF8.GetBytes(message);
                        await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes, 0, bytes.Length),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
                        user_action = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                        game.DiscardCard(user, Encoding.UTF8.GetString(buffer, 0, user_action.Count));
 
                        game.CheckWincons();
                        break;
                    case "knock":
                        game.setKnock(user);
                        game.IncrementTurn();
                        break;
                    case "stand":
                        game.CheckWincons();
                        break;
                    default:
                        break;
                }
            }
        }
        public async void MakeGame()
        {
            var gameconns = userConnections.Take(max_users).ToDictionary();
            var game_guid = Guid.NewGuid();
            switch (type) {
                case GameType.Whitejack:
                    var game = new WhiteJack31(gameconns.Keys.ToArray(), game_guid);
                    game.PlayerSockets = gameconns;
                    InstanceManager.AddGame(game,game_guid);
                    await game.SendAll("guid", game_guid.ToString());
                    await game.SendAll("starter", game.PlayerSockets.ElementAt(0).Key);
                    foreach(var c in gameconns)
                    {
                        GameDispatcher(c.Value, c.Key,game);
                    }
                    break;
                default:
                    Console.WriteLine("Not Implemented");
                    break;
            }
            foreach(var user in gameconns.Keys)
            {
                userConnections.Remove(user);
            }
        }
    }
}
