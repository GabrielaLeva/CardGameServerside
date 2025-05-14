using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
        public List<string> users;
        public int max_users;
        public Lobby(GameType type, string starter, int max_users)
        {
            this.type = type;
            users = [starter];
            this.max_users = max_users;
        }
        public void PlayerJoin(string user)
        {
            users.Add(user);
        }
        public void MakeGame()
        {
            switch (type) {
                case GameType.Whitejack:
                    InstanceManager.AddGame(new WhiteJack31(users.GetRange(users.GetRange(0, max_users));
                    break;
                default:
                    Console.WriteLine("Not Implemented");
                    break;
            }
        }
    }
}
