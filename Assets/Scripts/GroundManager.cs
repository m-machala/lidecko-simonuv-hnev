using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundManager : MonoBehaviour
{
    private List<GameObject> spawnedTiles = new List<GameObject>();
    private List<Vector2> tilePositions = new List<Vector2>();
    private Quaternion defaultRotation = Quaternion.identity;
    private GameManager gameManager;
    public void AddGameManager(GameManager gameManager) {
        this.gameManager = gameManager;
    }

    public void SpawnTiles(int xCount, int zCount, List<GameObject> prefabs) {
        for (int x = 0; x < xCount; x++) {
            for (int z = 0; z < zCount; z++) {
                int randomIndex = UnityEngine.Random.Range(0, prefabs.Count);
                GameObject newObject = Instantiate(prefabs[randomIndex], new Vector3(x, 0, z), defaultRotation);
                spawnedTiles.Add(newObject);
                tilePositions.Add(new Vector2(x, z));
            }
        }
    }
}
