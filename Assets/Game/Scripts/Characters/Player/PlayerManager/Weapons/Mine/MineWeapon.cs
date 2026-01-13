using UnityEngine;

public class MineWeapon : MonoBehaviour
{
    [SerializeField] private GameObject minePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform facingVisual;

    [SerializeField] private int maxMines = 3;
    [SerializeField] private float replenishInterval = 8f;

    private int currentMines;
    private float replenishTimer;

    private PlayerMovement movement;

    private void Awake()
    {
        movement = GetComponentInParent<PlayerMovement>();
        currentMines = maxMines;
    }

    public void Tick()
    {
        if (currentMines >= maxMines)
            return;

        replenishTimer += Time.deltaTime;
        if (replenishTimer >= replenishInterval)
        {
            replenishTimer = 0f;
            currentMines++;
        }
    }

    public void TryPlaceMine()
    {
        if (currentMines <= 0) return;
        if (minePrefab == null || spawnPoint == null) return;

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

        GameObject mineGO = Instantiate(minePrefab, spawnPoint.position, Quaternion.identity);
        Mine mine = mineGO.GetComponent<Mine>();
        if (mine != null)
            mine.Launch(dir);

        currentMines--;
    }
}
