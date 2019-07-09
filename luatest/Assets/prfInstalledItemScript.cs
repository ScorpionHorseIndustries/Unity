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
  public TMP_Text currentRecipeText;
  InstalledItem item;
  List<GameObject> kvpGo;
  bool ok = false;

  public void OnConditionValueChanged(string s) {
    workCondition = s;

  }

  public void OnConditionEditEnded(string s) {
    OnConditionValueChanged(s);
  }

  public void Set(InstalledItem item) {
    ok = false;
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

    currentRecipeText.text = "";
    if (item.recipe != null) {
      currentRecipeText.text = Funcs.PadPair(46,"current recipe", item.recipe.name);
    }

    foreach (string k in kvps.Keys) {
      CreateKVPControl(k, kvps[k]);
    }
  }

  private void CreateKVPControl(string k, string v) {
    GameObject go = SimplePool.Spawn(prfKVP, Vector3.one, Quaternion.identity);
    go.transform.SetParent(goContent.transform);
    go.transform.localScale = Vector3.one;
    prfInstalledItemKVPScript kvpcontrol = go.GetComponent<prfInstalledItemKVPScript>();
    kvpcontrol.Set(k, v);

    kvpGo.Add(go);
  }

  void Start() {
    tmpInputField.onValueChanged.AddListener(OnConditionValueChanged);
    tmpInputField.onEndEdit.AddListener(OnConditionEditEnded);
  }

  // Update is called once per frame
  void Update() {


  }

  public void AddBlankKVP() {
    CreateKVPControl("##new key", "##new value");
  }


  public void OkClicked() {
    ok = true;
    this.item.workCondition = workCondition;
    Deactivate();
  }

  public void CancelClicked() {
    Deactivate();

  }

  private void Deactivate() {

    foreach (GameObject kvp in kvpGo) {
      prfInstalledItemKVPScript kvps = kvp.GetComponent<prfInstalledItemKVPScript>();
      if (ok) {
        item.itemParameters.SetString(kvps.k, kvps.v);
      }
      SimplePool.Despawn(kvp);
    }



    gameObject.SetActive(false);
    WorldController.Instance.inputController.SetInputMode(INPUT_MODE.GAME);
    WorldController.Instance.gameState = GAME_STATE.PLAY;


  }
}
