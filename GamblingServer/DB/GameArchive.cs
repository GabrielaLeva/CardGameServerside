namespace GamblingServer.DB
{
    public enum GameResult
    {
        Loss,
        Draw,
        Win
    }
    
    public class GameArchive
    {
        public Guid game_id;
        public GameResult Result;
        public GameType Type;
        public string username;
        public int? score;
    }
}
