using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Best_FirstSearch : PathFinding_Algo
{
	override public bool FindPath (Node startNode, Node goalNode, PathFinding_Heuristics heursiticFunction, ref Dictionary<Node, Node_Data> nodeData)
	{
		List<Node> openSet = new List<Node>();

		Node curNode = startNode;
		nodeData[curNode] = new Node_Data();
		nodeData[curNode].inClosedSet = true;

		do
		{
			foreach (Node node in curNode.adjacentNodes)
			{
				if (!nodeData.ContainsKey(node))
					nodeData[node] = new Node_Data();

				if (!nodeData[node].inClosedSet)
				{
					nodeData[node].parentNode = curNode;

					if (!nodeData[node].inOpenSet)
					{
						nodeData[node].inOpenSet = true;
						nodeData[node].weight = heursiticFunction.GetHeuristicValue(node, goalNode);

						openSet.Add(node);
					}
				}
			}

			if (openSet.Count == 0)
				break;

			int minNodeIndx = base.FindNodeWithMinWeight(ref openSet, ref nodeData);
			curNode = openSet[minNodeIndx];
			openSet.RemoveAt(minNodeIndx);

			nodeData[curNode].inClosedSet = true;
			nodeData[curNode].inOpenSet = false;

		} while (curNode != goalNode);

		return (curNode == goalNode) ? true : false;
	}
}
