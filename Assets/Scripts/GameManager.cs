using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<Tile> groundPrefabs;
    public GroundManager groundManager;
    public Player player;

    public void Notify(string sender, string message) {
        if (sender == "GroundManager" && message == "UpdateOccupiedTiles") {
            groundManager.occupiedTilePositions.Clear();
            groundManager.occupiedTilePositions.Append(player.GetXZPosition());
        }
    }
    void Start()
    {
        groundManager.AddGameManager(this);

        groundManager.SpawnTiles(5, 10, groundPrefabs);
    }

    void Update()
    {
        
    }
}
