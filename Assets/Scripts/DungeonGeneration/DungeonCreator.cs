using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;

using UnityEditor.Experimental.GraphView;

using UnityEngine;
using UnityEngine.UI;

using Image = UnityEngine.UIElements.Image;
using Random = UnityEngine.Random;

public class DungeonCreator : MonoBehaviour
{
	[Header("Dungeon Attributes")] public int dungeonWidth;
	public int dungeonLength;
	public int roomWidthMin;
	public int roomLengthMin;
	public int maxIterations;
	public int corridorWidth;
	public Material floorMat;
	public Material ceilingMat;
	[Range(0.1f, 0.3f)] public float roomBottomCornerModifier;
	[Range(0.7f, 1.0f)] public float roomTopCornerModifier;
	[Range(2.0f, 5.0f)] public int ceilingHeight;
	[Range(3, 6)] public int roomOffset;

	public GameObject wallVertical, wallHorizontal;
	public GameObject player;

	private List<GameObject> dungeonParents;

	private GameObject floorParent;
	private GameObject ceilingParent;
	private GameObject furnitureParent;
	private GameObject wallFurnitureParent;
	private GameObject torchParent;
	private GameObject hazardParent;

	List<Vector3Int> possibleDoorVerticalPosition;
	List<Vector3Int> possibleDoorHorizontalPosition;
	List<Vector3Int> possibleWallHorizontalPosition;
	List<Vector3Int> possibleWallVerticalPosition;

	[Header("Furniture")] public bool haveFurniture;
	public bool haveWallFurniture;
	public bool haveTorches;
	public GameObject torch;
	public GameObject endRoomEscapeHatch;
	public List<GameObject> furnitureList;
	public List<GameObject> wallFurnitureList;

	[Header("Hazards")] public bool haveCorridorTraps;
	public int maxHazardsPerRoom;
	public GameObject corridorTrap;
	public List<GameObject> hazardList;

	[Header("Special Rooms")] public bool haveStartingRoom;
	public bool haveEscapeHatchRoom;

	[Header("Mini Map")] public bool haveMiniMap;
	public int miniMapZoom;
	public GameObject miniMapCam;
	public GameObject minimapCanvas;
	public GameObject playerIcon;
	public GameObject spawnIcon;

	[Header("Map")]
	public GameObject mapCanvas;
	private GameObject parentMap;

	private List<GameObject> miniMapTiles;
	
	private void Awake()
	{
		// Instantiates minimap components
		if(haveMiniMap)
		{
			if(miniMapCam) miniMapCam = Instantiate(miniMapCam);

			if(playerIcon)
			{
				playerIcon = Instantiate(playerIcon);
			}

			if(minimapCanvas) Instantiate(minimapCanvas);
		}


		if(mapCanvas)
		{
			mapCanvas = Instantiate(mapCanvas);
			parentMap = mapCanvas.transform.GetChild(0).gameObject;
			parentMap.GetComponent<RectTransform>().sizeDelta = new Vector2(dungeonLength, dungeonWidth);

		}

		//parentMap
		// Parents every object spawned in
		dungeonParents = new List<GameObject>();
		dungeonParents.Add(floorParent = new GameObject("Floor Parent"));
		dungeonParents.Add(ceilingParent = new GameObject("Ceiling Parent"));
		dungeonParents.Add(furnitureParent = new GameObject("Furniture Parent"));
		dungeonParents.Add(wallFurnitureParent = new GameObject("Wall Furniture Parent"));
		dungeonParents.Add(torchParent = new GameObject("Torch Parent"));
		hazardParent = new GameObject("Hazards Parent");
		hazardParent.layer = LayerMask.NameToLayer("Hazards");

		foreach(GameObject parent in dungeonParents)
		{
			parent.transform.parent = transform;
		}
	}

	void Start()
	{
		CreateDungeon();
	}

	private void Update()
	{
		// Updates Icons on mini map
		if(haveMiniMap)
		{
			miniMapCam.transform.position = new Vector3(player.transform.position.x, 100, player.transform.position.z);
			miniMapCam.transform.eulerAngles = new Vector3(90, player.transform.eulerAngles.y, 0);
			playerIcon.transform.position = new Vector3(player.transform.position.x, 50, player.transform.position.z);
			playerIcon.transform.eulerAngles = new Vector3(playerIcon.transform.eulerAngles.x, player.transform.eulerAngles.y, 0);
		}
	}

