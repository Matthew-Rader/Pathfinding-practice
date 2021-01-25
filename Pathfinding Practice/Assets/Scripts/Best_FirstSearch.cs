using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Best_FirstSearch
{
	public bool FindPath (Node startNode, Node goalNode, PathFinding_Heuristics heursiticFunction)
	{
		List<Node> openSet = new List<Node>();

		Node curNode = startNode;
		curNode.inClosedSet = true;

		do
		{
			foreach (Node node in curNode.adjacentNodes)
			{
				if (!node.inClosedSet)
				{
					node.parentNode = curNode;

					if (!node.inOpenSet)
					{
						node.inOpenSet = true;
						node.weight = heursiticFunction.GetHeuristicValue(node, goalNode);

						openSet.Add(node);
					}
				}
			}

			if (openSet.Count == 0)
				break;

			int minNodeIndx = FindNodeWithMinWeight(openSet);
			curNode = openSet[minNodeIndx];
			openSet.RemoveAt(minNodeIndx);

			curNode.inClosedSet = true;
			curNode.inOpenSet = false;

		} while (curNode != goalNode);

		return (curNode == goalNode) ? true : false;
	}

	private int FindNodeWithMinWeight (List<Node> set)
	{
		float minWeightFound = set[0].weight;
		int nodeIndexWithMinWeight = 0;

		for (int i = 1; i < set.Count; ++i)
		{
			if (set[i].weight < minWeightFound)
			{
				minWeightFound = set[i].weight;
				nodeIndexWithMinWeight = i;
			}
		}

		return nodeIndexWithMinWeight;
	}
}
