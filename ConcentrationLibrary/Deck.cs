using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ConcentrationLibrary
{
    [DataContract]
    public class Deck
    {
        [DataMember] private List<Card> cards;   // The deck of cards
        [DataMember] private int cardIdx;        // The next index to draw a card from

        // Constructor
        public Deck() => Repopulate();

        // Draw a card from the deck
        public Card Draw() => cards[cardIdx++];

        // Repopulate the deck of cards
        public void Repopulate() {

            // Create a new deck if there is not one, or clear the current deck
            if (cards == null)
                cards = new List<Card>();
            else
                cards.Clear();

            // Populate the deck with one of each card (52 total)
            foreach (Card.SuitID s in Enum.GetValues(typeof(Card.SuitID)))
                foreach (Card.RankID r in Enum.GetValues(typeof(Card.RankID)))
                    if (s == Card.SuitID.D || s == Card.SuitID.H)
                        cards.Add(new Card(s, r, Card.ColorID.Red));
                    else
                        cards.Add(new Card(s, r, Card.ColorID.Black));

            // Randomize the deck
            Shuffle();
        }

        // Private Helper Method to shuffle the deck
        private void Shuffle() {
            Random rng = new Random();
            cards = cards.OrderBy(number => rng.Next()).ToList();
            cardIdx = 0;
        }

    }
}
