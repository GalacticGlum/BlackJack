using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Deck
{
    private readonly List<Card> cards;
    private readonly int amount;

    public Deck(int amount = 6)
    {
        cards = new List<Card>();
        this.amount = amount;
        Create();
    }

    private void Create()
    {
        for (int i = 0; i < amount; i++)
        {
            Generate();
        }

        Shuffle();
    }

    private void Generate()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 1; j < 14; j++)
            {
                cards.Add(new Card((CardSuit)i, (CardValue)j));
            }
        }
    }

    public void Shuffle()
    {
        int n = cards.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            Card value = cards[k];
            cards[k] = cards[n];
            cards[n] = value;
        }
    }

    public Card Pop(bool isFaceDown = false)
    {
        if (cards.Count <= 0)
        {
            Create();
        }

        Card result = cards[cards.Count - 1];
        cards.Remove(result);

        result.IsFaceDown = isFaceDown;
        return result;
    }
}