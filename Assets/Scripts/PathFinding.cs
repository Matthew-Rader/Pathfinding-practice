using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PathFinding : MonoBehaviour
{
	//Singleton reference
	private static PathFinding _instance;
	public static PathFinding Instance { get { return _instance; } }

	[Header ("Graph Properties")]
	[Tooltip ("Properties representing the graph our nodes exist in")]
	public Grid nodeGraph = new Grid();

	public enum PathFindingAlgoSelect { BFS, GBFS, A_STAR};

	[Header("Path-Finding Algorithms")]
	[Tooltip("The algorithm used to find a path")]
	public PathFindingAlgoSelect algoToUse = PathFindingAlgoSelect.BFS;

	Dictionary<PathFindingAlgoSelect, PathFinding_Algo> algorithmReferences = new Dictionary<PathFindingAlgoSelect, PathFinding_Algo>();

	public PathFinding_Heuristics heuristic = new PathFinding_Heuristics();

	private void Awake ()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		}
		else
		{
			_instance = this;
		}
	}

	private void Start ()
	{
		nodeGraph.InitializeGrid();

		algorithmReferences[PathFindingAlgoSelect.BFS] = new Breadth_FirstSearch();
		algorithmReferences[PathFindingAlgoSelect.GBFS] = new Best_FirstSearch();
		algorithmReferences[PathFindingAlgoSelect.A_STAR] = new A_Star();
	}

	public void FindPath (Transform start, Transform goal, ref Dictionary<Node, Node_Data> nodeData)
	{
		Node startNode = nodeGraph.NodeFromWorldPoint(start.position);
		Node goalNode = nodeGraph.NodeFromWorldPoint(goal.position);

		algorithmReferences[algoToUse].FindPath(startNode, goalNode, heuristic, ref nodeData, nodeGraph.MaxGridSize);
	}

	//private void OnDrawGizmos ()
	//{
	//	Handles.DrawWireCube(transform.position, new Vector3(nodeGraph.gridSize.x, 1, nodeGraph.gridSize.y));

	//	if (nodeGraph.grid != null)
	//	{
	//		foreach (Node n in nodeGraph.grid)
	//		{
	//			Handles.color = (n.walkable) ? Color.white : Color.grey;

	//			if (nodeData.ContainsKey(n))
	//			{
	//				if (nodeData[n].parentNode != null)
	//				{
	//					Handles.color = new Color(71.0f / 255.0f, 92.0f / 255.0f, 255.0f / 255.0f, 1.0f);
	//				}
	//			}

	//			if (path != null)
	//			{
	//				if (path.Contains(n))
	//				{
	//					Handles.color = Color.yellow;
	//				}
	//			}

	//			if (nodeGraph.NodeFromWorldPoint(startPosition.position) == n)
	//			{
	//				Handles.color = Color.green;
	//			}
	//			else if (nodeGraph.NodeFromWorldPoint(goalPosition.position) == n)
	//			{
	//				Handles.color = Color.red;
	//			}

	//			Handles.CubeCap(0, n.position, Quaternion.identity, nodeGraph.nodeDiameter - 0.1f);

	//			if (nodeData.ContainsKey(n))
	//			{
	//				if (n.walkable && nodeData[n].fCost > 0 && nodeGraph.displayWeight)
	//				{
	//					GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
	//					{
	//						alignment = TextAnchor.MiddleCenter,
	//						fontSize = 14,
	//						fontStyle = FontStyle.Bold
	//					};

	//					GUI.color = Color.black;

	//					Handles.Label(new Vector3(n.position.x - 0.1f, 0.0f, n.position.z + 0.25f), nodeData[n].hCost.ToString(), labelStyle);
	//				}
	//			}
	//		}
	//	}
	//}
}
