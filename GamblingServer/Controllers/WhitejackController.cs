using GamblingServer.Games;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net.WebSockets;
using System.Text;

namespace GamblingServer.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class WhitejackController : ControllerBase
    {
        [HttpGet(Name = "GetHand")]
        public List<string> GetHand(int id)
        {
            return InstanceManager.GetCardgame(0).PlayerHands[id];
        }
        [HttpPost(Name = "DiscardAndDraw")]
        public ActionResult Discarddraw(int id, string card )
        {
            ContentResult res;
            WhiteJack31 whiteJack31 = InstanceManager.GetCardgame(0) as WhiteJack31;
            try
            {
                if (whiteJack31.ValidateTurn(id))
                {
                    whiteJack31.DiscardCard(id, card);
                    whiteJack31.DrawCards(id, 1);
                    whiteJack31.IncrementTurn();
                    return Content(whiteJack31.PlayerHands[id][2]);
                }
            }
            catch (ArgumentException)
            {
                res = Content("Invalid card or player");
                res.StatusCode=400;
                return res;
            }
            res= Content("Trun order violation");
            res.StatusCode = 400;
            return res;
        }
        [Route("/ws")]
        public async Task Get(string guid)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await WaitForOpponent(webSocket);
                if (webSocket.State == WebSocketState.Open)
                {
                    var uname= HttpContext.User.Identity.Name;
                    GameDispatcher(webSocket,uname);
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private static async Task WaitForOpponent(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var message = "Waiting for the opponent";
            var bytes = Encoding.UTF8.GetBytes(message);
            while (webSocket.State!=WebSocketState.Closed && webSocket.State!=WebSocketState.Aborted)
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(bytes, 0, bytes.Length),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);

            }
        }
        private static async Task GameDispatcher(WebSocket webSocket,string user)
        {
            WhiteJack31 game = InstanceManager.GetCardgame(0) as WhiteJack31;
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
