using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement; //Screeeeeenus Maxima :)
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
    public ActionSelection actionSelection;
    public SpearCounter spearCounter;
    public SkipButton skipButton;
    public Healthbar healthbar;
    public Manabar manabar;
    public Character player;
    public List<(Character, EnemyAI, Skills, EnemyHealthOrb)> enemies = new List<(Character, EnemyAI, Skills, EnemyHealthOrb)>();
    Animator animator;

    [Range(0, 100)] public int walkDistance = 5;
    public bool waiting = false;

    // New fields for sequential enemy processing
    private int enemyTurnIndex = 0;
    private bool enemyTurnMoveInitiated = false;
    private bool enemyHasAttacked = false;
    public void FinishedMoving() {
        if (gameState == GameState.PlayerMoving) {
            groundManager.UntintAllTiles();
            player.GetComponent<Skills>().attackMode = AttackMode.Melee;
            FightRange();
            gameState = GameState.PlayerAction;
            actionSelection.EnableIcons();
        }
    }

    public void ReadyToMove()
    {
        gameState = GameState.PlayerMoving;
        var playerSkills = player.GetComponent<Skills>();
        playerSkills.turnEnder();
        manabar.SetMana(player.GetComponent<Skills>().mana);
        List<UnityEngine.Vector2> blockedTiles = getBlockedPositions();
        List<UnityEngine.Vector2> reachableTiles = groundManager.FindReachableTiles(player.GetPosition(), blockedTiles, walkDistance);
        groundManager.TintTiles(reachableTiles, Color.blue);
        waiting = false;
        skipButton.EnableSkip();
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

    private IEnumerator triggerAction(string attackType, Animator attackerAnimator, Animator enemyAnimator, float getHitOffset, float projectileDistanceTime=0.0f)
    {
        attackerAnimator.SetTrigger(attackType);
        yield return new WaitForSeconds(getHitOffset);
        enemyAnimator.SetTrigger("getHit");                 
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
                actionSelection.EnableIcons();
                /*gameState = GameState.PlayerAction;
                player.GetComponent<Skills>().ToggleMele();
                FightRange();*/
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
                animator = GetComponentInChildren<Animator>(); 
                groundManager.UntintAllTiles();
                float endWaitTime = 0f;
                
                switch (attackMode) {
                    case AttackMode.Melee:
                        foreach (var enemy in enemies)
                        {
                            if (enemy.Item1.GetPosition() == position)
                            {                                
                                UnityEngine.Vector3 directionToEnemy = enemy.Item1.transform.position - player.transform.position;
                                directionToEnemy.y = 0;
                                if (directionToEnemy !=  UnityEngine.Vector3.zero)
                                {
                                    player.transform.rotation = UnityEngine.Quaternion.LookRotation(directionToEnemy);
                                } 
                                               
                                UnityEngine.Vector3 directionToPlayer = player.transform.position - enemy.Item1.transform.position;
                                directionToPlayer.y = 0;
                                if (directionToPlayer !=  UnityEngine.Vector3.zero)
                                {
                                    enemy.Item1.transform.rotation = UnityEngine.Quaternion.LookRotation(directionToPlayer);
                                }                                                     
                                playerSkills.meleeAttack(enemy.Item3);
                                enemy.Item4.UpdateColor();
                                float playerAttackAnimationLength = player.animator.GetCurrentAnimatorStateInfo(0).length;
                                float enemyGetHitAnimationLength = enemy.Item1.animator.GetCurrentAnimatorStateInfo(0).length;
                                float offset = 0.4f;                                
                                endWaitTime = Math.Max(playerAttackAnimationLength, enemyGetHitAnimationLength + offset);
                                player.StartCoroutine(triggerAction("attackMelee", player.animator, enemy.Item1.animator, offset));      
                            }
                        }
                        break;

                    case AttackMode.Ranged:
                        foreach (var enemy in enemies) {
                            if (enemy.Item1.GetPosition() == position) {
                                playerSkills.arrowAttack(enemy.Item3);
                                enemy.Item4.UpdateColor();
                                endWaitTime = UnityEngine.Vector2.Distance(player.GetPosition(), enemy.Item1.GetPosition()) * 0.5f;
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
                                endWaitTime = UnityEngine.Vector2.Distance(player.GetPosition(), enemy.Item1.GetPosition()) * 0.5f;
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
                            mainTarget.GetComponent<EnemyHealthOrb>().UpdateColor();

                            foreach (var enemy in surroundingTargets) {
                                enemy.GetComponent<EnemyHealthOrb>().UpdateColor();
                            }
                        }
                        break;

                    case AttackMode.Bolt:
                        if (playerSkills.mana < playerSkills.boltCost) return;
                        endWaitTime = 1f;
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
                        foreach (var enemy in targets) {
                            enemy.GetComponent<EnemyHealthOrb>().UpdateColor();
                        }
                        break;
                        
                    default:
                        if (playerSkills.mana < playerSkills.healCost) return;
                        playerSkills.heal();
                        healthbar.SetHealth(playerSkills.health);
                        endWaitTime = 1f;
                        break;
                }
                Debug.Log(playerSkills.maxMana);
                Debug.Log(playerSkills.mana);
                manabar.SetMana(player.GetComponent<Skills>().mana);
                spearCounter.SetSpears(player.GetComponent<Skills>().arrowCount);
                actionSelection.DisableIcons();
                skipButton.DisableSkip();
                Invoke("ActionComplete", endWaitTime);               
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

    private IEnumerator WaitForActionSelection()
    {
        while (actionSelection == null)
        {
            actionSelection = FindObjectOfType<ActionSelection>();
            yield return null;
        }
        Debug.Log("ActionSelection byl nalezen v GameManageru.");
    }

    private IEnumerator WaitForHealthbar()
    {
        while (healthbar == null)
        {
            healthbar = FindObjectOfType<Healthbar>();
            yield return null;
        }
        healthbar.SetMaxHealth(player.GetComponent<Skills>().maxHealth);
        Debug.Log("Healthbar byl nalezen v GameManageru.");
    }


    private IEnumerator WaitForManabar()
    {
        while (manabar == null)
        {
            manabar = FindObjectOfType<Manabar>();
            yield return null;
        }
        manabar.SetMaxMana(player.GetComponent<Skills>().maxMana);
        Debug.Log("Manabar byl nalezen v GameManageru.");
    }

    private IEnumerator WaitForSpears()
    {
        while (spearCounter == null)
        {
            spearCounter = FindObjectOfType<SpearCounter>();
            yield return null;
        }
        spearCounter.SetSpears(player.GetComponent<Skills>().arrowCount);
        Debug.Log("SpearCount byl nalezen v GameManageru.");
    }

    private IEnumerator WaitForSkip()
    {
        while (skipButton == null)
        {
            skipButton = FindObjectOfType<SkipButton>();
            yield return null;
        }

        Debug.Log("Skip byl nalezen v GameManageru.");
    }

    void Start()
    {
        groundManager.setGameManager(this);
        player.setGameManager(this);
        List<UnityEngine.Vector2> obstaclePositions = ObstacleData.Positions;   
        
        groundManager.SpawnTiles(15, 15, groundPrefabs, obstaclePrefabs, obstaclePositions);
        player.gameObject.AddComponent<Skills>();

        var enemy = Instantiate(meleeEnemyPrefab, new UnityEngine.Vector3(1f, 1.2f, 1f), UnityEngine.Quaternion.identity);
        Skills skillsMele = enemy.GetComponent<Skills>();
        Skills skillsTank = enemy.GetComponent<Skills>();
        Skills skillsRanged = enemy.GetComponent<Skills>();
        Skills skillsMage = enemy.GetComponent<Skills>();
        skillsMele.Initialize(meleeDamage: 5, maxHealth: 20, health: 20);
        skillsTank.Initialize(meleeDamage: 3, maxHealth: 40, health: 40);
        skillsRanged.Initialize(meleeDamage: 2, maxHealth: 15, health: 15, arrowCount: 20, arrowDamage: 7, arrowHitChance: 0.9f);
        skillsMage.Initialize(meleeDamage: 1, maxHealth: 10, health: 10, maxMana: 15, mana: 15, manaRegen: 3, fireballMainDamage: 10, fireballSurroundingDamage: 0, fireballCost: 10);

        enemies.Add((enemy.GetComponent<Character>(), enemy.GetComponent<EnemyAI>(), skillsMele, enemy.GetComponent<EnemyHealthOrb>()));
        /*enemy = Instantiate(meleeEnemyPrefab, new UnityEngine.Vector3(13f, 1.2f, 1f), UnityEngine.Quaternion.identity);
        enemies.Add((enemy.GetComponent<Character>(), enemy.GetComponent<EnemyAI>(), enemy.GetComponent<Skills>(), enemy.GetComponent<EnemyHealthOrb>()));
        enemy = Instantiate(tankEnemyPrefab, new UnityEngine.Vector3(1f, 1.2f, 5f), UnityEngine.Quaternion.identity);
        enemies.Add((enemy.GetComponent<Character>(), enemy.GetComponent<EnemyAI>(), enemy.GetComponent<Skills>(), enemy.GetComponent<EnemyHealthOrb>()));*/
        enemy = Instantiate(tankEnemyPrefab, new UnityEngine.Vector3(13f, 1.2f, 5f), UnityEngine.Quaternion.identity);
        enemies.Add((enemy.GetComponent<Character>(), enemy.GetComponent<EnemyAI>(), enemy.GetComponent<Skills>(), enemy.GetComponent<EnemyHealthOrb>()));
        enemy = Instantiate(archerEnemyPrefab, new UnityEngine.Vector3(1f, 1.2f, 9f), UnityEngine.Quaternion.identity);
        enemies.Add((enemy.GetComponent<Character>(), enemy.GetComponent<EnemyAI>(), enemy.GetComponent<Skills>(), enemy.GetComponent<EnemyHealthOrb>()));
        /*enemy = Instantiate(archerEnemyPrefab, new UnityEngine.Vector3(13f, 1.2f, 9f), UnityEngine.Quaternion.identity);
        enemies.Add((enemy.GetComponent<Character>(), enemy.GetComponent<EnemyAI>(), enemy.GetComponent<Skills>(), enemy.GetComponent<EnemyHealthOrb>()));*/
        enemy = Instantiate(mageEnemyPrefab, new UnityEngine.Vector3(1f, 1.2f, 13f), UnityEngine.Quaternion.identity);
        enemies.Add((enemy.GetComponent<Character>(), enemy.GetComponent<EnemyAI>(), enemy.GetComponent<Skills>(), enemy.GetComponent<EnemyHealthOrb>()));
        /*enemy = Instantiate(mageEnemyPrefab, new UnityEngine.Vector3(13f, 1.2f, 13f), UnityEngine.Quaternion.identity);
        enemies.Add((enemy.GetComponent<Character>(), enemy.GetComponent<EnemyAI>(), enemy.GetComponent<Skills>(), enemy.GetComponent<EnemyHealthOrb>()));*/

        foreach (var addedEnemy in enemies) { addedEnemy.Item1.setGameManager(this); }
        Invoke("ReadyToMove", 1f);
        StartCoroutine(WaitForActionSelection());
        StartCoroutine(WaitForHealthbar());
        StartCoroutine(WaitForManabar());
        StartCoroutine(WaitForSpears());
        StartCoroutine(WaitForSkip());
    }

    void Update()    
    {
        float endWaitTime = 0f;
        float offset = 0f;
        string attackType = "";
        animator = GetComponentInChildren<Animator>();        

        switch (gameState) {
            case GameState.EnemyMoving:
                if (enemyTurnIndex < enemies.Count)
                {
                    var enemy = enemies[enemyTurnIndex];

                    if (enemy.Item3.health <= 0) {
                        Debug.Log("Enemy died");
                        Destroy(enemy.Item1.gameObject);
                        enemies.RemoveAt(enemyTurnIndex);
                        enemyTurnMoveInitiated = false;
                        enemyHasAttacked = false;
                        Debug.Log("Enemákus poètus maximus: " + enemies.Count);
                        if (enemies.Count <= 0)
                        {
                            //Zatím toto nevím jestli má smysl sem kvol totomu tahat celej script
                            //Zas možná dyláj pro Simóna heheheheh
                            SceneManager.LoadScene("Victory");
                        }
                        break;
                    }
                    if (!enemy.Item1.moving && !enemyTurnMoveInitiated) {
                        Debug.Log("Initiating move for enemy " + enemyTurnIndex);
                        enemy.Item2.move();
                        enemyTurnMoveInitiated = true;
                    }
                    if (enemyTurnMoveInitiated && !enemy.Item1.moving) {
                        if (!enemyHasAttacked) {                             
                            float realDistance = UnityEngine.Vector2.Distance(enemy.Item1.GetPosition(), player.GetPosition());
                            float attackRange = 0f;                            
                            string currentPrefab = enemy.Item1.gameObject.name;
                            float enemyAttackAnimationLength = enemy.Item1.animator.GetCurrentAnimatorStateInfo(0).length;
                            float playerGetHitAnimationLength = player.animator.GetCurrentAnimatorStateInfo(0).length;          

                            if (currentPrefab == "Melee Enemy(Clone)") {   
                                attackType = "attackMelee";                             
                                attackRange = 1.5f;
                                offset = 0.85f;       
                            } else if (currentPrefab == "Tank Enemy(Clone)") {
                                attackType = "attackMelee"; 
                                attackRange = 1.5f;
                                offset = 0.85f;                      
                            } else if (currentPrefab == "Mage Enemy(Clone)") {
                                attackType = "attackFireball"; 
                                attackRange = 5f;
                                offset = 0.85f;
                            } else if (currentPrefab == "Archer Enemy(Clone)") {
                                attackType = "attackRanged"; 
                                attackRange = 3f;
                                offset = 0.85f;
                            }

                            if (attackRange >= realDistance) {
                                Debug.Log("Enemy " + enemyTurnIndex + " attacking");                          
                                enemy.Item2.attack();                                                         
                                gameState = GameState.EnemyMoving;
                                enemy.Item3.turnEnder();                             
                                Debug.Log("Current prefab: " + enemy.Item1.gameObject.name);              
                                enemyHasAttacked = true;                                                            
                                endWaitTime = Math.Max(enemyAttackAnimationLength, playerGetHitAnimationLength + offset);
                                player.StartCoroutine(triggerAction(attackType, enemy.Item1.animator, player.animator, offset));
                                healthbar.SetHealth(player.GetComponent<Skills>().health);                                            
                            }
                            Invoke("ProcessNextEnemyTurn", endWaitTime);            
                        }
                    }
                    if(player.GetComponent<Skills>().health <= 0)
                    {
                        SceneManager.LoadScene("Lost"); // Dát kdyžtak delay pro hit a dead animaci. (Èeský komentík pro Simóna ìšèøžýáíéúù)
                    }
                } 
                else 
                {
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

    void ProcessNextEnemyTurn()
    {
        enemyTurnIndex++;
        enemyTurnMoveInitiated = false;
        enemyHasAttacked = false;
    }
}
