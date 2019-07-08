using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class prfInstalledItemKVPScript : MonoBehaviour {
  public TMP_Text propertyName;
  public TMP_InputField input;
  public string k;
  public string v;

  public void Set(string k, string v) {
    this.k = k;
    this.v = v;

    propertyName.text = k;
    input.text = v;
  }

  // Start is called before the first frame update
  void Start() {

    input.onValueChanged.AddListener(OnValueChanged);
    input.onEndEdit.AddListener(OnEndEdit);
    
  }

  private void OnEndEdit(string s) {
    v = s;
  }

  private void OnValueChanged(string s) {
    v = s;
    
  }

  // Update is called once per frame
  void Update() {

  }
}
