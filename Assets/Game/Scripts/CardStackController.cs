using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public delegate void ZoomChangedEventHandler(object sender, CardStackEventArgs args);
public delegate void CardAddedEventHandler(object sender, CardAddedEventArgs args);
public delegate void CardValueChangedEventHandler(object sender, ValueChangedEventArgs args);

public class CardAddedEventArgs : EventArgs 
{
    public CardStackController CardStack { get; }
    public Card Card { get; }

    public CardAddedEventArgs(CardStackController cardStack, Card card)
    {
        CardStack = cardStack;
        Card = card;
    }
}

public class CardStackEventArgs : EventArgs
{
    public CardStackController CardStack { get; }
    public CardStackEventArgs(CardStackController cardStack)
    {
        CardStack = cardStack;
    }
}

public class CardStackController : MonoBehaviour
{
    public const float ZoomLerpDuration = 0.25f;

    public int Count => cardGameObjects.Count;
    public CardStackValue Value => new CardStackValue(cards.ToArray());
    public bool HasNaturalBlackJack => cards.Count == 2 && Value.NumberValue == 21;
    public bool HasThreeOrMoreBlackJack => Value.NumberValue == 21 && cards.Count > 2;
    public bool HasAnyBlackJack => HasNaturalBlackJack || HasThreeOrMoreBlackJack;

    public bool HasBust => Value.NumberValue > 21;

    public bool IsZoomInPlace { get; private set; }
    public bool IsZoomOutInPlace { get; private set; } = true;
    public bool IsZoomed { get; private set; }

    public event ZoomChangedEventHandler ZoomedIn;
    public event ZoomChangedEventHandler ZoomedOut;
    public event CardAddedEventHandler CardAdded;
    public event CardValueChangedEventHandler ValueChanged;

    private List<Card> cards;
    private List<GameObject> cardGameObjects;
    private Dictionary<int, LerpInformation<Vector3>> cardMovementLerpInformations;

    private Dictionary<GameObject, Vector3> positionsBeforeZoom;
    private Dictionary<GameObject, Quaternion> rotationsBeforeZoom;

    private Dictionary<GameObject, LerpInformation<Vector3>> zoomMovementLerpInformations;
    private Dictionary<GameObject, LerpInformation<Quaternion>> zoomRotationLerpInformations;

    private LerpInformation<Vector3> zoomScaleLperInformation;

    private void OnEnable()
    {
        cards = new List<Card>();
        cardGameObjects = new List<GameObject>();
        cardMovementLerpInformations = new Dictionary<int, LerpInformation<Vector3>>();

        zoomMovementLerpInformations = new Dictionary<GameObject, LerpInformation<Vector3>>();
        zoomRotationLerpInformations = new Dictionary<GameObject, LerpInformation<Quaternion>>();

        positionsBeforeZoom = new Dictionary<GameObject, Vector3>();
        rotationsBeforeZoom = new Dictionary<GameObject, Quaternion>();
    }

    private void Awake()
    {
        GameController.Instance.Started += (sender, args) =>
        {
            Clear();
        };
    }

