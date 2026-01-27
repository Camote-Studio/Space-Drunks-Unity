using UnityEngine;

public class MachineWeapon : WeaponBase
{
    // ==========================================
    // REFERENCIAS
    // ==========================================
    [Header("Refs")]
    [SerializeField] private Transform firePoint;        // Punto desde donde salen las balas
    [SerializeField] private GameObject bulletPrefab;    // El prefab de la bala (debe tener el script MachineBullet)
    [SerializeField] private PlayerAnimation playerAnimation; // Referencia para animaciones
    [SerializeField] private PlayerMovement movement;    // Referencia para bloquear movimiento

    // ==========================================
    // CONFIGURACIÓN
    // ==========================================
    [Header("Lógica de máquina")]
    [SerializeField] private float activeDuration = 18f;   // Tiempo que dura el arma activa
    [SerializeField] private float cooldownDuration = 24f; // Tiempo de espera para volver a usarla

    [SerializeField] private float setupDuration = 1.0f;

    [Header("Disparo")]
    [SerializeField] private int bulletsPerBurst = 8;      // Cuántas balas salen en una ráfaga
    [SerializeField] private float timeBetweenBullets = 0.1f; // Velocidad entre bala y bala de la ráfaga
    [SerializeField] private float burstCooldown = 0.5f;   // Tiempo de espera entre ráfaga y ráfaga

    [Header("Apuntado")]
    [SerializeField] private bool useGamepadAim = false;
    [SerializeField] private string aimHorizontalAxis = "AimHorizontal";
    [SerializeField] private string aimVerticalAxis = "AimVertical";

    // ==========================================
    // VARIABLES DE ESTADO (Privadas)
    // ==========================================
    private bool isActive;           // ¿Está el arma activada disparando?
    private bool isSettingUp;        // ¿Está en proceso de activación?
    private float activeTimer;       // Temporizador de duración de la habilidad
    private float cooldownTimer;     // Temporizador de enfriamiento
    private float setupTimer;        // Temporizador de preparación
    private int bulletsLeftInBurst;  // Balas que faltan por salir en la ráfaga actual
    private float shotTimer;         // Temporizador para controlar el ritmo de disparo
    private bool burstInProgress;    // ¿Estamos en medio de una ráfaga automática?

    private Vector2 lastShootDirection = Vector2.right; // Última dirección válida para no disparar a (0,0)
    
    // Variable para guardar la cámara y no usar Camera.main en cada frame (Optimización)
    private Camera _mainCamera;

    /// <summary>
    /// Awake se ejecuta una sola vez al iniciar.
    /// Aquí obtenemos referencias y cacheamos la cámara para mejorar el rendimiento.
    /// </summary>
    private void Awake()
    {
        _mainCamera = Camera.main;

        // Si no se asignaron manualmente en el inspector, intentamos buscarlos en el padre
        if (playerAnimation == null)
            playerAnimation = GetComponentInParent<PlayerAnimation>();
        if (movement == null)
            movement = GetComponentInParent<PlayerMovement>();
    }

    // Propiedades públicas para saber el estado del arma desde otros scripts
    public bool IsReady => !isActive && !isSettingUp;
    public bool IsActive => isActive;
    public bool CanActivate => !isActive && cooldownTimer <= 0f;

    /// <summary>
    /// Intenta activar el modo "Machine Gun".
    /// Resetea los temporizadores y bloquea el movimiento del jugador.
    /// </summary>
    public void TryActivate()
    {
        if (!CanActivate) return; // Si está en cooldown o ya activa, no hace nada

        isActive = true;
        isSettingUp = true; // Comenzamos la fase de transformación
        setupTimer = setupDuration; //Inicimaos el timer de la transformación

        activeTimer = activeDuration;

        //Reseteo de disparo
        bulletsLeftInBurst = 0;
        shotTimer = 0f;
        burstInProgress = false;

        // Bloqueamos el movimiento del personaje para que dispare quieto (tipo torreta)
        if (movement != null)
            movement.SetMovementLocked(true);

        playerAnimation?.PlayMachineStart();
    }

