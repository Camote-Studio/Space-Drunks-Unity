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

    void Awake()
    {
        enemigo = GetComponent<enemigo_base>();
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

        GetComponent<enemigo_animacion>()?.SetAtacando(true);
        objetivo.GetComponent<VidaJugador>()?.RecibirDaño(daño);

        Invoke(nameof(FinAtaque), 0.4f);
    }

    void FinAtaque()
    {
        enemigo.estaAtacando = false;
        GetComponent<enemigo_animacion>()?.SetAtacando(false);
    }
}
