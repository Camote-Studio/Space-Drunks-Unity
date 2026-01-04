using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public abstract void Tick(bool fireDown, bool fireHeld, bool fireUp);
    public virtual void OnSelected() { }
    public virtual void OnDeselected() { }
}
