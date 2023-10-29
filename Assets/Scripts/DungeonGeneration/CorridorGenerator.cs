using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System;

public class CorridorGenerator
{


	/// <summary>
	/// Uses the collected nodes to add to a list of corridors
	/// </summary>
	/// <param name="allNodesCollection">List of previously collected nodes</param>
	/// <param name="corridorWidth">Width of a corridor</param>
	/// <returns>Updated Nodes to Fit Corridors</returns>
	public List<Node> CreateCorridor(List<RoomNode> allNodesCollection, int corridorWidth)
	{
		
		List<Node> corridorList = new List<Node>();
		Queue<RoomNode> structuresToCheck = new Queue<RoomNode>
			(allNodesCollection.OrderByDescending(node => node.TreeLayerIndex).ToList());
		while(structuresToCheck.Count > 0)
		{
			var node = structuresToCheck.Dequeue();
			if(node.ChildrenNodeList.Count == 0)
			{
				continue;
			}

			CorridorNode corridor = new CorridorNode(node.ChildrenNodeList[0], node.ChildrenNodeList[1], corridorWidth);
			corridorList.Add(corridor);
		}

		return corridorList;
	}
}
