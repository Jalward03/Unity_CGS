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
	public GameObject parentMap;
	private GameObject dungeon;
	public GameObject playerIcon;

	public GameObject hazardIcon;

	//public Color 
	//private GameObject 

	private void Awake()
	{
		parentMap = FindObjectOfType<MapIdentifier>().gameObject;
		dungeon = FindObjectOfType<DungeonCreator>().gameObject;

		playerIcon = dungeon.GetComponent<DungeonCreator>().playerIconCircle;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.CompareTag("Player"))
		{
			Vector3[] verts = GetComponent<FloorVerticesStorage>().meshVerts;

			if(!hasRevealed)
			{
				gameObject.layer = LayerMask.NameToLayer("Default");
				playerIcon.transform.SetParent(parentMap.transform);
				GameObject tile = new GameObject("tile", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
				GenerateMapTileData(tile);
				hasRevealed = true;
				playerIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2
					(
					 -(verts[0].z + verts[2].z),
					 (verts[3].x + verts[2].x)
					);
			}
			else if(hasRevealed)
			{
				playerIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2
					(
					 -(verts[0].z + verts[2].z),
					 (verts[3].x + verts[2].x)
					);
			}
		}
	}

	/// <summary>
	/// Uses Vertices of Collided Floor Mesh to calculate a size and position on A UI Canvas
	/// </summary>
	/// <param name="newGameObject">New Generated Tile</param>
	private void GenerateMapTileData(GameObject newGameObject)
	{
		parentMap.tag = "Spike";
		if(!gameObject.CompareTag("Shop")) newGameObject.GetComponent<Image>().color = dungeon.GetComponent<DungeonCreator>().mapRoomColor;
		else if(gameObject.CompareTag("Shop")) newGameObject.GetComponent<Image>().color = dungeon.GetComponent<DungeonCreator>().mapShopRoomColor;
		newGameObject.layer = LayerMask.NameToLayer("UI");
		newGameObject.transform.SetParent(parentMap.transform);

		Vector3[] verts = GetComponent<FloorVerticesStorage>().meshVerts;

		newGameObject.GetComponent<RectTransform>().sizeDelta = new Vector2
			(
			 (verts[0].z - verts[2].z) * 2,
			 (verts[3].x - verts[2].x ) * 2
			);

		newGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2
			(
			 -(verts[0].z + verts[2].z),
			 (verts[3].x + verts[2].x)
			);
	}
}