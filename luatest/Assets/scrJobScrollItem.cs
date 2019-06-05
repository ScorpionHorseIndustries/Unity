using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class scrJobScrollItem : MonoBehaviour {

  public Button btnCancel;
  public Text nameText;
  public Text typeText;
  Job job;
  // Start is called before the first frame update
  void Start() {

  }
  public void Setup(Job job) {
    this.job = job;
    nameText.text = job.jobType.ToString();
    switch (job.jobType) {
      case JOB_TYPE.NONE:
        break;
      case JOB_TYPE.BUILD:
        typeText.text = job.recipe.name + " (" + job.tile.world_x + "," + job.tile.world_y + ")";
        break;
      case JOB_TYPE.HAUL:
        typeText.text = job.recipeResouceQty + " of " + job.recipeResourceName + " to (" + job.tile.world_x + "," + job.tile.world_y + ")";
        break;
      default:
        break;
    }
  

  }
}
