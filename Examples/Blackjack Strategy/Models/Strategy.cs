using System.Collections.Generic;
using System.Linq;

namespace BlackjackStrategy.Models
{
    // encapsulates one complete strategy to play Blackjack
    class OverallStrategy
    {
        private Dictionary<string, ActionToTake> pairsStrategy = new Dictionary<string, ActionToTake>();
        private Dictionary<string, ActionToTake> softStrategy = new Dictionary<string, ActionToTake>();
        private Dictionary<string, ActionToTake> hardStrategy = new Dictionary<string, ActionToTake>();

        //-------------------------------------------------------------------------------------------

        public void AddPairStrategy(string pairRank, string dealerUpcardRank, ActionToTake action)
        {
            pairsStrategy[pairRank + dealerUpcardRank] = action;
        }

        public void AddSoftStrategy(string secondaryCardRank, string dealerUpcardRank, ActionToTake action)
        {
            // secondary rank is the non-Ace
            softStrategy[secondaryCardRank + dealerUpcardRank] = action;
        }

        public void AddHardStrategy(int handTotal, string dealerUpcardRank, ActionToTake action)
        {
            // handTotal goes from 5 (since a total of 4 means a pair of 2s) to 20
            hardStrategy[handTotal + dealerUpcardRank] = action;
        }

        //-------------------------------------------------------------------------------------------

        public ActionToTake GetAction(Hand hand, string dealerUpcardRank)
        {
            if (hand.IsPair())
            {
                string rank = hand.Cards[0].Rank;
                return pairsStrategy[rank + dealerUpcardRank];
            }

            if (hand.HasSoftAce())
            {
                string secondaryCardRank = hand.Cards
                    .Single(c => c.Rank != "A")
                    .Rank;
                return softStrategy[secondaryCardRank + dealerUpcardRank];
            }

            return hardStrategy[hand.HandValue() + dealerUpcardRank];
        }
    }
}
