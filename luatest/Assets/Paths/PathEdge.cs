using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathEdge<T> {

  public T type;
  public PathNode<T> node;

  public float cost { get; private set; } = 0; //cost to traverse edge (i.e. cost to enter tile)

  public PathEdge(T type, PathNode<T> n, float cost) {
    this.node = n;
    this.cost = cost;
    this.type = type;
  }

  public override string ToString() {
    return "edge (" + cost + ") heading to: " + node; 
  }
}
