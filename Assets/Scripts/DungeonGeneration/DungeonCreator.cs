﻿using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;

public class DungeonCreator : MonoBehaviour
{
	public int dungeonWidth, dungeonLength;
	public int roomWidthMin, roomLengthMin;
	public int maxIterations;
	public int corridorWidth;
	public Material floorMat;
	public Material ceilingMat;
	[Range(0.0f, 0.3f)] public float roomBottomCornerModifier;
	[Range(0.7f, 1.0f)] public float roomTopCornerModifier;
	[Range(0, 3)] public int roomOffset;
	public GameObject wallVertical, wallHorizontal;
	public GameObject player;
	[Range(1.0f, 5.0f)] public int ceilingHeight;
	List<Vector3Int> possibleDoorVerticalPosition;
	List<Vector3Int> possibleDoorHorizontalPosition;
	List<Vector3Int> possibleWallHorizontalPosition;
	List<Vector3Int> possibleWallVerticalPosition;

	// Start is called before the first frame update
	void Start()
	{
		CreateDungeon();
	}


	public void CreateDungeon()
	{
		wallVertical.transform.localScale = new Vector3(wallVertical.transform.localScale.x, ceilingHeight, wallVertical.transform.localScale.z);
		wallHorizontal.transform.localScale = new Vector3(wallHorizontal.transform.localScale.x, ceilingHeight, wallHorizontal.transform.localScale.z);
		
		DungeonGenerator generator = new DungeonGenerator(dungeonWidth, dungeonLength);
		var listOfRooms = generator.CalculateDungeon(maxIterations,
		                                             roomWidthMin,
		                                             roomLengthMin,
		                                             roomBottomCornerModifier,
		                                             roomTopCornerModifier,
		                                             roomOffset,
		                                             corridorWidth);
		GameObject wallParent = new GameObject("WallParent");
		wallParent.transform.parent = transform;
		possibleDoorVerticalPosition = new List<Vector3Int>();
		possibleDoorHorizontalPosition = new List<Vector3Int>();
		possibleWallHorizontalPosition = new List<Vector3Int>();
		possibleWallVerticalPosition = new List<Vector3Int>();

		for(int i = 0; i < listOfRooms.Count; i++)
		{
			CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner);
			CreateCeiling(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, ceilingHeight);

			if(i == 0)
			{
				SpawnPlayer(new Vector3(
					(listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2, 
					1, 
					(listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2));
			}
		}

		CreateWalls(wallParent);
	}
	public void SpawnPlayer(Vector3 position)
	{
		Instantiate(player, position, Quaternion.identity);
	}
	private void CreateWalls(GameObject wallParent)
	{
		foreach(var wallPosition in possibleWallHorizontalPosition)
		{
			CreateWall(wallParent, wallPosition, wallHorizontal);
		}

		foreach(var wallPosition in possibleWallVerticalPosition)
		{
			CreateWall(wallParent, wallPosition, wallVertical);
		}
	}

	private void CreateWall(GameObject wallParent, Vector3Int wallPosition, GameObject wallPrefab)
	{
		Instantiate(wallPrefab, wallPosition, Quaternion.identity, wallParent.transform);
	}

	private void CreateMesh(Vector2 bottomLeftCorner, Vector2 topRightCorner)
	{
		Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, 0, bottomLeftCorner.y);
		Vector3 bottomRightV = new Vector3(topRightCorner.x, 0, bottomLeftCorner.y);
		Vector3 topLeftV = new Vector3(bottomLeftCorner.x, 0, topRightCorner.y);
		Vector3 topRightV = new Vector3(topRightCorner.x, 0, topRightCorner.y);

		Vector3[] vertices = new Vector3[]
		{
			topLeftV,
			topRightV,
			bottomLeftV,
			bottomRightV
		};

		Vector2[] uvs = new Vector2[vertices.Length];
		for(int i = 0; i < uvs.Length; i++)
		{
			uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
		}

		int[] triangles = new int[]
		{
			0,
			1,
			2,
			2,
			1,
			3
		};
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = triangles;

		GameObject dungeonFloor = new GameObject("Floor" + bottomLeftCorner, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));

		dungeonFloor.transform.position = Vector3.zero;
		dungeonFloor.transform.localScale = Vector3.one;
		dungeonFloor.GetComponent<MeshFilter>().mesh = mesh;
		dungeonFloor.GetComponent<MeshRenderer>().material = floorMat;
		dungeonFloor.GetComponent<MeshCollider>().sharedMesh = mesh;
		dungeonFloor.transform.parent = transform;

		for(int row = (int) bottomLeftV.x; row < (int) bottomRightV.x; row++)
		{
			var wallPosition = new Vector3(row, 0, bottomLeftV.z);
			AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
		}

		for(int row = (int) topLeftV.x; row < (int) topRightCorner.x; row++)
		{
			var wallPosition = new Vector3(row, 0, topRightV.z);
			AddWallPositionToList(wallPosition, possibleWallHorizontalPosition, possibleDoorHorizontalPosition);
		}

		for(int col = (int) bottomLeftV.z; col < (int) topLeftV.z; col++)
		{
			var wallPosition = new Vector3(bottomLeftV.x, 0, col);
			AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
		}

		for(int col = (int) bottomRightV.z; col < (int) topRightV.z; col++)
		{
			var wallPosition = new Vector3(bottomRightV.x, 0, col);
			AddWallPositionToList(wallPosition, possibleWallVerticalPosition, possibleDoorVerticalPosition);
		}
	}

	private void CreateCeiling(Vector2 bottomLeftCorner, Vector2 topRightCorner, int ceilingHeight)
	{
		Vector3 bottomLeftV = new Vector3(bottomLeftCorner.x, ceilingHeight, bottomLeftCorner.y);
		Vector3 bottomRightV = new Vector3(topRightCorner.x, ceilingHeight, bottomLeftCorner.y);
		Vector3 topLeftV = new Vector3(bottomLeftCorner.x, ceilingHeight, topRightCorner.y);
		Vector3 topRightV = new Vector3(topRightCorner.x, ceilingHeight, topRightCorner.y);

		Vector3[] vertices = new Vector3[]
		{
			topLeftV,
			topRightV,
			bottomLeftV,
			bottomRightV
		};

		Vector2[] uvs = new Vector2[vertices.Length];
		for(int i = 0; i < uvs.Length; i++)
		{
			uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
		}

		int[] triangles = new int[]
		{
			0,
			2,
			1,
			2,
			3,
			1
		};
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = triangles;

		GameObject dungeonCeiling = new GameObject("Ceiling" + bottomLeftCorner, typeof(MeshFilter), typeof(MeshRenderer));

		dungeonCeiling.transform.position = new Vector3(0, ceilingHeight * 1.18f, 0);
		dungeonCeiling.transform.localScale = Vector3.one;
		//dungeonCeiling.transform.eulerAngles = new Vector3(180, 0, 0);
		dungeonCeiling.GetComponent<MeshFilter>().mesh = mesh;
		dungeonCeiling.GetComponent<MeshRenderer>().material = ceilingMat;
		dungeonCeiling.transform.parent = transform;

	
	}
	private void AddWallPositionToList(Vector3 wallPosition, List<Vector3Int> wallList, List<Vector3Int> doorList)
	{
		Vector3Int point = Vector3Int.CeilToInt(wallPosition);
		if(wallList.Contains(point))
		{
			doorList.Add(point);
			wallList.Remove(point);
		}
		else
		{
			wallList.Add(point);
		}
	}
}