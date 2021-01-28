using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
	private bool walkable;
	public bool Walkable
	{
		get { return walkable; }
		set { walkable = value; }
	}

	public Vector3 position;
	public Vector3 Position
	{
		get { return position; }
		set { position = value; }
	}

	public List<Node> adjacentNodes = new List<Node>();

	public bool scanned = false;

	public Node (bool inWalkable, Vector3 inPosition)
	{
		walkable = inWalkable;
		position = inPosition;
	}

	public void ResetNode ()
	{
		walkable = false;
		scanned = false;
	}
}
