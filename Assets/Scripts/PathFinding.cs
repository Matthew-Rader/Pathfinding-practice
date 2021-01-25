using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
	public enum PathFindingAlgoSelect { BFS, GBFS, A_STAR};
	public PathFindingAlgoSelect algoToUse = PathFindingAlgoSelect.BFS;

	// Search Algorithms
	Dictionary<PathFindingAlgoSelect, PathFinding_Algo> algorithmReferences = new Dictionary<PathFindingAlgoSelect, PathFinding_Algo>();

	public PathFinding_Heuristics heuristic = new PathFinding_Heuristics();

	public Transform startPosition;
	public Transform goalPosition;
	public Grid nodeGrid;

	[HideInInspector] public Dictionary<Node, Node_Data> nodeData = new Dictionary<Node, Node_Data>();
	[HideInInspector] public List<Node> path = new List<Node>();

	public float updateDelay = 0.5f;
	bool updatePath = true;

	private void Start ()
	{
		heuristic.UpdateHeuristic();

		algorithmReferences[PathFindingAlgoSelect.BFS] = new Breadth_FirstSearch();
		algorithmReferences[PathFindingAlgoSelect.GBFS] = new Best_FirstSearch();
	}

	private void Update ()
	{
		if (updatePath)
		{
			Node startNode = nodeGrid.NodeFromWorldPoint(startPosition.position);
			Node goalNode = nodeGrid.NodeFromWorldPoint(goalPosition.position);
			nodeData.Clear();

			algorithmReferences[algoToUse].FindPath(startNode, goalNode, heuristic, ref nodeData);

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
			curNode = nodeData[curNode].parentNode;
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
