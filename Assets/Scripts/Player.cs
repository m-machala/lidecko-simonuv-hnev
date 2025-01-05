using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Vector2 GetPosition() {
        Vector3 fullPosition = transform.position;
        Renderer renderer = gameObject.GetComponent<Renderer>();
        Vector3 size = renderer.bounds.size;

        Vector2 modifiedPosition = new Vector2((int)(fullPosition.x + size.x / 2), (int)(fullPosition.z + size.z / 2));

        return modifiedPosition;
    }
}
