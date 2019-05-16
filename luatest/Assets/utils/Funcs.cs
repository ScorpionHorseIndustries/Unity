using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
public static class Funcs {
  public static string getSpriteName(string name) {
    name = name.Replace(':', '_');



    return name;
  }

  public static string jsonGetString(JToken token, string df) {

    if (token == null) {
      return df;
    } else {
      return (string)token;
    }
  }

  public static bool jsonGetBool(JToken token, bool df) {
    if (token == null) {
      return df;
    } else {
      return (bool)token;
    }
  }

  public static int jsonGetInt(JToken token, int df) {
    if (token == null) {
      return df;
    } else {
      return (int)token;
    }
  }

  public static float jsonGetFloat(JToken token, float df) {
    if (token == null) {
      return df;
    } else {
      return (float)token;
    }
  }

  public static float QuickDistance(float x1, float y1, float x2, float y2) {
    float d = Mathf.Pow(x1 - x2, 2) + Mathf.Pow(y1 - y2, 2);


    return d;
  }

  public static float TaxiDistance(Tile A, Tile B) {
    return TaxiDistance(A.x, A.y, B.x, B.y);
  }

  public static float TaxiDistance(float x1, float y1, float x2, float y2) {

    return Mathf.Abs(x2 - x1) + Mathf.Abs(y2 - y1);
  }

  public static float Distance(Tile A, Tile B) {
    return Distance(A.x, A.y, B.x, B.y);
  }
  public static float Distance(float x1, float y1, float x2, float y2) {
    float d = Mathf.Sqrt(Mathf.Pow(x1 - x2, 2) + Mathf.Pow(y1 - y2, 2));


    return d;
  }

}
