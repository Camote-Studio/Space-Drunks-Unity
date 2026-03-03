using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class PropItems : MonoBehaviour
{
    [Header("Saltable")]
    [SerializeField] private bool esSaltable = false;
    [SerializeField] private float landingRangeY = 0.4f;
    [SerializeField] private float landingOffsetY = 0f;

    public bool EsSaltable => esSaltable;
    public float LandingY => transform.position.y + landingOffsetY;
    public float LandingRangeY => landingRangeY;

    [Header("Hits para romper")]
    [SerializeField] private int hitsToBreak = 3;
    private int hitsLeft;

    [Header("Anim")]
    [SerializeField] private PropsAnimation propsAnim;
    [SerializeField] private float breakDelay = 0.25f;

    [Header("Shake Config")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeAmount = 0.08f;
    [SerializeField] private float shakeSpeed = 40f;

    private Vector3 originalPosition;
    private Coroutine shakeRoutine;

    [Header("Drop: ChipiDrunks")]
    [SerializeField] private ChipiDrunks chipiDrunksPrefab;
    [SerializeField] private int chipidrunkCount = 5;
    [SerializeField] private float spawnRadius = 0.25f;
    [SerializeField] private float spawnForce = 2.5f;

    [Header("Qué lo puede golpear")]
    [SerializeField] private LayerMask hitMask;

    private bool broken;

    private void OnValidate()
    {
        if (propsAnim == null) propsAnim = GetComponent<PropsAnimation>();
        if (propsAnim == null) propsAnim = GetComponentInChildren<PropsAnimation>();
    }

    private void Awake()
    {
        hitsLeft = hitsToBreak;
        originalPosition = transform.localPosition;

        if (propsAnim == null) propsAnim = GetComponent<PropsAnimation>();
        if (propsAnim == null) propsAnim = GetComponentInChildren<PropsAnimation>();

        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }

    public void TakeHit(int amount = 1)
    {
        if (broken) return;

        hitsLeft -= amount;

        propsAnim?.PlayHit();
        StartShake();

        if (hitsLeft <= 0)
            StartCoroutine(BreakRoutine());
    }

    private void StartShake()
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Mathf.Sin(elapsed * shakeSpeed) * shakeAmount;
            transform.localPosition = originalPosition + new Vector3(x, 0f, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }

    private IEnumerator BreakRoutine()
    {
        if (broken) yield break;
        broken = true;

        propsAnim?.PlayBreak();
        yield return new WaitForSeconds(breakDelay);

        SpawnChips();
        Destroy(gameObject);
    }

    private void SpawnChips()
    {
        if (chipiDrunksPrefab == null || chipidrunkCount <= 0) return;

        for (int i = 0; i < chipidrunkCount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            var coin = Instantiate(chipiDrunksPrefab, transform.position + (Vector3)offset, Quaternion.identity);

            var rb = coin.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (Random.insideUnitCircle.normalized + Vector2.up * 0.35f).normalized;
                rb.AddForce(dir * spawnForce, ForceMode2D.Impulse);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (broken) return;

        if (((1 << other.gameObject.layer) & hitMask.value) != 0)
            TakeHit(1);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (broken) return;

        if (((1 << collision.gameObject.layer) & hitMask.value) != 0)
            TakeHit(1);
    }

    private void OnDrawGizmosSelected()
    {
        if (!esSaltable) return;

        Gizmos.color = Color.cyan;
        Vector3 center = transform.position + Vector3.up * landingOffsetY;
        Gizmos.DrawWireCube(center, new Vector3(1f, landingRangeY, 0f));
    }
}