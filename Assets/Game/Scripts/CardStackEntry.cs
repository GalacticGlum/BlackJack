using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CardStackEntry : MonoBehaviour
{
    public Card Card { get; private set; }
    public CardStackController CardStack { get; private set; }
    private const float TurnOverDuration = 0.4f;

    private bool isTurnOverDone;
    private LerpInformation<Quaternion> cardTurnOverLerpInformation;

    private void Update()
    {
        if (cardTurnOverLerpInformation == null) return;
        if (!isTurnOverDone && cardTurnOverLerpInformation.TimeLeft <= 0)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.flipX = true;
            spriteRenderer.sprite = Card.FaceUpSprite;

            Vector3 eulerAngles = transform.rotation.eulerAngles;
            eulerAngles.y = -180;

            cardTurnOverLerpInformation = new LerpInformation<Quaternion>(transform.rotation, Quaternion.Euler(eulerAngles), TurnOverDuration / 2f, Quaternion.Lerp);
            isTurnOverDone = true;
        }

        transform.rotation = cardTurnOverLerpInformation.Step(Time.deltaTime);
    }

    private void OnMouseDown()
    {
        if (GameController.Instance.IsPaused || GameController.Instance.IsGameOver) return;
        CardStack.Zoom();
    }

    public void TurnOver()
    {
        if (!Card.IsFaceDown) return;
        Card.IsFaceDown = false;
        isTurnOverDone = false;

        Vector3 eulerAngles = transform.rotation.eulerAngles;
        eulerAngles.y = -90;

        cardTurnOverLerpInformation = new LerpInformation<Quaternion>(transform.rotation, Quaternion.Euler(eulerAngles), TurnOverDuration / 2f, Quaternion.Lerp);
    }

    public static void Attach(GameObject cardGameObject, Card card, CardStackController cardStack)
    {
        if (cardGameObject == null || cardStack == null) return;

        CardStackEntry cardStackEntry = cardGameObject.AddComponent<CardStackEntry>();
        cardStackEntry.CardStack = cardStack;
        cardStackEntry.Card = card;
    }
}