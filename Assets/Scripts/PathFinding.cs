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

	#region Thread Control Variables
	public int maxNumberOfThreads = 5;

	// Counter to keep track of the number of active threads running
	private int _ActiveThreads = 0;

	private bool waitingToScanGrid = false;
	#endregion

	[Header ("Graph Properties")]
	[Tooltip ("Properties representing the graph our nodes exist in")]
	public Grid nodeGraph = new Grid();

	public enum PathFindingAlgoSelect { BFS, GBFS, A_STAR};

	[Header("Path-Finding Algorithms")]
	[Tooltip("The algorithm used to find a path")]
	public PathFindingAlgoSelect algoToUse = PathFindingAlgoSelect.BFS;

	// Dictionary used to call the selected path finding algorithm's FindPath function
	Dictionary<PathFindingAlgoSelect, PathFinding_Algo> algorithmReferences = new Dictionary<PathFindingAlgoSelect, PathFinding_Algo>();

	public PathFinding_Heuristics heuristic = new PathFinding_Heuristics();

	private Queue pathRequests = new Queue();

	// Debug variables
	public bool _DrawDebugData = false;

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

		nodeGraph.InitializeGrid();

		algorithmReferences[PathFindingAlgoSelect.BFS] = new Breadth_FirstSearch();
		algorithmReferences[PathFindingAlgoSelect.GBFS] = new Best_FirstSearch();
		algorithmReferences[PathFindingAlgoSelect.A_STAR] = new A_Star();
	}

	private void Update ()
	{
		if (!waitingToScanGrid)
		{
			nodeGraph.rescanTimer += Time.deltaTime;
			if (nodeGraph.rescanTimer > nodeGraph.rescanRate)
			{
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

		if (pathRequests.Count > 0)
		{
			// Spin up a path finding thread if it's safe to do so and we haven't hit our max thread count
			if (!waitingToScanGrid && _ActiveThreads < maxNumberOfThreads)
			{
				StartPathFindingThread((PathFinding_Request)pathRequests.Dequeue());
				//FindPath((PathFinding_Request)pathRequests.Dequeue());
			}
		}
	}

	public static void RequestPath (Transform start, Transform goal, PathFindingEntity caller)
	{
		Node startNode = _instance.nodeGraph.NodeFromWorldPoint(start.position);
		Node goalNode = _instance.nodeGraph.NodeFromWorldPoint(goal.position);
		PathFinding_Request newRequest = new PathFinding_Request(startNode, goalNode, caller);
		_instance.pathRequests.Enqueue(newRequest);
	}

	private void StartPathFindingThread (PathFinding_Request request)
	{
		Thread pathFindingThread = new Thread(() => FindPath(request));
		pathFindingThread.Priority = System.Threading.ThreadPriority.Lowest;
		pathFindingThread.IsBackground = true;
		pathFindingThread.Start();
		_ActiveThreads++;
	}

	private void FindPath(PathFinding_Request request)
	{
		bool foundPath = algorithmReferences[algoToUse].FindPath(request.start, request.goal, heuristic, ref request.nodeData, nodeGraph.MaxGridSize);

		PathFinding_Result result = new PathFinding_Result(foundPath);

		if (foundPath)
		{
			// If a path was found reverse it and return the request data
			result.path = ReversePath(request);
			result.path = SimplifyPath(result);
			request.caller.UpdatePathData(result);
		}
		else
		{
			// If a path was not found return a null path
			result.path = null;
			request.caller.UpdatePathData(result);
		}

		// Lock the PathFinding class instance and decrement the active threads count
		lock (_instance)
		{
			_ActiveThreads--;
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

	List<Node> SimplifyPath (PathFinding_Result pathFinding_Result)
	{
		List<Node> curPath = pathFinding_Result.path;
		List<Node> simplifiedPath = new List<Node>();

		Vector3 curNodePos;
		Vector3 nextNodePos;

		// Add the starting postiion node
		simplifiedPath.Add(curPath[0]);

		Vector2 previousMoveVec;

		// Get the starting movement direction of the starting node to the first node in the path
		curNodePos = curPath[0].position;
		nextNodePos = curPath[1].position;

		previousMoveVec = new Vector2(nextNodePos.x - curNodePos.x,
									  nextNodePos.z - curNodePos.z);

		// Normlize the vector because we only care about the direction
		previousMoveVec.Normalize();

		for (int i = 1; i < curPath.Count - 1; ++i)
		{
			curNodePos = curPath[i].position;
			nextNodePos = curPath[i + 1].position;

			Vector2 dir = new Vector2(nextNodePos.x - curNodePos.x,
									  nextNodePos.z - curNodePos.z);

			dir.Normalize();

			// If there is a turn in the path add the node to the simplified path, else ignore it
			if (dir != previousMoveVec)
			{
				previousMoveVec = dir;
				simplifiedPath.Add(curPath[i]);
			}
		}

		// Add the last node of the path since it is the goal
		simplifiedPath.Add(curPath[curPath.Count - 1]);

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
	public PathFindingEntity caller;
	public Node start;
	public Node goal;
	public Dictionary<Node, Node_Data> nodeData;

	public PathFinding_Request (Node _start, Node _goal, PathFindingEntity _caller)
	{
		caller = _caller;
		start = _start;
		goal = _goal;
		nodeData = new Dictionary<Node, Node_Data>();
	}
}

public struct PathFinding_Result
{
	public bool pathFound;
	public List<Node> path;

	public PathFinding_Result (bool _pathFound)
	{
		pathFound = _pathFound;
		path = null;
	}
}
