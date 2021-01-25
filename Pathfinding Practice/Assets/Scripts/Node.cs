using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
	public bool walkable;
	public Vector3 position;
	public Node parentNode;
	public float weight;
	public bool inClosedSet = false;
	public bool inOpenSet = false;
	public List<Node> adjacentNodes = new List<Node>();

	public Node (bool inWalkable, Vector3 inPosition)
	{
		walkable = inWalkable;
		position = inPosition;
	}
}
