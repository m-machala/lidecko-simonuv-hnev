using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{   
    private GameManager gameManager;
    public void AddGameManager(GameManager gameManager) {
        this.gameManager = gameManager;
    }
}
