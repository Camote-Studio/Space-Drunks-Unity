using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private LocalPlayer playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private readonly List<PlayerBase> players = new();

    private void Start()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            var p = Instantiate(playerPrefab, spawnPoints[i].position, Quaternion.identity);
            var input = p.GetComponent<PlayerInput>();
            players.Add(p);
        }
    }
}
