using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float travelTime;
    private float currentTime;
    private bool moving = false;
    public void StartMovement(Vector3 startPosition, Vector3 endPosition, float travelTime) {
        this.startPosition = startPosition;
        this.endPosition = endPosition;
        this.travelTime = travelTime;
        currentTime = 0;
        moving = true;
    }

    void Update()
    {
        if (!moving) return;
        
        currentTime = Math.Min(currentTime + Time.deltaTime, travelTime);
        transform.position = Vector3.Lerp(startPosition, endPosition, currentTime / travelTime);

        if (currentTime >= travelTime) {
            Destroy(gameObject);
        }
    }
}
