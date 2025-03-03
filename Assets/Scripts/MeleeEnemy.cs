using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeleeEnemy : MonoBehaviour, EnemyAI
{
    Animator animator;
    int movementRange = 2;
    float randomMovementChance = 0.25f;
    public void move() {
        Debug.Log("MeleeEnemy: Enemy move");
        var character = GetComponent<Character>();
        var gameManager = character.gameManager;
        var blockedPositions = gameManager.getBlockedPositions();
        var playerPosition = gameManager.player.GetPosition();
        var availablePositions = gameManager.groundManager.FindReachableTiles(character.GetPosition(), blockedPositions, movementRange);
        
        if(availablePositions.Count == 0) {
            return;
        }
        
        var goal = availablePositions[0];
        if (UnityEngine.Random.Range(0f, 1f) <= randomMovementChance) {
            Debug.Log("MeleeEnemy: Moving randomly");
            goal = availablePositions[UnityEngine.Random.Range(0, availablePositions.Count)];
        }
        else {
            Debug.Log("MeleeEnemy: Moving closer to the player");
            float closestDistance = float.MaxValue;
            Vector2 closestVector = character.GetPosition();

            foreach (Vector2 position in availablePositions) {
                float calculatedDistance = gameManager.groundManager.FindShortestPath(gameManager.groundManager.tilePositions, position, playerPosition).Count;
                if (calculatedDistance < closestDistance) {
                    closestDistance = calculatedDistance;
                    closestVector = position;
                }
            }
            goal = closestVector;
        }
        Debug.Log(goal);
        var pathToGoal = gameManager.groundManager.FindShortestPath(availablePositions, character.GetPosition(), goal);
        character.Move(pathToGoal, 0.2f);
    }

    public void attack()
    {
        var character = GetComponent<Character>();
        animator = GetComponentInChildren<Animator>();
        var gameManager = character.gameManager;
        
        if (Vector2.Distance(character.GetPosition(), gameManager.player.GetPosition()) <= 1.5f) {            
            Vector3 directionToAttacker = character.transform.position - gameManager.player.transform.position;
            directionToAttacker.y = 0;    
            if (directionToAttacker != Vector3.zero) {
                gameManager.player.transform.rotation = Quaternion.LookRotation(directionToAttacker);
            }              
            GetComponent<Skills>().meleeAttack(gameManager.player.GetComponent<Skills>());            
            float offset = 0.85f;           
            gameManager.player.StartCoroutine(triggerAction(character.animator, gameManager.player.animator, offset));
        }
    }

    private IEnumerator triggerAction(Animator attackerAnimation, Animator enemyAnimator, float getHitOffset, float projectileDistanceTime=0.0f)
    {
        attackerAnimation.SetTrigger("attackMelee");
        yield return new WaitForSeconds(getHitOffset);
        enemyAnimator.SetTrigger("getHit");                 
    }
}
