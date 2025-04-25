using GamblingServer.Games;
namespace GamblingServer
{
    /// <summary>
    /// Singleton class carrying instances of ongoing games, 
    /// </summary>
    public sealed class InstanceManager
    {
        private List<BaseCardgame> cardgames;

        private InstanceManager()
        {
            
            cardgames= [];
            cardgames.Add(new WhiteJack31([1,2]));
        }

        private static readonly Object _objLock = new Object();
        private static InstanceManager _instOfSingleton;

        public static InstanceManager GetInstance()
        {
            if (_instOfSingleton != null)
                return _instOfSingleton;

            Monitor.Enter(_objLock); 
            if (_instOfSingleton == null)
            {
                InstanceManager tempInstance = new InstanceManager();

                Volatile.Write(ref _instOfSingleton, tempInstance);
            }
            Monitor.Exit(_objLock);

            return _instOfSingleton;
        }
        public static BaseCardgame GetCardgame(int idx) {
            return _instOfSingleton.cardgames[idx];
        }
    }
}
