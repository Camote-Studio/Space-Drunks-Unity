using UnityEngine;

public class enemigo_animacion : MonoBehaviour
{
    private Animator animator;

    private void Awake()
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

    // 🔥 llamada desde Animation Event
    public void FinDefensa()
    {
        if (!animator) return;
        animator.SetTrigger("idle");
    }
    public void ForcePlay(string stateName)
    {
        if (animator == null) return;

        animator.Play(stateName, 0, 0f); // capa 0, tiempo 0
        animator.Update(0f);            // aplica en el mismo frame
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
