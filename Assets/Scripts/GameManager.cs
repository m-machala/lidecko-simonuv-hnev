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

    public void FinishedAction()
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
            reachableTiles = groundManager.FindReachableTiles(player.GetPosition(), blockedTiles, 1); // p¯edÏlat blocked tiles??? ProË jsem sem dal tu pozn·mku? Vûdyù to funguje?
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
                gameState = GameState.PlayerMoving;
                FinishedAction();
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
        //FinishedAction();    z nÏjakÈho d˘vodu to hodÌ permatint?
    }

    void Update()
    {
        switch (gameState) {
            case GameState.PlayerMoving:
            break;

            case GameState.PlayerAction:
            break;

            case GameState.EnemyAction:
            break;
        }

        if (Input.GetKeyDown(KeyCode.Tab) && gameState == GameState.PlayerAction)
        { // P¯epnutÌ reûimu boje
            player.GetComponent<Skills>().ToggleAttackMode();
            groundManager.UntintAllTiles();
            FightRange();
        }


        //TODO: Tu jsem se snaûil implementovat ten skip akce/pohybu pomocÌ backspace. Nefunguje. Dont know why. Jdu sp·t GL kdokoliv na to bude koukat.
        /*if (Input.GetKeyDown(KeyCode.Backspace) && gameState == GameState.PlayerAction)
        { // Skip reûimu boje
            //groundManager.UntintAllTiles();
            //FightRange();
            Debug.Log("Clicked");
            groundManager.UntintAllTiles();
            gameState = GameState.PlayerMoving;
            //FinishedAction();
            Debug.Log(gameState);
        }

        if (Input.GetKeyDown(KeyCode.Backspace) && gameState == GameState.PlayerMoving)
        { // Skip pohybu
            //groundManager.UntintAllTiles();
            //FightRange();
            Debug.Log("clicledus");
            groundManager.UntintAllTiles();
            gameState = GameState.PlayerAction;
            FinishedMoving();
            Debug.Log(gameState);
        }*/
    }
}
