using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breadth_FirstSearch
{
	public bool FindPath (Node startNode, Node goalNode, PathFinding_Heuristics heursiticFunction)
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
				Node parent = node.parentNode;

				if (parent == null && node != startNode)
				{
					node.parentNode = curNode;
					nodeQueue.Enqueue(node);
				}
			}
		}

		return pathFound;
	}
}
