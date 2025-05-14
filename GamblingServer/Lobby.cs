using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Net.WebSockets;
using System.Threading.Tasks.Dataflow;
using GamblingServer.Games;
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
            userConnections= new Dictionary<string, WebSocket>() { {starter,webSocket } };
            this.max_users = max_users;
        }
        public void PlayerJoin(string user, WebSocket webSocket)
        {
            userConnections.Add(user, webSocket);
            if (userConnections.Count >= max_users)
            {
                MakeGame();
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
