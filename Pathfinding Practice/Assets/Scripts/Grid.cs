using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Grid : MonoBehaviour 
{
	// Layers which our path should not be able to pass through
    public LayerMask unwalkableLayers;

	// The size of our grid in world units
    public Vector2 gridSize;

    public float nodeRadius;
    Node[,] grid;

	float nodeDiameter;
	int gridSizeX, gridSizeY;

	// Currently used to visualize the path; not necessary for grid functions
	PathFinding pf;

	[Header ("Debugging Variables")]
	[SerializeField] private bool displayWeight = false;

	private void Start ()
	{
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridSize.y / nodeDiameter);

		CreateGrid();

		pf = GetComponent<PathFinding>();
	}

	void CreateGrid ()
	{
		grid = new Node[gridSizeX, gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridSize.x / 2 - Vector3.forward * gridSize.y / 2;

		// Generate all the grid nodes and determine if they are walkable
		for (int x = 0; x < gridSizeX; ++x)
		{
			for (int y = 0; y < gridSizeY; ++y)
			{
				Vector3 nodePosition = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				bool walkable = !(Physics.CheckBox(nodePosition, new Vector3(nodeRadius, nodeRadius, nodeRadius), Quaternion.identity, unwalkableLayers));
				grid[x, y] = new Node(walkable, nodePosition);
			}
		}

		// Create the adjacency list for each node.
		// Adjacency is determined if a node in the cardinal directions is walkable
		for (int x = 0; x < gridSizeX; ++x)
		{
			for (int y = 0; y < gridSizeY; ++y)
			{
				Node curNode = grid[x, y];

				if (y != 0)
				{
					if (grid[x, y-1].walkable)
						curNode.adjacentNodes.Add(grid[x, y - 1]);
				}

				if (y <= gridSizeY - 2)
				{
					if (grid[x, y + 1].walkable)
						curNode.adjacentNodes.Add(grid[x, y + 1]);
				}

				if (x != 0)
				{
					if (grid[x - 1, y].walkable)
						curNode.adjacentNodes.Add(grid[x - 1, y]);
				}

				if (x <= gridSizeX - 2)
				{
					if (grid[x + 1, y].walkable)
						curNode.adjacentNodes.Add(grid[x + 1, y]);
				}
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

	private void OnDrawGizmos () 
    {
		Handles.DrawWireCube(transform.position, new Vector3(gridSize.x, 1, gridSize.y));

		if (grid != null)
		{
			foreach (Node n in grid)
			{
				Handles.color = (n.walkable) ? Color.white : Color.grey;

				if (pf.nodeData.ContainsKey(n))
				{
					if (pf.nodeData[n].parentNode != null)
					{
						Handles.color = new Color(71.0f / 255.0f, 92.0f / 255.0f, 255.0f / 255.0f, 1.0f);
					}
				}

				if (pf.path != null)
				{
					if (pf.path.Contains(n))
					{
						Handles.color = Color.yellow;
					}
				}

				if (NodeFromWorldPoint(pf.startPosition.position) == n)
				{
					Handles.color = Color.green;
				}
				else if (NodeFromWorldPoint(pf.goalPosition.position) == n)
				{
					Handles.color = Color.red;
				}

				Handles.CubeCap(0, n.position, Quaternion.identity, nodeDiameter - 0.1f);

				if (pf.nodeData.ContainsKey(n))
				{
					if (n.walkable && pf.nodeData[n].weight > 0 && displayWeight)
					{
						GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
						{
							alignment = TextAnchor.MiddleCenter,
							fontSize = 14,
							fontStyle = FontStyle.Bold
						};

						GUI.color = Color.black;

						Handles.Label(new Vector3(n.position.x - 0.1f, 0.0f, n.position.z + 0.25f), pf.nodeData[n].weight.ToString(), labelStyle);
					}
				}
			}
		}
	}
}
