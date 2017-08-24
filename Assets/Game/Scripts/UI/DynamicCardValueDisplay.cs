using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class DynamicCardValueDisplay : MonoBehaviour
{
    private Text textComponent;
    private bool hasZoomedOut;

    private void Start()
    {
        textComponent = GetComponent<Text>();
        textComponent.text = string.Empty;

        GameController.Instance.DealerCardStack.ZoomedIn += OnCardStackZoomedIn;
        GameController.Instance.DealerCardStack.ZoomedOut += (sender, args) => gameObject.SetActive(false);

        GameController.Instance.PlayerCardStack.ZoomedIn += OnCardStackZoomedIn;       
        GameController.Instance.PlayerCardStack.ZoomedOut += (sender, args) => gameObject.SetActive(false);
    }

    private void OnCardStackZoomedIn(object sender, CardStackEventArgs args)
    {
        CardStackValue value = args.CardStack.Value;
        textComponent.text = value.NumberValue.ToString();
        gameObject.SetActive(true);
    }
}