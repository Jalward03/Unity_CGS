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

	public List<Node> CalculateDungeon(int maxIterations, int roomWidthMin, int roomLengthMin, float roomBottomCornerModifier, float roomTopCornerModifier, int roomOffset, int corridorWidth)
	{
		// Initialises all the variables and store them in other classes to use
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
