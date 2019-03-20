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
        [DataMember] private int index;         // The next index to draw a card from

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
