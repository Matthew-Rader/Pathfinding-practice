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

	private Queue pathRequests = new Queue();

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
		if (Time.time > nodeGraph.rescanTimer)
		{
			nodeGraph.ScanGrid();

			nodeGraph.rescanTimer = Time.time + nodeGraph.rescanRate;
		}

		// Find a path for a caller waiting in line
		if (pathRequests.Count > 0)
			FindPath((PathFinding_Request)pathRequests.Dequeue());
	}

	public void RequestPath (Transform start, Transform goal, PathFindingEntity caller)
	{
		PathFinding_Request newRequest = new PathFinding_Request(start, goal, caller);
		pathRequests.Enqueue(newRequest);
	}

	public void FindPath(PathFinding_Request request)
	{
		Node startNode = nodeGraph.NodeFromWorldPoint(request.start.position);
		Node goalNode = nodeGraph.NodeFromWorldPoint(request.goal.position);

		bool foundPath = algorithmReferences[algoToUse].FindPath(startNode, goalNode, heuristic, ref request.nodeData, nodeGraph.MaxGridSize);

		if (foundPath)
		{
			// If a path was found reverse it and return the request data
			request.path = ReversePath(request);
			request.caller.UpdatePathData(request);
		}
		else
		{
			// If a path was not found return a null path
			request.path = null;
			request.caller.UpdatePathData(request);
		}
	}

	List<Node> ReversePath (PathFinding_Request pathFinding_Request)
	{
		List<Node> newPath = new List<Node>();

		Node goalNode = nodeGraph.NodeFromWorldPoint(pathFinding_Request.goal.position);
		Node startNode = nodeGraph.NodeFromWorldPoint(pathFinding_Request.start.position);

		Node curNode = goalNode;

		while (curNode != startNode)
		{
			newPath.Add(curNode);
			curNode = pathFinding_Request.nodeData[curNode].parentNode;
		}

		newPath.Add(startNode);

		return newPath;
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

public struct PathFinding_Request
{
	public PathFindingEntity caller;
	public bool pathFound;
	public Transform start;
	public Transform goal;
	public Dictionary<Node, Node_Data> nodeData;
	public List<Node> path;

	public PathFinding_Request (Transform _start, Transform _goal, PathFindingEntity _caller)
	{
		caller = _caller;
		pathFound = false;
		start = _start;
		goal = _goal;
		nodeData = new Dictionary<Node, Node_Data>();
		path = null;
	}
}
