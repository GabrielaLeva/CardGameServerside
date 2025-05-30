﻿using GamblingServer.Games;
using Microsoft.AspNetCore.SignalR;
using System.Net.WebSockets;
namespace GamblingServer
{
    /// <summary>
    /// Singleton class carrying instances of ongoing games, 
    /// </summary>
    public sealed class InstanceManager
    {
        private Dictionary<Guid,BaseCardgame> cardgames;
        private List<Lobby> lobbies;
        private InstanceManager()
        {
            cardgames= [];
            lobbies = [];
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
        public static Lobby GetLobby(GameType type, string user,WebSocket webSocket)
        {
            Lobby lobby = _instOfSingleton.lobbies.Find(l=>l.type == type);
            if (lobby == null)
            {
                lobby = new Lobby(type, user, webSocket, 2);
                _instOfSingleton.lobbies.Add(lobby);
            }
            else {
                lobby.PlayerJoin(user,webSocket);
            }
            return lobby;
        }
        public static void AddGame(BaseCardgame game,Guid guid) {
            _instOfSingleton.cardgames.Add(guid,game);
        }
        public static void RemoveGame(Guid guid)
        {
            _instOfSingleton.cardgames.Remove(guid);
        }
        public static BaseCardgame GetCardgame(Guid guid) {
            return _instOfSingleton.cardgames[guid];
        }
    }
}
