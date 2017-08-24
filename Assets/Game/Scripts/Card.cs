using System.Collections.Generic;
using UnityEngine;

public class Card 
{
    public CardSuit Suit { get; }
    public CardValue Value { get; }
    public bool IsFaceDown { get; set; }

    public Sprite FaceUpSprite => Resources.Load<Sprite>($"Sprites/card_b_{CardSuitIdentifier[Suit]}{CardIdentifier[Value]}_large");
    public Sprite FaceDownSprite => Resources.Load<Sprite>($"Sprites/card_facedown_4");
    public Sprite Sprite => IsFaceDown ? FaceDownSprite : FaceUpSprite;

    /// <summary>
    /// The identifier which represents the specific card suit when loading the card sprite.
    /// </summary>
    private static readonly Dictionary<CardSuit, string> CardSuitIdentifier = new Dictionary<CardSuit, string>
    {
        {CardSuit.Diamond, "d"},
        {CardSuit.Club, "c"},
        {CardSuit.Heart, "h"},
        {CardSuit.Spade, "s"}
    };

    /// <summary>
    /// The identifier which represents the specific card value when loading the card sprite.
    /// </summary>
    private static readonly Dictionary<CardValue, string> CardIdentifier = new Dictionary<CardValue, string>
    {
        {CardValue.Ace,  "a"},
        {CardValue.Two,  "2"},
        {CardValue.Three,  "3"},
        {CardValue.Four,  "4"},
        {CardValue.Five,  "5"},
        {CardValue.Six,  "6"},
        {CardValue.Seven,  "7"},
        {CardValue.Eight,  "8"},
        {CardValue.Nine,  "9"},
        {CardValue.Ten,  "10"},
        {CardValue.Jack,  "j"},
        {CardValue.Queen,  "q"},
        {CardValue.King,  "k" }
    };

    public Card(CardSuit suit, CardValue value, bool isFaceDown = false)
    {
        Suit = suit;
        Value = value;
        IsFaceDown = isFaceDown;
    }

    public GameObject Create()
    {
        GameObject cardGameObject = new GameObject("Card", typeof(SpriteRenderer));
        SpriteRenderer spriteRenderer = cardGameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = Sprite;
        spriteRenderer.sortingLayerName = "Cards";

        return cardGameObject;
    }

    /// <summary>
    /// Generate a random card.
    /// </summary>
    public static Card Random(bool isFaceDown = false)
    {
        return new Card(EnumHelper.RandomEnumValue<CardSuit>(), EnumHelper.RandomEnumValue<CardValue>(), isFaceDown);
    }
}