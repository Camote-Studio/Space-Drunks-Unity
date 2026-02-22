using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ChipiDrunks : MonoBehaviour
{
    [SerializeField] private int value = 1;

    [Header("Heal Charge")]
    [SerializeField] private float healChargePerCoin = 5f;

    private SpriteNumberDisplay coinsDisplay;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Start()
    {
        coinsDisplay = FindObjectOfType<SpriteNumberDisplay>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Igual que XPOrb: agarra VidaJugador aunque el collider sea de un hijo
        VidaJugador jugador = other.GetComponentInParent<VidaJugador>();
        if (jugador == null) return;

        if (coinsDisplay != null)
            coinsDisplay.Add(value);

        jugador.AgregarMonedas(value);

        float add = healChargePerCoin * value;
        jugador.AddHealCharge(add);

        Debug.Log($"[ChipiDrunks] +HealCharge {add} (value={value}) Heal01={jugador.HealCharge01}", this);

        Destroy(gameObject);
    }
}