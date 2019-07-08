using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NoYouDoIt.DataModels;
using NoYouDoIt.TheWorld;
using NoYouDoIt.Utils;
using NoYouDoIt.Controller;
public class prfInstalledItemScript : MonoBehaviour {
  public TextMeshProUGUI txtItemName;
  public TMP_InputField tmpInputField;
  public Toggle tglActive;
  public GameObject goContent;
  public GameObject prfKVP;
  string itemName;
  string workCondition;
  InstalledItem item;
  List<GameObject> kvpGo;

  public void OnConditionValueChanged(string s) {
    workCondition = s;

  }

  public void OnConditionEditEnded(string s) {
    OnConditionValueChanged(s);
  }

  public void Set(InstalledItem item) {
    kvpGo = new List<GameObject>();
    this.item = item;
    this.itemName = item.niceName;
    this.workCondition = item.workCondition == null ? "" : item.workCondition;
    txtItemName.text = item.type;
    tmpInputField.text = this.workCondition;
    gameObject.SetActive(true);
    WorldController.Instance.inputController.SetInputMode(INPUT_MODE.SHOWING_DIALOGUE);
    WorldController.Instance.gameState = GAME_STATE.PAUSE;
    Dictionary<string, string> kvps = item.itemParameters.GetItems();

    foreach(string k in kvps.Keys) {
      GameObject go = SimplePool.Spawn(prfKVP, Vector3.one, Quaternion.identity);
      go.transform.SetParent(goContent.transform);
      go.transform.localScale = Vector3.one;
      kvpGo.Add(go);
    }
  }

  void Start() {
    tmpInputField.onValueChanged.AddListener(OnConditionValueChanged);
    tmpInputField.onEndEdit.AddListener(OnConditionEditEnded);
  }

  // Update is called once per frame
  void Update() {


  }

  public void OkClicked() {
    this.item.workCondition = workCondition;
    Deactivate();
  }

  public void CancelClicked() {
    Deactivate();

  }

  private void Deactivate() {
    gameObject.SetActive(false);
    WorldController.Instance.inputController.SetInputMode(INPUT_MODE.GAME);
    WorldController.Instance.gameState = GAME_STATE.PLAY;

    foreach (GameObject kvp in kvpGo) {
      SimplePool.Despawn(kvp);
    }
  }
}
