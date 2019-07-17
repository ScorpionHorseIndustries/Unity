using Newtonsoft.Json.Linq;
using NoYouDoIt.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoYouDoIt.DataModels {
  public class NYDIAnimation {
    public string name;
    public NYDIAnimationFrame[] frames;

  }

  public class NYDIAnimationFrame {

    public static NYDIAnimationFrame MakeFrame(JObject json) {
      NYDIAnimationFrame frame = new NYDIAnimationFrame();

      float randomTimeMin = Funcs.jsonGetFloat(json["randomTimeMin"], -1);
      float randomTimeMax = Funcs.jsonGetFloat(json["randomTimeMax"], -1);
      float time = Funcs.jsonGetFloat(json["time"], -1);
      string sprite = Funcs.jsonGetString(json["sprite"], null);
      frame.sprite = sprite;
      if (sprite != null && randomTimeMin > 0 && randomTimeMax > 0) {
        frame.randomTime = true;
        frame.randomTimeMin = randomTimeMin;
        frame.randomTimeMax = randomTimeMax;
        frame.time = 0;



      } else if (sprite != null && time > 0) {
        frame.randomTime = false;
        frame.randomTimeMin = 0;
        frame.randomTimeMax = 0;
        frame.time = time;

      } else {
        frame = null;
      }



      return frame;

    }

    public string sprite { get; private set; }

    public float randomTimeMin { get; private set; }
    public float randomTimeMax { get; private set; }
    public float time { get; private set; }
    public bool randomTime { get; private set; } = false;

    private NYDIAnimationFrame() {

    }

    public NYDIAnimationFrame(string sprite, float randomTimeMin, float randomTimeMax) {
      this.sprite = sprite;
      this.randomTimeMin = randomTimeMin;
      this.randomTimeMax = randomTimeMax;
      this.randomTime = true;

    }
    public NYDIAnimationFrame(string sprite, float time) {
      this.sprite = sprite;
      this.randomTime = false;
      this.time = time;
    }

    public float Time {
      get {
        if (randomTime) {
          return UnityEngine.Random.Range(randomTimeMin, randomTimeMax);
        } else {
          return time;
        }
      }
    }




  }
}
