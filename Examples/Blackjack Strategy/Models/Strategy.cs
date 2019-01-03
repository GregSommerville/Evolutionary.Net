using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackjackStrategy.Models
{
    // encapsulates one complete strategy to play Blackjack
    class OverallStrategy
    {
        // how to play pairs, key: "77 A" (meaning, a pair of 7s with a dealer upcard of an ace)
        private Dictionary<string, ActionToTake> pairsStrategy = new Dictionary<string, ActionToTake>();

        // how to play soft hands, key: "A33 6" (Ace-three-three in hand, with dealer upcard of six)
        private Dictionary<string, ActionToTake> softStrategy = new Dictionary<string, ActionToTake>();

        // how to play hard hands, keyed by the hand total and upcard: "17 3"
        private Dictionary<string, ActionToTake> hardStrategy = new Dictionary<string, ActionToTake>();

        //-------------------------------------------------------------------------------------------

        public void AddPairStrategy(string pairRank, string dealerUpcard, ActionToTake action)
        {
            pairsStrategy[pairRank + " " + dealerUpcard] = action;
        }
        public void AddSoftStrategy(string hand, string dealerUpcard, ActionToTake action)
        {
            softStrategy[hand + " " + dealerUpcard] = action;
        }
        public void AddHardStrategy(int handTotal, string dealerUpcard, ActionToTake action)
        {
            hardStrategy[handTotal + " " + dealerUpcard] = action;
        }

        //-------------------------------------------------------------------------------------------

        public ActionToTake GetAction(string hand, string dealerUpcard)
        {
            // if it's a pair, then 
            //return pairsStrategy[hand + " " + dealerUpcard];
            throw new NotImplementedException();
        }
    }
}
