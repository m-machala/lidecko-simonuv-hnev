using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static GameManager;

public class SkipButton : MonoBehaviour
{
    GameManager gameManager;


    private IEnumerator WaitForGameManager()
    {
        while (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            yield return null;
        }

        Debug.Log("GameManager byl nalezen v SkipButton.");
    }

    void Start()
    {
        StartCoroutine(WaitForGameManager());
    }

    public void DisableSkip()
    {
        Image skip = GameObject.FindWithTag("SkipTag").GetComponent<Image>();

        skip.color = Color.grey;
        skip.raycastTarget = false;
        Debug.Log("Dis··bluju");
    }


    public void EnableSkip()
    {
        Image skip = GameObject.FindWithTag("SkipTag").GetComponent<Image>();

        skip.color = Color.white;
        skip.raycastTarget = true;
    }

    public void SkipStage()
    {
        if (gameManager.gameState == GameState.PlayerMoving)
        {
            gameManager.groundManager.UntintAllTiles();
            gameManager.FightRange();
            gameManager.FinishedMoving();
            gameManager.actionSelection.EnableIcons();
        }
        else if(gameManager.gameState == GameState.PlayerAction)
        {
            gameManager.groundManager.UntintAllTiles();
            gameManager.Invoke("ActionComplete", 1f);
            gameManager.waiting = true;
            gameManager.actionSelection.DisableIcons();
            DisableSkip();
        }
    }
}
