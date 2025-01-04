using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector3 GetPosition() {
        return transform.position;
    }

    public Vector2 GetXZPosition() {
        Vector3 position = GetPosition();
        Vector2 XZPosition = new Vector2(position.x, position.z);
        return XZPosition;
    }
}
