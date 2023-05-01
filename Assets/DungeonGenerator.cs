using System;
using System.Collections.Generic;
public class DungeonGenerator
{
	RoomNode rootNode;
	private List<RoomNode> allSpaceNodes = new List<RoomNode>();
	private int dungeonWidth;
	private int dungeonLength;

	public DungeonGenerator(int dungeonWidth, int dungeonLength)
	{
		this.dungeonWidth = dungeonWidth;
		this.dungeonLength = dungeonLength;
	}

	public List<Node> CalculateRooms(int maxIterations, int roomWidthMin, int roomLengthMin)
	{
		BinarySpaceParitioner bsp = new BinarySpaceParitioner(dungeonWidth, dungeonLength);
		allSpaceNodes = bsp.PrepareNodesCollection(maxIterations, roomWidthMin, roomLengthMin);
		List<Node> roomSpaces = StructureHelper.TraverseGraphToExtractLowestLeaves(bsp.RootNode);
		RoomGenerator roomGenerator = new RoomGenerator(maxIterations, roomLengthMin, roomWidthMin);
		List<RoomNode> roomList = roomGenerator.GenerateRoomsInGivenSpaces(roomSpaces);
		return new List<Node>(roomList);
	}
}
