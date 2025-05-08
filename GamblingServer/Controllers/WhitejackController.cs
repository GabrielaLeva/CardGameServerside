using GamblingServer.Games;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;


namespace GamblingServer.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
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
        /*
        [Route("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await Echo(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private static async Task Echo(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                    receiveResult.MessageType,
                    receiveResult.EndOfMessage,
                    CancellationToken.None);

                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }
        public void Knock()
        {
            WhiteJack31 whiteJack31 = InstanceManager.GetCardgame(0) as WhiteJack31;
            //if(whiteJack31.setKnock())
        }*/
    }
}
