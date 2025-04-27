using GamblingServer.Games;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

namespace GamblingServer.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class WhitejackController : ControllerBase
    {
        WhiteJack31 whiteJack31;
        public List<string> GetHand(int id)
        {
            return whiteJack31.PlayerHands[id];
        }
        
        [Route("/ws")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                string playerID= HttpContext.Request.Query["id"]; //placeholder with bad security
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                whiteJack31.connections.Add(webSocket);

                string msg= "hand:"+String.Join(",",GetHand(Int16.Parse(playerID)));
                var bytes = Encoding.UTF8.GetBytes(msg);

                await whiteJack31.SendAll("playerjoined", playerID);
                await webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length),
                        WebSocketMessageType.Text, true, CancellationToken.None);
                await GameActionDispatcher(webSocket, Int16.Parse(playerID));
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        private  async Task GameActionDispatcher(WebSocket webSocket,int id)
        {
            var buffer = new byte[1024 * 4];
            while (webSocket.State==WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
                string gameaction=Encoding.UTF8.GetString(buffer,0,result.Count);
                string msg = "";
                var action=gameaction.Split(':');
                switch (action[0].Trim())
                {
                    case "discard":
                        msg="card:"+whiteJack31.Discarddraw(id,action[1].Trim());
                        var bytes = Encoding.UTF8.GetBytes(msg);
                        await webSocket.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length),
                        WebSocketMessageType.Text, true, CancellationToken.None);
                        break;
                    case "knock":
                        whiteJack31.setKnock(id);
                        break;
                    case "pass":
                        whiteJack31.IncrementTurn();
                        break;
                    default:
                        break;
                }
            }
        }
        public WhitejackController() {
            whiteJack31 = InstanceManager.GetCardgame(0) as WhiteJack31;
        }
    }
}
