using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skills : MonoBehaviour
{
    public int meleeDamage = 7;

    public int maxMana = 10;
    public int mana = 10;
    public int manaRegen = 1;


    public int maxHealth = 20;
    public int health = 20;
    public int healCost = 2;
    public int healStrength = 5;

    public int arrowCount = 10;
    public float arrowHitChance = 0.95f;
    public int arrowDamage = 5;

    public int fireballMainDamage = 10;
    public int fireballSurroundingDamage = 5;
    public int fireballCost = 5;

    public int boltDamage = 3;
    public int boltCost = 4;

    public AttackMode attackMode = AttackMode.Melee;

    public void Initialize(
        int meleeDamage = 7, int maxMana = 10, int mana = 10, int manaRegen = 1,
        int maxHealth = 20, int health = 20, int healCost = 2, int healStrength = 5,
        int arrowCount = 10, float arrowHitChance = 0.95f, int arrowDamage = 5,
        int fireballMainDamage = 10, int fireballSurroundingDamage = 5, int fireballCost = 5,
        int boltDamage = 3, int boltCost = 4, AttackMode attackMode = AttackMode.Melee
    )
    {
        this.meleeDamage = meleeDamage;
        this.maxMana = maxMana;
        this.mana = mana;
        this.manaRegen = manaRegen;
        this.maxHealth = maxHealth;
        this.health = health;
        this.healCost = healCost;
        this.healStrength = healStrength;
        this.arrowCount = arrowCount;
        this.arrowHitChance = arrowHitChance;
        this.arrowDamage = arrowDamage;
        this.fireballMainDamage = fireballMainDamage;
        this.fireballSurroundingDamage = fireballSurroundingDamage;
        this.fireballCost = fireballCost;
        this.boltDamage = boltDamage;
        this.boltCost = boltCost;
        this.attackMode = attackMode;
    }

    private AudioSource audioSource;
    private AudioClip melee;
    private AudioClip hurt;
    private AudioClip spear;
    private AudioClip cast;
    private AudioClip fireball;
    private AudioClip iceball;
    private AudioClip healing;
    private AudioClip bolt;


    public void Start() {
        audioSource = GetComponent<AudioSource>();
        melee = Resources.Load<AudioClip>("Audio/melee");
        hurt = Resources.Load<AudioClip>("Audio/hurt");
        spear = Resources.Load<AudioClip>("Audio/spear");
        cast = Resources.Load<AudioClip>("Audio/cast");
        fireball = Resources.Load<AudioClip>("Audio/fireball");
        iceball = Resources.Load<AudioClip>("Audio/iceball");
        healing = Resources.Load<AudioClip>("Audio/heal");
        bolt = Resources.Load<AudioClip>("Audio/bolt"); 
    }
     
    public void turnEnder() {
        mana = Math.Min(manaRegen + mana, maxMana);
    }

    public void meleeAttack(Skills target) {
        audioSource.PlayOneShot(melee);
        target.health -= meleeDamage;       
    }

    public void rangedAttack(Skills target) {
        if (arrowCount > 0) {
            audioSource.PlayOneShot(spear);
            if (UnityEngine.Random.Range(0f, 1f) <= arrowHitChance) {
                target.health -= arrowDamage;
            }
            arrowCount--;
        }
    }

    public void fireballAttack(Skills mainTarget, List<Skills> surroundingTargets) {
        if (mana >= fireballCost) {
            mana -= fireballCost;
            foreach (Skills target in surroundingTargets) {
                target.health -= fireballSurroundingDamage;
            }
            mainTarget.health -= fireballMainDamage;

            if (GetComponent<Character>().gameManager.gameState == GameManager.GameState.PlayerAction) {
                audioSource.PlayOneShot(fireball);
            } 
            else {
                audioSource.PlayOneShot(iceball);
            }
        }
    }

    public void boltAttack(List<Skills> targets) {
        if (mana >= boltCost) {
            audioSource.PlayOneShot(bolt);
            mana -= boltCost;
            foreach (Skills target in targets) {
                target.health -= boltDamage;
            }
        }
    }

    public void heal() {
        if (mana >= healCost) {
            audioSource.PlayOneShot(healing);
            health = Math.Min(health + healStrength, maxHealth);
            mana -= healCost;
        }
        Debug.Log("Livus maximaaaaaaaaaaaa: " + health);
    }

    public enum AttackMode
    {
        Melee,
        Ranged,
        Fireball,
        Bolt,
        Heal,
    }

    /*public void ToggleAttackMode()
    {
        attackMode = (attackMode == AttackMode.Melee) ? AttackMode.Ranged : AttackMode.Melee;
        Debug.Log("Attack mode switched to: " + attackMode);
    }*/

    public void ToggleMele()
    {
        attackMode = AttackMode.Melee;
        Debug.Log("Attack mode switched to: " + attackMode);
    }

    public void ToggleRanged()
    {
        attackMode = AttackMode.Ranged;
        Debug.Log("Attack mode switched to: " + attackMode);
    }

    public void ToggleFireball()
    {
        attackMode = AttackMode.Fireball;
        Debug.Log("Attack mode switched to: " + attackMode);
    }

    public void ToggleBolt()
    {
        attackMode = AttackMode.Bolt;
        Debug.Log("Attack mode switched to: " + attackMode);
    }

    public void ToggleHeal()
    {
        attackMode = AttackMode.Heal;
        Debug.Log("Attack mode switched to: " + attackMode);
    }
}
