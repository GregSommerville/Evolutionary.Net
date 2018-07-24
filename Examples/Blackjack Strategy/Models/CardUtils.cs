using System;
using System.Collections.Generic;
using System.Linq;

namespace BlackjackStrategy.Models
{
    class Card
    {
        public string Rank { get; set; }
        public string Suit { get; set; }

        public enum Suits {  Hearts, Spades, Clubs, Diamonds};
        public enum Ranks {  Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace};

        public int RankValueHigh
        {
            // For straights where A is treated as above the King
            get
            {
                switch (Rank)
                {
                    case "A":
                        return 14;
                    case "K":
                        return 13;
                    case "Q":
                        return 12;
                    case "J":
                        return 11;
                    case "T":
                        return 10;
                    default:
                        return Convert.ToInt32(Rank);
                }
            }
        }

        public int RankValueLow
        {
            // For straights where A is treated as below the 2
            get
            {
                switch (Rank)
                {
                    case "A":
                        return 1;
                    case "K":
                        return 13;
                    case "Q":
                        return 12;
                    case "J":
                        return 11;
                    case "T":
                        return 10;
                    default:
                        return Convert.ToInt32(Rank);
                }
            }
        }

        public Card(string rank, string suit)
        {
            Rank = rank;
            Suit = suit;
        }

        public Card(string combined)
        {
            Rank = combined[0].ToString();
            Suit = combined[1].ToString();
        }

        public Card(Ranks rankValue, string suit)
        {
            var rankChars = "  23456789TJQKA".ToCharArray();
            Rank = rankChars[(int)rankValue].ToString();
            Suit = suit;
        }

        public Card(Ranks rankValue, Suits suit)
        {
            var rankChars = "  23456789TJQKA".ToCharArray();
            var suitChars = "HSCD";
            Rank = rankChars[(int)rankValue].ToString();
            Suit = suitChars[(int)suit].ToString();
        }

        public static string RankText(int rank)
        {
            var rankChars = "  23456789TJQKA".ToCharArray();
            return rankChars[rank].ToString();
        }

        public static string RankDescription(int rank, bool pluralForm = true)
        {
            string suffix = "";
            if (pluralForm)
            {
                suffix = "s";
                if (rank == 6) suffix = "es";
            }

            switch (rank)
            {
                case 14:
                    return "Ace" + suffix;
                case 13:
                    return "King" + suffix;
                case 12:
                    return "Queen" + suffix;
                case 11:
                    return "Jack" + suffix;
                case 10:
                    return "Ten" + suffix;
                case 9:
                    return "Nine" + suffix;
                case 8:
                    return "Eight" + suffix;
                case 7:
                    return "Seven" + suffix;
                case 6:
                    return "Six" + suffix;
                case 5:
                    return "Five" + suffix;
                case 4:
                    return "Four" + suffix;
                case 3:
                    return "Three" + suffix;
                case 2:
                    return "Two" + suffix;
            }
            return "";
        }

        public override string ToString()
        {
            return Rank + Suit;
        }
    }

    class CardUtils
    {
        static public List<Card> GetRandomDeck()
        {
            // initially populate
            List<Card> deck = new List<Card>(52);
            foreach (Card.Ranks rank in Enum.GetValues(typeof(Card.Ranks)))
                foreach (Card.Suits suit in Enum.GetValues(typeof(Card.Suits)))
                {
                    var card = new Card(rank, suit);
                    deck.Add(card);
                }

            // then shuffle using Fisher-Yates: one pass through, swapping the current card with a random one below it
            for (int i = 51; i > 1; i--)
            {
                int swapWith = Randomizer.IntLessThan(i);

                Card hold = deck[i];
                deck[i] = deck[swapWith];
                deck[swapWith] = hold;
            }

            return deck;
        }

        static public List<Card> GetRandomCards(int numCards)
        {
            // optimized way of getting a full deck
            if (numCards == 52)
                return GetRandomDeck();

            string suits = "HSCD";
            string ranks = "23456789TJQKA";

            List<Card> cards = new List<Card>(numCards);
            string suit, rank;
            while (cards.Count < numCards)
            {
                // Generate a card and make sure we don't already have it 
                do
                {
                    suit = suits[Randomizer.IntLessThan(4)].ToString();
                    rank = ranks[Randomizer.IntLessThan(13)].ToString();
                } while (cards.Any(c => c.Rank == rank && c.Suit == suit));

                cards.Add(new Card(rank, suit));
            }
            return cards;
        }

    }
}
