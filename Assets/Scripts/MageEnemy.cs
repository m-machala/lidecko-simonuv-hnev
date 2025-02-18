using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageEnemy : MonoBehaviour, EnemyAI
{
    Animator animator;
    int movementRange = 2;
    float randomMovementChance = 0.25f;

    public void move()
    {
        Debug.Log("MageEnemy: Enemy move");
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
            Debug.Log("MageEnemy: Moving randomly");
            goal = availablePositions[UnityEngine.Random.Range(0, availablePositions.Count)];
        }
        else {
            if (Vector2.Distance(playerPosition, character.GetPosition()) > 5) {
                Debug.Log("MageEnemy: Moving closer to the player");
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
            else if(Vector2.Distance(playerPosition, character.GetPosition()) <= 3 || GetComponent<Skills>().mana < GetComponent<Skills>().fireballCost) {
                Debug.Log("MageEnemy: Moving further away from the player");
                float furthestDistance = 0;
                Vector2 furthestVector = playerPosition;

                foreach (Vector2 position in availablePositions) {
                    float calculatedDistance = gameManager.groundManager.FindShortestPath(gameManager.groundManager.tilePositions, position, playerPosition).Count;
                    if (calculatedDistance > furthestDistance) {
                        furthestDistance = calculatedDistance;
                        furthestVector = position;
                    }
                }
                goal = furthestVector;
            }
            else {
                Debug.Log("MageEnemy: Not moving");
                return;
            }
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
        
        if (Vector2.Distance(character.GetPosition(), gameManager.player.GetPosition()) <= 5) {
            GetComponent<Skills>().fireballAttack(gameManager.player.GetComponent<Skills>(), new List<Skills>());
            animator.SetTrigger("fireballAttack");

            float attackAnimationLength = animator.GetCurrentAnimatorStateInfo(0).length * 1.3f;            
            StartCoroutine(TriggerGetHitWithDelay(gameManager.player.animator, attackAnimationLength));
        }
    }

    private IEnumerator TriggerGetHitWithDelay(Animator playerAnimator, float delay)
    {
        yield return new WaitForSeconds(delay);
        playerAnimator.SetTrigger("getHit");
    }
}
