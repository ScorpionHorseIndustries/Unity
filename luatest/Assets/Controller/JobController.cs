using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobController : MonoBehaviour {

  public static JobController Instance { get; private set; }
  private void Awake() {
    if (Instance != null) {
      Debug.LogError("THERE SHOULD ONLY BE ONE SPRITE CONTROLLER YOU DING DONG");
    }
    Instance = this;
  }
  // Start is called before the first frame update
  //void Init(string s) {
  //  Debug.Log("init " + this.name);

  //}
  void Start() {
    Debug.Log("init done" + this.name);
    //WorldController.Instance.cbRegisterReady(Init);

  }

  // Update is called once per frame
  void Update() {

  }
}
