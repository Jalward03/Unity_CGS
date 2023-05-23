using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttributes : MonoBehaviour
{
    public DungeonCreator dungeon;

    [Header("Health")] 
    public int maxHealth;
    public float damageCooldown;
    public bool canTakeDamage = true;
    public int currentHealth;
    
    private void Awake()
    {
	    if(maxHealth < 1) maxHealth = 1;
	    currentHealth = maxHealth;

    }
    

    private void OnTriggerStay(Collider other)
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
	    foreach(GameObject hazard in dungeon.hazardList)
	    {
		    if(hazard.CompareTag(tag))
		    {
			    return hazard.GetComponent<Hazards>().damageAmount;
		    }
	    }

	    return 0;
    }
    
}
