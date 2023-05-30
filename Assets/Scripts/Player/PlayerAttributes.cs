using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerAttributes : MonoBehaviour
{
	public DungeonCreator dungeon;

	[Header("Health")] public int maxHealth;
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

	private void Update()
	{
		if(currentHealth <= 0)
		{
			SceneManager.LoadScene("Demo");
			Debug.Log("Player Died");
			
		}
	}

	private int CalculateDamageAmount(string tag)
	{
		foreach(GameObject hazard in dungeon.hazardList)
		{
			if(hazard.CompareTag(tag))
			{
				return maxHealth * hazard.GetComponent<Hazards>().damagePercentage / 100;

			}
		}

		if(dungeon.corridorTrap.CompareTag(tag))
			return maxHealth * dungeon.corridorTrap.GetComponent<Hazards>().damagePercentage / 100;

		return 0;
	}
}