
using UnityEngine;

public class RoomNode : Node
{
	// Assigns room specific data using the node class
	public RoomNode(Vector2Int bottomLeftAreaCorner, Vector2Int topRightAreaCorner, Node parentNode, int index) : base(parentNode)
	{
		this.BottomLeftAreaCorner = bottomLeftAreaCorner;
		this.TopRightAreaCorner = topRightAreaCorner;
		this.BottomRightAreaCorner = new Vector2Int(topRightAreaCorner.x, bottomLeftAreaCorner.y);
		this.TopLeftAreaCorner = new Vector2Int(bottomLeftAreaCorner.x, topRightAreaCorner.y);
		this.TreeLayerIndex = index;
	}
	
	/// <summary>
	/// Returns Width Of A Room
	/// </summary>
	public int Width { get => (int) (TopRightAreaCorner.x - BottomLeftAreaCorner.x); }
	
	/// <summary>
	/// Returns Length Of A Room
	/// </summary>
	public int Length { get => (int) (TopRightAreaCorner.y - BottomLeftAreaCorner.y); }
}