using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NoYouDoIt.Utils {
  public class Circular {
    float angle, angle_inc, radius;
    float cx, cy;

    public Circular(float angle, float angle_inc, float radius, float cx, float cy) {
      this.angle = angle;
      this.angle_inc = angle_inc;
      this.radius = radius;
      this.cx = cx;
      this.cy = cy;
    }

    public float x { get; private set; }
    public float y { get; private set; }



    public void Update() {
      x = cx + radius * Mathf.Cos(angle);
      y = cy + radius * Mathf.Sin(angle);
      angle += angle_inc;
      angle = angle % (Mathf.PI * 2f);
    }
  }

  public class MarketSimv2 {
    FastNoise noise;
    string seed;
    private float value;
    private float x, y, z;
    public float min { get; private set; }
    public float max { get; private set; }

    private Circular Cxy, Cz;


    public MarketSimv2 (float min, float max) {
      this.min = min;
      this.max = max;
      seed = DateTime.Now.ToLongDateString() + "_" + DateTime.Now.ToLongTimeString() + "_" + DateTime.Now.ToUniversalTime();
      noise = new FastNoise(seed.GetHashCode());
      noise.SetNoiseType(FastNoise.NoiseType.Simplex);
      Cxy = new Circular(0f, 0.01f, 0.1f, UnityEngine.Random.Range(0f, 100f), UnityEngine.Random.Range(0f, 100f));
      Cz = new Circular(0f, 0.01f, 1.1f, UnityEngine.Random.Range(0f, 100f), UnityEngine.Random.Range(0f, 100f));
      
    }

    public void Update() {
      Cxy.Update();
      Cz.Update();
      z = Cz.x;
      x = Cxy.x;
      y = Cxy.y;
      value = SimplexNoise(x, y, z);
      value = Mathf.Lerp(min, max, value);
    }

    public float GetNextValue() {
      Update();
      return GetCurrentValue();
    }

    public float GetCurrentValue() {

      return value;

    }

    

    float SimplexNoise(float x, float y) {
      float n = noise.GetNoise(x, y);
      n = (n + 1) * 0.5f;
      return n;
    }

    float SimplexNoise(float x, float y, float z) {
      float n = noise.GetNoise(x, y, z);
      n = (n + 1) * 0.5f;
      return n;
    }
  }
}
