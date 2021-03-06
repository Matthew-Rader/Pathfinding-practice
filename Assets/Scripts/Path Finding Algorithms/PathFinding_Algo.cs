﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PathFinding_Algo
{
	public abstract bool FindPath (Node startNode, Node goalNode, PathFinding_Heuristics heursiticFunction, ref Dictionary<Node, Node_Data> nodeData, int maxGridSize);

	protected virtual int FindNodeWithMinWeight (ref List<Node> set, ref Dictionary<Node, Node_Data> nodeData)
	{
		float minWeightFound = nodeData[set[0]].fCost;
		int nodeIndexWithMinWeight = 0;

		for (int i = 1; i < set.Count; ++i)
		{
			if (nodeData[set[i]].fCost < minWeightFound)
			{
				minWeightFound = nodeData[set[i]].fCost;
				nodeIndexWithMinWeight = i;
			}
		}

		return nodeIndexWithMinWeight;
	}
}
