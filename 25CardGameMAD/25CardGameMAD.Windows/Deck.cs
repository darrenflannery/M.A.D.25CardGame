using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _25CardGameMAD
{
    public class Deck
    {

        private Card[] deck;
        private int currentCard;
        private const int NUMBER_OF_CARDS = 52;
        private Random ranNum;
        private int suitValue;
        private int faceValue;


        public Deck()
        {
            string[] faces = { "Ace", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Jack", "Queen", "King" };
            string[] suits = { "Hearts", "Clubs", "Diamonds", "Spades" };
            deck = new Card[NUMBER_OF_CARDS];
            currentCard = 0;
            ranNum = new Random();

            for (int count = 0; count < deck.Length; count++)
            {
                suitValue = (count / 13) + 1;
                faceValue = (count % 13) + 1;
                //Creates a deck assining each value of deck[] a number and a suit
                // % - Gets remainder from each number divided by 13 ---  so 4 sets of 1 - 13 is created.
                //the last paramter counts the number of the card in the deck to match to the image
                deck[count] = new Card(faces[count % 13], suits[count / 13], count + 1, suitValue, faceValue);


            }
        }

        //goes thorugh each card in deck[1 - 52] and swaps it with a random card in the deck
        public void Shuffle()
        {
            int j = 0;
            currentCard = 0;
            for (int i = 0; i < deck.Length; i++)
            {
                j = ranNum.Next(NUMBER_OF_CARDS);   //assign random number to j  (1-52)
                Card temp = deck[i];                    // assign current card to temp
                deck[i] = deck[j];                     //current card in deck is given random card (number j)
                deck[j] = temp;                        //the card in position j is swapped with the current card - so there are no duplicates
            }

        }

        //Deals first card of deck ---- deck[0]  - (Ace of Hearts(unless shuffled)) - Will run through deck until shuffled again
        public Card DealCard()
        {
            //ensures within deck - below 52
            if (currentCard < deck.Length)
            {
                return deck[currentCard++];
            }

            else
            {
                return null;
            }
        }
    }
}
