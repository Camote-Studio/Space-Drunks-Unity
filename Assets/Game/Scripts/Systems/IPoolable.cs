using UnityEngine;

public interface IPoolable
{
    void OnSpawnFromPool(); // Se invoca al salir del pool (Nacer)
    void OnDespawnToPool(); // Se invoca al regresar al pool (Morir)
}