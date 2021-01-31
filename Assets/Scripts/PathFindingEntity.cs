using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFindingEntity : MonoBehaviour
{
	[HideInInspector] public Transform startPosition;
	public Transform goalPosition;

	[HideInInspector] public List<Node> path = new List<Node>();
	[HideInInspector] public bool pathFound = false;

	[HideInInspector] public bool waitingForPath = false;
	public float updateDelay = 0.5f;
	private float updateDelayTimer = 0.0f;

	private void Start ()
	{
		startPosition = GetComponent<Transform>();
	}

	private void Update ()
	{
		if (!waitingForPath)
		{
			updateDelayTimer += Time.deltaTime;

			if (updateDelayTimer > updateDelay)
			{
				waitingForPath = true; 
				PathFinding.RequestPath(startPosition, goalPosition, UpdatePathData);
				updateDelayTimer = 0.0f;
			}
		}
	}

	public void UpdatePathData (List<Node> _path, bool _pathFound)
	{
		path = _path;
		pathFound = _pathFound;
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
				Gizmos.DrawSphere(curNode.position, 0.25f);
			}
		}
	}
}