	public void CreateDungeon()
	{
		// Creates and assigns dungeon variables from inspector
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

		int endRoomIndex = Random.Range(maxIterations / 2, listOfRooms.Count / 2);

		// Generates contents of each room in the list
		for(int i = 0; i < listOfRooms.Count; i++)
		{
			CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner);
			CreateCeiling(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, ceilingHeight, false);

			// Spawn Room
			if(i == 0)
			{
				SpawnPlayer(new Vector3(
				                        (listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2,
				                        1,
				                        (listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2));

				if(haveMiniMap)
				{
					spawnIcon = Instantiate(spawnIcon);
					spawnIcon.transform.position = new Vector3(
					                                           (listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2,
					                                           49,
					                                           (listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2);
					spawnIcon.transform.eulerAngles = new Vector3(spawnIcon.transform.eulerAngles.x, player.transform.eulerAngles.y, spawnIcon.transform.eulerAngles.z);
				}

				if(!haveStartingRoom) SpawnHazards(listOfRooms, i);
			}

			// Rooms after spawn room
			if(i > 0 && i <= listOfRooms.Count / 2 && i != endRoomIndex)
			{
				// Spawns random piece of furniture in middle of each room
				SpawnFurniture(new Vector3(
				                           (listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2,
				                           0,
				                           (listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2));

				// Spawn lantern and in the middle ceiling of every room
				SpawnTorches(new Vector3(
				                         (listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2,
				                         ceilingHeight * 2.18f,
				                         (listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2));

				SpawnHazards(listOfRooms, i);
			}

			// End Room
			if(i == endRoomIndex)
			{
				SpawnHatch(new Vector3(
				                       (listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2,
				                       0,
				                       (listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2),
				           haveEscapeHatchRoom);

				SpawnHazards(listOfRooms, i);
			}

			// Corridors
			if(i > 0 && i > listOfRooms.Count / 2)
			{
				// All vertical corridor nodes
				if((listOfRooms[i].BottomLeftAreaCorner.x - listOfRooms[i].TopRightAreaCorner.x)
				   < (listOfRooms[i].BottomLeftAreaCorner.y - listOfRooms[i].TopRightAreaCorner.y))
				{
					// Right Side Of Wall
					SpawnWallFurniture(
					                   new Vector3(
					                               (listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2,
					                               ceilingHeight * 1.18f,
					                               (listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2 - (corridorWidth / 2)),
					                   Quaternion.Euler(0, 90, 0));

					// Left Side Of Wall
					SpawnWallFurniture(
					                   new Vector3(
					                               (listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2,
					                               ceilingHeight * 1.18f,
					                               (listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2 + (corridorWidth) / 1.75f),
					                   Quaternion.Euler(0, -90, 0));

					// Vertical
					SpawnCorridorTrap(new Vector3(
					                              (listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2 - (corridorWidth) / 2f,
					                              ceilingHeight * 2.3f,
					                              (listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2),
					                  new Vector3(
					                              (listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2 + (corridorWidth) / 2f,
					                              ceilingHeight * 2.3f,
					                              (listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2),
					                  Quaternion.Euler(0, 90, 0));
				}

				// All horizontal corridor nodes
				if((listOfRooms[i].BottomLeftAreaCorner.x - listOfRooms[i].TopRightAreaCorner.x)
				   > (listOfRooms[i].BottomLeftAreaCorner.y - listOfRooms[i].TopRightAreaCorner.y))
				{
					// Right Side Of Wall
					SpawnWallFurniture(
					                   new Vector3(
					                               (listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2 - (corridorWidth / 2),
					                               ceilingHeight * 1.18f,
					                               (listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2),
					                   Quaternion.Euler(0, 180, 0));

					// Left Side Of Wall
					SpawnWallFurniture(
					                   new Vector3(
					                               (listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2 + (corridorWidth / 1.75f),
					                               ceilingHeight * 1.18f,
					                               (listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2),
					                   Quaternion.Euler(0, 0, 0));
					// Horizontal
					SpawnCorridorTrap(new Vector3(
					                              (listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2,
					                              ceilingHeight * 2.3f,
					                              (listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2 - (corridorWidth) / 2f),
					                  new Vector3(
					                              (listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2,
					                              ceilingHeight * 2.3f,
					                              (listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2 + (corridorWidth) / 2f),
					                  Quaternion.identity);
				}
			}
		}

		CreateWalls(wallParent);
	}

	public int GetNewRand(int maxNum)
	{
		// Gets a new random number within range
		int rand = Random.Range(0, maxNum);

		return rand;
	}

	public void SpawnCorridorTrap(Vector3 position1, Vector3 position2, Quaternion rotation)
	{
		if(haveCorridorTraps)
		{
			// Chooses to spawn 0, 1, or 2 traps on each corridor
			if(GetNewRand(2) == 1)
			{
				Instantiate(corridorTrap, position1, rotation, hazardParent.transform);
			}

			if(GetNewRand(2) == 1)
			{
				Instantiate(corridorTrap, position2, rotation, hazardParent.transform);
			}
		}
	}

	public void SpawnHazards(List<Node> listOfRooms, int index)
	{
		if(maxHazardsPerRoom > 0)
		{
			int hazardsSpawned = 0;
			List<Vector3> hazardSpawnPositions = new List<Vector3>();

			// Top Right corner
			hazardSpawnPositions.Add(new Vector3(
			                                     (listOfRooms[index].BottomLeftAreaCorner.x + listOfRooms[index].TopRightAreaCorner.x) / 2 +
			                                     (listOfRooms[index].BottomLeftAreaCorner.x - listOfRooms[index].TopRightAreaCorner.x) / 4,
			                                     0,
			                                     (listOfRooms[index].BottomLeftAreaCorner.y + listOfRooms[index].TopRightAreaCorner.y) / 2 +
			                                     (listOfRooms[index].BottomLeftAreaCorner.y - listOfRooms[index].TopRightAreaCorner.y) / 4));
			// Bottom Right corner
			hazardSpawnPositions.Add(new Vector3(
			                                     (listOfRooms[index].BottomLeftAreaCorner.x + listOfRooms[index].TopRightAreaCorner.x) / 2 +
			                                     (listOfRooms[index].BottomLeftAreaCorner.x - listOfRooms[index].TopRightAreaCorner.x) / 4,
			                                     0,
			                                     (listOfRooms[index].BottomLeftAreaCorner.y + listOfRooms[index].TopRightAreaCorner.y) / 2 -
			                                     (listOfRooms[index].BottomLeftAreaCorner.y - listOfRooms[index].TopRightAreaCorner.y) / 4));
			// Top left corner
			hazardSpawnPositions.Add(new Vector3(
			                                     (listOfRooms[index].BottomLeftAreaCorner.x + listOfRooms[index].TopRightAreaCorner.x) / 2 -
			                                     (listOfRooms[index].BottomLeftAreaCorner.x - listOfRooms[index].TopRightAreaCorner.x) / 4,
			                                     0,
			                                     (listOfRooms[index].BottomLeftAreaCorner.y + listOfRooms[index].TopRightAreaCorner.y) / 2 +
			                                     (listOfRooms[index].BottomLeftAreaCorner.y - listOfRooms[index].TopRightAreaCorner.y) / 4));
			// Bottom left corner
			hazardSpawnPositions.Add(new Vector3(
			                                     (listOfRooms[index].BottomLeftAreaCorner.x + listOfRooms[index].TopRightAreaCorner.x) / 2 -
			                                     (listOfRooms[index].BottomLeftAreaCorner.x - listOfRooms[index].TopRightAreaCorner.x) / 4,
			                                     0,
			                                     (listOfRooms[index].BottomLeftAreaCorner.y + listOfRooms[index].TopRightAreaCorner.y) / 2 -
			                                     (listOfRooms[index].BottomLeftAreaCorner.y - listOfRooms[index].TopRightAreaCorner.y) / 4));

			//  Extra hazards positions for rooms longer than 50 units
			if(Mathf.Abs(listOfRooms[index].BottomLeftAreaCorner.x - listOfRooms[index].BottomRightAreaCorner.x) > 50.0f)
			{
				hazardSpawnPositions.Add(new Vector3(
				                                     (listOfRooms[index].BottomLeftAreaCorner.x + listOfRooms[index].TopRightAreaCorner.x) / 2 -
				                                     (listOfRooms[index].BottomLeftAreaCorner.x - listOfRooms[index].TopRightAreaCorner.x) / 2.5f,
				                                     0,
				                                     (listOfRooms[index].BottomLeftAreaCorner.y + listOfRooms[index].TopRightAreaCorner.y) / 2));
				hazardSpawnPositions.Add(new Vector3(
				                                     (listOfRooms[index].BottomLeftAreaCorner.x + listOfRooms[index].TopRightAreaCorner.x) / 2 +
				                                     (listOfRooms[index].BottomLeftAreaCorner.x - listOfRooms[index].TopRightAreaCorner.x) / 2.5f,
				                                     0,
				                                     (listOfRooms[index].BottomLeftAreaCorner.y + listOfRooms[index].TopRightAreaCorner.y) / 2));

				//  Extra hazards positions for rooms wider than 75 units
				if(Mathf.Abs(listOfRooms[index].BottomLeftAreaCorner.x - listOfRooms[index].BottomRightAreaCorner.x) > 75.0f)
				{
					hazardSpawnPositions.Add(new Vector3(
					                                     (listOfRooms[index].BottomLeftAreaCorner.x + listOfRooms[index].TopRightAreaCorner.x) / 2 -
					                                     (listOfRooms[index].BottomLeftAreaCorner.x - listOfRooms[index].TopRightAreaCorner.x) / 6f,
					                                     0,
					                                     (listOfRooms[index].BottomLeftAreaCorner.y + listOfRooms[index].TopRightAreaCorner.y) / 2));
					hazardSpawnPositions.Add(new Vector3(
					                                     (listOfRooms[index].BottomLeftAreaCorner.x + listOfRooms[index].TopRightAreaCorner.x) / 2 +
					                                     (listOfRooms[index].BottomLeftAreaCorner.x - listOfRooms[index].TopRightAreaCorner.x) / 6f,
					                                     0,
					                                     (listOfRooms[index].BottomLeftAreaCorner.y + listOfRooms[index].TopRightAreaCorner.y) / 2));
				}
			}

			// Extra hazards positions for rooms longer than 50 units
			if(Mathf.Abs(listOfRooms[index].TopRightAreaCorner.y - listOfRooms[index].BottomRightAreaCorner.y) > 50.0f)
			{
				hazardSpawnPositions.Add(new Vector3(
				                                     (listOfRooms[index].BottomLeftAreaCorner.x + listOfRooms[index].TopRightAreaCorner.x) / 2,
				                                     0,
				                                     (listOfRooms[index].BottomLeftAreaCorner.y + listOfRooms[index].TopRightAreaCorner.y) / 2 +
				                                     (listOfRooms[index].BottomLeftAreaCorner.y - listOfRooms[index].TopRightAreaCorner.y) / 2.5f));
				hazardSpawnPositions.Add(new Vector3(
				                                     (listOfRooms[index].BottomLeftAreaCorner.x + listOfRooms[index].TopRightAreaCorner.x) / 2,
				                                     0,
				                                     (listOfRooms[index].BottomLeftAreaCorner.y + listOfRooms[index].TopRightAreaCorner.y) / 2 -
				                                     (listOfRooms[index].BottomLeftAreaCorner.y - listOfRooms[index].TopRightAreaCorner.y) / 2.5f));

				//  Extra hazards positions for rooms longer than 75 units
				if(Mathf.Abs(listOfRooms[index].TopRightAreaCorner.y - listOfRooms[index].BottomRightAreaCorner.y) > 75.0f)
				{
					hazardSpawnPositions.Add(new Vector3(
					                                     (listOfRooms[index].BottomLeftAreaCorner.x + listOfRooms[index].TopRightAreaCorner.x) / 2,
					                                     0,
					                                     (listOfRooms[index].BottomLeftAreaCorner.y + listOfRooms[index].TopRightAreaCorner.y) / 2 +
					                                     (listOfRooms[index].BottomLeftAreaCorner.y - listOfRooms[index].TopRightAreaCorner.y) / 6));
					hazardSpawnPositions.Add(new Vector3(
					                                     (listOfRooms[index].BottomLeftAreaCorner.x + listOfRooms[index].TopRightAreaCorner.x) / 2,
					                                     0,
					                                     (listOfRooms[index].BottomLeftAreaCorner.y + listOfRooms[index].TopRightAreaCorner.y) / 2 -
					                                     (listOfRooms[index].BottomLeftAreaCorner.y - listOfRooms[index].TopRightAreaCorner.y) / 6f));
				}
			}

			foreach(Vector3 pos in hazardSpawnPositions)
			{
				// Instantiates a hazard at every collected position if there is room
				if(hazardsSpawned < maxHazardsPerRoom)
				{
					GameObject hazard = hazardList[GetNewRand(hazardList.Count)];
					Instantiate(hazard, pos, Quaternion.identity, hazardParent.transform);
					hazardsSpawned++;
				}
			}
		}
	}

	public void SpawnWallFurniture(Vector3 position, Quaternion rotation)
	{
		// Spawns wall furniture
		if(haveWallFurniture)
		{
			int rand = Random.Range(0, wallFurnitureList.Count);
			GameObject wallFurniture = wallFurnitureList[rand];
			Instantiate(wallFurniture, position, rotation, wallFurnitureParent.transform);
		}
	}

	public void SpawnTorches(Vector3 position)
	{
		// Spawns torches
		if(haveTorches) Instantiate(torch, position, Quaternion.identity, torchParent.transform);
	}

	public void SpawnFurniture(Vector3 position)
	{
		if(haveFurniture)
		{
			int rand = Random.Range(0, furnitureList.Count);
			GameObject furniture = furnitureList[rand];
			Instantiate(furniture, position, Quaternion.identity, furnitureParent.transform);
		}
	}

	public void SpawnPlayer(Vector3 position)
	{
		// Spawns player
		player = Instantiate(player, position, Quaternion.identity);
	}

	public void SpawnHatch(Vector3 position, bool haveHatchRoom)
	{
		// Spawns hatch
		if(haveHatchRoom) Instantiate(endRoomEscapeHatch, position, Quaternion.identity, transform);
	}

	private void CreateWalls(GameObject wallParent)
	{
		// Instantiate a wall for every wall spawn collected
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
		// Initialising the vertices of each corner into an array for mesh
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

		// Initialising the uvs for mesh
		Vector2[] uvs = new Vector2[vertices.Length];
		for(int i = 0; i < uvs.Length; i++)
		{
			uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
		}

		// Initialising the points of the triangles into an array for mesh
		int[] triangles = new int[]
		{
			0,
			1,
			2,
			2,
			1,
			3
		};
		// Initialising the mesh and assigning the variables previously made 
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = triangles;

		// Initialising the the mesh as a GameObject with necessary components
		GameObject dungeonFloor = new GameObject("Floor" + bottomLeftCorner, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider), typeof(PlayerFloorDetection), typeof(FloorVerticesStorage));
		dungeonFloor.transform.position = Vector3.zero;
		dungeonFloor.transform.localScale = Vector3.one;
		dungeonFloor.GetComponent<MeshFilter>().mesh = mesh;
		dungeonFloor.GetComponent<MeshRenderer>().material = floorMat;
		dungeonFloor.GetComponent<MeshCollider>().sharedMesh = mesh;
		dungeonFloor.GetComponent<FloorVerticesStorage>().meshVerts = vertices;
		dungeonFloor.layer = LayerMask.NameToLayer("Water");
		dungeonFloor.tag = "Floor";
		dungeonFloor.transform.parent = floorParent.transform;

		// Checks edges of the mesh for a potential room for a wall spawn
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

	private void CreateCeiling(Vector2 bottomLeftCorner, Vector2 topRightCorner, int ceilingHeight, bool faceUp)
	{
		// Initialising the vertices of each corner into an array for mesh

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

		// Initialising the uvs for mesh
		Vector2[] uvs = new Vector2[vertices.Length];
		for(int i = 0; i < uvs.Length; i++)
		{
			uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
		}
		// Initialising the points of the triangles into an array for mesh

		int[] triangles = new int[]
		{
			0,
			2,
			1,
			2,
			3,
			1
		};

		// Readjusting the triangle points to render on the other side
		if(faceUp)
		{
			triangles[0] = 1;
			triangles[1] = 2;
			triangles[2] = 0;
			triangles[3] = 2;
			triangles[4] = 1;
			triangles[5] = 3;
		}

		// Initialising the mesh and assigning the variables previously made 
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = triangles;

		// Initialising the the mesh as a GameObject with necessary components
		GameObject dungeonCeiling = new GameObject("Ceiling" + bottomLeftCorner, typeof(MeshFilter), typeof(MeshRenderer));
		dungeonCeiling.transform.position = new Vector3(0, ceilingHeight * 1.18f, 0);
		dungeonCeiling.transform.localScale = Vector3.one;
		dungeonCeiling.GetComponent<MeshFilter>().mesh = mesh;
		dungeonCeiling.GetComponent<MeshRenderer>().material = ceilingMat;
		
		dungeonCeiling.transform.parent = ceilingParent.transform;
	}

	private void AddWallPositionToList(Vector3 wallPosition, List<Vector3Int> wallList, List<Vector3Int> doorList)
	{
		// Any possible spots for a wall are added into a list
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