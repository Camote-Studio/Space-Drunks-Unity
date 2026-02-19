using UnityEngine;
using UnityEngine.UI;

public class SpriteNumberDisplay : MonoBehaviour
{
    [SerializeField] private Image[] digits;
    [SerializeField] private Sprite[] numbers;

    private int currentValue;

    void Start()
    {
        SetValue(0);
    }

    public void SetValue(int value)
    {
        currentValue = Mathf.Max(0, value);

        string valueString = currentValue.ToString().PadLeft(digits.Length, '0');

        for (int i = 0; i < digits.Length; i++)
        {
            int digit = valueString[i] - '0';
            digits[i].sprite = numbers[digit];
        }

    }
    public void Add(int amount)
    {
        SetValue(currentValue + amount);
    }


}
