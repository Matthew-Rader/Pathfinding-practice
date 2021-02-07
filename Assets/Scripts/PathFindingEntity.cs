using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// A EntityMovement component is needed to make the objects move
[RequireComponent(typeof(EntityMovement))]

public class PathFindingEntity : MonoBehaviour
{
	// Start position when searching for a path
	[HideInInspector] public Transform startPosition;

	// Goal position when searching for a path
	public Transform goalPosition;

	// Last known Goal position to check if we need to research for the path
	private Vector3 goalPositionLastUpdate;

	// Found path
	[HideInInspector] public List<Node> path = new List<Node>();
	[HideInInspector] public bool pathFound = false;

	// PathFinding delay length and timers
	public float updateDelay = 0.5f;
	private float updateDelayTimer = 0.0f;

	[HideInInspector] public bool _WaitingForPath = false;

	// EntityMovement is in charge of moveing the entity, we simply pass it a direction to a waypoint node
	private EntityMovement entityMovement;

	// Coroutine variable so we can stop and start following a path
	private Coroutine followPathCoroutine;

	private void Start ()
	{
		startPosition = GetComponent<Transform>();
		entityMovement = GetComponent<EntityMovement>();

		goalPositionLastUpdate = goalPosition.position;

		// Find our initial path
		_WaitingForPath = true;
		PathFinding.RequestPath(startPosition, goalPosition, UpdatePathData);
	}

	private void Update ()
	{
		if (!_WaitingForPath)
		{
			updateDelayTimer += Time.deltaTime;

			if (updateDelayTimer > updateDelay)
			{
				if (goalPositionLastUpdate != goalPosition.position)
				{
					_WaitingForPath = true;
					goalPositionLastUpdate = goalPosition.position;
					PathFinding.RequestPath(startPosition, goalPosition, UpdatePathData);
				}

				updateDelayTimer = 0.0f;
			}
		}
	}

	public void UpdatePathData (List<Node> _path, bool _pathFound)
	{
		path = _path;
		pathFound = _pathFound;
		_WaitingForPath = false;

		// Stop following the revious path
		if (followPathCoroutine != null)
		{
			StopCoroutine(followPathCoroutine);
			followPathCoroutine = null;
		}

		if (pathFound)
		{
			if (path.Count > 1)
			{
				// Start following the new path
				followPathCoroutine = StartCoroutine(FollowPath());
			}
		}
		else
		{
			entityMovement.StopMoving();
		}
	}

	IEnumerator FollowPath ()
	{
		Vector3 movementDir;
		int curWaypointNodeIndex = 1;
		Node curWaypointNode = path[curWaypointNodeIndex];

		// Make sure we let the entityMovement component know that it's good to move
		entityMovement.CanMove(true);

		// Since we are on a grid we can zero out the y position for nodes and entities
		Vector3 curEntityPosition = new Vector3(transform.position.x, 0.0f, transform.position.z);
		Vector3 curWaypointNodePosition = new Vector3(curWaypointNode.position.x, 0.0f, curWaypointNode.position.z);

		// Move towards the starting node
		movementDir = (curWaypointNodePosition - curEntityPosition).normalized;
		entityMovement.UpdateTargetMovementDirection(movementDir);

		while (true)
		{
			curEntityPosition = new Vector3(transform.position.x, 0.0f, transform.position.z);

			// If our current position is near the current node we're moving towards ...
			if ((curWaypointNodePosition - curEntityPosition).magnitude <= 0.1f)
			{
				// ... iterate the waypoint node index ...
				curWaypointNodeIndex++;

				// ... check if we have reached the goal node ...
				if (curWaypointNodeIndex >= path.Count)
				{
					entityMovement.StopMoving();
					yield break;
				}

				// ... update curWaypointNode to the new waypoint node ...
				curWaypointNode = path[curWaypointNodeIndex];

				// .. update our local position vector ...
				curWaypointNodePosition.x = curWaypointNode.position.x;
				curWaypointNodePosition.z = curWaypointNode.position.z;
			}

			// ... update out movement direction.
			movementDir = (curWaypointNodePosition - curEntityPosition).normalized;
			entityMovement.UpdateTargetMovementDirection(movementDir);

			Debug.DrawRay(transform.position, movementDir);

			yield return null;
		}
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
