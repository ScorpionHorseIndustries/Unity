using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseWave 
{
  public static readonly float TWO_PI = Mathf.PI * 2.0f;

  
  float angle = 0;
  float inc = 0;
  float incRange;
  float incNoiseAmt;
  float amp;
  float ampRange;
  float ampNoiseAmt;
  public Noiser incNoise, ampNoise;
  public NoiseWave() {
    incRange = Random.Range(0.000001f, 0.5f);
    incNoiseAmt = Random.Range(0.0000001f, 0.1f);
    incNoise = new Noiser(Mathf.Deg2Rad * -incRange, Mathf.Deg2Rad * incRange, incNoiseAmt);


    amp = 0;
    ampRange = (1f / incRange);
    ampNoiseAmt = Random.Range(0.01f,0.1f);
    ampNoise = new Noiser(Mathf.Deg2Rad * -ampRange, Mathf.Deg2Rad * ampRange, ampNoiseAmt);


    angle = Random.Range(0f, TWO_PI);



  }


  public float GetValue() {
    inc = incNoise.GetValue();
    amp = ampNoise.GetValue();
    angle += inc;
    return Mathf.Sin(angle) * amp;
  }
  



  public class Noiser {
    public float min, max, inc, angle, value, x, y, cx, cy,cz,radius;

    public Noiser(float min, float max, float inc) {
      this.min = min;
      this.max = max;
      this.inc = inc;
      this.angle = Random.Range(0, TWO_PI);

      radius = Random.Range(-10f, 10f);
      this.cx = Random.Range(-100f, 100f);
      this.cy = Random.Range(-100f, 100f);
      this.cz = Random.Range(-100f, 100f);
    }


    public float GetValue() {
      angle += inc;
      angle %= TWO_PI;
      x = cx + radius * Mathf.Cos(angle);
      y = cy + radius * Mathf.Sin(angle);

      value = World.current.SimplexNoise(x, y, cz);
      return Funcs.Map(value, 0, 1, min, max);

    }
  }

}
