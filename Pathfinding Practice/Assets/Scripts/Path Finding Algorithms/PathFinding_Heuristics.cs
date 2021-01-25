using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathFinding_Heuristics
{
	public enum Heuristic { MANHATTAN, EUCLIDEAN }
	public Heuristic heursitic = Heuristic.MANHATTAN;

	public delegate float HeursiticFunction (Node position, Node goal);
	public HeursiticFunction GetHeuristicValue;

	public void UpdateHeuristic ()
	{
		switch (heursitic)
		{
			case Heuristic.MANHATTAN:
				GetHeuristicValue = FindManhattanDistance;
				break;
		}
	}

	float FindManhattanDistance (Node start, Node goal)
	{
		return Mathf.Abs(start.position.x - goal.position.x) + Mathf.Abs(start.position.z - goal.position.z);
	}
}
