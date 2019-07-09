using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class prfInstalledItemKVPScript : MonoBehaviour {
  public TMP_InputField inputKey;
  public TMP_InputField inputValue;
  public string k;
  public string v;

  public void Set(string k, string v) {
    this.k = k;
    this.v = v;

    inputKey.text = k;
    inputValue.text = v;
  }

  // Start is called before the first frame update
  void Start() {

    inputValue.onValueChanged.AddListener(OnValueChanged);
    inputKey.onValueChanged.AddListener(OnKeyValueChanged);

    inputValue.onEndEdit.AddListener(OnEndEdit);
    inputKey.onEndEdit.AddListener(OnKeyEndEdit);
    


  }


  private void OnKeyEndEdit(string s) {
    k = s;
  }

  private void OnKeyValueChanged(string s) {
    k = s;

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
