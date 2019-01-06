using System;
using System.Collections.Generic;

namespace BlackjackStrategy.Models
{
    class ProblemState
    {
        // Before evaluating a candidate, we load the state data with the current hand and dealer card (all we know)
        public Hand PlayerHand { get; set; }
        public Card DealerUpcard { get; set; }
       
        // Each candidate expression tree will have stateful functions that indicate a strategy (stand, hit, etc.)  
        // Since a given candidate could potentially have several of these in the tree, each time such a function 
        // is executed, it's considered a vote for a strategy.  When the tree is completely evaluated, we examine the votes
        // to determine the final strategy to use
        public int VotesForHit { get; set; }
        public int VotesForStand { get; set; }
        public int VotesForDoubleDown { get; set; }
        public int VotesForSplit { get; set; }
    }
}
