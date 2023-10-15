using System;
using System.Collections;
using System.Collections.Generic;using Unity.VisualScripting;

using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Hazards : MonoBehaviour
{
	// Stores a damage variable
	[Range(1f, 100f)] public int damagePercentage;
}