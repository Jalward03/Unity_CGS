using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator
{
	private int maxIterations;
	private int roomLengthMin;
	private int roomWidthMin;

	public RoomGenerator(int maxIterations, int roomLengthMin, int roomWidthMin)
	{
		this.maxIterations = maxIterations;
		this.roomLengthMin = roomLengthMin;
		this.roomWidthMin = roomWidthMin;
	}

	/// <summary>
	/// Generates the list of rooms and the data needed to generate
	/// </summary>
	/// <param name="roomSpaces">List Of Room Spaces</param>
	/// <param name="roomBottomCornerModifier">Spacing Of Bottom Corner Vertices</param>
	/// <param name="roomTopCornerModifier">Spacing Of Top Corner Vertices</param>
	/// <param name="roomOffset">Distance Between Rooms</param>
	/// <returns>List Of Room Spaces</returns>
	public List<RoomNode> GenerateRoomsInGivenSpaces(List<Node> roomSpaces, float roomBottomCornerModifier, float roomTopCornerModifier, int roomOffset)
	{
		
		List<RoomNode> listToReturn = new List<RoomNode>();
		foreach(var space in roomSpaces)
		{
			Vector2Int newBottomLeftPoint = 
				StructureHelper.GenerateBottomLeftCornerBetween(space.BottomLeftAreaCorner, space.TopRightAreaCorner, roomBottomCornerModifier, roomOffset);
			Vector2Int newTopRightPoint = 
				StructureHelper.GenerateTopRightCornerBetween(space.BottomLeftAreaCorner, space.TopRightAreaCorner, roomTopCornerModifier, roomOffset);

			space.BottomLeftAreaCorner = newBottomLeftPoint;
			space.TopRightAreaCorner = newTopRightPoint;
			space.BottomRightAreaCorner = new Vector2Int(newTopRightPoint.x, newBottomLeftPoint.y);
			space.TopLeftAreaCorner = new Vector2Int(newBottomLeftPoint.x, newTopRightPoint.y);
			listToReturn.Add((RoomNode)space);
		}

		return listToReturn;
	}
}
