namespace BlackjackStrategy.Models
{
    class TestConditions
    {
        public const int NumDecks = 2;
        public const int NumHandsToPlay = 500;
        public const int BetSize = 2;
        public const int BlackjackPayoffSize = 3;   // if you have a blackjack, most casinos pay off 3:2

        public enum GameState
        {
            PlayerBlackjack,
            PlayerDrawing,
            PlayerBusted,
            DealerDrawing,
            DealerBusted,
            HandComparison
        }
    }
}
