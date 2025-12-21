using UnityEngine;
public class enemigo_animacion : MonoBehaviour
{
    Animator animator;
    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }
    public void SetAtacando(bool value)
    {
        if (!animator) return;
        animator.SetBool("atacando", value);
    }
    public void PlayTrigger(string triggerName)
    {
        if (!animator) return;
        animator.SetTrigger(triggerName);
    }
    public void SetBool(string nombre, bool valor)
    {
        if (!animator) return;
        animator.SetBool(nombre, valor);
    }
    public void ResetBool(string nombre)
    {
        if (!animator) return;
        animator.SetBool(nombre, false);
    }
}
