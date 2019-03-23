// Names:   Jordan Travaux & Abel Emun
// Date:    March 20, 2019
// Purpose: Definition of a deck of cards to be used for the game

using System;
using System.Collections.Generic;
using System.Linq;

namespace ConcentrationLibrary
{
    public class Deck
    {
        private List<Card> cards;   // The deck of cards
        private int index;         // The next index to draw a card from

        // Constructor
        public Deck()
        {
            // Create a new deck of cards
            cards = new List<Card>();

            // Populate the deck with one of each card (52 total)
            foreach (Card.SuitID s in Enum.GetValues(typeof(Card.SuitID)))
                foreach (Card.RankID r in Enum.GetValues(typeof(Card.RankID)))
                    if (s == Card.SuitID.D || s == Card.SuitID.H)
                        cards.Add(new Card(s, r, Card.ColorID.Red));
                    else
                        cards.Add(new Card(s, r, Card.ColorID.Black));

            // Randomize the deck
            Random rng = new Random();
            cards = cards.OrderBy(number => rng.Next()).ToList();
            index = 0;
        }

        // Draw a card from the deck
        public Card Draw() => cards[index++];
    }
}
