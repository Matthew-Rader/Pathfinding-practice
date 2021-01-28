using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class A_Star : PathFinding_Algo
{
	override public bool FindPath (Node startNode, Node goalNode, PathFinding_Heuristics heursiticFunction, ref Dictionary<Node, Node_Data> nodeData, int maxGridSize)
	{
		BinaryHeap<Node_Data> openSet = new BinaryHeap<Node_Data>(maxGridSize);

		if (nodeData.ContainsKey(startNode))
			nodeData[startNode].Reset();
		else
			nodeData[startNode] = new Node_Data(startNode);

		foreach (var nd in nodeData.Values)
			nd.Reset();

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
					if (!nodeData[node].inOpenSet)
					{
						nodeData[node].parentNode = curNode;
						nodeData[node].hCost = heursiticFunction.GetHCost(node, goalNode);
						nodeData[node].gCost = nodeData[curNode].gCost + heursiticFunction.GetHCost(curNode, node);
						nodeData[node].inOpenSet = true;
						openSet.Add(nodeData[node]);
					}
					else
					{
						float newG = nodeData[curNode].gCost + heursiticFunction.GetHCost(curNode, node);
						if (newG < nodeData[node].gCost)
						{
							// Current Node should adopt this node
							nodeData[node].parentNode = curNode;
							nodeData[node].gCost = newG;
						}
					}
				}
			}

			if (openSet.Count == 0)
				break;

		} while (curNode != goalNode);

		return (curNode == goalNode) ? true : false;
	}
}