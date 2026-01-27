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
    }

    private void Awake()
    {
        timer = 0f;
    }

    private void Update()
    {
        //Movimiento para bala
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        //Control de tiempo de vida de la bala
        timer += Time.deltaTime;

        //Si no chocó con nada, se destruye al pasar el tiempo
        if (timer >= lifeTimeNormal)
        {
            Destroy(gameObject);
        }
    }

    //Aseguremenos que se destruya al salir de la pantalla
    private void OnBecameInvisible() {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {   //Detección de colisión con enemigos
        enemigo_base enemy = collision.GetComponentInParent<enemigo_base>();

        if (enemy != null)
        {
            enemy.RecibirDaño(damage);
            Destroy(gameObject);
        }

        //Si choca con paredes se agrega...
    }
}
