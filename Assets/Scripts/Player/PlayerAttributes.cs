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

	private bool playerDied;

	private void Awake()
	{
		// Initialise Health variables
		if(maxHealth < 1) maxHealth = 1;
		currentHealth = maxHealth;
	}

	private void OnTriggerStay(Collider other)
	{
		// Checks overlapping of player and hazards
		if(other.gameObject.layer == LayerMask.NameToLayer("Hazards"))
		{
			if(canTakeDamage)
				StartCoroutine(TakeDamage(other.gameObject.tag));
		}
	}

	public IEnumerator TakeDamage(string tag)
	{
		// Makes player take damage every second
		if(canTakeDamage)
		{
			currentHealth -= CalculateDamageAmount(tag);
			canTakeDamage = false;
		}

		yield return new WaitForSeconds(damageCooldown);

		canTakeDamage = true;
	}

	private IEnumerator PlayerDeath()
	{
		// Lose Condition
		playerDied = true;
		GetComponent<PlayerMover>().canMove = false;
		GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
		GetComponent<Rigidbody>().AddForce(-transform.forward, ForceMode.Impulse);

		yield return new WaitForSeconds(2.5f);
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		SceneManager.LoadScene("LevelFailed");
	}

	private void Update()
	{
		// Checking for lose condition
		if(currentHealth <= 0 && !playerDied)
		{
			StartCoroutine(PlayerDeath());
		}
	}

	private int CalculateDamageAmount(string tag)
	{
		// Calculates correct amount of damage for specific hazard
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