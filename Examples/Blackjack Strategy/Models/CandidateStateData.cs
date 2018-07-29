using System.Collections.Generic;

namespace BlackjackStrategy.Models
{
    class ProblemState
    {
        // Before evaluating a candidate, we load the state data with the current game situation, which is the current hands
        public Hand PlayerHand{ get; set; }
        public Hand DealerHand { get; set; }    // .Cards[0] is the upcard

        // Each candidate expression tree will have terminal functions that indicate a strategy (stand, hit, etc.)  
        // Since a given candidate could potentially have several of these in the tree, each time such a terminal function 
        // is executed, it's considered a vote for a strategy.  When the tree is completely evaluated, we examine the votes
        // to determine the final strategy to use
        public int VotesForHit { get; set; }
        public int VotesForStand { get; set; }
        public int VotesForDoubleDown { get; set; }
    }
}
