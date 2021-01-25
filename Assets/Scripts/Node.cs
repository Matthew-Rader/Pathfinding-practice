using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
	public bool walkable;
	public Vector3 position;
	public List<Node> adjacentNodes = new List<Node>();

	public Node (bool inWalkable, Vector3 inPosition)
	{
		walkable = inWalkable;
		position = inPosition;
	}
}
