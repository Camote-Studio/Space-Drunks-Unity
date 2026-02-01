    using UnityEngine;
    using System.Collections.Generic;

    public class PoolManager : MonoBehaviour
    {
        //Singleton para acceso global desde GunWeapon, WaveManager, etc
        public static PoolManager Instance;

        [System.Serializable] // Importante para que se vea en el Inspector
        public class Pool
        {
            public string tag; //Id único (ej: "Quacko", "Michifus")
            public GameObject prefab; //Prefab a instanciar
            public int size; //Cantidad inicial de objetos en el pool a pre-calentar
            public bool expandable=true; //¿Puede crecer dinamicamente si se acaban los objetos en el pool?
        }

        public List<Pool> pools; //Configurable desde el inspector
        public Dictionary<string, Queue<GameObject>> poolDictionary; //Diccionario para acceso rápido a los pools

        void Awake()
        {
            //Implementación del patrón Singleton
            if (Instance == null) Instance = this; //Primera instancia
            else Destroy(gameObject); //Destruir instancias adicionales

            //Inicialización del diccionario de pools
            poolDictionary = new Dictionary<string, Queue<GameObject>>();

            InitializePools();
        }

        void InitializePools()
        {
            //Crear cada pool definido en el inspector
            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                //Crear jerarquía en el editor para mantener el orden
                GameObject poolParent = new GameObject(pool.tag + "_Pool");
                poolParent.transform.SetParent(this.transform);

                //Pre-calentar el pool con la cantidad definida de objetos
                for (int i =0; i<pool.size; i++)
                {
                    CreateNewObject(pool.prefab, objectPool, poolParent.transform);
                }

                poolDictionary.Add(pool.tag, objectPool);
            }
        }

        private GameObject CreateNewObject(GameObject prefab, Queue<GameObject> queue, Transform parent)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(parent);
            queue.Enqueue(obj);
            return obj;
        }

        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            // Si no ves este mensaje, el WaveManager NO está llamando a esta función.
            Debug.Log($"[PoolManager] Solicitud de spawn: '{tag}'");


            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning("Pool con tag " + tag + " no existe.");
                return null;
            }

            GameObject objectToSpawn;
            Queue<GameObject> queue = poolDictionary[tag];

            if (queue.Count == 0)
            {
                //Buscar el pool correspondiente para ver si es expandible y no romper juego
                Pool config = pools.Find(x => x.tag == tag);
                if (config != null && config.expandable)
                {
                    Transform parent = transform.Find(tag + "_Pool");
                    //Crear un nuevo objeto y añadirlo al pool
                    objectToSpawn = CreateNewObject(config.prefab, queue, parent);
                    // No lo encolamos porque ya lo vamos a usar
                    // Sacamos el que acabamos de meter para mantener la lógica consistente
                    objectToSpawn = queue.Dequeue();
                }

                else
                {
                    Debug.LogWarning("No hay objetos disponibles en el pool " + tag + " y no es expandible.");
                    return null;
                }
            }
            else
            {
                objectToSpawn = queue.Dequeue();
            }

            //Activación y posicionamiento
            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;

            // Inyección de dependencia de ciclo de vida
            IPoolable[] poolables = objectToSpawn.GetComponents<IPoolable>();
            if (poolables.Length > 0)
            {
                foreach (var p in poolables)
                {
                    p.OnSpawnFromPool();
                }
            }
        else
        {
            Debug.LogWarning("El objeto " + tag + " no implementa IPoolable.");
        }

            return objectToSpawn;
        }

        public void ReturnToPool(string tag, GameObject obj)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogError($"[PoolManager] Intentando devolver objeto a pool inexistente: {tag}");
                Destroy(obj); // Failsafe para no dejar basura
                return;
            }

            // Avisar a todos los scripts que se van a guardar
            IPoolable[] poolables = obj.GetComponents<IPoolable>();
            foreach (var p in poolables)
            {
                p.OnDespawnToPool();
            }

            obj.SetActive(false);
            poolDictionary[tag].Enqueue(obj);
        }
    }
