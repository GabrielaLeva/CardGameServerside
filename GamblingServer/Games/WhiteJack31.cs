using System.Security.Cryptography.Xml;

namespace GamblingServer.Games
{
    public class WhiteJack31:BaseCardgame
    {
        public WhiteJack31(int[] ids): base(ids) {
            foreach (int id in ids) {
                DrawCards(id, 3);
            }
            knock_indicator = -1;
        }
        protected int knock_indicator;
        public override bool ValidateTurn(int id)
        {
            if (knock_indicator == id) {
                return false;
            }
            return base.ValidateTurn(id);
        }
        public void setKnock(int knocker) {
        knock_indicator = knocker;
        }
    }
}
