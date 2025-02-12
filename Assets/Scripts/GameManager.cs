using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Scripting;
using static Skills;

public class GameManager : MonoBehaviour
{
    public enum GameState {
        PlayerMoving,
        PlayerAction,
        EnemyMoving,
        EnemyAction,
        GameOver
    }

    public GameState gameState = GameState.PlayerMoving;

    public GameObject meleeEnemyPrefab;

    public List<Tile> groundPrefabs;
    public GroundManager groundManager;
    public Character player;
    public List<(Character, EnemyAI, Skills)> enemies = new List<(Character, EnemyAI, Skills)>();

    [Range(0, 100)] public int walkDistance = 5;

    public void FinishedMoving() {
        if (gameState == GameState.PlayerMoving) {
            groundManager.UntintAllTiles();
            FightRange();
            gameState = GameState.PlayerAction;
        }
    }

    public void ReadyToMove()
    {
        var playerSkills = player.GetComponent<Skills>();
        playerSkills.turnEnder();
        List<UnityEngine.Vector2> blockedTiles = getBlockedPositions();
        List<UnityEngine.Vector2> reachableTiles = groundManager.FindReachableTiles(player.GetPosition(), blockedTiles, walkDistance);
        groundManager.TintTiles(reachableTiles, Color.blue);
    }

