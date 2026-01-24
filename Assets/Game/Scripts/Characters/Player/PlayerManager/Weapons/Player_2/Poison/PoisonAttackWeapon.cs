using UnityEngine;

public class PoisonAttackWeapon : WeaponBase
{
    [SerializeField] private GameObject poisonPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform facingVisual;

    [SerializeField] private int maxPoison = 4;
    [SerializeField] private float replenishInterval = 8f;

    private int currentPoison;
    private float replenishTimer;

    private PlayerMovement movement;

    private void Awake()
    {
        movement = GetComponentInParent<PlayerMovement>();
        currentPoison = maxPoison;
    }
    public override void Tick(bool fireDown, bool fireHeld, bool fireUp)
    {
        if (currentPoison < maxPoison)
        {
            replenishTimer += Time.deltaTime;
            if (replenishTimer >= replenishInterval)
            {
                replenishTimer = 0f;
                currentPoison++;
            }
        }

        if (fireDown)
            TryPlacePoison();
    }


    public void TryPlacePoison()
    {
        if (currentPoison <= 0) return;
        if (poisonPrefab == null || spawnPoint == null) return;

        Vector2 dir = Vector2.zero;

        if (movement != null && movement.MoveInput.sqrMagnitude > 0.01f)
        {
            dir = movement.MoveInput.normalized;
        }
        else if (facingVisual != null && Mathf.Abs(facingVisual.localScale.x) > 0.01f)
        {
            dir = new Vector2(Mathf.Sign(facingVisual.localScale.x), 0f);
        }
        else
        {
            dir = Vector2.right;
        }

        GameObject mineGO = Instantiate(poisonPrefab, spawnPoint.position, Quaternion.identity);
        Mine mine = mineGO.GetComponent<Mine>();
        if (mine != null)
            mine.Launch(dir);

        currentPoison--;

    }
}
