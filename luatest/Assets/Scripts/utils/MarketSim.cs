using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace NoYouDoIt.Utils {
  public class MarketSim {

    float min, max;
    float wavesMax;
    List<NoiseWave> waves = new List<NoiseWave>();
    public MarketSim(int numWaves, float min, float max) {
      for (int i = 0; i < numWaves; i += 1) {
        waves.Add(new NoiseWave());
      }
      this.min = min;
      this.max = max;
      wavesMax = 0;
      foreach (NoiseWave w in waves) {
        wavesMax += w.ampNoise.max;
      }



    }



    public float GetValue() {
      float value = 0;
      foreach (NoiseWave nw in waves) {
        value += nw.GetValue() / wavesMax;
      }
      value = (value + 1f) * 0.5f;

      return Funcs.Map(value, 0f, 1f, min, max);
    }


  }

}