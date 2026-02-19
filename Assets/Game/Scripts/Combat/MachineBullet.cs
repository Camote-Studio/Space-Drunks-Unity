using UnityEngine;

public class MachineBullet : MonoBehaviour
{
    [Header("MachineBullet Ajustes")]
    [SerializeField] private float speed = 25f;
    [SerializeField] private float damage = 3f;
    [SerializeField] private float lifeTimeNormal = 4f;

    private Vector2 direction = Vector2.right;
    private float timer;

    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;
        timer = 0f;

        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (direction.x >= 0 ? 1f : -1f);
        transform.localScale = s;
    }

    private void Awake()
    {
        timer = 0f;
    }

    private void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        timer += Time.deltaTime;

        if (timer >= lifeTimeNormal)
        {
            Destroy(gameObject);
        }
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        enemigo_base enemy = collision.GetComponentInParent<enemigo_base>();

        if (enemy != null)
        {
            enemy.RecibirDaño(damage);
            Destroy(gameObject);
        }
    }
}
