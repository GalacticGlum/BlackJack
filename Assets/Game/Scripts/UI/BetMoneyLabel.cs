using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class BetMoneyLabel : MonoBehaviour
{
    private Text moneyTextComponent;

    private void Start()
    {
        moneyTextComponent = GetComponent<Text>();
        GameController.Instance.BetValueChanged += OnBetValueChanged;
        OnBetValueChanged(this, new ValueChangedEventArgs(0, 0));
    }

    private void OnBetValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.Value <= 0)
        {
            moneyTextComponent.text = string.Empty;
            return;
        }

        moneyTextComponent.text = $"${args.Value}";
    }
}