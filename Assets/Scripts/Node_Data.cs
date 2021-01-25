using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node_Data
{
	public Node parentNode = null;
	public float weight;
	public bool inClosedSet = false;
	public bool inOpenSet = false;
}
