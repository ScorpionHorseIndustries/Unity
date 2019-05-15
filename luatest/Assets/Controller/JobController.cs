using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobController : MonoBehaviour {

  
  // Start is called before the first frame update
  void Init(string s) {
    Debug.Log("init " + this.name);

  }
  void Start() {
    WorldController.Instance.cbRegisterReady(Init);

  }

  // Update is called once per frame
  void Update() {

  }
}
