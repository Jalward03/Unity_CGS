using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttributes : MonoBehaviour
{
    public DungeonCreator dungeon;

    [Header("Health")] 
    public int maxHealth;
    public int damageCooldown;
    public bool canTakeDamage = true;
    public int currentHealth;
    
    private void Awake()
    {
	    if(maxHealth < 1) maxHealth = 1;
	    currentHealth = maxHealth;

    }

    //private void OnControllerColliderHit(ControllerColliderHit hit)
    //{
	//    if(hit.gameObject.layer == LayerMask.NameToLayer("Hazards"))
	//    {
	//	    if(canTakeDamage) 
	//		    StartCoroutine(TakeDamage(hit.gameObject.tag));
	//    }
    //}

    private void OnTriggerEnter(Collider other)
    {
	    if(other.gameObject.layer == LayerMask.NameToLayer("Hazards"))
	    {
		    if(canTakeDamage) 
			    StartCoroutine(TakeDamage(other.gameObject.tag));
	    }
    }


    public IEnumerator TakeDamage(string tag)
    {
	    if(canTakeDamage)
	    {
		    currentHealth -= CalculateDamageAmount(tag);
		    canTakeDamage = false;
	    }

	    yield return new WaitForSeconds(damageCooldown);

	    canTakeDamage = true;

    }
    private int CalculateDamageAmount(string tag)
    {
	    for(int i = 0; i < dungeon.hazardList.Count; i++)
	    {
		    if(dungeon.hazardList[i].CompareTag(tag))
		    {
			    return dungeon.hazardList[i].GetComponent<Hazards>().damageAmount;
		    }
	    }

	    return 0;
    }
    
}
