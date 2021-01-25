using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
	public enum PathFindingAlgo { BFS, GBFS, A_STAR};
	public PathFindingAlgo algoToUse = PathFindingAlgo.BFS;

	public PathFinding_Heuristics heuristic = new PathFinding_Heuristics();

	public Transform startPosition;
	public Transform goalPosition;
	public Grid nodeGrid;

	[HideInInspector]public List<Node> path = new List<Node>();

	// Search Algorithms
	Breadth_FirstSearch bfs = new Breadth_FirstSearch();
	Best_FirstSearch gbfs = new Best_FirstSearch();

	public float updateDelay = 0.5f;
	bool updatePath = true;

	private void Start ()
	{
		heuristic.UpdateHeuristic();
	}

	private void Update ()
	{
		if (updatePath)
		{
			Node startNode = nodeGrid.NodeFromWorldPoint(startPosition.position);
			Node goalNode = nodeGrid.NodeFromWorldPoint(goalPosition.position);
			nodeGrid.ResetNodes();

			switch (algoToUse)
			{
				case (PathFindingAlgo.BFS):
					bfs.FindPath(startNode, goalNode, heuristic);
					break;

				case (PathFindingAlgo.GBFS):
					gbfs.FindPath(startNode, goalNode, heuristic);
					break;
			}

			ReversePath();

			StartCoroutine(UpdateDelay());
		}
	}

	void ReversePath ()
	{
		path.Clear();

		Node goalNode = nodeGrid.NodeFromWorldPoint(goalPosition.position);
		Node startNode = nodeGrid.NodeFromWorldPoint(startPosition.position);

		Node curNode = goalNode;

		while (curNode != startNode)
		{
			path.Add(curNode);
			curNode = curNode.parentNode;
		}

		path.Add(startNode);
	}

	IEnumerator UpdateDelay ()
	{
		updatePath = false;
		yield return new WaitForSeconds(updateDelay);
		updatePath = true;
	}
}
