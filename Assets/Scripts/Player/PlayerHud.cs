using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.UI;

public class PlayerHud : MonoBehaviour
{
	public DungeonCreator dungeon;
	
	[Header("Crosshair")]
	public Sprite crosshairTexture;
	public Vector2 crosshairSize;

	[Header("Health")] 
	public int maxHealth;
	public int damageCooldown;
	public bool canTakeDamage = true;
	private int currentHealth;
	
	private void Awake()
	{
		SetCrosshair();
		if(maxHealth < 1) maxHealth = 1;
		currentHealth = maxHealth;

	}

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.layer == LayerMask.NameToLayer("Hazards"))
		{
			StartCoroutine(TakeDamage(collision.gameObject.tag));
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
	private void SetCrosshair()
	{
		GetComponentInChildren<Image>().sprite = crosshairTexture;
		//GetComponentInChildren<Image>().rectTransform = new Rect(0, 0, crosshairSize.x, crosshairSize.y);
	}

	private void Update()
	{
		Debug.Log(currentHealth);
	}
}
