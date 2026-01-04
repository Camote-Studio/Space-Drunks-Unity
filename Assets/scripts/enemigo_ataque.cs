using UnityEngine;

[RequireComponent(typeof(enemigo_base))]
public class enemigo_ataque : MonoBehaviour
{
    [Header("Ataque Melee")]
    public float rangoAtaque = 1.5f;
    public float daño = 10f;
    public float enfriamiento = 3f;

    private float temporizadorAtaque;
    private enemigo_base enemigo;
    private Transform objetivo;
    private enemigo_animacion anim;

    void Awake()
    {
        enemigo = GetComponent<enemigo_base>();
        anim = GetComponent<enemigo_animacion>();
        objetivo = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (enemigo == null || objetivo == null) return;
        if (enemigo.estaMuerto) return;

        temporizadorAtaque -= Time.deltaTime;

        float distancia = Vector2.Distance(transform.position, objetivo.position);

        if (!enemigo.estaAtacando &&
            temporizadorAtaque <= 0 &&
            distancia <= rangoAtaque)
        {
            Atacar();
        }
    }

    void Atacar()
    {
        enemigo.estaAtacando = true;
        temporizadorAtaque = enfriamiento;

        // animación del enemigo atacando
        if (anim != null)
            anim.SetAtacando(true);

        // aplicar daño al jugador
        var vida = objetivo.GetComponentInParent<VidaJugador>();
        if (vida != null)
        {
            Debug.Log("enemigo_ataque: golpeo al jugador");
            vida.RecibirDaño(daño); // aquí se dispara OnDamaged -> Hit en el Animator del player
        }
        else
        {
            Debug.LogWarning("enemigo_ataque: no encontré VidaJugador en el Player");
        }

        // terminar ataque después de un pequeño delay
        Invoke(nameof(FinAtaque), 0.4f);
    }

    void FinAtaque()
    {
        enemigo.estaAtacando = false;

        if (anim != null)
            anim.SetAtacando(false);
    }
}
