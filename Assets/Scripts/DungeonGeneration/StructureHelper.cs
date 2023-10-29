using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Random = UnityEngine.Random;

public static class StructureHelper
{
	/// <summary>
	/// Re-Assigns parents of nodes
	/// </summary>
	/// <param name="parentNode">Parent Node</param>
	/// <returns>Updated List</returns>
	public static List<Node> TraverseGraphToExtractLowestLeaves(Node parentNode)
	{
		Queue<Node> nodesToCheck = new Queue<Node>();
		List<Node> listToReturn = new List<Node>();
		if(parentNode.ChildrenNodeList.Count == 0)
		{
			return new List<Node>() { parentNode };
		}

		foreach(var child in parentNode.ChildrenNodeList)
		{
			nodesToCheck.Enqueue(child);
		}

		while(nodesToCheck.Count > 0)
		{
			var currentNode = nodesToCheck.Dequeue();
			if(currentNode.ChildrenNodeList.Count == 0)
			{
				listToReturn.Add(currentNode);
			}
			else
			{
				foreach(var child in currentNode.ChildrenNodeList)
				{
					nodesToCheck.Enqueue(child);
				}
			}
		}

		return listToReturn;
	}

	/// <summary>
	/// Assigns bottom left vertices of a room
	/// </summary>
	/// <param name="boundaryLeftPoint">Min And Max Points For Left</param>
	/// <param name="boundaryRightPoint">Min And Max Points For Right</param>
	/// <param name="pointModifier">Top Or Bottom Corner Modifier</param>
	/// <param name="offset">Offset to Boundaries</param>
	/// <returns>Bottom Left Vertices</returns>
	public static Vector2Int GenerateBottomLeftCornerBetween(Vector2Int boundaryLeftPoint, Vector2Int boundaryRightPoint, float pointModifier, int offset)
	{
		int minX = boundaryLeftPoint.x + offset;
		int maxX = boundaryRightPoint.x - offset;
		int minY = boundaryLeftPoint.y + offset;
		int maxY = boundaryRightPoint.y - offset;

		return new Vector2Int(
		                      Random.Range(minX, (int) (minX + (maxX - minX) * pointModifier)),
		                      Random.Range(minY, (int) (minY + (maxY - minY) * pointModifier)));
	}
	
	/// <summary>
	/// Assigns top right vertices of a room
	/// </summary>
	/// <param name="boundaryLeftPoint">Min And Max Points For Left</param>
	/// <param name="boundaryRightPoint">Min And Max Points For Right</param>
	/// <param name="pointModifier">Top Or Bottom Corner Modifier</param>
	/// <param name="offset">Offset to Boundaries</param>
	/// <returns>Top Right Vertices</returns>
	public static Vector2Int GenerateTopRightCornerBetween(Vector2Int boundaryLeftPoint, Vector2Int boundaryRightPoint, float pointModifier, int offset)
	{
		// Assigns top right vertices of a room
		int minX = boundaryLeftPoint.x + offset;
		int maxX = boundaryRightPoint.x - offset;
		int minY = boundaryLeftPoint.y + offset;
		int maxY = boundaryRightPoint.y - offset;
		return new Vector2Int(
		                      Random.Range((int)(minX+(maxX-minX)*pointModifier), maxX), 
		                      Random.Range((int)(minY+(maxY-minY)*pointModifier),maxY)
		                      );



	}

	public enum RelativePosition
	{
		// Identifier for corridor direction
		Up,
		Down,
		Right,
		Left
	}

	/// <summary>
	/// Calculates middle points using bottom left vertices and top right vertices
	/// </summary>
	/// <param name="v1">Point 1</param>
	/// <param name="v2">Point 2</param>
	/// <returns>Middle Point</returns>
	public static Vector2Int CalculateMiddlePoint(Vector2Int v1, Vector2Int v2)
	{
		
		Vector2 sum = v1 + v2;
		Vector2 tempVector = sum / 2;

		return new Vector2Int((int)tempVector.x, (int)tempVector.y);
	}
}