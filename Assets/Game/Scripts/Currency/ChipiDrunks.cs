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
        Debug.Log($"[ChipiDrunks] START {name} trigger={GetComponent<Collider2D>().isTrigger}", this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[ChipiDrunks] OnTriggerEnter2D con: {other.name} tag={other.tag}", this);

        // 1) Si el collider que entra ES el Player
        VidaJugador jugador = other.GetComponent<VidaJugador>();

        // 2) Si el collider es un hijo del Player (Hitbox/Body/etc.)
        if (jugador == null)
            jugador = other.GetComponentInParent<VidaJugador>();

        if (jugador == null)
        {
            Debug.LogWarning("[ChipiDrunks] No encontré VidaJugador en el objeto que colisionó.", this);
            return;
        }

        if (coinsDisplay != null)
            coinsDisplay.Add(value);

        float add = healChargePerCoin * value;
        jugador.AddHealCharge(add);

        Debug.Log($"[ChipiDrunks] +HealCharge {add}. Heal01={jugador.HealCharge01}", this);

        Destroy(gameObject);
    }
}