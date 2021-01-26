using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathFinding_Heuristics
{
	public enum Heuristic { MANHATTAN, EUCLIDEAN, EUCLIDEANSQRT }

	[Header("Heuristic Functions")]
	[Tooltip("The heuristic function the path-finding algorithm will use to calculate a node's h-cost")]
	public Heuristic heuristic = Heuristic.MANHATTAN;

	Dictionary<Heuristic, HeursiticFunction> heuristicFunctions = new Dictionary<Heuristic, HeursiticFunction>();

	private delegate float HeursiticFunction (Node position, Node goal);
	private HeursiticFunction GetManhattanDistance;
	private HeursiticFunction GetEuclideanDistance;
	private HeursiticFunction GetEuclideanDistanceSqrt;

	public PathFinding_Heuristics ()
	{
		GetManhattanDistance = FindManhattanDistance;
		GetEuclideanDistance = FindEuclideanDistance;
		GetEuclideanDistanceSqrt = FindEuclideanDistanceSqrt;

		heuristicFunctions.Add(Heuristic.MANHATTAN, GetManhattanDistance);
		heuristicFunctions.Add(Heuristic.EUCLIDEAN, GetEuclideanDistance);
		heuristicFunctions.Add(Heuristic.EUCLIDEANSQRT, GetEuclideanDistanceSqrt);
	}

	public float GetHCost (Node start, Node goal)
	{
		return heuristicFunctions[heuristic](start, goal);
	}

	float FindManhattanDistance (Node start, Node goal)
	{
		return Mathf.Abs(start.position.x - goal.position.x) + Mathf.Abs(start.position.z - goal.position.z);
	}

	float FindEuclideanDistance (Node start, Node goal)
	{
		float x = Mathf.Pow(start.position.x - goal.position.x, 2.0f);
		float y = Mathf.Pow(start.position.z - goal.position.z, 2.0f);

		return x + y;
	}

	float FindEuclideanDistanceSqrt (Node start, Node goal)
	{
		float x = Mathf.Pow(start.position.x - goal.position.x, 2.0f);
		float y = Mathf.Pow(start.position.z - goal.position.z, 2.0f);

		return Mathf.Sqrt(x + y);
	}
}
