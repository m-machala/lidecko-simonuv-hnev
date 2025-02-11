using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.Scripting;
using static Skills;

public class GameManager : MonoBehaviour
{
    public enum GameState {
        PlayerMoving,
        PlayerAction,
        EnemyMoving,
        EnemyAction
    }

    public GameState gameState = GameState.PlayerMoving;

    public List<Tile> groundPrefabs;
    public GroundManager groundManager;
    public Character player;
    public List<Character> enemies = new List<Character>();

    [Range(0, 100)] public int walkDistance = 5;

    public void FinishedMoving() {
        groundManager.UntintAllTiles();
        FightRange();
    }

    public void ReadyToMove()
    {
        List<UnityEngine.Vector2> blockedTiles = getBlockedPositions();
        List<UnityEngine.Vector2> reachableTiles = groundManager.FindReachableTiles(player.GetPosition(), blockedTiles, walkDistance);
        groundManager.TintTiles(reachableTiles, Color.blue);
    }

    public void FightRange()
    {
        List<UnityEngine.Vector2> blockedTiles = new List<UnityEngine.Vector2>
        {
            player.GetPosition(),
        };
        List<UnityEngine.Vector2> reachableTiles;

        AttackMode attack = player.GetComponent<Skills>().attackMode;
        Debug.Log(attack);
        if (player.GetComponent<Skills>().attackMode == AttackMode.Melee)
        {
            reachableTiles = groundManager.FindReachableTiles(player.GetPosition(), blockedTiles, 1); // p�ed�lat blocked tiles??? Pro� jsem sem dal tu pozn�mku? V�dy� to funguje?
        }
        else
        {
            reachableTiles = groundManager.FindReachableTiles(player.GetPosition(), blockedTiles, 3);
        }
        Debug.Log(reachableTiles);
        groundManager.TintTiles(reachableTiles, Color.red);
    }

    public void TileClicked(UnityEngine.Vector2 position) {
        if (!player.moving && gameState == GameState.PlayerMoving) {
            List<UnityEngine.Vector2> blockedTiles = getBlockedPositions();
            List<UnityEngine.Vector2> reachableTiles = groundManager.FindReachableTiles(player.GetPosition(), blockedTiles, walkDistance);
            if (reachableTiles.Contains(position)) {
                var path = groundManager.FindShortestPath(reachableTiles, player.GetPosition(), position);
                player.Move(path, 0.2f);
                groundManager.UntintAllTiles();
                gameState = GameState.PlayerAction;
            }
        }
        else if(!player.moving && gameState == GameState.PlayerAction)
        {
            List<UnityEngine.Vector2> blockedTiles = new List<UnityEngine.Vector2>
            {
                player.GetPosition(),
            };
            List<UnityEngine.Vector2> reachableTiles;

            if (player.GetComponent<Skills>().attackMode == AttackMode.Melee)
            {
                reachableTiles = groundManager.FindReachableTiles(player.GetPosition(), blockedTiles, 1);
            }
            else
            {
                reachableTiles = groundManager.FindReachableTiles(player.GetPosition(), blockedTiles, 3);
            }

            if (reachableTiles.Contains(position))
            {
                groundManager.UntintAllTiles();
                gameState = GameState.EnemyMoving;
            }
        }
        Debug.Log(gameState);
    }

    List<UnityEngine.Vector2> getBlockedPositions() {
        List<UnityEngine.Vector2> blockedPositions = new List<UnityEngine.Vector2>
        {
            player.GetPosition(),
        };

        foreach (Character enemy in enemies) {
            blockedPositions.Add(enemy.GetPosition());
        }

        return blockedPositions;
    }

    void Start()
    {
        groundManager.setGameManager(this);
        player.setGameManager(this);
        groundManager.SpawnTiles(50, 50, groundPrefabs);
        player.moving = true;
        player.gameObject.AddComponent<Skills>();
        foreach (Character enemy in enemies) { enemy.setGameManager(this); }
        Invoke("ReadyToMove", 1f);
    }

    void Update()
    {
        switch (gameState) {
            case GameState.PlayerMoving:
            if (Input.GetKeyDown(KeyCode.Backspace) && gameState == GameState.PlayerMoving)
            { // Skip pohybu
                Debug.Log("clicledus");
                groundManager.UntintAllTiles();
                gameState = GameState.PlayerAction;
                FightRange();
                FinishedMoving();
                Debug.Log(gameState);
        }   
            break;

            case GameState.PlayerAction:
            if (Input.GetKeyDown(KeyCode.Tab))
            { // Přepnutí režimu boje
                player.GetComponent<Skills>().ToggleAttackMode();
                groundManager.UntintAllTiles();
                FightRange();
            }

            if (Input.GetKeyDown(KeyCode.Backspace))
            { // Skip re�imu boje
                Debug.Log("Clicked");
                groundManager.UntintAllTiles();
                gameState = GameState.EnemyMoving;
                Debug.Log(gameState);
            }
            break;

            case GameState.EnemyMoving:
            break;

            case GameState.EnemyAction:
            break;
        }



        

        
    }
}
