using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.UI;

public class PlayerHud : MonoBehaviour
{
	private PlayerAttributes playerAttributes;
	private Slider healthBar;
	private TextMeshProUGUI healthText;

	[Header("Crosshair")] public Sprite crosshairTexture;
	public Vector2 crosshairSize;

	[Header("Health Bar")] public Image healthBarImage;

	public Color criticalHealthColor;

	public Color lowHealthColor;

	[Range(0, 100)]
	[Tooltip("Percentage(%)")]
	public int lowHealthPercentage;

	public Color mediumHealthColor;

	[Range(0, 100)]
	[Tooltip("Percentage(%)")]
	public int mediumHealthPercentage;

	public Color highHealthColor;

	[Range(0, 100)]
	[Tooltip("Percentage(%")]
	public int highHealthPercentage;

	private void Awake()
	{
		// Gets player data from child objects
		playerAttributes = GetComponentInParent<PlayerAttributes>();
		healthBar = GetComponentInChildren<Slider>();
		healthText = GetComponentInChildren<TextMeshProUGUI>();

		SetCrosshair();
		SetHealthBarValues();
	}

	/// <summary>
	/// Sets Health Bar Values
	/// </summary>
	private void SetHealthBarValues()
	{
		healthBar.maxValue = playerAttributes.maxHealth;
		healthBar.value = healthBar.maxValue;
		healthText.text = healthBar.value.ToString();
		healthBarImage.color = GetHealthBarColor();
	}

	/// <summary>
	/// Changes colour of health bar depending on current health 
	/// </summary>
	/// <returns></returns>
	private Color GetHealthBarColor()
	{
	
		if(playerAttributes.currentHealth < playerAttributes.maxHealth * highHealthPercentage / 100)
		{
			if(playerAttributes.currentHealth < playerAttributes.maxHealth * mediumHealthPercentage / 100)
			{
				return playerAttributes.currentHealth < playerAttributes.maxHealth * lowHealthPercentage / 100 ? criticalHealthColor : lowHealthColor;
			}
			return mediumHealthColor;
		}
		return highHealthColor;
	}

	private void Update()
	{
		healthBar.value = playerAttributes.currentHealth;
		healthText.text = healthBar.value.ToString();
		healthBarImage.color = GetHealthBarColor();
	}

	/// <summary>
	/// Sets Crosshair
	/// </summary>
	private void SetCrosshair()
	{
		GetComponentInChildren<Image>().sprite = crosshairTexture;
	}
}