using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<Tile> groundPrefabs;
    public GroundManager groundManager;
    public Player player;

    [Range(0, 100)] public int walkDistance = 3;

    void Start()
    {
        groundManager.SpawnTiles(50, 50, groundPrefabs);
    }

    void Update()
    {
        groundManager.UntintAllTiles();

        List<Vector2> reachableTiles = groundManager.FindReachableTiles(player.GetPosition(), new List<Vector2>(), walkDistance);

        groundManager.TintTiles(reachableTiles, Color.blue);
    }
}
