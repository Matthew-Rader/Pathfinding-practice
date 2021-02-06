using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// EntityMovement requires a Rigidbody component to function
[RequireComponent(typeof(Rigidbody))]

public class EntityMovement : MonoBehaviour
{
	private Rigidbody entityRigi;

	private Vector3 velocitySmoothing;

	private bool entityMovingLastFrame = false;

	private Vector3 targetMovementDir;

	private bool canMove;

	[SerializeField] private float movementSpeed;
	public float MovementSpeed
	{
		get { return movementSpeed; }
		set { movementSpeed = value; }
	}

	[SerializeField] private float accelerationTime;
	public float AccelerationTime
	{
		get { return accelerationTime; }
		set { accelerationTime = value; }
	}

	[SerializeField] private float decelerationTime;
	public float DecelerationTime
	{
		get { return decelerationTime; }
		set { decelerationTime = value; }
	}

	#region UNITY EVENTS
	[System.Serializable]
		public struct MovementEvents
		{
			public UnityEvent OnStartWalking;
			public UnityEvent OnStopWalking;
		}

		public MovementEvents events;
	#endregion

    private void Start() 
	{
        entityRigi = GetComponent<Rigidbody>();
    }

	private void Update ()
	{
		if (canMove)
			MoveEntity();
	}

	/// <summary>
	/// Update the target direction the entity should move
	/// </summary>
	/// <param name="targetDirection"> Target movement direction </param>
	public void UpdateTargetMovementDirection (Vector3 targetDirection)
	{
		targetMovementDir = targetDirection;

		if (targetMovementDir.magnitude != 1.0f)
			targetMovementDir.Normalize();

		transform.LookAt(targetMovementDir);
	}

	/// <summary>
	/// Disable entity movement
	/// </summary>
	public void StopMoving ()
	{
		CanMove(false);
		entityRigi.velocity = Vector3.zero;
	}

	/// <summary>
	/// Set whether the entity can move
	/// </summary>
	/// <param name="_canMove"></param>
	public void CanMove (bool _canMove)
	{
		canMove = _canMove;
	}

	private void MoveEntity () 
	{
		if (entityRigi != null)
        {
			Vector3 currentEntityVelocity = entityRigi.velocity;
			Vector3 newEntityVelocity;

			Vector3 targetVelocity = targetMovementDir * movementSpeed;

			// Determine if we are accelerating or decelerating
			if (targetMovementDir != Vector3.zero) // ACCELERATION
			{
				if (!entityMovingLastFrame)
				{
					events.OnStartWalking.Invoke();
					entityMovingLastFrame = true;
				}

				newEntityVelocity = Vector3.SmoothDamp(currentEntityVelocity, targetVelocity, ref velocitySmoothing, accelerationTime);
			}
			else // DECELERATION
			{
				if (entityMovingLastFrame)
				{
					events.OnStopWalking.Invoke();
					entityMovingLastFrame = false;
				}

				newEntityVelocity = Vector3.SmoothDamp(currentEntityVelocity, targetVelocity, ref velocitySmoothing, decelerationTime);
			}

			entityRigi.velocity = newEntityVelocity;
		} 
	}
}
