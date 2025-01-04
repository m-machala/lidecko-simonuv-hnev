using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public List<GameObject> groundPrefabs;
    public GroundManager groundManager;
    public EntityManager entityManager;

    public void Notify(string sender, string message) {

    }
    void Start()
    {
        groundManager.AddGameManager(this);
        entityManager.AddGameManager(this);

        groundManager.SpawnTiles(5, 10, groundPrefabs);
    }

    void Update()
    {
        
    }
}
