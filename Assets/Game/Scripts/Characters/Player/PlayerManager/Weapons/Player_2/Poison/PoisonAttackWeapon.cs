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
    private Player2Animation p2Anim;

    private void Awake()
    {
        p2Anim = GetComponentInParent<Player2Animation>();
        movement = GetComponentInParent<PlayerMovement>();
        currentPoison = maxPoison;
    }

    public override void Tick(bool fireDown, bool fireHeld, bool fireUp)
    {
        if (p2Anim != null && p2Anim.IsCombatLocked) return;
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

        Vector2 dir;

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

        GameObject poisonGO = Instantiate(poisonPrefab, spawnPoint.position, Quaternion.identity);
        PoisonAttack poison = poisonGO.GetComponent<PoisonAttack>();
        if (poison != null)
            poison.Launch(dir, spawnPoint.position);

        currentPoison--;
    }
}
