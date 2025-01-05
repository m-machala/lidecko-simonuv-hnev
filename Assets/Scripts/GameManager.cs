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

    public void Event(string sender, string message) {
        ;
    }

    void Start()
    {
        player.setGameManager(this);
        groundManager.SpawnTiles(50, 50, groundPrefabs);
        List<Vector2> stepList = new List<Vector2>
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1)
        };
        player.Move(stepList, 1);
    }

    void Update()
    {
        groundManager.UntintAllTiles();

        List<Vector2> reachableTiles = groundManager.FindReachableTiles(player.GetPosition(), new List<Vector2>(), walkDistance);

        groundManager.TintTiles(reachableTiles, Color.blue);
    }
}