    private void Update()
    {
        for (int i = cardMovementLerpInformations.Count - 1; i >= 0; i--)
        {
            HandleCardMovement(cardMovementLerpInformations.Keys.ElementAt(i));
        }

        HandleZoom();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ZoomOut();
        }
    }

    private void HandleCardMovement(int index)
    {
        if (cardMovementLerpInformations[index].TimeLeft <= 0)
        {
            cardMovementLerpInformations.Remove(index);
            return;
        }

        cardGameObjects[index].transform.position = cardMovementLerpInformations[index].Step(Time.deltaTime);
    }

    private void HandleZoom()
    {
        if (zoomScaleLperInformation == null || zoomScaleLperInformation.TimeLeft <= 0)
        {
            zoomScaleLperInformation = null;
        }
        else
        {
            transform.localScale = zoomScaleLperInformation.Step(Time.deltaTime);
        }

        foreach (GameObject cardGameObject in cardGameObjects)
        {
            if (!zoomMovementLerpInformations.ContainsKey(cardGameObject)) continue;
            if (zoomMovementLerpInformations[cardGameObject] == null ||
                zoomMovementLerpInformations[cardGameObject].TimeLeft <= 0)
            {
                if (zoomMovementLerpInformations[cardGameObject]?.TimeLeft <= 0 && !IsZoomed)
                {
                    cardGameObject.GetComponent<SpriteRenderer>().sortingLayerName = "Cards";
                    IsZoomOutInPlace = true;
                }
                else if (IsZoomed)
                {
                    IsZoomInPlace = true;
                }

                zoomMovementLerpInformations[cardGameObject] = null;
            }
            else
            {
                cardGameObject.transform.position = zoomMovementLerpInformations[cardGameObject].Step(Time.deltaTime);
            }

            if (zoomRotationLerpInformations[cardGameObject] == null ||
                zoomRotationLerpInformations[cardGameObject].TimeLeft <= 0)
            {
                zoomRotationLerpInformations[cardGameObject] = null;
            }
            else
            {
                cardGameObject.transform.rotation = zoomRotationLerpInformations[cardGameObject].Step(Time.deltaTime);
            }
        }
    }

    public void Add(Card card)
    {
        if (card == null) return;

        GameObject cardGameObject = card.Create();
        CardStackEntry.Attach(cardGameObject, card, this);

        Vector3 destination = transform.position + new Vector3(Random.Range(0, 0.2f), Random.Range(-0.2f, 0.1f), 0) + new Vector3((cardGameObjects.Count - 1) * 0.5f, 0);
        cardGameObject.transform.localScale = new Vector3(0.5f, 0.5f, 1);
        cardGameObject.transform.Rotate(0, 0, Random.Range(destination.x <= 0 ? 0 : -2.5f, 2.5f));

        cardGameObject.transform.position = transform.position + new Vector3(7.5f, 5, -transform.position.z);
        cardGameObject.transform.SetParent(transform, true);

        int oldValue = Value.NumberValue;

        cardGameObject.GetComponent<SpriteRenderer>().sortingOrder = cardGameObjects.Count + 1;
        cardGameObjects.Add(cardGameObject);
        cards.Add(card);

        cardMovementLerpInformations.Add(cardGameObjects.Count - 1,
            new LerpInformation<Vector3>(cardGameObject.transform.position, destination, 0.5f, Vector3.Lerp));

        CardAdded?.Invoke(this, new CardAddedEventArgs(this, card));
        ValueChanged?.Invoke(this, new ValueChangedEventArgs(oldValue, Value.NumberValue));
    }

    public void Zoom()
    {
        if (IsZoomed || cardMovementLerpInformations.Count > 0 || !IsZoomOutInPlace) return;

        IsZoomed = true;
        IsZoomOutInPlace = false;
        GameController.Instance.Pause();

        zoomMovementLerpInformations.Clear();
        zoomRotationLerpInformations.Clear();
        positionsBeforeZoom.Clear();
        rotationsBeforeZoom.Clear();

        const float gapBetweenEachCard = 1.0f;
        float offset = (cardGameObjects.Count - 1) * gapBetweenEachCard / 2f;

        const float totalCardCurve = 50f;
        const float turnOffset = -1f * (totalCardCurve / 2f);
        float turnPerCard = totalCardCurve / cardGameObjects.Count;
        float rotationOffset = turnOffset + (cardGameObjects.Count - 1) * turnPerCard / 2f;

        float yOffsetScale = cardGameObjects.Count / 500f;

        for (int i = 0; i < cardGameObjects.Count; i++)
        {
            float zRotation = -(turnOffset + i * turnPerCard - rotationOffset);

            Quaternion destinationRotation = Quaternion.Euler(0, 0, zRotation);
            LerpInformation<Quaternion> rotationLerpInformation = new LerpInformation<Quaternion>(cardGameObjects[i].transform.rotation, destinationRotation, ZoomLerpDuration, Quaternion.Lerp);

            Vector3 destination = new Vector3(i * gapBetweenEachCard - offset, -(Mathf.Abs(zRotation) * yOffsetScale));
            LerpInformation<Vector3> positionLerpInformation = new LerpInformation<Vector3>(cardGameObjects[i].transform.position, destination, ZoomLerpDuration, Vector3.Lerp);

            zoomMovementLerpInformations.Add(cardGameObjects[i], positionLerpInformation);
            zoomRotationLerpInformations.Add(cardGameObjects[i], rotationLerpInformation);

            positionsBeforeZoom.Add(cardGameObjects[i], cardGameObjects[i].transform.position);
            rotationsBeforeZoom.Add(cardGameObjects[i], cardGameObjects[i].transform.rotation);

            cardGameObjects[i].GetComponent<SpriteRenderer>().sortingLayerName = "Display Card";
        }

        zoomScaleLperInformation =
            new LerpInformation<Vector3>(transform.localScale, Vector3.one, ZoomLerpDuration, Vector3.Lerp);

        ZoomedIn?.Invoke(this, new CardStackEventArgs(this));

    }

    public void ZoomOut()
    {
        if (!IsZoomed || !IsZoomInPlace) return;

        IsZoomed = false;
        IsZoomInPlace = false;

        GameController.Instance.Unpause();

        foreach (GameObject cardGameObject in cardGameObjects)
        {
            LerpInformation<Vector3> positionLerpInformation = new LerpInformation<Vector3>(
                cardGameObject.transform.position, positionsBeforeZoom[cardGameObject],
                ZoomLerpDuration, Vector3.Lerp);
            zoomMovementLerpInformations[cardGameObject] = positionLerpInformation;

            LerpInformation<Quaternion> rotationLerpInformation = new LerpInformation<Quaternion>(
                cardGameObject.transform.rotation, rotationsBeforeZoom[cardGameObject],
                ZoomLerpDuration, Quaternion.Lerp);
            zoomRotationLerpInformations[cardGameObject] = rotationLerpInformation;
        }

        zoomScaleLperInformation = new LerpInformation<Vector3>(transform.localScale, new Vector3(0.5f, 0.5f, 1),
            ZoomLerpDuration, Vector3.Lerp);

        ZoomedOut?.Invoke(this, new CardStackEventArgs(this));
    }

    /// <summary>
    /// Turns all the cards so they are facing up.
    /// </summary>
    public void TurnCardsUp()
    {
        foreach (GameObject cardGameObject in cardGameObjects)
        {
            cardGameObject.GetComponent<CardStackEntry>().TurnOver();
        }

        CardStackValue value = Value;
        ValueChanged?.Invoke(this, new ValueChangedEventArgs(value.NumberValue, value.NumberValue));
    }

    public bool PeekBust(Card card)
    {
        return Value.NumberValue + new CardStackValue(card).NumberValue > 21;
    }

    public bool Contains(Card card)
    {
        return cards.Any(c => c.Suit == card.Suit && c.Value == card.Value);
    }

    public bool ContainsAny(params CardSuit[] suits)
    {
        return cards.Any(c => suits.Contains(c.Suit));
    }

    public bool ContainsAny(params CardValue[] values)
    {
        return cards.Any(c => values.Contains(c.Value));
    }

    private void Clear()
    {
        if (cardGameObjects.Count <= 0) return;
        foreach (GameObject cardGameObject in cardGameObjects)
        {
            Destroy(cardGameObject);
        }

        cardGameObjects.Clear();
        cards.Clear();
    }

    private static Card GetCardFromGameObject(GameObject cardGameObject)
    {
        return cardGameObject?.GetComponent<CardStackEntry>()?.Card;
    }
}