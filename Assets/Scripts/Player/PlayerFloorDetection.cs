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
	public int[] lols;
	//private GameObject 

	private void Awake()
	{
		parentMap = FindObjectOfType<MapIdentifier>().gameObject;
		dungeon = FindObjectOfType<DungeonCreator>().gameObject;
	}

	private IEnumerator FadeMap()
	{
		foreach(Image childTiles in parentMap.GetComponentsInChildren<Image>())
		{
			childTiles.color = new Color(childTiles.color.r, childTiles.color.g, childTiles.color.b, 1);
		}

		float t = 0;
		float fadeTime = 2.0f;
		Color startColor = dungeon.GetComponent<DungeonCreator>().mapRoomColor;
		Color endColor = startColor;
		endColor.a = 0;

		while(t < fadeTime * 1.1f)
		{
			dungeon.GetComponent<DungeonCreator>().mapRoomColor = Color.Lerp(startColor, endColor, t / fadeTime);
			foreach(Image childTiles in parentMap.GetComponentsInChildren<Image>())
			{
				childTiles.color = dungeon.GetComponent<DungeonCreator>().mapRoomColor;
			}

			yield return null;

			t += Time.deltaTime;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.CompareTag("Player") && !hasRevealed)
		{
			gameObject.layer = LayerMask.NameToLayer("Default");

			GameObject tile = new GameObject("tile", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
			GenerateMapTileData(tile);
			hasRevealed = true;
			//StopAllCoroutines();
			//StartCoroutine(FadeMap());
		
		}
	}

	private void GenerateMapTileData(GameObject newGameObject)
	{
		
	
		parentMap.tag = "Spike";
		if (!gameObject.CompareTag("Shop")) newGameObject.GetComponent<Image>().color = dungeon.GetComponent<DungeonCreator>().mapRoomColor;
		else if (gameObject.CompareTag("Shop")) newGameObject.GetComponent<Image>().color = dungeon.GetComponent<DungeonCreator>().mapShopRoomColor;
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
			 verts[0].z - verts[2].z,
			 verts[3].x - verts[2].x
			);

		newGameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2
			(
			 -(verts[0].z + verts[2].z),
			 (verts[3].x + verts[2].x)
			);
	}
}