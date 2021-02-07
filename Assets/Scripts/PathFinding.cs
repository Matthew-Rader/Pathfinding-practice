using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Threading;

public class PathFinding : MonoBehaviour
{
	#region Singleton reference
	private static PathFinding _instance;
	public static PathFinding Instance { get { return _instance; } }
	#endregion

	#region Node Graph Data
	[Header ("Graph Properties")]
	[Tooltip ("Properties representing the graph our nodes exist in")]
	public Grid nodeGraph = new Grid();
	#endregion

	#region PathFinding Algorithm Data
	public enum PathFindingAlgoSelect { BFS, GBFS, A_STAR};

	[Header("PathFinding Algorithms")]
	[Tooltip("The algorithm used to find a path")]
	public PathFindingAlgoSelect algoToUse = PathFindingAlgoSelect.BFS;

	// Dictionary used to call the selected pathfinding algorithm's FindPath function
	Dictionary<PathFindingAlgoSelect, PathFinding_Algo> algorithmReferences = new Dictionary<PathFindingAlgoSelect, PathFinding_Algo>();

	// Algorithm heuristic selection
	public PathFinding_Heuristics heuristic = new PathFinding_Heuristics();
	#endregion

	#region Thread Control Variables
	[Header ("Threading")]
	[Tooltip ("The max number of threads allowed to be active at once")]
	public int maxNumberOfThreads = 5;

	// Counter to keep track of the number of active threads running
	private int _ActiveThreads = 0;

	private bool waitingToScanGrid = false;
	#endregion

	#region Local Private Variables
	// FIFO way to control the rate of path finding threads being spun up
	private Queue pathRequests = new Queue();
	private Queue pathRequesetResults = new Queue();
	#endregion

	#region Debug Variables
	[Header ("Debug Variables")]
	public bool _DrawDebugData = false;
	#endregion

	private void Awake ()
	{
		// Establishing the singleton
		if (_instance != null && _instance != this)
		{
			Destroy(this.gameObject);
		}
		else
		{
			_instance = this;
		}

		// Initialize our Grid
		nodeGraph.InitializeGrid();

		// Initialize our algorithm selection dictionary
		algorithmReferences[PathFindingAlgoSelect.BFS] = new Breadth_FirstSearch();
		algorithmReferences[PathFindingAlgoSelect.GBFS] = new Best_FirstSearch();
		algorithmReferences[PathFindingAlgoSelect.A_STAR] = new A_Star();
	}

	private void Update ()
	{
		// If we aren't waiting to scan the grid, increment our timer for the next time we need to
		if (!waitingToScanGrid)
		{
			nodeGraph.rescanTimer += Time.deltaTime;

			if (nodeGraph.rescanTimer > nodeGraph.rescanRate)
			{
				// Flag that we would like to scan the grid
				waitingToScanGrid = true;
				nodeGraph.rescanTimer = 0.0f;
			}
		}

		// Wait to scan the grid until there are no active threads which could be using the 
		//previous scans grid data.
		if (waitingToScanGrid && _ActiveThreads == 0)
		{
			nodeGraph.ScanGrid();
			waitingToScanGrid = false;
		}

		// Check if we have any requests to spin up finding a path for
		if (pathRequests.Count > 0)
		{
			// Spin up a path finding thread if it's safe to do so and we haven't hit our max thread count
			if (!waitingToScanGrid && _ActiveThreads < maxNumberOfThreads)
			{
				StartPathFindingThread((PathFinding_Request)pathRequests.Dequeue());
				//FindPath((PathFinding_Request)pathRequests.Dequeue());
			}
		}

		// Check if we have any path results to call back to
		if (pathRequesetResults.Count > 0)
		{
			PathFinding_Result result = (PathFinding_Result)pathRequesetResults.Dequeue();
			result.callBack(result.path, result.foundPath);
		}
	}

	/// <summary>
	/// Create a pathfinding request from a start to a goal position. 
	/// </summary>
	/// <param name="start">Start position Transform</param>
	/// <param name="goal">Goal position Transform</param>
	/// <param name="caller">Self reference to call back and update path data</param>
	public static void RequestPath (Transform start, Transform goal, Action<List<Node>, bool> callBack)
	{
		Node startNode = _instance.nodeGraph.NodeFromWorldPoint(start.position);
		Node goalNode = _instance.nodeGraph.NodeFromWorldPoint(goal.position);
		PathFinding_Request newRequest = new PathFinding_Request(startNode, goalNode, callBack);
		_instance.pathRequests.Enqueue(newRequest);
	}

