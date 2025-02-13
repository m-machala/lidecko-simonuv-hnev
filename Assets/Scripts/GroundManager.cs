using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.VisualScripting;
using UnityEngine;

public class GroundManager : MonoBehaviour
{
    private List<Tile> spawnedTiles = new List<Tile>();
    private List<Tile> spawnedObstacles = new List<Tile>();
    private List<Vector2> tilePositions = new List<Vector2>();
    private List<Vector2> obstaclePositions = new List<Vector2>();
    private Quaternion defaultRotation = Quaternion.identity;
    private GameManager gameManager;

    public void setGameManager(GameManager gameManager) {
        this.gameManager = gameManager;
    }

    public void TileClicked(Vector2 position) {
        gameManager.TileClicked(position);
    }
    public void SpawnTiles(int xCount, int zCount, List<Tile> groundPrefabs, List<Tile> obstaclePrefabs, List<Vector2> obstaclePositions) {
        for (int x = 0; x < xCount; x++) {
            for (int z = 0; z < zCount; z++) {
                if (obstaclePositions.Contains(new Vector2(x, z))) {
                    int randomIndex = UnityEngine.Random.Range(0, obstaclePrefabs.Count);
                    Tile newObject = Instantiate(obstaclePrefabs[randomIndex], new Vector3(x, 0, z), defaultRotation);
                    newObject.setGroundManager(this);
                    spawnedObstacles.Add(newObject);
                    obstaclePositions.Add(new Vector2(x, z));

                }
                else
                {
                    int randomIndex = UnityEngine.Random.Range(0, groundPrefabs.Count);
                    Tile newObject = Instantiate(groundPrefabs[randomIndex], new Vector3(x, 0, z), defaultRotation);
                    newObject.setGroundManager(this);
                    spawnedTiles.Add(newObject);
                    tilePositions.Add(new Vector2(x, z));
                }
            }
        }
    }

    public void DespawnTiles() {
        for (int i = 0; i < spawnedTiles.Count; i++) {
            Destroy(spawnedTiles[i]);
        }

        for (int i = 0; i < spawnedObstacles.Count; i++) {
            Destroy(spawnedObstacles[i]);
        }

        spawnedTiles.Clear();
        spawnedObstacles.Clear();
        tilePositions.Clear();
        obstaclePositions.Clear();
    }

    public void UntintAllTiles() {
        for (int i = 0; i < spawnedTiles.Count; i++) {
            spawnedTiles[i].UntintTile();
        }
    }

    public void TintTile(Vector2 position, Color color) {
        if (tilePositions.Contains(position)) {
            spawnedTiles[tilePositions.IndexOf(position)].TintTile(color);
        }
    }

    public void TintTiles(List<Vector2> positions, Color color) {
        for (int i = 0; i < positions.Count; i++) {
            TintTile(positions[i], color);
        }
    }

    public List<Vector2> FindReachableTiles(Vector2 startingPosition, List<Vector2> blockedPositions, int stepCount) {
        var reachableTiles = new List<Vector2>();
        var visited = new HashSet<Vector2>();
        var queue = new Queue<(Vector2 position, int steps)>();

        queue.Enqueue((startingPosition, 0));
        visited.Add(startingPosition);

        while (queue.Count > 0) {
            var (currentPosition, currentSteps) = queue.Dequeue();

            if (currentSteps < stepCount) {
                var neighbors = new List<Vector2> {
                    new Vector2(currentPosition.x + 1, currentPosition.y),
                    new Vector2(currentPosition.x - 1, currentPosition.y),
                    new Vector2(currentPosition.x, currentPosition.y + 1),
                    new Vector2(currentPosition.x, currentPosition.y - 1)
                };

                if(gameManager.gameState == GameManager.GameState.PlayerMoving || gameManager.gameState == GameManager.GameState.EnemyMoving)
                {
                    foreach (var neighbor in neighbors)
                    {
                        if (!visited.Contains(neighbor) && !blockedPositions.Contains(neighbor) && !obstaclePositions.Contains(neighbor) && tilePositions.Contains(neighbor))
                        {
                            queue.Enqueue((neighbor, currentSteps + 1));
                            visited.Add(neighbor);
                            reachableTiles.Add(neighbor);
                        }
                    }
                }else if (gameManager.gameState == GameManager.GameState.PlayerAction || gameManager.gameState == GameManager.GameState.EnemyAction)
                {
                    foreach (var neighbor in neighbors)
                    {
                        if (!visited.Contains(neighbor) && !blockedPositions.Contains(neighbor) && tilePositions.Contains(neighbor))
                        {
                            Debug.Log(neighbor);
                            queue.Enqueue((neighbor, currentSteps + 1));
                            visited.Add(neighbor);
                            reachableTiles.Add(neighbor);
                        }
                    }
                    List<UnityEngine.Vector2> allowed = new List<UnityEngine.Vector2> { };
                    foreach (var enemy in gameManager.enemies)
                    {
                        allowed.Add(enemy.Item1.GetPosition());
                    }
                    reachableTiles = reachableTiles.Intersect(allowed).ToList();
                }
                
            }
        }

        return reachableTiles;
    }

    public List<Vector2> FindShortestPath(List<Vector2> reachableTiles, Vector2 start, Vector2 end) {
        var path = new List<Vector2>();
        var visited = new HashSet<Vector2>();
        var queue = new Queue<(Vector2 position, List<Vector2> path)>();

        queue.Enqueue((start, new List<Vector2> { start }));
        visited.Add(start);

        while (queue.Count > 0) {
            var (currentPosition, currentPath) = queue.Dequeue();

            if (currentPosition == end) {
                return currentPath;
            }

            var neighbors = new List<Vector2> {
                new Vector2(currentPosition.x + 1, currentPosition.y),
                new Vector2(currentPosition.x - 1, currentPosition.y),
                new Vector2(currentPosition.x, currentPosition.y + 1),
                new Vector2(currentPosition.x, currentPosition.y - 1)
            };

            foreach (var neighbor in neighbors) {
                if (reachableTiles.Contains(neighbor) && !visited.Contains(neighbor)) {
                    var newPath = new List<Vector2>(currentPath) { neighbor };
                    queue.Enqueue((neighbor, newPath));
                    visited.Add(neighbor);
                }
            }
        }

        return path;
    }

    public List<Vector2> FindTilesInDirection(List<Vector2> reachableTiles, List<Vector2> blockedTiles, Vector2 start, Vector2 directionVector) {
        List<Vector2> output = new List<Vector2>();

        Vector2 currentPosition = start;

        while (true) {
            currentPosition += directionVector;
            if (reachableTiles.Contains(currentPosition) && !blockedTiles.Contains(currentPosition)) {
                output.Add(currentPosition);
            }
            else {
                break;
            }
        }

        return output;
    }

    public List<Vector2> GetSurroundingTiles(Vector2 center, int distance)
    {
        List<Vector2> surroundingTiles = new List<Vector2>();

        for (int dx = -distance; dx <= distance; dx++)
        {
            for (int dy = -distance; dy <= distance; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                surroundingTiles.Add(new Vector2(center.x + dx, center.y + dy));
            }
        }
        List<UnityEngine.Vector2> allowed = new List<UnityEngine.Vector2> { };
        foreach (var enemy in gameManager.enemies)
        {
            allowed.Add(enemy.Item1.GetPosition());
        }
        surroundingTiles = surroundingTiles.Intersect(allowed).ToList();

        return surroundingTiles;
    }
}
