using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class scrUIBuildMenu : MonoBehaviour {
  // Start is called before the first frame update
  public Button btnBuildPrefab;

    
  
  void Start() {

    //add a button for building each type of installed item
    foreach (InstalledItem proto in 
      InstalledItem.prototypes.Values.
        Where(e => e.build).
        OrderBy(e => e.niceName)) {
      Button btn = Instantiate(btnBuildPrefab, Vector2.zero, Quaternion.identity);
      btn.transform.SetParent(this.transform);
      
      btn.GetComponentInChildren<Text>().text = proto.niceName;
      btn.name = "btnBuild::" + proto.type;
      btn.onClick.AddListener(delegate { WorldController.Instance.SetBuildType_InstalledItem(proto.type); });
      btn.transform.localScale = Vector2.one;
       
    }

  }


}
