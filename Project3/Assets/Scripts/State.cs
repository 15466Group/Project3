using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class State {

	public List<Node> open { get; set; }
	public List<Node> closed { get; set; }
	public Dictionary<Node, Node> dictPath { get; set; }
	public Node startNode { get; set; }
	public Node endNode { get; set; }
	public float swampCost { get; set; }
	public Grid sGrid { get; set; }
	public List<Node> path { get; set; }
	public bool ongoing { get; set; }
	public bool hasFullPath { get; set; }

	public State (List<Node> o, List<Node> c, Dictionary<Node, Node> d, 
	              Node s, Node e, float sw, Grid sg, List<Node> p,
	              bool og, bool hFP){
		open = o;
		closed = c;
		dictPath = d;
		startNode = s;
		endNode = e;
		swampCost = sw;
		sGrid = sg;
		path = p;
		ongoing = og;
		hasFullPath = hFP;
	}
}
