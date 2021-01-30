using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Grid 
{
	// Layers which our path should not be able to pass through
    public LayerMask unwalkableLayers;

	// The size of our grid in world units
    public Vector2 gridSize;
	[HideInInspector] public int gridSizeX, gridSizeY;
	public int MaxGridSize
	{
		get { return Mathf.RoundToInt(gridSizeX * gridSizeY); }
	}

	// Where the grid originates from in world space
	public Vector3 gridOriginPosition;

	// The size of each node in units 
    public float nodeRadius;
	[HideInInspector] public float nodeDiameter;

	[HideInInspector] public Node[,] grid;

	[Header("Adjacency Modifiers")]
	public bool allowDiagonalMovement = false;
	public bool cutCorners = false;

	[Header("Rate at which the grid will rescan to create graph")]
	public float rescanRate = 0.1f;
	[HideInInspector] public float rescanTimer = 0.0f;

	public void InitializeGrid ()
	{
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridSize.y / nodeDiameter);

		grid = new Node[gridSizeX, gridSizeY];

		ScanGrid();
	}

	public void ScanGrid ()
	{
		Vector3 worldBottomLeft = gridOriginPosition - Vector3.right * gridSize.x / 2 - Vector3.forward * gridSize.y / 2;
		Node curNode;

		// Generate all the grid nodes and determine if they are walkable
		for (int x = 0; x < gridSizeX; ++x)
		{
			for (int y = 0; y < gridSizeY; ++y)
			{
				curNode = grid[x, y];
				Vector3 nodePosition = curNode != null ? 
											curNode.position : 
											(worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius));

				bool walkable = !(Physics.CheckBox(nodePosition, new Vector3(nodeRadius, nodeRadius, nodeRadius), Quaternion.identity, unwalkableLayers));

				if (curNode == null)
					grid[x, y] = new Node(walkable, nodePosition);
				else
					grid[x, y].Walkable = walkable;
			}
		}

		// Create the adjacency list for each node.
		// Adjacency is determined if a node in the cardinal directions is walkable
		for (int x = 0; x < gridSizeX; ++x)
		{
			for (int y = 0; y < gridSizeY; ++y)
			{
				bool foundLeft, foundRight, foundUp, foundDown;
				foundLeft = foundRight = foundUp = foundDown = false;
				curNode = grid[x, y];

				if (curNode.adjacentNodes.Count > 0)
					curNode.adjacentNodes.Clear();

				#region Find cardinal direction adjacency
				// Left
				if (y != 0)
				{
					if (grid[x, y - 1].Walkable)
					{
						curNode.adjacentNodes.Add(grid[x, y - 1]);
						foundLeft = true;
					}
				}

				// Right
				if (y <= gridSizeY - 2)
				{
					if (grid[x, y + 1].Walkable)
					{
						curNode.adjacentNodes.Add(grid[x, y + 1]);
						foundRight = true;
					}
				}

				// Down
				if (x != 0)
				{
					if (grid[x - 1, y].Walkable)
					{
						curNode.adjacentNodes.Add(grid[x - 1, y]);
						foundDown = true;
					}
				}

				// Up
				if (x <= gridSizeX - 2)
				{
					if (grid[x + 1, y].Walkable)
					{
						curNode.adjacentNodes.Add(grid[x + 1, y]);
						foundUp = true;
					}
				}
				#endregion

				#region Find diagonal adjacency
				if (allowDiagonalMovement)
				{
					// Top Left
					if (x + 1 <= gridSizeX - 2 && y - 1 >= 0)
					{
						if (grid[x + 1, y - 1].Walkable)
							if (cutCorners || (foundLeft && foundUp))
								curNode.adjacentNodes.Add(grid[x + 1, y - 1]);

					}

					// Top Right
					if (x + 1 <= gridSizeX - 2 && y + 1 <= gridSizeY - 2)
					{
						if (grid[x + 1, y + 1].Walkable)
							if (cutCorners || (foundRight && foundUp))
								curNode.adjacentNodes.Add(grid[x + 1, y + 1]);
					}

					// Bottom Left
					if (x - 1 >= 0 && y - 1 >= 0)
					{
						if (grid[x - 1, y - 1].Walkable)
							if (cutCorners || (foundLeft && foundDown))
								curNode.adjacentNodes.Add(grid[x - 1, y - 1]);
					}

					// Bottom Right
					if (x - 1 >= 0 && y + 1 <= gridSizeY - 2)
					{
						if (grid[x - 1, y + 1].Walkable)
							if (cutCorners || (foundRight && foundDown))
								curNode.adjacentNodes.Add(grid[x - 1, y + 1]);
					}
				}
				#endregion
			}
		}
	}

	public Node NodeFromWorldPoint (Vector3 worldPosition)
	{
		float percentX = (worldPosition.x + gridSize.x / 2) / gridSize.x;
		float percentY = (worldPosition.z + gridSize.y / 2) / gridSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

		return grid[x, y];
	}
}