    /// <summary>
    /// Tick reemplaza al Update. Se llama desde el controlador del jugador.
    /// Maneja toda la lógica temporal: Cooldowns, duración activa y frecuencia de disparo.
    /// </summary>
    /// <param name="fireDown">Se presionó el botón justo ahora</param>
    /// <param name="fireHeld">El botón se mantiene presionado</param>
    /// <param name="fireUp">Se soltó el botón</param>
    public override void Tick(bool fireDown, bool fireHeld, bool fireUp)
    {
        // 1. Reducir el cooldown si existe
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        // 2. Si el arma NO está activa, comprobamos si el jugador quiere activarla
        if (!isActive)
        {
            if (fireDown && CanActivate)
                TryActivate();
            else
                return; // Si no está activa, no hacemos nada más
        }

        // 3. Controlar el tiempo de vida de la habilidad (ej. 18 segundos)
        activeTimer -= Time.deltaTime;
        if (activeTimer <= 0f)
        {
            EndMachine();
            return;
        }

        if (isSettingUp)
        {
            // Estamos en la fase de transformación
            setupTimer -= Time.deltaTime;
            if (setupTimer <= 0f)
            {
                isSettingUp = false; // Terminó la transformación
            }
            else
            {
                return; // Mientras se transforma, no puede disparar
            }
        }

        // 4. Controlar el ritmo de disparo (cadencia)
        if (shotTimer > 0f)
        {
            shotTimer -= Time.deltaTime;
            return; // Si estamos esperando entre balas, salimos
        }

        // 5. Iniciar una nueva ráfaga si no estamos disparando ya
        if (!burstInProgress)
        {
            // Si NO presiona ni mantiene el botón, no disparamos
            if (!fireDown && !fireHeld)
                return;

            // Iniciamos la ráfaga
            bulletsLeftInBurst = bulletsPerBurst;
            burstInProgress = true;
        }
        

        // 6. Ejecutar el disparo
        Vector2 dir = GetAimDirection();
        FireOnce(dir);
        lastShootDirection = dir;

        // 7. Lógica de la ráfaga
        bulletsLeftInBurst--;
        if (bulletsLeftInBurst > 0)
        {
            // Aún faltan balas en esta ráfaga, esperar un poco (tiempo corto)
            shotTimer = timeBetweenBullets;
        }
        else
        {
            // Ráfaga terminada, esperar el tiempo de "recuperación" (tiempo largo)
            shotTimer = burstCooldown;
            burstInProgress = false;
        }
    }

    /// <summary>
    /// Se llama cuando se acaba el tiempo de la habilidad (18s).
    /// Desactiva el arma, inicia el cooldown global y devuelve el control de movimiento.
    /// </summary>
    private void EndMachine()
    {
        isActive = false;
        isSettingUp = false;
        cooldownTimer = cooldownDuration;
        bulletsLeftInBurst = 0;
        burstInProgress = false;

        // Desbloqueamos el movimiento del jugador
        if (movement != null)
            movement.SetMovementLocked(false);

        playerAnimation?.PlayMachineEnd();
    }

    /// <summary>
    /// Calcula la dirección de disparo basada en Gamepad o Mouse.
    /// </summary>
    private Vector2 GetAimDirection()
    {
        Vector2 dir = Vector2.zero;

        // Opción A: Joystick (Gamepad)
        if (useGamepadAim)
        {
            float ax = Input.GetAxis(aimHorizontalAxis);
            float ay = Input.GetAxis(aimVerticalAxis);
            dir = new Vector2(ax, ay);
        }

        // Opción B: Ratón (si el joystick no se usa)
        if (dir.sqrMagnitude < 0.01f)
        {
            if (_mainCamera != null && firePoint != null)
            {
                // Convertimos la posición del mouse en pantalla a posición en el mundo
                Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector3 to = mouseWorld - firePoint.position;
                to.z = 0f;
                dir = new Vector2(to.x, to.y);
            }
        }

        // Opción C: Fallback (si no hay input, usar la última dirección conocida)
        if (dir.sqrMagnitude < 0.01f)
            dir = (lastShootDirection.sqrMagnitude > 0.01f) ? lastShootDirection : Vector2.right;

        return dir.normalized;
    }

    /// <summary>
    /// Instancia la bala y configura su dirección.
    /// </summary>
    private void FireOnce(Vector2 dir)
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        // Rotar el punto de salida hacia la dirección de disparo
        dir = dir.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        firePoint.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // Crear la bala
        GameObject bulletGO = Object.Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        
        // --- CAMBIO IMPORTANTE ---
        // Buscamos el script MachineBullet en lugar de Bullet normal
        MachineBullet bullet = bulletGO.GetComponent<MachineBullet>();
        
        if (bullet != null)
        {
            // Inicializamos solo con dirección (la MachineBullet no tiene lógica de carga/tamaño)
            bullet.Initialize(dir);
        }
        else
        {
            Debug.LogWarning("¡El prefab asignado no tiene el script MachineBullet!");
        }

        // Reproducir animación de retroceso o disparo
        playerAnimation?.PlayMachineShoot();
    }

    // Dibuja una línea amarilla en el editor para ver hacia dónde apunta el arma
    private void OnDrawGizmosSelected()
    {
        if (firePoint == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.right * 2f);
    }
}