using GamblingServer.Games;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net.WebSockets;
using System.Text;

namespace GamblingServer.Controllers
{

    [Route("/whitejack")]
    [ApiController]
    public class WhitejackController : ControllerBase
    {
        
        [Route("/fetchgameid")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {

                var uname = HttpContext.User.Identity.Name;
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                var lobby = InstanceManager.GetLobby(GameType.Whitejack,uname,webSocket);
                await WaitForOpponent(uname,webSocket,lobby);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        [Route("/game")]
        public async Task Game(string guid)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {

                var uname = HttpContext.User.Identity.Name;
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                WhiteJack31 game = InstanceManager.GetCardgame(Guid.Parse(guid)) as WhiteJack31;
                await GameDispatcher(webSocket,uname,game);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private static async Task WaitForOpponent(string uname,WebSocket webSocket,Lobby lobby)
        {
            var buffer = new byte[1024 * 4];
            var message = "Waiting for the opponent";
            var bytes = Encoding.UTF8.GetBytes(message);
            while (webSocket.State!=WebSocketState.Closed && webSocket.State!=WebSocketState.Aborted && lobby.userConnections.ContainsKey(uname))
            {
                message = "Players in lobby"+lobby.userConnections.Count.ToString();
                await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes, 0, bytes.Length),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
                Thread.Sleep(1000);

            }
        }
        private static async Task GameDispatcher(WebSocket webSocket,string user,WhiteJack31 game)
        {
            var buffer = new byte[1024 * 4];
            var message = game.PlayerHands[user].ToString();
            var bytes = Encoding.UTF8.GetBytes(message);
            while (webSocket.State != WebSocketState.Closed && webSocket.State != WebSocketState.Aborted)
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes, 0, bytes.Length),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
                var user_action = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                switch (Encoding.UTF8.GetString(buffer,0,user_action.Count))
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
                        user_action = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                        message = game.PlayerHands[user][3];
                        bytes = Encoding.UTF8.GetBytes(message);
                        await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes, 0, bytes.Length),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
                        game.DiscardCard(user, Encoding.UTF8.GetString(buffer, 0, user_action.Count));
                        game.CheckWincons();
                        break;
                    case "knock":
                        game.setKnock(user);
                        game.IncrementTurn();
                        break;
                    case "stand":
                        game.IncrementTurn();
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
