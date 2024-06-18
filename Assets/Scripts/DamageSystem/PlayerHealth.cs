using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerHealth : Health
{
    //variables for assigning who should score a point on the players death
    public int lastDamagedBy; //the number of the player that last damaged this character
    public int playerNum; //the number of this player;

    //variables for ressetting the lastDamagedBy setting;
    private float damagedByReset = 3f;
    public Slider slider;
    private Animator anim; //must reference the animator in order to play death animations

    private void Start()
    {
        lastDamagedBy = playerNum; //default this number to the players number. This means that if the player is killed by an environment object it will subtract points from itself;
        anim = GetComponentInChildren<Animator>();
        anim.SetBool("Dead", false);
    }
    public override void TakeDamage(float damageAmount)
    {
        base.TakeDamage(damageAmount);
        if(slider)slider.value = health;
    }
    public override void Die()
    {
        if (dead) return;
        base.Die();
        dead = true;
        anim.SetBool("Dead", true);
        

        //make the character respawn, but on a delay so that the animation can play out
        StartCoroutine(RespawnCharacterOnDelay(playerNum));
    }
    IEnumerator RespawnCharacterOnDelay(int playerNumber)
    {
        yield return new WaitForSeconds(2);
        GamePlayManager.instance.SpawnPlayer(playerNum);
        yield return null;
    }
    public IEnumerator ResetDamagedBy()
    {
        yield return new WaitForSeconds(damagedByReset);
        lastDamagedBy = playerNum;
    }
}
