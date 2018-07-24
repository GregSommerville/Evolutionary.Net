namespace BlackjackStrategy.Models
{
    class ProblemState
    {
        // Before evaluating a candidate, we load the state data with the curren game situation - player cards and dealer upcard
        public Card Card1 { get; set; }
        public Card Card2 { get; set; }
        public Card DealerCard { get; set; }

        // Each candidate expression tree will have terminal functions that indicate a strategy (stand, hit, etc.)  
        // Since a given candidate could potentially have several of these in the tree, each time such a terminal function 
        // is executed, it's considered a vote for a strategy.  When the tree is completely evaluated, we examine the votes
        // to determine the final strategy to use
        public int VotesForHit { get; set; }
        public int VotesForStand { get; set; }
        public int VotesForDoubleDown { get; set; }
    }
}
