using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Threading;

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

	private void Update ()
	{
		nodeGraph.ScanGrid();
	}

	public void FindPath (Transform start, Transform goal, ref Dictionary<Node, Node_Data> nodeData)
	{
		Node startNode = nodeGraph.NodeFromWorldPoint(start.position);
		Node goalNode = nodeGraph.NodeFromWorldPoint(goal.position);

		algorithmReferences[algoToUse].FindPath(startNode, goalNode, heuristic, ref nodeData, nodeGraph.MaxGridSize);
	}

	public void FinishedFindingPath ()
	{

	}

	private void OnDrawGizmos ()
	{
		Gizmos.DrawWireCube(transform.position, new Vector3(nodeGraph.gridSize.x, 1, nodeGraph.gridSize.y));

		if (nodeGraph.grid != null)
		{
			foreach (Node n in nodeGraph.grid)
			{
				Gizmos.color = (n.Walkable) ? Color.white : Color.grey;

				Gizmos.DrawCube(n.position, Vector3.one * (nodeGraph.nodeDiameter - 0.1f));
			}
		}
	}
}
