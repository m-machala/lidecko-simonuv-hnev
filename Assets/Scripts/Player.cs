using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    GameManager gameManager;
    List<(Vector2, float)> nextPositions = new List<(Vector2, float)>();
    Vector2 previousPosition;
    public bool moving = false;
    float elapsedMovementTime = 0f;

    public void setGameManager(GameManager gameManager) {
        this.gameManager = gameManager;
    }

    public Vector2 GetPosition() {
        Vector3 fullPosition = transform.position;
        Renderer renderer = gameObject.GetComponent<Renderer>();
        Vector3 size = renderer.bounds.size;

        Vector2 modifiedPosition = new Vector2((int)(fullPosition.x + size.x / 2), (int)(fullPosition.z + size.z / 2));

        return modifiedPosition;
    }

    public void Move(List<Vector2> positions, float stepTime) {
        foreach (Vector2 position in positions) {
            nextPositions.Add((position, stepTime));
        }
        previousPosition = GetPosition();
        moving = true;
    }

    void Update() {
        if (nextPositions.Count == 0 && moving) {
            moving = false;
            gameManager.PlayerFinishedMoving();
        }

        if (nextPositions.Count == 0) {
            return;
        }
        float travelDistance = Vector2.Distance(previousPosition, nextPositions[0].Item1);
        elapsedMovementTime += Time.deltaTime;
        if (elapsedMovementTime >= nextPositions[0].Item2 * travelDistance) {
            elapsedMovementTime = 0;
            transform.position = new Vector3(nextPositions[0].Item1.x, transform.position.y, nextPositions[0].Item1.y);
            previousPosition = nextPositions[0].Item1;
            nextPositions.RemoveAt(0);
            return;
        }
        transform.position = Vector3.Lerp(new Vector3(previousPosition.x, transform.position.y, previousPosition.y), new Vector3(nextPositions[0].Item1.x, transform.position.y, nextPositions[0].Item1.y), elapsedMovementTime / (nextPositions[0].Item2 * travelDistance));    
    }
}
