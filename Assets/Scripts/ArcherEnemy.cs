using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcherEnemy : MonoBehaviour, EnemyAI
{
    Animator animator;
    int movementRange = 2;
    float randomMovementChance = 0.25f;
    public void move()
    {
        Debug.Log("ArcherEnemy: Enemy move");
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
            Debug.Log("ArcherEnemy: Moving randomly");
            goal = availablePositions[UnityEngine.Random.Range(0, availablePositions.Count)];
        }
        else {
            if (Vector2.Distance(playerPosition, character.GetPosition()) > 3) {
                Debug.Log("ArcherEnemy: Moving closer to the player");
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
            else if(Vector2.Distance(playerPosition, character.GetPosition()) <= 1 || GetComponent<Skills>().arrowCount <= 0) {
                Debug.Log("ArcherEnemy: Moving further away from the player");
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
                Debug.Log("ArcherEnemy: Not moving");
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
        
        Vector3 directionToAttacker = character.transform.position - gameManager.player.transform.position;
        directionToAttacker.y = 0;    
        if (directionToAttacker != Vector3.zero) {
            gameManager.player.transform.rotation = Quaternion.LookRotation(directionToAttacker);
        } 

        GetComponent<Skills>().rangedAttack(gameManager.player.GetComponent<Skills>());
    }
}
