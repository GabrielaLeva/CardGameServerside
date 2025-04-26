using GamblingServer.Games;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        public string Discarddraw(int id, string card )
        {
            WhiteJack31 whiteJack31 = InstanceManager.GetCardgame(0) as WhiteJack31;
            try
            {
                if (whiteJack31.ValidateTurn(id))
                {
                    whiteJack31.DiscardCard(id, card);
                    whiteJack31.DrawCards(id, 1);
                    whiteJack31.IncrementTurn();
                    return whiteJack31.PlayerHands[id][2];
                }
            }
            catch (ArgumentException)
            {
                this.HttpContext.Response.StatusCode = 400;
                return "Invalid card or player";
            }
            return "turn violation";
        }
        public void Knock()
        {
            WhiteJack31 whiteJack31 = InstanceManager.GetCardgame(0) as WhiteJack31;
            if(whiteJack31.setKnock())
        }
    }
}
