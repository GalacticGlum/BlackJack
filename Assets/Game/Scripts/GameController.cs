using System;
using System.Collections;
using UnityEngine;

public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs args);
public class ValueChangedEventArgs : EventArgs
{
    public int OldValue { get; }
    public int Value { get; }

    public ValueChangedEventArgs(int oldValue, int value)
    {
        OldValue = oldValue;
        Value = value;
    }
}

public delegate void GameStartedEventHandler(object sender, GameControllerEventArgs args);
public delegate void BetFinalizedEventHandler(object sender, GameControllerEventArgs args);
public delegate void BustEventHandler(object sender, GameControllerEventArgs args);
public delegate void PushEventHandler(object sender, GameControllerEventArgs args);
public delegate void WonEventHandler(object sender, GameControllerEventArgs args);

public class GameControllerEventArgs : EventArgs
{
    public GameController GameController { get; }
    public GameControllerEventArgs(GameController gameController)
    {
        GameController = gameController;
    }
}

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    private int money;
    public int Money
    {
        get { return money; }
        set
        {
            int oldMoney = money;
            money = value;
            if (oldMoney == money) return;

            OnMoneyChanged(new ValueChangedEventArgs(oldMoney, money));
        }
    }

    private int betValue;
    public int BetValue
    {
        get { return betValue; }
        set
        {
            int oldBetValue = betValue;
            betValue = value;
            if (oldBetValue == betValue) return;

            OnBetValueChanged(new ValueChangedEventArgs(oldBetValue, betValue));
        }
    }

    public int BetChipsCount { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsBetFinalized { get; private set; }
    public bool IsGameOver { get; private set; }

    public CardStackController DealerCardStack => dealerCardStack;
    public CardStackController PlayerCardStack => playerCardStack;

    public event ValueChangedEventHandler MoneyChanged;
    private void OnMoneyChanged(ValueChangedEventArgs args)
    {
        ValidateChips();
        MoneyChanged?.Invoke(this, args);
    }

    public event ValueChangedEventHandler BetValueChanged;
    private void OnBetValueChanged(ValueChangedEventArgs args)
    {
        BetValueChanged?.Invoke(this, args);
    }

    public event BetFinalizedEventHandler BetFinalized;
    private void OnBetFinalized()
    {
        IsBetFinalized = true;
        BetFinalized?.Invoke(this, new GameControllerEventArgs(this));
    }

    public event BustEventHandler Bust;
    private void OnBust()
    {
        Bust?.Invoke(this, new GameControllerEventArgs(this));
    }

    public event PushEventHandler Push;
    private void OnPush()
    {
        Push?.Invoke(this, new GameControllerEventArgs(this));
    }

    public event WonEventHandler Won;
    private void OnWon()
    {
        Won?.Invoke(this, new GameControllerEventArgs(this));
    }

    public event GameStartedEventHandler Started;
    private void OnStarted()
    {
        Started?.Invoke(this, new GameControllerEventArgs(this));
    }

    [SerializeField]
    private int startingMoney = 2500;
    private bool canTakeAction = true;

    [Header("Chips")]
    [SerializeField]
    private ChipInstance fiveHundredChipInstance;
    [SerializeField]
    private ChipInstance hundredChipInstance;
    [SerializeField]
    private ChipInstance tenChipInstance;
    [SerializeField]
    private ChipInstance oneChipInstance;

    [Header("Card Stacks")]
    [SerializeField]
    private CardStackController dealerCardStack;
    [SerializeField]
    private CardStackController playerCardStack;

    [Header("Rules")]
    [SerializeField]
    private bool mustHitOnSoftAce = true;

    private Deck deck;
    private bool isPlayersTurn;
    private bool isFirstHit;
    private bool waitBeforeNextDealerHit;
    private float dealerHitTimer;

    private void OnEnable()
    {
        Instance = this;
    }

    private void Start()
    {
        deck = new Deck();
        Money = startingMoney;
        StartGame();
    }

    public void StartGame()
    {
        ChipContainer.Initialize();
        
        IsGameOver = false;

        isPlayersTurn = true;
        isFirstHit = true;
        IsBetFinalized = false;
        IsPaused = false;

        BetValue = 0;
        BetChipsCount = 0;

        OnStarted();
    }

    private void Update()
    {
        ChipContainer.Update();

        if (IsGameOver || isPlayersTurn) return;

        dealerHitTimer -= Time.deltaTime;
        if (dealerHitTimer <= 0)
        {
            waitBeforeNextDealerHit = false;
        }

        if (waitBeforeNextDealerHit) return;
        if (dealerCardStack.Value.NumberValue < 17)
        {
            DealerHit(1, false);
            WaitBeforeNextDealerHit();
        }
        else if (dealerCardStack.Value.NumberValue == 17 && mustHitOnSoftAce && dealerCardStack.ContainsAny(CardValue.Ace))
        {
            DealerHit(1, false);
            WaitBeforeNextDealerHit();
        }
        else 
        {
            if (dealerCardStack.HasBust && playerCardStack.HasBust ||
                dealerCardStack.Value.NumberValue > playerCardStack.Value.NumberValue && !dealerCardStack.HasBust || 
                dealerCardStack.HasNaturalBlackJack && playerCardStack.HasThreeOrMoreBlackJack)
            {
                StartCoroutine(ExecuteBust());
            }
            else if (dealerCardStack.Value.NumberValue == playerCardStack.Value.NumberValue)
            {
                StartCoroutine(ExecutePush());
            }
            else if (playerCardStack.Value.NumberValue > dealerCardStack.Value.NumberValue &&
                     !playerCardStack.HasBust || playerCardStack.Value.NumberValue < dealerCardStack.Value.NumberValue && dealerCardStack.HasBust
                     || PlayerCardStack.HasNaturalBlackJack && dealerCardStack.HasThreeOrMoreBlackJack)
            {
                StartCoroutine(ExecuteWon());
            }
        }
    }

    public void Bet(ChipValue chipValue)
    {
        int value = (int)chipValue;
        if (value > money) return;

        Money -= value;
        BetValue += value;
        BetChipsCount += 1;
    }

    private void ValidateChips()
    {
        fiveHundredChipInstance.gameObject.SetActive(true);
        hundredChipInstance.gameObject.SetActive(true);
        tenChipInstance.gameObject.SetActive(true);
        oneChipInstance.gameObject.SetActive(true);

        if (money < 500)
        {
            fiveHundredChipInstance.gameObject.SetActive(false);
        }

        if (money < 100)
        {
            hundredChipInstance.gameObject.SetActive(false);
        }

        if (money < 10)
        {
            tenChipInstance.gameObject.SetActive(false);
        }

        if (money < 1)
        {
            oneChipInstance.gameObject.SetActive(false);
        }
    }

    public void Pause()
    {
        IsPaused = true;
    }

    public void Unpause()
    {
        IsPaused = false;
    }

    public void Hit()
    {
        if (!canTakeAction) return;
        Hit(1);
    }

    public void Hit(int amount)
    {
        if (!isPlayersTurn || BetValue <= 0) return;
        if (isFirstHit)
        {
            isFirstHit = false;
            StartCoroutine(SetupCards());

            return;
        }

        for (int i = 0; i < amount; i++)
        {
            Card card = deck.Pop();
            if (playerCardStack.PeekBust(card))
            {
                StartCoroutine(ExecuteBust());
            }

            playerCardStack.Add(card);

            if (playerCardStack.HasNaturalBlackJack)
            {
                StartCoroutine(ExecuteWon());
            }
        }
    }

    private void DealerHit(int amount = 1, bool isFaceDown = true, bool force = false)
    {
        if (isPlayersTurn && !force) return;
        for (int i = 0; i < amount; i++)
        {
            dealerCardStack.Add(deck.Pop(isFaceDown));
        }
    }

    public void EndTurn()
    {
        if (!isPlayersTurn || !canTakeAction) return;
        isPlayersTurn = false;

        dealerCardStack.TurnCardsUp();
        WaitBeforeNextDealerHit();
    }

    private IEnumerator SetupCards()
    {
        OnBetFinalized();
        canTakeAction = false;

        // Setup dealers cards
        DealerHit(1, false, true);
        yield return new WaitForSeconds(0.5f);
        DealerHit(1, true, true);
        yield return new WaitForSeconds(0.5f);

        Hit(1);
        yield return new WaitForSeconds(0.5f);
        Hit(1);
        yield return new WaitForSeconds(0.5f);
        canTakeAction = true;
    }

    private IEnumerator ExecuteBust()
    {
        IsGameOver = true;
        yield return new WaitForSeconds(0.5f);
        OnBust();
        ChipContainer.TakeChips();
    }

    private IEnumerator ExecutePush()
    {
        IsGameOver = true;
        yield return new WaitForSeconds(0.5f);
        OnPush();
        Money += betValue;
        ChipContainer.ReturnChips();
    }

    private IEnumerator ExecuteWon()
    {
        IsGameOver = true;
        yield return new WaitForSeconds(0.5f);

        OnWon();
        if (playerCardStack.HasNaturalBlackJack)
        {
            Money += Mathf.RoundToInt(1.5f * betValue) + betValue;
        }
        else
        {
            Money += betValue * 2;
        }

        ChipContainer.ReturnChips();
    }

    private void WaitBeforeNextDealerHit(float time = 0.5f)
    {
        waitBeforeNextDealerHit = true;
        dealerHitTimer = time;
    }
}