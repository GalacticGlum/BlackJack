using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScreenFadeController))]
public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }
    public ScreenFadeController ScreenFadeController { get; private set; }
    public CanvasGroup GameOverCanvasGroup => gameOverCanvasGroup;

    [SerializeField]
    private GameObject cardDisplayMenu;
    [SerializeField]
    private Button hitButton;
    [SerializeField]
    private Button standButton;
    [SerializeField]
    private GameObject hudGameObject;
    [SerializeField]
    private CanvasGroup gameOverCanvasGroup;
    [SerializeField]
    private Text gameOverText;

    private CardStackController activeCardStackController;

    private void OnEnable()
    {
        Instance = this;
        ScreenFadeController = GetComponent<ScreenFadeController>();
    }

    private void Awake()
    {
        SetupEvents();
        GameController.Instance.Started += (sender, args) =>
        {
            hudGameObject.SetActive(true);
            hudGameObject.GetComponent<CanvasGroup>().alpha = 1;

            cardDisplayMenu.SetActive(false);
            standButton.gameObject.SetActive(false);

            gameOverCanvasGroup.alpha = 0;
            gameOverCanvasGroup.gameObject.SetActive(false);

            hitButton.GetComponent<Text>().text = "Deal";
        };
    }

    private void SetupEvents()
    {
        GameController.Instance.DealerCardStack.ZoomedIn += OnCardStackZoomedIn;
        GameController.Instance.DealerCardStack.ZoomedOut += OnCardStackZoomedOut;
        GameController.Instance.PlayerCardStack.ZoomedIn += OnCardStackZoomedIn;
        GameController.Instance.PlayerCardStack.ZoomedOut += OnCardStackZoomedOut;

        GameController.Instance.BetFinalized += (sender, args) =>
        {
            standButton.gameObject.SetActive(true);
            hitButton.GetComponent<Text>().text = "Hit";
        };

        GameController.Instance.Bust += (sender, args) => OnGameOver(GameController.Instance.Money > 0 ? "Dealer Won!" : 
            "<size=80>Dealer Won!</size>\n<size=40><color=#FF5E5EFF>You've ran out of chips!</color></size>");

        GameController.Instance.Push += (sender, args) => OnGameOver("Push");
        GameController.Instance.Won += (sender, args) => OnGameOver($"You Won ${GameController.Instance.BetValue}");
    }

    private void OnGameOver(string text)
    {
        gameOverCanvasGroup.alpha = 0;
        gameOverCanvasGroup.gameObject.SetActive(true);

        ScreenFadeController.FadeIn(0.5f, null, 140);
        ScreenFadeController.FadeOut(0.5f, gameOverCanvasGroup);

        gameOverText.text = text;
        hudGameObject.SetActive(false);
        gameOverCanvasGroup.gameObject.AddComponent<GameOverScreenInput>();
    }

    private void OnCardStackZoomedIn(object sender, CardStackEventArgs args)
    {
        ScreenFadeController.FadeIn(CardStackController.ZoomLerpDuration);
        activeCardStackController = args.CardStack;
        cardDisplayMenu.SetActive(true);
        hudGameObject.GetComponent<CanvasGroup>().interactable = false;
    }

    private void OnCardStackZoomedOut(object sender, CardStackEventArgs args)
    {
        ScreenFadeController.FadeOut(CardStackController.ZoomLerpDuration);
        activeCardStackController = null;
        cardDisplayMenu.SetActive(false);
        hudGameObject.GetComponent<CanvasGroup>().interactable = true;
    }

    public void ZoomOutActiveCardStack()
    {
        activeCardStackController?.ZoomOut();
    }
}
