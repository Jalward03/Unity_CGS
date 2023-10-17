using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

using Image = UnityEngine.UI.Image;

public class PlayerFloorDetection : MonoBehaviour
{
	private bool hasRevealed;

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.CompareTag("Player") && !hasRevealed)
		{
			gameObject.layer = LayerMask.NameToLayer("Default");

			GameObject tile = new GameObject("tile", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			GenerateMapTileData(tile);
			hasRevealed = true;
		}
	}

	private void GenerateMapTileData(GameObject newGameObject)
	{
		GameObject parentMap = FindObjectOfType<MapIdentifier>().gameObject;
		GameObject dungeon = FindObjectOfType<DungeonCreator>().gameObject;
		parentMap.tag = "Spike";
		newGameObject.layer = LayerMask.NameToLayer("UI");
		newGameObject.transform.SetParent(parentMap.transform);

		//newGameObject.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
		//newGameObject.GetComponent<RectTransform>().anchorMax = new Vector2(1, 0);
		//newGameObject.GetComponent<RectTransform>().pivot = new Vector2(1, 0);

		Vector3[] verts = GetComponent<FloorVerticesStorage>().meshVerts;

		//newGameObject.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

		
	
		
		newGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2
			(
			 //verts[3].x - verts[2].x,
			 //verts[0].z - verts[2].z
			 verts[0].z - verts[2].z ,
			 verts[3].x - verts[2].x 
			);
	
		newGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2
			(
			 -(verts[0].z + verts[2].z) ,
			 (verts[3].x + verts[2].x) 
			);

	}
}