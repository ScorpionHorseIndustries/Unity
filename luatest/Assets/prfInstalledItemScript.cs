using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NoYouDoIt.DataModels;
using NoYouDoIt.TheWorld;
public class prfInstalledItemScript : MonoBehaviour {
  public TextMeshProUGUI txtItemName;
  public TextMeshProUGUI txtEntryField;
  public Toggle tglActive;
  string itemName;
  string workCondition;
  InstalledItem item;

  void Set(InstalledItem item) {
    this.item = item;
    this.itemName = item.type;
    this.workCondition = item.workCondition == null ? "" : item.workCondition;
    txtItemName.text = item.type;
    txtEntryField.text = this.workCondition;

  }

  void Start() {

  }

  // Update is called once per frame
  void Update() {


  }

  public void OkClicked() {

  }

  public void CancelClicked() {

  }
}
