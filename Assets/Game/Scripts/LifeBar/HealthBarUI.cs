using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fill;

    private void Reset()
    {
        if (fill == null)
            fill = GetComponentInChildren<Image>(true);
    }

    public void SetFill(Image fillImage)
    {
        fill = fillImage;
    }

    public void Set01(float value01)
    {
        if (fill == null) return;
        fill.fillAmount = Mathf.Clamp01(value01);
    }
}