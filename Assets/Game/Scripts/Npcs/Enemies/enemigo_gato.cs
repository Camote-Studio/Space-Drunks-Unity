using UnityEngine;
using System.Collections;

public class enemigo_gato : enemigo_base
{
    [Header("Abducción")]
    public float tiempoAbduccion = 20f;
    public ZonaAbduccionGato zonaAbduccion;

    private bool abduciendo;

    // ===================== UNITY =====================
    protected override void Awake()
    {
        base.Awake();

        if (zonaAbduccion != null)
            zonaAbduccion.Configurar(this);
    }

    protected override void Update()
    {
        if (estaMuerto) return;

        // 🚫 Mientras abduce no se mueve
        if (abduciendo)
        {
            if (agent != null && agent.isOnNavMesh)
                agent.isStopped = true;

            ForzarIdle();
            return;
        }

        base.Update();
        ActualizarAnimacionMovimiento();
    }

    // ===================== ABDUCCIÓN =====================
    public bool PuedeAbducir()
    {
        return !abduciendo && !estaMuerto;
    }

    public void IniciarAbduccion()
    {
        if (!PuedeAbducir()) return;
        StartCoroutine(RutinaAbduccion());
    }

    IEnumerator RutinaAbduccion()
    {
        abduciendo = true;
        estaAtacando = true;

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = true;

        ForzarIdle();

        if (zonaAbduccion != null)
            zonaAbduccion.IniciarLaser();

        yield return new WaitForSeconds(tiempoAbduccion);

        FinalizarAbduccion();
    }

    public void FinalizarAbduccion()
    {
        if (!abduciendo) return;

        abduciendo = false;
        estaAtacando = false;

        if (zonaAbduccion != null)
            zonaAbduccion.FinalizarLaser();

        if (agent != null && agent.isOnNavMesh)
            agent.isStopped = false;
    }

    // ===================== ANIMACIONES =====================
    void ActualizarAnimacionMovimiento()
    {
        if (animator == null || agent == null) return;

        bool moviendo = agent.velocity.magnitude > 0.1f;
        animator.Play(moviendo ? "walk" : "idle");
    }

    void ForzarIdle()
    {
        if (animator == null) return;
        animator.Play("idle");
    }

    // ===================== POOL =====================
    public override void OnSpawnFromPool()
    {
        base.OnSpawnFromPool();

        abduciendo = false;

        if (zonaAbduccion != null)
            zonaAbduccion.Resetear();
    }
}
