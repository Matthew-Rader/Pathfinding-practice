using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingEntity : MonoBehaviour
{
	PathFinding _PathFinder;

	public Transform startPosition;
	public Transform goalPosition;

	[HideInInspector] public Dictionary<Node, Node_Data> nodeData = new Dictionary<Node, Node_Data>();
	[HideInInspector] public List<Node> path = new List<Node>();

	public float updateDelay = 0.5f;
	private float updateDelayTimer = 0.0f;

	private void Start ()
	{
		_PathFinder = PathFinding.Instance;
	}

	private void Update ()
	{
		if (Time.time > updateDelayTimer)
		{
			nodeData.Clear();

			_PathFinder.FindPath(startPosition, goalPosition, ref nodeData);

			ReversePath();

			updateDelayTimer = Time.time + updateDelay;
		}
	}

	void ReversePath ()
	{
		path.Clear();

		Node goalNode = _PathFinder.nodeGraph.NodeFromWorldPoint(goalPosition.position);
		Node startNode = _PathFinder.nodeGraph.NodeFromWorldPoint(startPosition.position);

		Node curNode = goalNode;

		while (curNode != startNode)
		{
			path.Add(curNode);
			curNode = nodeData[curNode].parentNode;
		}

		path.Add(startNode);
	}

	private void OnDrawGizmos ()
	{
		for( int i = 0; i < path.Count-1; ++i)
		{
			Node curNode = path[i];
			Gizmos.DrawLine(curNode.position, path[i + 1].position);
		}
	}
}
