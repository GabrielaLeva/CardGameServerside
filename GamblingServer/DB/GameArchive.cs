namespace GamblingServer.DB
{
    public enum GameResult
    {
        Loss,
        Draw,
        Win
    }
    public enum GameType
    {
        Whitejack,
        War,
        Poker
    }
    public class GameArchive
    {
        public int id;
        public GameResult Result;
        public GameType Type;
        public string Nickname;
        public int score;
    }
}
