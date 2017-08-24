public class CardStackValue
{
    public int NumberValue { get; private set; }

    public CardStackValue(params Card[] cards)
    {
        foreach (Card card in cards)
        {
            Add(card);
        }
    }

    public void Add(Card card)
    {
        if (card.IsFaceDown) return;
        switch (card.Value)
        {
            case CardValue.Ace:
                int sum = NumberValue + 11;
                if (sum > 21)
                {
                    sum = NumberValue + 1;
                }

                NumberValue = sum;
                break;          
            case CardValue.Jack:
                NumberValue += 10;
                break;
            case CardValue.Queen:
                NumberValue += 10;
                break;
            case CardValue.King:
                NumberValue += 10;
                break;
            default:
                NumberValue += (int) card.Value;
                break;
        }
    }
}