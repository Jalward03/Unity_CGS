using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;


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
	public Material shopRoomMat;
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

	[Header("Furniture")]
	public bool haveFurniture= true;
	public bool haveWallFurniture= true;
	public bool haveTorches= true;
	public GameObject torch;
	public GameObject endRoomEscapeHatch;
	public GameObject shopStall;
	public List<GameObject> furnitureList;
	public List<GameObject> wallFurnitureList;

	[Header("Hazards")]
	public bool haveCorridorTraps= true;
	public int maxHazardsPerRoom;
	public GameObject corridorTrap;
	public List<GameObject> hazardList;

	[Header("Special Rooms")] 
	public bool haveStartingRoom = true;
	public bool haveEscapeHatchRoom= true;
	public bool haveShopRoom= true;


	[Header("Map")] public GameObject mapCanvas;
	private GameObject parentMap;
	public GameObject playerIconCircle;
	public Color mapRoomColor;
	public Color mapShopRoomColor;

	private List<GameObject> miniMapTiles;

	private void Awake()
	{

		if(mapCanvas)
		{
			mapCanvas = Instantiate(mapCanvas);
			parentMap = mapCanvas.transform.GetChild(0).gameObject;
			parentMap.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 200);
			playerIconCircle = Instantiate(playerIconCircle);
			
		}

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
	

	/// <summary>
	/// Creates and assigns dungeon variables from inspector
	/// </summary>
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

		int endRoomIndex = Random.Range(maxIterations / 2, listOfRooms.Count / 2);
		int shopRoomIndex = Random.Range(maxIterations / 2, listOfRooms.Count / 2);

		// Fail safe in case same room
		while(shopRoomIndex == endRoomIndex)
		{
			shopRoomIndex = Random.Range(maxIterations / 2, listOfRooms.Count / 2);
		}

		// Generates contents of each room in the list
		for(int i = 0; i < listOfRooms.Count; i++)
		{
			CreateMesh(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, (i == shopRoomIndex && haveShopRoom));
			CreateCeiling(listOfRooms[i].BottomLeftAreaCorner, listOfRooms[i].TopRightAreaCorner, ceilingHeight, false);

			// Spawn Room
			if(i == 0)
			{
				SpawnPlayer(new Vector3(
				                        (listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2,
				                        1,
				                        (listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2));

				if(!haveStartingRoom)
				{
					SpawnHazards(listOfRooms, i);

					SpawnFurniture(new Vector3(
					                           (listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2,
					                           0,
					                           (listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2));

					// Spawn lantern and in the middle ceiling of every room
					SpawnTorches(new Vector3(
					                         (listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2,
					                         ceilingHeight * 2.18f,
					                         (listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2));
				}
			}

			// Rooms after spawn room
			if(i > 0 && i <= listOfRooms.Count / 2 && i != endRoomIndex && i != shopRoomIndex)
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

			// Shop Room
			if(i == shopRoomIndex)
			{
				if(haveShopRoom)
					SpawnStall(new Vector3(
					                       (listOfRooms[i].BottomLeftAreaCorner.x + listOfRooms[i].TopRightAreaCorner.x) / 2,
					                       0,
					                       (listOfRooms[i].BottomLeftAreaCorner.y + listOfRooms[i].TopRightAreaCorner.y) / 2));

				if(!haveShopRoom) SpawnHazards(listOfRooms, i);
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

	/// <summary>
	/// Gets a new random number within range
	/// </summary>
	/// <param name="maxNum">Random Max Range</param>
	/// <returns></returns>
	public int GetNewRand(int maxNum)
	{
		
		int rand = Random.Range(0, maxNum);

		return rand;
	}

	/// <summary>
	/// Chooses to spawn 0, 1, or 2 traps on each corridor 
	/// </summary>
	/// <param name="position1">Potential First Position Of a Corridor Trap</param>
	/// <param name="position2">Potential Second Position Of a Corridor Trap</param>
	/// <param name="rotation">Rotation Of A Potential Corridor Trap</param>
	public void SpawnCorridorTrap(Vector3 position1, Vector3 position2, Quaternion rotation)
	{
		if(haveCorridorTraps)
		{
			
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

	/// <summary>
	/// Spawns Random Hazards Relative To Room Sizes
	/// </summary>
	/// <param name="listOfRooms">List Of All Rooms</param>
	/// <param name="index">Index Of Specified Room</param>
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

	/// <summary>
	/// Spawns wall furniture
	/// </summary>
	/// <param name="position">Position Of Wall Furniture</param>
	/// <param name="rotation">Rotation Of Wall Furniture</param>
	public void SpawnWallFurniture(Vector3 position, Quaternion rotation)
	{
		if(haveWallFurniture)
		{
			int rand = Random.Range(0, wallFurnitureList.Count);
			GameObject wallFurniture = wallFurnitureList[rand];
			Instantiate(wallFurniture, position, rotation, wallFurnitureParent.transform);
		}
	}

	/// <summary>
	/// Spawns A Torch
	/// </summary>
	/// <param name="position">Position Of Torch</param>
	public void SpawnTorches(Vector3 position)
	{
		
		if(haveTorches) Instantiate(torch, position, Quaternion.identity, torchParent.transform);
	}

	/// <summary>
	/// Spawns Furniture
	/// </summary>
	/// <param name="position">Position Of Furniture</param>
	public void SpawnFurniture(Vector3 position)
	{
		if(haveFurniture)
		{
			int rand = Random.Range(0, furnitureList.Count);
			GameObject furniture = furnitureList[rand];
			Instantiate(furniture, position, Quaternion.identity, furnitureParent.transform);
		}
	}

	/// <summary>
	/// Spawns Stall For Shop Room
	/// </summary>
	/// <param name="position">Position Of Stall</param>
	public void SpawnStall(Vector3 position)
	{
		Instantiate(shopStall, position, Quaternion.identity, furnitureParent.transform);
	}

	/// <summary>
	/// Spawns Player
	/// </summary>
	/// <param name="position">Position Of Player</param>
	public void SpawnPlayer(Vector3 position)
	{
		player = Instantiate(player, position, Quaternion.identity);
	}

	/// <summary>
	/// Spawns hatch
	/// </summary>
	/// <param name="position">Position Of Hatch</param>
	/// <param name="haveHatchRoom">Will A Hatch Spawn?</param>
	public void SpawnHatch(Vector3 position, bool haveHatchRoom)
	{
		if(haveHatchRoom) Instantiate(endRoomEscapeHatch, position, Quaternion.identity, transform);
	}

	/// <summary>
	/// Creates Walls using Calculated Wall Positions
	/// </summary>
	/// <param name="wallParent">Parent For All Walls</param>
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

	/// <summary>
	/// Instantiates A Wall
	/// </summary>
	/// <param name="wallParent">Parent For A Wall</param>
	/// <param name="wallPosition">Position Of Wall</param>
	/// <param name="wallPrefab">Wall Prefab</param>
	private void CreateWall(GameObject wallParent, Vector3Int wallPosition, GameObject wallPrefab)
	{
		Instantiate(wallPrefab, wallPosition, Quaternion.identity, wallParent.transform);
	}

	/// <summary>
	/// Creates Floor Mesh
	/// </summary>
	/// <param name="bottomLeftCorner">Bottom Left Vertices</param>
	/// <param name="topRightCorner">Top Right Vertices</param>
	/// <param name="isShopRoom">Is A Shop Room?</param>
	private void CreateMesh(Vector2 bottomLeftCorner, Vector2 topRightCorner, bool isShopRoom)
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
		if(!isShopRoom) dungeonFloor.GetComponent<MeshRenderer>().material = floorMat;
		if(isShopRoom)
		{
			dungeonFloor.tag = "Shop";
			dungeonFloor.GetComponent<MeshRenderer>().material = shopRoomMat;
		}

		dungeonFloor.GetComponent<MeshCollider>().sharedMesh = mesh;
		dungeonFloor.GetComponent<FloorVerticesStorage>().meshVerts = vertices;
		dungeonFloor.layer = LayerMask.NameToLayer("Water");

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

	/// <summary>
	/// Creates Mesh For Ceiling
	/// </summary>
	/// <param name="bottomLeftCorner">Bottom Left Vertices</param>
	/// <param name="topRightCorner">Top Right Vertices</param>
	/// <param name="ceilingHeight">Height Of Ceiling</param>
	/// <param name="faceUp">Will Ceiling Face Up?</param>
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

	/// <summary>
	/// Any possible spots for a wall are added into a list
	/// </summary>
	/// <param name="wallPosition">Position Of A Wall</param>
	/// <param name="wallList">List Of Walls</param>
	/// <param name="doorList">List Of Gaps</param>
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