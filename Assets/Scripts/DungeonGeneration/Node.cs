using System.Collections.Generic;

using UnityEngine;

public abstract class Node
{
	// Stores all the data needed for nodes
	private List<Node> childrenNodeList;

	public List<Node> ChildrenNodeList
	{
		get => childrenNodeList;
	}

	public bool Visited { get; set; }

	public Vector2Int BottomLeftAreaCorner { get; set; }
	public Vector2Int BottomRightAreaCorner { get; set; }
	public Vector2Int TopRightAreaCorner { get; set; }
	public Vector2Int TopLeftAreaCorner { get; set; }

	public Node Parent { get; set; }

	public int TreeLayerIndex { get; set; }

	public Node(Node parentNode)
	{
		childrenNodeList = new List<Node>();
		this.Parent = parentNode;
		if(parentNode != null)
		{
			parentNode.AddChild(this);
		}
	}

	/// <summary>
	/// Adds Child Node To List
	/// </summary>
	/// <param name="node">Node To Add</param>
	public void AddChild(Node node)
	{
		childrenNodeList.Add(node);
	}

	/// <summary>
	/// Removes Child From List
	/// </summary>
	/// <param name="node">Node To Remove</param>
	public void RemoveChild(Node node)
	{
		childrenNodeList.Remove(node);
	}
}