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
    private enemigo_animacion anim;
    private Transform objetivo;

    void Awake()
    {
        enemigo = GetComponent<enemigo_base>();
        anim = GetComponent<enemigo_animacion>();
    }

    void Update()
    {
        if (enemigo == null || enemigo.estaMuerto) return;

        // buscar siempre al objetivo más cercano
        ActualizarObjetivo();
        if (objetivo == null) return;

        temporizadorAtaque -= Time.deltaTime;

        float distancia = Vector2.Distance(transform.position, objetivo.position);

        if (!enemigo.estaAtacando &&
            temporizadorAtaque <= 0f &&
            distancia <= rangoAtaque)
        {
            Atacar();
        }
    }

    void ActualizarObjetivo()
    {
        GameObject[] jugadores1 = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] jugadores2 = GameObject.FindGameObjectsWithTag("Player_2");

        float menorDistancia = Mathf.Infinity;
        Transform masCercano = null;

        foreach (GameObject j in jugadores1)
        {
            float d = Vector2.Distance(transform.position, j.transform.position);
            if (d < menorDistancia)
            {
                menorDistancia = d;
                masCercano = j.transform;
            }
        }

        foreach (GameObject j in jugadores2)
        {
            float d = Vector2.Distance(transform.position, j.transform.position);
            if (d < menorDistancia)
            {
                menorDistancia = d;
                masCercano = j.transform;
            }
        }

        objetivo = masCercano;
    }

    void Atacar()
    {
        enemigo.estaAtacando = true;
        temporizadorAtaque = enfriamiento;

        // animación de ataque
        if (anim != null)
            anim.SetAtacando(true);

        // aplicar daño
        VidaJugador vida = objetivo.GetComponentInParent<VidaJugador>();
        if (vida != null)
        {
            Debug.Log("enemigo_ataque: golpeo al jugador");
            vida.RecibirDanio(daño);
        }
        else
        {
            Debug.LogWarning("enemigo_ataque: no encontré VidaJugador");
        }

        Invoke(nameof(FinAtaque), 0.4f);
    }

    void FinAtaque()
    {
        enemigo.estaAtacando = false;

        if (anim != null)
            anim.SetAtacando(false);
    }
}
