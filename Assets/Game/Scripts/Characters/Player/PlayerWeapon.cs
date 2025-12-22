using UnityEngine;
using UnityEngine.Rendering;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform visual;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;  

    [Header("Disparo")]
    [SerializeField] private int maxBullets = 20;
    [SerializeField] private float fireInterval = 0.2f;
    [SerializeField] private float reloadTime = 1.2f;


    private float fireCooldown;
    private int bulletsFired;
    private float reloadTimer;
    private bool isReloading;

    private void Awake()
    {
        if (visual == null)
        {
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
                visual = sr.transform;
        }

        if (firePoint == null && visual != null)
        {
            var fp = new GameObject("FirePoint");
            fp.transform.SetParent(visual);
            fp.transform.localPosition = new Vector3(0.5f, 0f, 0f);
            firePoint = fp.transform;
        }
    }

    private void Update()
    {
        if(fireCooldown > 0f)
        {
            fireCooldown -= Time.deltaTime;
        }
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                isReloading = false;
                bulletsFired = 0;
            }
            return;
        }
        if (bulletsFired >= maxBullets)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                isReloading = true;
                reloadTimer = reloadTime;
            }
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }
    }

    private void Fire()
    {
        if (fireCooldown > 0f)
            return;

        float facingX = 1f;
        if (visual != null && visual.localScale.x != 0f)
            facingX = Mathf.Sign(visual.localScale.x);

        Vector2 dir = new Vector2(facingX, 0f);

        GameObject bulletGO = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletGO.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(dir);
        }

        bulletsFired++;
        fireCooldown = fireInterval;
    }
}
