using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PathFinding_Algo
{
	public abstract bool FindPath (Node startNode, Node goalNode, PathFinding_Heuristics heursiticFunction, ref Dictionary<Node, Node_Data> nodeData);

	protected virtual int FindNodeWithMinWeight (ref List<Node> set, ref Dictionary<Node, Node_Data> nodeData)
	{
		float minWeightFound = nodeData[set[0]].weight;
		int nodeIndexWithMinWeight = 0;

		for (int i = 1; i < set.Count; ++i)
		{
			if (nodeData[set[i]].weight < minWeightFound)
			{
				minWeightFound = nodeData[set[i]].weight;
				nodeIndexWithMinWeight = i;
			}
		}

		return nodeIndexWithMinWeight;
	}
}
