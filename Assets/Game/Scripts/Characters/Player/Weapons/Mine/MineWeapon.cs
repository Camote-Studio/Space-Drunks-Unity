using UnityEngine;

public class MineWeapon : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform mineSpawnPoint;
    [SerializeField] private GameObject minePrefab;

    [Header("Stock")]
    [SerializeField] private int maxMines = 3;
    [SerializeField] private float rechargeTime = 8f;

    private int currentMines;
    private float rechargeTimer;

    private PlayerAnimation playerAnim;

    private void Reset()
    {
        playerAnim = GetComponentInParent<PlayerAnimation>();
    }

    private void Awake()
    {
        if (playerAnim == null)
            playerAnim = GetComponentInParent<PlayerAnimation>();

        currentMines = maxMines;
    }

    public void Tick()
    {

        if (currentMines >= maxMines) return;

        rechargeTimer += Time.deltaTime;
        if (rechargeTimer >= rechargeTime)
        {
            rechargeTimer = 0f;
            currentMines++;
        }
    }

    public void TryPlaceMine()
    {
        if (currentMines <= 0) return;
        if (minePrefab == null || mineSpawnPoint == null) return;

        GameObject mineGO = Instantiate(
            minePrefab,
            mineSpawnPoint.position,
            Quaternion.identity
        );

        Vector2 dir = Vector2.right;
        if (playerAnim != null)
        {
            float facingX = playerAnim.transform.localScale.x;
            if (Mathf.Abs(facingX) < 0.01f) facingX = 1f;
            dir = new Vector2(Mathf.Sign(facingX), 0f);
        }

        Mine mine = mineGO.GetComponent<Mine>();
        if (mine != null)
            mine.Launch(dir);

        currentMines--;
        playerAnim?.PlayPlaceMine();   
    }
}