	/// <summary>
	/// Spin up a thread to find a path given PathFinding Request data
	/// </summary>
	/// <param name="request"></param>
	private void StartPathFindingThread (PathFinding_Request request)
	{
		Thread pathFindingThread = new Thread(() => FindPath(request));

		// Set the threads priority to low so it doesn't take resources from the main thread
		pathFindingThread.Priority = System.Threading.ThreadPriority.Lowest;

		// Tell the thread to run in the background
		pathFindingThread.IsBackground = true;

		pathFindingThread.Start();

		_ActiveThreads++;
	}

	private void FindPath(PathFinding_Request request)
	{
		bool foundPath = algorithmReferences[algoToUse].FindPath(request.start, request.goal, heuristic, ref request.nodeData, nodeGraph.MaxGridSize);
		List<Node> path;

		if (foundPath)
		{
			// Build the found path list from Goal to Start
			path = BuildPath(request);

			if (path.Count > 1)
			{
				// Reverse the found path to be from Start to Goal
				path.Reverse();

				// Simplify the path to take up less space and only track pivot points
				path = SimplifyPath(path);
			}
		}
		else
		{
			// If a path was not found return a null path
			path = null;
		}

		PathFinding_Result result = new PathFinding_Result(request.callBack, path, foundPath);

		// Lock the PathFinding class instance and decrement the active threads count
		lock (_instance)
		{
			_ActiveThreads--;
			pathRequesetResults.Enqueue(result);
		}
	}

	/// <summary>
	/// Uses pathfinding nodeData to build the found path from Goal to Start
	/// </summary>
	/// <param name="pathFinding_Request"></param>
	/// <returns>Returns found path from Goal to Start</returns>
	List<Node> BuildPath (in PathFinding_Request pathFinding_Request)
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

	/// <summary>
	/// Takes a path and returns a simplified version of it. Nodes in the path will now represent pivot/turning points.
	/// </summary>
	/// <param name="originalPath"></param>
	/// <returns> Simplified list of nodes </returns>
	List<Node> SimplifyPath (in List<Node> originalPath)
	{
		List<Node> simplifiedPath = new List<Node>();

		Vector3 curNodePos;
		Vector3 nextNodePos;

		// Add the starting postiion node
		simplifiedPath.Add(originalPath[0]);

		Vector2 previousMoveVec;

		// Get the starting movement direction of the starting node to the first node in the path
		curNodePos = originalPath[0].position;
		nextNodePos = originalPath[1].position;

		previousMoveVec = new Vector2(nextNodePos.x - curNodePos.x,
									  nextNodePos.z - curNodePos.z);

		// Normlize the vector because we only care about the direction
		previousMoveVec.Normalize();

		for (int i = 1; i < originalPath.Count - 1; ++i)
		{
			curNodePos = originalPath[i].position;
			nextNodePos = originalPath[i + 1].position;

			Vector2 dir = new Vector2(nextNodePos.x - curNodePos.x,
									  nextNodePos.z - curNodePos.z);

			dir.Normalize();

			// If there is a turn in the path add the node to the simplified path, else ignore it
			if (dir != previousMoveVec)
			{
				previousMoveVec = dir;
				simplifiedPath.Add(originalPath[i]);
			}
		}

		// Add the last node of the path since it is the goal
		simplifiedPath.Add(originalPath[originalPath.Count - 1]);

		return simplifiedPath;
	}

	private void OnDrawGizmos ()
	{
		Gizmos.DrawWireCube(transform.position, new Vector3(nodeGraph.gridSize.x, 1, nodeGraph.gridSize.y));

		if (nodeGraph.grid != null && _DrawDebugData)
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
	public Action<List<Node>, bool> callBack;
	public Node start;
	public Node goal;
	public Dictionary<Node, Node_Data> nodeData;

	public PathFinding_Request (Node _start, Node _goal, Action<List<Node>, bool> _callBack)
	{
		callBack = _callBack;
		start = _start;
		goal = _goal;
		nodeData = new Dictionary<Node, Node_Data>();
	}
}

public struct PathFinding_Result
{
	public Action<List<Node>, bool> callBack;
	public List<Node> path;
	public bool foundPath;

	public PathFinding_Result (Action<List<Node>, bool> _callBack, List<Node> _path, bool _foundPath)
	{
		callBack = _callBack;
		path = _path;
		foundPath = _foundPath;
	}
}
