using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fill;

    public void Set01(float value01)
    {
        fill.fillAmount = Mathf.Clamp01(value01);
    }
}
