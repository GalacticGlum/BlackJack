using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class CardStackValueLabel : MonoBehaviour
{
    private Text textComponent;
    [SerializeField]
    private CardStackController cardStack;

    private CardStackController zoomedOutCardStack;
    private LerpInformation<float> alphaLerpInformation;

    private void Awake()
    {
        textComponent = GetComponent<Text>();
        textComponent.text = string.Empty;

        GameController.Instance.Started += (sender, args) =>
        {
            textComponent.text = string.Empty;
        };
    }

	// Use this for initialization
	private void Start ()
	{
        cardStack.ValueChanged += UpdateLabel;

	    GameController.Instance.DealerCardStack.ZoomedIn += (sender, args) => textComponent.enabled = false;
        GameController.Instance.DealerCardStack.ZoomedOut += OnCardStackZoomedOut;
	    GameController.Instance.PlayerCardStack.ZoomedIn += (sender, args) => textComponent.enabled = false;
	    GameController.Instance.PlayerCardStack.ZoomedOut += OnCardStackZoomedOut;
	}

    private void Update()
    {
        HandleAlphaLerp();

        if (zoomedOutCardStack == null || !zoomedOutCardStack.IsZoomOutInPlace) return;
        textComponent.enabled = true;
        textComponent.color = new Color(0, 0, 0, 0);
        
        zoomedOutCardStack = null;
        alphaLerpInformation = new LerpInformation<float>(0, 1, 0.1f, Mathf.Lerp);
    }

    private void HandleAlphaLerp()
    {
        if (alphaLerpInformation == null || alphaLerpInformation.TimeLeft <= 0)
        {
            alphaLerpInformation = null;
            return;
        }

        textComponent.color = new Color(1, 1, 1, alphaLerpInformation.Step(Time.deltaTime));
    }

    private void UpdateLabel(object sender, ValueChangedEventArgs args)
    {
        textComponent.text = args.Value.ToString();
    }

    private void OnCardStackZoomedOut(object sender, CardStackEventArgs args)
    {
        zoomedOutCardStack = args.CardStack;
    }
}
