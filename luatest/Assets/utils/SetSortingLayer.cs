using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSortingLayer : MonoBehaviour {

  public string sortingLayerName = "default";
  // Start is called before the first frame update
  void Start() {
    GetComponent<Renderer>().sortingLayerName = sortingLayerName;
  }


}
