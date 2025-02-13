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
    public GameObject tankEnemyPrefab;
    public GameObject archerEnemyPrefab;
    public GameObject mageEnemyPrefab;

    public List<Tile> groundPrefabs;
    public List<Tile> obstaclePrefabs;
    public GroundManager groundManager;
    public Character player;
    public List<(Character, EnemyAI, Skills)> enemies = new List<(Character, EnemyAI, Skills)>();

    [Range(0, 100)] public int walkDistance = 5;
    bool waiting = false;

    // New fields for sequential enemy processing
    private int enemyTurnIndex = 0;
    private bool enemyTurnMoveInitiated = false;
    private bool enemyHasAttacked = false;
    private float attackDelayTimer = 0f;

    public void FinishedMoving() {
        if (gameState == GameState.PlayerMoving) {
            groundManager.UntintAllTiles();
            FightRange();
            gameState = GameState.PlayerAction;
        }
    }

    public void ReadyToMove()
    {
        gameState = GameState.PlayerMoving;
        var playerSkills = player.GetComponent<Skills>();
        playerSkills.turnEnder();
        List<UnityEngine.Vector2> blockedTiles = getBlockedPositions();
        List<UnityEngine.Vector2> reachableTiles = groundManager.FindReachableTiles(player.GetPosition(), blockedTiles, walkDistance);
        groundManager.TintTiles(reachableTiles, Color.blue);
        waiting = false;
    }

    public void ActionComplete() {
        // Initialize sequential enemy processing
        enemyTurnIndex = 0;
        enemyTurnMoveInitiated = false;
        enemyHasAttacked = false;
        gameState = GameState.EnemyMoving;
        Debug.Log("Enemies turn started sequentially");
        waiting = false;
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
            reachableTiles = groundManager.GetSurroundingTiles(player.GetPosition(), 1);
        }
        else if (player.GetComponent<Skills>().attackMode == AttackMode.Ranged)
        {
            reachableTiles = groundManager.GetSurroundingTiles(player.GetPosition(), 3);
        }
        else if (player.GetComponent<Skills>().attackMode == AttackMode.Fireball)
        { 
            reachableTiles = groundManager.GetSurroundingTiles(player.GetPosition(), 5);
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
        if (waiting) return;
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
                Invoke("ActionComplete", 1f);
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
        List<UnityEngine.Vector2> obstaclePositions = new List<UnityEngine.Vector2>
        {
            new UnityEngine.Vector2(0, 0),
            new UnityEngine.Vector2(0, 1),
            new UnityEngine.Vector2(0, 23),
            new UnityEngine.Vector2(0, 24),

            new UnityEngine.Vector2(1, 0),
            new UnityEngine.Vector2(1, 24),

            new UnityEngine.Vector2(2, 4),
            new UnityEngine.Vector2(2, 9),
            new UnityEngine.Vector2(2, 15),
            new UnityEngine.Vector2(2, 20),

            new UnityEngine.Vector2(3, 4),
            new UnityEngine.Vector2(3, 9),
            new UnityEngine.Vector2(3, 15),
            new UnityEngine.Vector2(3, 20),

            new UnityEngine.Vector2(4, 2),
            new UnityEngine.Vector2(4, 3),
            new UnityEngine.Vector2(4, 4),
            new UnityEngine.Vector2(4, 9),
            new UnityEngine.Vector2(4, 15),
            new UnityEngine.Vector2(4, 20),
            new UnityEngine.Vector2(4, 21),
            new UnityEngine.Vector2(4, 22),

            new UnityEngine.Vector2(7, 4),
            new UnityEngine.Vector2(7, 12),
            new UnityEngine.Vector2(7, 20),

            new UnityEngine.Vector2(8, 4),
            new UnityEngine.Vector2(8, 11),
            new UnityEngine.Vector2(8, 12),
            new UnityEngine.Vector2(8, 13),
            new UnityEngine.Vector2(8, 20),

            new UnityEngine.Vector2(9, 4),
            new UnityEngine.Vector2(9, 10),
            new UnityEngine.Vector2(9, 11),
            new UnityEngine.Vector2(9, 12),
            new UnityEngine.Vector2(9, 13),
            new UnityEngine.Vector2(9, 14),
            new UnityEngine.Vector2(9, 20),

            new UnityEngine.Vector2(10, 4),
            new UnityEngine.Vector2(10, 9),
            new UnityEngine.Vector2(10, 10),
            new UnityEngine.Vector2(10, 11),
            new UnityEngine.Vector2(10, 12),
            new UnityEngine.Vector2(10, 13),
            new UnityEngine.Vector2(10, 14),
            new UnityEngine.Vector2(10, 15),
            new UnityEngine.Vector2(10, 20),

            new UnityEngine.Vector2(11, 4),
            new UnityEngine.Vector2(11, 8),
            new UnityEngine.Vector2(11, 9),
            new UnityEngine.Vector2(11, 10),
            new UnityEngine.Vector2(11, 11),
            new UnityEngine.Vector2(11, 12),
            new UnityEngine.Vector2(11, 13),
            new UnityEngine.Vector2(11, 14),
            new UnityEngine.Vector2(11, 15),
            new UnityEngine.Vector2(11, 16),
            new UnityEngine.Vector2(11, 20),

            new UnityEngine.Vector2(12, 4),
            new UnityEngine.Vector2(12, 7),
            new UnityEngine.Vector2(12, 8),
            new UnityEngine.Vector2(12, 9),
            new UnityEngine.Vector2(12, 10),
            new UnityEngine.Vector2(12, 11),
            new UnityEngine.Vector2(12, 12),
            new UnityEngine.Vector2(12, 13),
            new UnityEngine.Vector2(12, 14),
            new UnityEngine.Vector2(12, 15),
            new UnityEngine.Vector2(12, 16),
            new UnityEngine.Vector2(12, 17),
            new UnityEngine.Vector2(12, 20),

            new UnityEngine.Vector2(13, 4),
            new UnityEngine.Vector2(13, 8),
            new UnityEngine.Vector2(13, 9),
            new UnityEngine.Vector2(13, 10),
            new UnityEngine.Vector2(13, 11),
            new UnityEngine.Vector2(13, 12),
            new UnityEngine.Vector2(13, 13),
            new UnityEngine.Vector2(13, 14),
            new UnityEngine.Vector2(13, 15),
            new UnityEngine.Vector2(13, 16),
            new UnityEngine.Vector2(13, 20),

            new UnityEngine.Vector2(14, 4),
            new UnityEngine.Vector2(14, 9),
            new UnityEngine.Vector2(14, 10),
            new UnityEngine.Vector2(14, 11),
            new UnityEngine.Vector2(14, 12),
            new UnityEngine.Vector2(14, 13),
            new UnityEngine.Vector2(14, 14),
            new UnityEngine.Vector2(14, 15),
            new UnityEngine.Vector2(14, 20),

            new UnityEngine.Vector2(15, 4),
            new UnityEngine.Vector2(15, 10),
            new UnityEngine.Vector2(15, 11),
            new UnityEngine.Vector2(15, 12),
            new UnityEngine.Vector2(15, 13),
            new UnityEngine.Vector2(15, 14),
            new UnityEngine.Vector2(15, 20),

            new UnityEngine.Vector2(16, 4),
            new UnityEngine.Vector2(16, 11),
            new UnityEngine.Vector2(16, 12),
            new UnityEngine.Vector2(16, 13),
            new UnityEngine.Vector2(16, 20),

            new UnityEngine.Vector2(17, 4),
            new UnityEngine.Vector2(17, 12),
            new UnityEngine.Vector2(17, 20),

            new UnityEngine.Vector2(20, 2),
            new UnityEngine.Vector2(20, 3),
            new UnityEngine.Vector2(20, 4),
            new UnityEngine.Vector2(20, 9),
            new UnityEngine.Vector2(20, 15),
            new UnityEngine.Vector2(20, 20),
            new UnityEngine.Vector2(20, 21),
            new UnityEngine.Vector2(20, 22),

            new UnityEngine.Vector2(21, 4),
            new UnityEngine.Vector2(21, 9),
            new UnityEngine.Vector2(21, 15),
            new UnityEngine.Vector2(21, 20),

            new UnityEngine.Vector2(22, 4),
            new UnityEngine.Vector2(22, 9),
            new UnityEngine.Vector2(22, 15),
            new UnityEngine.Vector2(22, 20),

            new UnityEngine.Vector2(23, 0),
            new UnityEngine.Vector2(23, 24),

            new UnityEngine.Vector2(24, 0),
            new UnityEngine.Vector2(24, 1),
            new UnityEngine.Vector2(24, 23),
            new UnityEngine.Vector2(24, 24),
        };
        
        groundManager.SpawnTiles(25, 25, groundPrefabs, obstaclePrefabs, obstaclePositions);
        player.gameObject.AddComponent<Skills>();
        
        
        var testEnemy = Instantiate(meleeEnemyPrefab, new UnityEngine.Vector3(2f, 1.2f, 2f), UnityEngine.Quaternion.identity);
        enemies.Add((testEnemy.GetComponent<Character>(), testEnemy.GetComponent<EnemyAI>(), testEnemy.GetComponent<Skills>()));
        testEnemy = Instantiate(meleeEnemyPrefab, new UnityEngine.Vector3(2f, 1.2f, 22f), UnityEngine.Quaternion.identity);
        enemies.Add((testEnemy.GetComponent<Character>(), testEnemy.GetComponent<EnemyAI>(), testEnemy.GetComponent<Skills>()));
        testEnemy = Instantiate(tankEnemyPrefab, new UnityEngine.Vector3(3f, 1.2f, 7f), UnityEngine.Quaternion.identity);
        enemies.Add((testEnemy.GetComponent<Character>(), testEnemy.GetComponent<EnemyAI>(), testEnemy.GetComponent<Skills>()));
        testEnemy = Instantiate(tankEnemyPrefab, new UnityEngine.Vector3(3f, 1.2f, 17f), UnityEngine.Quaternion.identity);
        enemies.Add((testEnemy.GetComponent<Character>(), testEnemy.GetComponent<EnemyAI>(), testEnemy.GetComponent<Skills>()));
        testEnemy = Instantiate(archerEnemyPrefab, new UnityEngine.Vector3(21f, 1.2f, 7f), UnityEngine.Quaternion.identity);
        enemies.Add((testEnemy.GetComponent<Character>(), testEnemy.GetComponent<EnemyAI>(), testEnemy.GetComponent<Skills>()));
        testEnemy = Instantiate(archerEnemyPrefab, new UnityEngine.Vector3(21f, 1.2f, 17f), UnityEngine.Quaternion.identity);
        enemies.Add((testEnemy.GetComponent<Character>(), testEnemy.GetComponent<EnemyAI>(), testEnemy.GetComponent<Skills>()));
        testEnemy = Instantiate(mageEnemyPrefab, new UnityEngine.Vector3(22f, 1.2f, 2f), UnityEngine.Quaternion.identity);
        enemies.Add((testEnemy.GetComponent<Character>(), testEnemy.GetComponent<EnemyAI>(), testEnemy.GetComponent<Skills>()));
        testEnemy = Instantiate(mageEnemyPrefab, new UnityEngine.Vector3(22f, 1.2f, 22f), UnityEngine.Quaternion.identity);
        enemies.Add((testEnemy.GetComponent<Character>(), testEnemy.GetComponent<EnemyAI>(), testEnemy.GetComponent<Skills>()));

        foreach (var enemy in enemies) { enemy.Item1.setGameManager(this); }
        Invoke("ReadyToMove", 1f);
    }

    void Update()
    {
        switch (gameState) {
            case GameState.PlayerMoving:
                if (Input.GetKeyDown(KeyCode.Backspace))
                { // Skip movement
                    Debug.Log("clicledus");
                    groundManager.UntintAllTiles();
                    FightRange();
                    FinishedMoving();
                    Debug.Log(gameState);
                }   
                break;

            case GameState.PlayerAction:
                if (waiting) {
                    break;
                }
                /*if (Input.GetKeyDown(KeyCode.Tab))
                { // Toggle attack mode
                    player.GetComponent<Skills>().ToggleAttackMode();
                    groundManager.UntintAllTiles();
                    FightRange();
                }

                if (Input.GetKeyDown(KeyCode.Q))
                { // Toggle fireball mode
                    player.GetComponent<Skills>().ToggleFireball();
                    groundManager.UntintAllTiles();
                    FightRange();
                }

                if (Input.GetKeyDown(KeyCode.W))
                { // Toggle bolt mode
                    player.GetComponent<Skills>().ToggleBolt();
                    groundManager.UntintAllTiles();
                    FightRange();
                }

                if (Input.GetKeyDown(KeyCode.E))
                { // Toggle heal mode
                    player.GetComponent<Skills>().ToggleHeal();
                    groundManager.UntintAllTiles();
                    FightRange();
                }*/

                if (Input.GetKeyDown(KeyCode.Backspace))
                { // Skip action phase
                    Debug.Log("Clicked");
                    groundManager.UntintAllTiles();
                    Debug.Log(gameState);
                    Invoke("ActionComplete", 1f);
                    waiting = true;
                }
                break;

            case GameState.EnemyMoving:
                // Process enemies sequentially: one enemy moves then attacks before moving on.
                if (enemyTurnIndex < enemies.Count)
                {
                    var enemy = enemies[enemyTurnIndex];
                    if (enemy.Item3.health <= 0) {
                        Debug.Log("Enemy died");
                        Destroy(enemy.Item1.gameObject);
                        enemies.RemoveAt(enemyTurnIndex);
                        enemyTurnMoveInitiated = false;
                        enemyHasAttacked = false;
                        break;
                    }
                    if (!enemy.Item1.moving && !enemyTurnMoveInitiated) {
                        Debug.Log("Initiating move for enemy " + enemyTurnIndex);
                        enemy.Item2.move();
                        enemyTurnMoveInitiated = true;
                    }
                    if (enemyTurnMoveInitiated && !enemy.Item1.moving) {
                        if (!enemyHasAttacked) {
                            Debug.Log("Enemy " + enemyTurnIndex + " attacking");
                            gameState = GameState.EnemyAction;
                            enemy.Item2.attack();
                            gameState = GameState.EnemyMoving;
                            enemy.Item3.turnEnder();
                            enemyHasAttacked = true;
                            attackDelayTimer = 1f; // Delay after attack
                        } else {
                            attackDelayTimer -= Time.deltaTime;
                            if (attackDelayTimer <= 0f) {
                                enemyTurnIndex++;
                                enemyTurnMoveInitiated = false;
                                enemyHasAttacked = false;
                            }
                        }
                    }
                } else {
                    if (player.GetComponent<Skills>().health <= 0) {
                        Debug.Log("Game over");
                        gameState = GameState.GameOver;
                    } else {
                        Debug.Log("Enemy turn complete");
                        gameState = GameState.EnemyAction;
                    }
                }
                break;

            case GameState.EnemyAction:
                if (waiting) break;
                Invoke("ReadyToMove", 1f);
                waiting = true;
                break;
        }
    }
}
