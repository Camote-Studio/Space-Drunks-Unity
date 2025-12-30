using UnityEngine;

public class MineWeapon : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform visual;          
    [SerializeField] private Transform mineSpawnPoint;  
    [SerializeField] private GameObject minePrefab;     

    [Header("Stock de minas")]
    [SerializeField] private int maxMines = 3;
    [SerializeField] private float regenInterval = 8f;  

    private int currentMines;
    private float regenTimer;

    private void Awake()
    {
        if (visual == null)
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                visual = sr.transform;
        }

        if (mineSpawnPoint == null && visual != null)
            mineSpawnPoint = visual; 

        currentMines = maxMines;
    }
    public void Tick()
    {
        RegenerarStock();
    }

    private void RegenerarStock()
    {
        if (currentMines >= maxMines)
            return;

        regenTimer += Time.deltaTime;
        if (regenTimer >= regenInterval)
        {
            regenTimer -= regenInterval;
            currentMines++;
        }
    }

    public void TryPlaceMine()
    {
        if (currentMines <= 0) return;
        if (minePrefab == null || mineSpawnPoint == null) return;

        float facingX = 1f;
        if (visual != null && Mathf.Abs(visual.localScale.x) > 0.01f)
            facingX = Mathf.Sign(visual.localScale.x);

        Vector2 dir = new Vector2(1f * facingX, 0.8f); 

        GameObject mineGO = Instantiate(minePrefab, mineSpawnPoint.position, Quaternion.identity);

        Mine mine = mineGO.GetComponent<Mine>();
        if (mine != null)
            mine.Launch(dir);

        currentMines--;
    }
}
