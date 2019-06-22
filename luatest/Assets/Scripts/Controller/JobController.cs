using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace NoYouDoIt.Controller {
  public class JobController : MonoBehaviour {


    private void Awake() {

    }
    // Start is called before the first frame update
    //void Init(string s) {
    //  Debug.Log("init " + this.name);

    //}

    public void Init() {

    }
    void Start() {
      Debug.Log("init done" + this.name);
      //WorldController.Instance.cbRegisterReady(Init);

    }

    // Update is called once per frame
    void Update() {

    }
  }
}
