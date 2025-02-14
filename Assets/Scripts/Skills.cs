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


    public int MaxHealth = 20;
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
     
    public void turnEnder() {
        mana = Math.Min(manaRegen + mana, maxMana);
    }

    public void meleeAttack(Skills target) {
        target.health -= meleeDamage;       
    }

    public void arrowAttack(Skills target) {
        if (UnityEngine.Random.Range(0f, 1f) <= arrowHitChance) {
            target.health -= arrowDamage;
        }
    }

    public void fireballAttack(Skills mainTarget, List<Skills> surroundingTargets) {
        if (mana >= fireballCost) {
            mana -= fireballCost;
            foreach (Skills target in surroundingTargets) {
                target.health -= fireballSurroundingDamage;
            }
            mainTarget.health -= fireballMainDamage;
        }
    }

    public void boltAttack(List<Skills> targets) {
        if (mana >= boltCost) {
            mana -= boltCost;
            foreach (Skills target in targets) {
                target.health -= boltDamage;
            }
        }
    }

    public void heal() {
        if (mana >= healCost) {
            Math.Min(health + healStrength, MaxHealth);
            mana -= healCost;
        }
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
