using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundManager : MonoBehaviour
{
    public List<GameObject> prefabs;
    private List<GameObject> spawnedObjects = new List<GameObject>();

    public Quaternion defaultRotation = Quaternion.identity;
    void Start()
    {
        for (int x = -5; x < 5; x++) {
            for (int z = -5; z < 5; z++) {
                int randomIndex = UnityEngine.Random.Range(0, prefabs.Count);
                GameObject newObject = Instantiate(prefabs[randomIndex], new Vector3(x, 0, z), defaultRotation);
                spawnedObjects.Add(newObject);
            }
        }
    }

    float elapsedTime = 0.0f;
    bool forward = true;
    GameObject selectedTile = null;
    Renderer selectedRenderer = null;
    Color oldColor;
    void Update() {
        if (forward) {
            elapsedTime += Time.deltaTime;
        }
        else {
            elapsedTime -= Time.deltaTime;
        }
        
        if(elapsedTime >= 0.2f && forward) {
            selectedTile = spawnedObjects[UnityEngine.Random.Range(0, spawnedObjects.Count)];
            selectedRenderer = selectedTile.GetComponent<Renderer>();
            oldColor = selectedRenderer.material.color;
            selectedRenderer.material.color = Color.red;
            forward = false;
        }

        if(elapsedTime <= 0.0f && !forward) {
            selectedRenderer.material.color = oldColor;
            forward = true;
        }
    }
}