    public void ActionComplete() {
        gameState = GameState.EnemyMoving;
        Debug.Log("Enemies moving");

        foreach (var enemy in enemies) {
            if (enemy.Item3.health <= 0) {
                Debug.Log("Enemy died");
                Destroy(enemy.Item1.gameObject);
                enemies.Remove(enemy);
                continue;
            }
            Debug.Log("Move enemy");
            enemy.Item2.move();
        }
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
            //reachableTiles = groundManager.FindReachableTiles(player.GetPosition(), blockedTiles, 1); // p�ed�lat blocked tiles??? Pro� jsem sem dal tu pozn�mku? V�dy� to funguje?
            reachableTiles = groundManager.GetSurroundingTiles(player.GetPosition(), 1);
        }
        else if (player.GetComponent<Skills>().attackMode == AttackMode.Ranged)
        {
            //reachableTiles = groundManager.FindReachableTiles(player.GetPosition(), blockedTiles, 3);
            reachableTiles = groundManager.GetSurroundingTiles(player.GetPosition(), 3);
        }
        else if (player.GetComponent<Skills>().attackMode == AttackMode.Fireball)
        { 
            reachableTiles = groundManager.GetSurroundingTiles(player.GetPosition(), 5); // Zatím je fireball na jednoho enemáka. možnost udělat splash damage pokud bude čas
        }
        else if (player.GetComponent<Skills>().attackMode == AttackMode.Bolt)
        {
            reachableTiles = groundManager.GetSurroundingTiles(player.GetPosition(), 2);
        }
        else
        {
            reachableTiles = new List<UnityEngine.Vector2> { player.GetPosition() };
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
            List<UnityEngine.Vector2> reachableTiles;
            
            var playerSkills = player.GetComponent<Skills>();
            var attackMode = playerSkills.attackMode;

            switch (attackMode) {
                case AttackMode.Melee:
                reachableTiles = groundManager.GetSurroundingTiles(player.GetPosition(), 1);
                break;

                case AttackMode.Ranged:
                reachableTiles = groundManager.GetSurroundingTiles(player.GetPosition(), 3);
                break;

                case AttackMode.Fireball:
                reachableTiles = groundManager.GetSurroundingTiles(player.GetPosition(), 5);
                break;

                case AttackMode.Bolt:
                reachableTiles = groundManager.GetSurroundingTiles(player.GetPosition(), 2);
                break;

                default:
                reachableTiles = new List<UnityEngine.Vector2> { player.GetPosition() };
                break;
            }


            if (reachableTiles.Contains(position))
            {
                groundManager.UntintAllTiles();

                switch (attackMode) {
                    case AttackMode.Melee:
                    foreach (var enemy in enemies) {
                        if (enemy.Item1.GetPosition() == position) {
                            playerSkills.meleeAttack(enemy.Item3);
                        }
                    }
                    break;

                    case AttackMode.Ranged:
                    foreach (var enemy in enemies) {
                        if (enemy.Item1.GetPosition() == position) {
                            playerSkills.arrowAttack(enemy.Item3);
                        }
                    }
                    break;

                    case AttackMode.Fireball:
                    if (playerSkills.mana < playerSkills.fireballCost) return;
                    var fireballTargetTiles = groundManager.GetSurroundingTiles(position, 3);
                    Skills mainTarget = null;
                    List<Skills> surroundingTargets = new List<Skills>();

                    foreach (var enemy in enemies) {
                        if (enemy.Item1.GetPosition() == position) {
                            mainTarget = enemy.Item3;
                        }
                        else {
                            if (fireballTargetTiles.Contains(enemy.Item1.GetPosition())) {
                                surroundingTargets.Add(enemy.Item3);
                            }
                        }
                    }

                    if (mainTarget != null) {
                        playerSkills.fireballAttack(mainTarget, surroundingTargets);
                    }
                    break;

                    case AttackMode.Bolt:
                    if (playerSkills.mana < playerSkills.boltCost) return;
                    List<UnityEngine.Vector2> hitPositions = new List<UnityEngine.Vector2>();
                    hitPositions.Add(position);

                    while (true) {
                        if (UnityEngine.Random.Range(0, 1) > 0.3) {
                            break;
                        }

                        var positionCandidates = groundManager.GetSurroundingTiles(hitPositions.Last(), 3);
                        var selectedCandidate = positionCandidates[UnityEngine.Random.Range(0, positionCandidates.Count)];

                        if (hitPositions.Contains(selectedCandidate)) {
                            break;
                        }

                        hitPositions.Add(selectedCandidate);
                    }


                    List<Skills> targets = new List<Skills>();
                    foreach (var enemy in enemies) {
                        if (hitPositions.Contains(enemy.Item1.GetPosition())) {
                            targets.Add(enemy.Item3);
                        }
                    }

                    playerSkills.boltAttack(targets);
                    break;

                    default:
                    if (playerSkills.mana < playerSkills.healCost) return;
                    playerSkills.heal();
                    break;
                }
                Debug.Log(playerSkills.maxMana);
                Debug.Log(playerSkills.mana);
                ActionComplete();
            }
        }
        Debug.Log(gameState);
    }



    public List<UnityEngine.Vector2> getBlockedPositions() {
        List<UnityEngine.Vector2> blockedPositions = new List<UnityEngine.Vector2>
        {
            player.GetPosition(),
        };

        foreach (var enemy in enemies) {
            blockedPositions.Add(enemy.Item1.GetPosition());
            
            if (enemy.Item1.nextPositions.Count > 0) {
                blockedPositions.Add(enemy.Item1.nextPositions.Last().Item1);
            }
        }

        return blockedPositions;
    }

    void Start()
    {
        groundManager.setGameManager(this);
        player.setGameManager(this);
        groundManager.SpawnTiles(50, 50, groundPrefabs);
        player.gameObject.AddComponent<Skills>();
        
        // TODO: implement enemy loading from a list of positions
        var testEnemy = Instantiate(meleeEnemyPrefab, new UnityEngine.Vector3(3f, 1.2f, 3f), UnityEngine.Quaternion.identity);
        enemies.Add((testEnemy.GetComponent<Character>(), testEnemy.GetComponent<EnemyAI>(), testEnemy.GetComponent<Skills>()));
        testEnemy = Instantiate(meleeEnemyPrefab, new UnityEngine.Vector3(1f, 1.2f, 6f), UnityEngine.Quaternion.identity);
        enemies.Add((testEnemy.GetComponent<Character>(), testEnemy.GetComponent<EnemyAI>(), testEnemy.GetComponent<Skills>()));
        testEnemy = Instantiate(meleeEnemyPrefab, new UnityEngine.Vector3(4f, 1.2f, 5f), UnityEngine.Quaternion.identity);
        enemies.Add((testEnemy.GetComponent<Character>(), testEnemy.GetComponent<EnemyAI>(), testEnemy.GetComponent<Skills>()));

        foreach (var enemy in enemies) { enemy.Item1.setGameManager(this); }
        Invoke("ReadyToMove", 1f);
    }

    void Update()
    {
        switch (gameState) {
            case GameState.PlayerMoving:
            if (Input.GetKeyDown(KeyCode.Backspace)/* && gameState == GameState.PlayerMoving*/)
            { // Skip pohybu
                Debug.Log("clicledus");
                groundManager.UntintAllTiles();
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

            if (Input.GetKeyDown(KeyCode.Q))
            { // Přepnutí režimu boje
                player.GetComponent<Skills>().ToggleFireball();
                groundManager.UntintAllTiles();
                FightRange();
            }

            if (Input.GetKeyDown(KeyCode.W))
            { // Přepnutí režimu boje
                player.GetComponent<Skills>().ToggleBolt();
                groundManager.UntintAllTiles();
                FightRange();
            }

            if (Input.GetKeyDown(KeyCode.E))
            { // Přepnutí režimu boje
                player.GetComponent<Skills>().ToggleHeal();
                groundManager.UntintAllTiles();
                FightRange();
            }

            if (Input.GetKeyDown(KeyCode.Backspace))
            { // Skip re�imu boje
                Debug.Log("Clicked");
                groundManager.UntintAllTiles();
                Debug.Log(gameState);
                ActionComplete();
            }
            break;

            case GameState.EnemyMoving:
            bool enemiesFinishedMoving = true;

            foreach (var enemy in enemies) {
                if (enemy.Item1.moving) {
                    enemiesFinishedMoving = false;
                    break;
                }
            }

            if (enemiesFinishedMoving) {
                Debug.Log("Enemy action");
                gameState = GameState.EnemyAction;

                foreach (var enemy in enemies) {
                    enemy.Item2.attack();
                    Debug.Log(player.GetComponent<Skills>().health);
                    enemy.Item3.turnEnder();
                }
                if (player.GetComponent<Skills>().health <= 0) {
                    Debug.Log("Game over");
                    gameState = GameState.GameOver;
                }
            }
            break;

            case GameState.EnemyAction:
            gameState = GameState.PlayerMoving;
            ReadyToMove();
            break;
        }
  
    }
}
