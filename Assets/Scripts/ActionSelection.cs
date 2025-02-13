using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActionSelection : MonoBehaviour
{
    public GameManager gameManager;
    public Image selectedIcon;


    /*public void setGameManager(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }*/

    private IEnumerator WaitForGameManager()
    {
        while (gameManager == null) // Dokud nen� p�i�azen� GameManager, �ekej
        {
            gameManager = FindObjectOfType<GameManager>();
            yield return null; // Po�kej 1 frame a zkus znovu
        }

        Debug.Log("GameManager byl nalezen v ActionSelection.");
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitForGameManager());
        DisableIcons();
    }

    public void DisableIcons()
    {
        Image meleBackg = GameObject.FindWithTag("MeleTag").GetComponent<Image>();
        Image rangedBackg = GameObject.FindWithTag("RangedTag").GetComponent<Image>();
        Image fireballBackg = GameObject.FindWithTag("FireballTag").GetComponent<Image>();
        Image boltBackg = GameObject.FindWithTag("BoltTag").GetComponent<Image>();
        Image healBackg = GameObject.FindWithTag("HealTag").GetComponent<Image>();

        meleBackg.enabled = false;
        rangedBackg.enabled = false;
        fireballBackg.enabled = false;
        boltBackg.enabled = false;
        healBackg.enabled = false;

        GameObject[] icons = GameObject.FindGameObjectsWithTag("IconTag");
        //Debug.Log(icons);
        List<Image> imgs = new List<Image>();
        foreach (GameObject icon in icons)
        {
            Image img = icon.GetComponent<Image>();
            if (img != null)
            {
                imgs.Add(img);
            }
        }

        //List<Image> imgs = GameObject.FindWithTag("IconTag").GetComponent<Image>();
        //Image rangedBackg = GameObject.FindWithTag("RangedTag").GetComponent<Image>();
        foreach (Image img in imgs)
        {
            img.color = Color.grey;
            img.raycastTarget = false;
        }
    }


    public void EnableIcons()
    {
        Image meleBackg = GameObject.FindWithTag("MeleTag").GetComponent<Image>();
        /*Image rangedBackg = GameObject.FindWithTag("RangedTag").GetComponent<Image>();
        Image fireballBackg = GameObject.FindWithTag("FireballTag").GetComponent<Image>();
        Image boltBackg = GameObject.FindWithTag("BoltTag").GetComponent<Image>();
        Image healBackg = GameObject.FindWithTag("HealTag").GetComponent<Image>();*/

        meleBackg.enabled = true;
        /*rangedBackg.enabled = false;
        fireballBackg.enabled = false;
        boltBackg.enabled = false;
        healBackg.enabled = false;*/

        GameObject[] icons = GameObject.FindGameObjectsWithTag("IconTag");
        List<Image> imgs = new List<Image>();
        foreach (GameObject icon in icons)
        {
            Image img = icon.GetComponent<Image>();
            if (img != null)
            {
                imgs.Add(img);
            }
        }

        //List<Image> imgs = GameObject.FindWithTag("IconTag").GetComponent<Image>();
        //Image rangedBackg = GameObject.FindWithTag("RangedTag").GetComponent<Image>();
        foreach (Image img in imgs)
        {
            img.color = Color.white;
            img.raycastTarget = true;
        }
    }



    public void ChangeOutline (){

        Image meleBackg = GameObject.FindWithTag("MeleTag").GetComponent<Image>();
        Image rangedBackg = GameObject.FindWithTag("RangedTag").GetComponent<Image>();
        Image fireballBackg = GameObject.FindWithTag("FireballTag").GetComponent<Image>();
        Image boltBackg = GameObject.FindWithTag("BoltTag").GetComponent<Image>();
        Image healBackg = GameObject.FindWithTag("HealTag").GetComponent<Image>();

        meleBackg.enabled = false;
        rangedBackg.enabled = false;
        fireballBackg.enabled = false;
        boltBackg.enabled = false;
        healBackg.enabled = false;

        Debug.Log(selectedIcon.name);
        //Debug.Log(selectedIcon.name != "Mele");
        if (selectedIcon.name == "Mele")
        {
            meleBackg.enabled=true;
            gameManager.player.GetComponent<Skills>().ToggleMele();
        }
        else if(selectedIcon.name == "Ranged")
        {
            rangedBackg.enabled = true;
            gameManager.player.GetComponent<Skills>().ToggleRanged();
        }
        else if(selectedIcon.name == "Fireball")
        {
            fireballBackg.enabled = true;
            gameManager.player.GetComponent<Skills>().ToggleFireball();
        }
        else if (selectedIcon.name == "Bolt")
        {
            boltBackg.enabled = true;
            gameManager.player.GetComponent<Skills>().ToggleBolt();
        }
        else if (selectedIcon.name == "Heal")
        {
            healBackg.enabled = true;
            gameManager.player.GetComponent<Skills>().ToggleHeal();
        }
        gameManager.groundManager.UntintAllTiles();
        gameManager.FightRange();
    }
}
