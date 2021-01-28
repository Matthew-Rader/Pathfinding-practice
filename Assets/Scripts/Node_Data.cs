using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node_Data : IHeapItem<Node_Data>
{
	public Node node;
	public Node parentNode;
	public float weight;
	public bool inClosedSet;
	public bool inOpenSet;
	public int heapIndex;

	public float hCost;
	public float gCost;
	public float fCost
	{
		get
		{
			return gCost + hCost;
		}
	}

	public Node_Data (Node associatedNode)
	{
		node = associatedNode;
		parentNode = null;
		inClosedSet = false;
		inOpenSet = false;
	}

	public void Reset ()
	{
		parentNode = null;
		inClosedSet = false;
		inOpenSet = false;
	}

	public int HeapIndex
	{
		get
		{
			return heapIndex;
		}
		set
		{
			heapIndex = value;
		}
	}

	public int CompareTo (Node_Data nodeToCompare)
	{

		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0)
		{
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}

		return -compare;
	}
}
