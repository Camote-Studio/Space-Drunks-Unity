using UnityEngine;

public class PlayerShip : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float speed = 8f;

    [Header("Disparo")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePoint;

    void Update()
    {
        Move();
        Shoot();
    }

    void Move()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, moveY, 0);
        transform.position += movement * speed * Time.deltaTime;
    }

    void Shoot()
    {
        if (Input.GetMouseButtonDown(0)) // click izquierdo
        {
            GameObject bulletObj = Instantiate(
                bulletPrefab,
                firePoint.position,
                Quaternion.identity
            );

            Bullet bullet = bulletObj.GetComponent<Bullet>();

            if (bullet != null)
            {
                bullet.Initialize(Vector2.right, false); // bala normal
            }
        }
    }
}