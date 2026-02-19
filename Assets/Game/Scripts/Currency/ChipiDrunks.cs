using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ChipiDrunks : MonoBehaviour
{
    [SerializeField] private int value = 1;
    private SpriteNumberDisplay coinsDisplay;

    private void Start()
    {
        coinsDisplay = FindObjectOfType<SpriteNumberDisplay>();
    }

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (coinsDisplay != null)
                coinsDisplay.Add(value);

            Destroy(gameObject);
        }
    }
}
