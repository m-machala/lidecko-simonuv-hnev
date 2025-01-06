using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.Scripting;

public class GameManager : MonoBehaviour
{
    enum GameState {
        PlayerDeciding,
        PlayerAction,
        EnemyAction
    }

    GameState gameState = GameState.PlayerDeciding;

    public List<Tile> groundPrefabs;
    public GroundManager groundManager;
    public Character player;
    List<Character> enemies = new List<Character>();

    [Range(0, 100)] public int walkDistance = 5;

    public void FinishedMoving() {
        List<UnityEngine.Vector2> blockedTiles = getBlockedPositions();
        List<UnityEngine.Vector2> reachableTiles = groundManager.FindReachableTiles(player.GetPosition(), blockedTiles, walkDistance);
        groundManager.TintTiles(reachableTiles, Color.blue);
    }

    public void TileClicked(UnityEngine.Vector2 position) {
        if (!player.moving) {
            List<UnityEngine.Vector2> blockedTiles = getBlockedPositions();
            List<UnityEngine.Vector2> reachableTiles = groundManager.FindReachableTiles(player.GetPosition(), blockedTiles, walkDistance);
            if (reachableTiles.Contains(position)) {
                var path = groundManager.FindShortestPath(reachableTiles, player.GetPosition(), position);
                player.Move(path, 0.2f);
                groundManager.UntintAllTiles();
            }
        }
    }

    List<UnityEngine.Vector2> getBlockedPositions() {
        List<UnityEngine.Vector2> blockedPositions = new List<UnityEngine.Vector2>
        {
            player.GetPosition(),
        };
        return blockedPositions;
    }

    void Start()
    {
        
        
        groundManager.setGameManager(this);
        player.setGameManager(this);
        groundManager.SpawnTiles(50, 50, groundPrefabs);
        player.moving = true;
    }

    void Update()
    {
        switch (gameState) {
            case GameState.PlayerDeciding:
            break;

            case GameState.PlayerAction:
            break;

            case GameState.EnemyAction:
            break;
        }
    }
}
