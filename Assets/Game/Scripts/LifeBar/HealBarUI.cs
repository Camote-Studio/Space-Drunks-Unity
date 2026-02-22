using UnityEngine;
using UnityEngine.UI;

public class HealBarUI : MonoBehaviour
{
    [SerializeField] private Image fill;

    private void Awake()
    {
        if (fill == null)
            Debug.LogError($"[HealBarUI] Fill NULL en {name}", this);
        else
            Debug.Log($"[HealBarUI] Fill asignado: {fill.name} | type={fill.type} | amount={fill.fillAmount}", this);
    }

    public void Set01(float value01)
    {
        if (fill == null) return;

        float v = Mathf.Clamp01(value01);
        fill.fillAmount = v;

        Debug.Log($"[HealBarUI] Set01({v}) -> fill={fill.name} ahora={fill.fillAmount}", this);
    }
}