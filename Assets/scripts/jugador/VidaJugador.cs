using UnityEngine;
using System;
using TMPro;

public class VidaJugador : MonoBehaviour
{
    [Header("Experiencia")]
    [SerializeField] private int nivel = 1;
    [SerializeField] private int experienciaActual = 0;
    [SerializeField] private int experienciaParaSubir = 20;

    [Header("Vida")]
    [SerializeField] private float vidaMaxima = 100f;
    [SerializeField] private float vidaActual;

    [Header("Referencias")]
    [SerializeField] private PlayerMovement playerMovement;

    public float VidaMaxima => vidaMaxima;
    public float VidaActual => vidaActual;

    [Header("Monedas")]
    [SerializeField] private int monedas = 0;
    [Tooltip("1 = Contador jugador 1 | 2 = Contador jugador 2")]
    public int idJugador = 1;
    [SerializeField] private TextMeshProUGUI textoMonedas;

    [Header("UI Vida")]
    [SerializeField] private HealthBarUI barraVidaUI;
    [SerializeField] private GameObject[] corazones;

    [Header("Heal Bar (Charge)")]
    [SerializeField] private HealBarUI healBarUI;
    [SerializeField] private float healMaxCharge = 100f;
    [SerializeField] private float healChargeActual = 0f;

    public float HealCharge01 => healMaxCharge <= 0f ? 0f : Mathf.Clamp01(healChargeActual / healMaxCharge);

    [Header("Daño por contacto")]
    [SerializeField] private bool damageOnContact = true;
    [SerializeField] private float contactDamage = 10f;
    [SerializeField] private float contactCooldown = 0.35f;

    [Header("Knockdown (3 golpes seguidos)")]
    [SerializeField] private int golpesParaDerribo = 3;
    [SerializeField] private float ventanaComboSeg = 1.2f;

    public event Action<string> OnDamaged;
    public event Action OnDeath;
    public event Action OnKnockdownStart;

    private bool intocable = false;
    private float nextContactDamageTime = 0f;
    private int golpesEnVentana = 0;
    private float expiraVentanaEn = 0f;
    private bool enKnockdown = false;

    private void Awake()
    {
        vidaActual = vidaMaxima;

        ActualizarUI();
        ActualizarBarraVida();
        ActualizarCorazones();

        ActualizarHealBar();
    }

    // ── Daño ─────────────────────────────────────────────────────

    public void RecibirDanio(float cantidad, string fuente = "")
    {
        if (intocable) return;
        if (enKnockdown) return;
        if (playerMovement != null && playerMovement.IsJumping) return;

        vidaActual = Mathf.Clamp(vidaActual - cantidad, 0f, vidaMaxima);
        ActualizarBarraVida();
        ActualizarCorazones();

        OnDamaged?.Invoke(fuente);
        RegistrarGolpeParaCombo();

        if (vidaActual <= 0f) Die();
    }

    public void RecibirDaño(float cantidad) => RecibirDanio(cantidad);

    private void RegistrarGolpeParaCombo()
    {
        float now = Time.time;

        if (now > expiraVentanaEn)
            golpesEnVentana = 0;

        golpesEnVentana++;
        expiraVentanaEn = now + ventanaComboSeg;

        if (golpesEnVentana >= golpesParaDerribo)
        {
            golpesEnVentana = 0;
            expiraVentanaEn = 0f;
            enKnockdown = true;
            OnKnockdownStart?.Invoke();
        }
    }

    public void FinishKnockdown()
    {
        enKnockdown = false;
    }

    // ── Heal Charge (se llena con ChipiDrunks) ────────────────────

    public void AddHealCharge(float amount)
    {
        if (amount <= 0f) return;

        healChargeActual = Mathf.Clamp(healChargeActual + amount, 0f, healMaxCharge);
        ActualizarHealBar();
    }

    public void ResetHealCharge()
    {
        healChargeActual = 0f;
        ActualizarHealBar();
    }

    private void ActualizarHealBar()
    {
        if (healBarUI == null) return;
        healBarUI.Set01(HealCharge01);
    }

    // ── Contacto con enemigos ─────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!damageOnContact) return;
        TryDamageFromEnemy(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!damageOnContact) return;
        TryDamageFromEnemy(other);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!damageOnContact) return;
        TryDamageFromEnemy(other.collider);
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (!damageOnContact) return;
        TryDamageFromEnemy(other.collider);
    }

    private void TryDamageFromEnemy(Collider2D other)
    {
        if (Time.time < nextContactDamageTime) return;

        var enemy = other.GetComponentInParent<enemigo_base>();
        if (enemy == null) return;

        nextContactDamageTime = Time.time + contactCooldown;
        RecibirDanio(contactDamage, enemy.name);
    }

    // ── Intocabilidad ─────────────────────────────────────────────

    public bool EsIntocable() => intocable;

    public void ActivarIntocable(float duracion)
    {
        if (!intocable)
            StartCoroutine(IntocableCoroutine(duracion));
    }

    private System.Collections.IEnumerator IntocableCoroutine(float duracion)
    {
        intocable = true;
        yield return new WaitForSeconds(duracion);
        intocable = false;
    }

    // ── Muerte ────────────────────────────────────────────────────

    private void Die()
    {
        Debug.Log($"Player {idJugador} dead");
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    // ── Monedas ───────────────────────────────────────────────────

    public void AgregarMonedas(int cantidad)
    {
        monedas += cantidad;
        ActualizarUI();
    }

    private void ActualizarUI()
    {
        if (textoMonedas != null)
            textoMonedas.text = monedas.ToString();
    }

    // ── Experiencia ───────────────────────────────────────────────

    public void AgregarExperiencia(int cantidad)
    {
        experienciaActual += cantidad;
        while (experienciaActual >= experienciaParaSubir)
        {
            experienciaActual -= experienciaParaSubir;
            SubirNivel();
        }
    }

    private void SubirNivel()
    {
        nivel++;
        experienciaParaSubir += 10;
        Debug.Log("¡Subiste a nivel " + nivel + "!");
    }

    // ── UI Vida ───────────────────────────────────────────────────

    private void ActualizarBarraVida()
    {
        if (barraVidaUI == null) return;
        barraVidaUI.Set01(vidaActual / vidaMaxima);
    }

    private void ActualizarCorazones()
    {
        if (corazones == null) return;

        float pct = vidaActual / vidaMaxima;
        int activos = pct > 0.66f ? 3 : pct > 0.33f ? 2 : pct > 0f ? 1 : 0;

        for (int i = 0; i < corazones.Length; i++)
            if (corazones[i] != null)
                corazones[i].SetActive(i < activos);
    }
}