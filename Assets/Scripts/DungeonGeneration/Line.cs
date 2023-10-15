
using UnityEngine;

public class Line
{
	// Stores the data used to split the spaces
	Orientation orientation;
	Vector2Int coordinates;

	public Line(Orientation orientation, Vector2Int coordinates)
	{
		this.orientation = orientation;
		this.coordinates = coordinates;
	}
	
	public Orientation Orientation { get => orientation; set => orientation = value; }
	
	public Vector2Int Coordinates { get => coordinates; set => coordinates = value; }
}

public enum Orientation
{
	Horizontal = 0,
	Vertical = 1
}
