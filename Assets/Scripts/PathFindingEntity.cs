using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingEntity : MonoBehaviour
{
	PathFinding _PathFinder;

	public Transform startPosition;
	public Transform goalPosition;

	[HideInInspector] public List<Node> path = new List<Node>();
	public bool waitingForPath = false;
	public bool pathFound = false;

	public float updateDelay = 0.5f;
	private float updateDelayTimer = 0.0f;

	private void Start ()
	{
		_PathFinder = PathFinding.Instance;
	}

	private void Update ()
	{
		if (!waitingForPath)
		{
			updateDelayTimer += Time.deltaTime;

			if (updateDelayTimer > updateDelay)
			{
				_PathFinder.RequestPath(startPosition, goalPosition, this);
				waitingForPath = true;
				updateDelayTimer = 0.0f;
			}
		}
	}

	public void UpdatePathData (PathFinding_Request request)
	{
		path = request.path;
		pathFound = request.pathFound;
		waitingForPath = false;
	}

	private void OnDrawGizmos ()
	{
		if (path != null)
		{
			for (int i = 0; i < path.Count - 1; ++i)
			{
				Node curNode = path[i];
				Gizmos.DrawLine(curNode.position, path[i + 1].position);
			}
		}
	}
}
