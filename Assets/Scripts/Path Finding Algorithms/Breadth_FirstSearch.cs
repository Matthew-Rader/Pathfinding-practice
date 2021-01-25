using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breadth_FirstSearch : PathFinding_Algo
{
	override public bool FindPath (Node startNode, Node goalNode, PathFinding_Heuristics heursiticFunction, ref Dictionary<Node, Node_Data> nodeData, int maxGridSize)
	{
		bool pathFound = false;

		Queue nodeQueue = new Queue();

		nodeQueue.Enqueue(startNode);

		while (nodeQueue.Count > 0)
		{
			Node curNode = (Node)nodeQueue.Dequeue();

			if (curNode == goalNode)
			{
				pathFound = true;
				break;
			}

			foreach (Node node in curNode.adjacentNodes)
			{
				if (!nodeData.ContainsKey(node))
					nodeData[node] = new Node_Data(node);

				Node parent = nodeData[node].parentNode;

				if (parent == null && node != startNode)
				{
					nodeData[node].parentNode = curNode;
					nodeQueue.Enqueue(node);
				}
			}
		}

		return pathFound;
	}
}
