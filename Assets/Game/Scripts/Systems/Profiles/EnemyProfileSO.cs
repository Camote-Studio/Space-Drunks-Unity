using UnityEngine;

[CreateAssetMenu(fileName = "EnemyProfile", menuName = "Space Drunks/Enemy Profile", order = 1)]
public class EnemyProfileSO : ScriptableObject
{
    [Header("Configuración visual")]
    public string enemyID; // ej: Quacko
    public GameObject prefab; // Prefab del enemigo

    [Header("Estadísticas")]
    public float maxHealth = 100f; // Salud máxima del enemigo
    public float moveSpeed = 2f; // Velocidad de movimiento del enemigo 
    public int scoreValue = 10; // Puntos otorgados al derrotar al enemigo
    public float damage = 10f; // Daño que inflige el enemigo al jugador

    [Header("Física")]
    public float acceleration = 8f; // Aceleración del enemigo


    //Se podría añadir más configuraciones, o crear una clase heredada 
    // para enemigos específicos (gato, pulpo, etc.)

}
