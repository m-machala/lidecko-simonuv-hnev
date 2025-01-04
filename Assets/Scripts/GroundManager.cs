using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundManager : MonoBehaviour
{
    private List<Tile> spawnedTiles = new List<Tile>();
    public List<Vector2> occupiedTilePositions = new List<Vector2>();
    private List<Vector2> tilePositions = new List<Vector2>();
    private Quaternion defaultRotation = Quaternion.identity;
    private GameManager gameManager;
    public void AddGameManager(GameManager gameManager) {
        this.gameManager = gameManager;
    }

    public void SpawnTiles(int xCount, int zCount, List<Tile> prefabs) {
        for (int x = 0; x < xCount; x++) {
            for (int z = 0; z < zCount; z++) {
                int randomIndex = UnityEngine.Random.Range(0, prefabs.Count);
                Tile newObject = Instantiate(prefabs[randomIndex], new Vector3(x, 0, z), defaultRotation);
                spawnedTiles.Add(newObject);
                tilePositions.Add(new Vector2(x, z));
            }
        }
    }

    void Update() {
        Tile randomTile = spawnedTiles[UnityEngine.Random.Range(0, spawnedTiles.Count)];
        randomTile.TintTile(new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f)));
        randomTile = spawnedTiles[UnityEngine.Random.Range(0, spawnedTiles.Count)];
        randomTile.UntintTile();
    }
}
