using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class MoneyLabel : MonoBehaviour
{
    private Text moneyTextComponent;

    private void Start()
    {
        moneyTextComponent = GetComponent<Text>();
        GameController.Instance.MoneyChanged += OnMoneyChanged;
    }

    private void OnMoneyChanged(object sender, ValueChangedEventArgs args)
    {
        moneyTextComponent.text = $"${args.Value}";
    }
}
