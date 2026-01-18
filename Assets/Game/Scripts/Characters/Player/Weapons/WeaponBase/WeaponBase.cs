using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public virtual void OnSelected()
    {
        gameObject.SetActive(true);
    }
    public virtual void OnDeselected()
    {
        gameObject.SetActive(false);
    }   
    public abstract void Tick(bool fireDown, bool fireHeld, bool fireUp);
}
