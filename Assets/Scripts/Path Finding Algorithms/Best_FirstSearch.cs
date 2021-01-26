using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Best_FirstSearch : PathFinding_Algo
{
	override public bool FindPath (Node startNode, Node goalNode, PathFinding_Heuristics heursiticFunction, ref Dictionary<Node, Node_Data> nodeData, int maxGridSize)
	{
		BinaryHeap<Node_Data> openSet = new BinaryHeap<Node_Data>(maxGridSize);

		nodeData[startNode] = new Node_Data(startNode);
		openSet.Add(nodeData[startNode]);

		Node curNode;

		do
		{
			curNode = openSet.RemoveFirst().node;

			nodeData[curNode].inClosedSet = true;
			nodeData[curNode].inOpenSet = false;

			foreach (Node node in curNode.adjacentNodes)
			{
				if (!nodeData.ContainsKey(node))
					nodeData[node] = new Node_Data(node);

				if (!nodeData[node].inClosedSet)
				{
					nodeData[node].parentNode = curNode;

					if (!nodeData[node].inOpenSet)
					{
						nodeData[node].inOpenSet = true;
						nodeData[node].hCost = heursiticFunction.GetHCost(node, goalNode);

						openSet.Add(nodeData[node]);
					}
				}
			}

			if (openSet.Count == 0)
				break;

		} while (curNode != goalNode);

		return (curNode == goalNode) ? true : false;
	}
}
