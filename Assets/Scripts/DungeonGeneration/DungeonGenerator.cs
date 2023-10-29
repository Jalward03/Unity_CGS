using System;
using System.Collections.Generic;
using System.Linq;

public class DungeonGenerator
{
	RoomNode rootNode;
	private List<RoomNode> allNodesCollection = new List<RoomNode>();
	private int dungeonWidth;
	private int dungeonLength;

	public DungeonGenerator(int dungeonWidth, int dungeonLength)
	{
		this.dungeonWidth = dungeonWidth;
		this.dungeonLength = dungeonLength;
	}

	/// <summary>
	/// Initialises all the variables and store them in other classes to use
	/// </summary>
	/// <param name="maxIterations">Max Rooms For Dungeon</param>
	/// <param name="roomWidthMin">Minimum Width Of A Given Room</param>
	/// <param name="roomLengthMin">>Minimum Length Of A Given Room</param>
	/// <param name="roomBottomCornerModifier">Spacing Of Bottom Corner Vertices</param>
	/// <param name="roomTopCornerModifier">Spacing Of Top Corner Vertices</param>
	/// <param name="roomOffset">Distance Between Rooms</param>
	/// <param name="corridorWidth">Width Of Corridor</param>
	/// <returns></returns>
	public List<Node> CalculateDungeon(int maxIterations, int roomWidthMin, int roomLengthMin, float roomBottomCornerModifier, float roomTopCornerModifier, int roomOffset, int corridorWidth)
	{
		
		BinarySpacePartitioner bsp = new BinarySpacePartitioner(dungeonWidth, dungeonLength);
		allNodesCollection = bsp.PrepareNodesCollection(maxIterations, roomWidthMin, roomLengthMin);
		List<Node> roomSpaces = StructureHelper.TraverseGraphToExtractLowestLeaves(bsp.RootNode);
		RoomGenerator roomGenerator = new RoomGenerator(maxIterations, roomLengthMin, roomWidthMin);
		List<RoomNode> roomList = roomGenerator.GenerateRoomsInGivenSpaces(roomSpaces, roomBottomCornerModifier, roomTopCornerModifier, roomOffset);

		CorridorGenerator corridorsGenerator = new CorridorGenerator();
		var corridorList = corridorsGenerator.CreateCorridor(allNodesCollection, corridorWidth);
		return new List<Node>(roomList).Concat(corridorList).ToList();
	}
	
	
}
