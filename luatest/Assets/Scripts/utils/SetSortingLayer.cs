using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSortingLayer : MonoBehaviour {

  public string sortingLayerName = "default";
  public float lifeTime = -1;
  // Start is called before the first frame update
  void Start() {
    GetComponent<Renderer>().sortingLayerName = sortingLayerName;
  }


}
