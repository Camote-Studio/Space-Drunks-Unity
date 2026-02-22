using UnityEngine;
using UnityEngine.UI;

public abstract class BaseBar : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] protected Image fill;

    [Header("Settings")]
    [SerializeField] protected bool smooth = false;
    [SerializeField] protected float smoothSpeed = 8f;

    protected float targetValue = 0f;

    protected virtual void Awake()
    {
        if (fill == null)
            fill = GetComponentInChildren<Image>(true);

        targetValue = 0f;

        if (fill != null)
            fill.fillAmount = 0f;
    }

    protected virtual void Update()
    {
        if (!smooth || fill == null) return;

        fill.fillAmount = Mathf.Lerp(
            fill.fillAmount,
            targetValue,
            Time.deltaTime * smoothSpeed
        );
    }

    public virtual void Set01(float value01)
    {
        targetValue = Mathf.Clamp01(value01);

        if (!smooth && fill != null)
            fill.fillAmount = targetValue;
    }

    public virtual void SetRaw(float current, float max)
    {
        if (max <= 0f)
        {
            Set01(0f);
            return;
        }

        Set01(current / max);
    }

    public float GetValue01() => targetValue;

    public void ResetToZero()
    {
        targetValue = 0f;
        if (fill != null) fill.fillAmount = 0f;
    }
}