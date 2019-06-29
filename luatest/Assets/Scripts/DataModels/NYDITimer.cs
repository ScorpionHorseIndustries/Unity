using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoYouDoIt.DataModels {
  public class NYDITimer {
    float time;
    float initialTime;
    string name;
    Action func;
    bool random = false;
    float randomTimeFrom, randomTimeTo;

    public NYDITimer(string name, float timeFrom, float timeTo, Action func) {
      this.func += func;
      this.name = name;
      this.randomTimeFrom = timeFrom;
      this.randomTimeTo = timeTo;


      
      
      this.random = true;

      

      this.time = initialTime;
    }

    private float GetResetTime() {
      if (random) {
        return UnityEngine.Random.Range(randomTimeFrom, randomTimeTo);
      } else {
        return initialTime;
      }
    }

    public NYDITimer(string name, float initialTime, Action func) {
      this.func += func;
      this.name = name;
      this.initialTime = initialTime;
      this.time = initialTime;
    }

    public void Update(float deltaTime) {
      time -= deltaTime;
      if (time < 0) {
        time += GetResetTime();
        if (func != null) {
          func();
        }
      }

    }



  }

}