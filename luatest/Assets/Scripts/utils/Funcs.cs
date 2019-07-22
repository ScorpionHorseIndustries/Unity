using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Text;
using System.IO.Compression;

using NoYouDoIt.TheWorld;
namespace NoYouDoIt.Utils {
  public static class Funcs {

    static Funcs() {
      LoadSettings();

    }
    private static Dictionary<string, string> settings;
    public static string GetSettingString(string name) {
      string n = name.ToLower();
      return settings[n];
    }

    public static bool GetSettingBool(string name) {
      string n = name.ToLower();
      bool result;
      if (bool.TryParse(settings[n], out result)) {
        return result;
      } else {
        return false;
      }
    }

    public static float GetSettingFloat(string name) {
      string n = name.ToLower();
      float result;
      if (float.TryParse(settings[n], out result)) {
        return result;
      } else {
        return 0;
      }
    }

    public static int GetSettingInt(string name) {
      string n = name.ToLower();
      int result;
      if (int.TryParse(settings[n], out result)) {
        return result;
      } else {
        return 0;
      }
    }

    public static void LoadSettings() {
      settings = new Dictionary<string, string>();
      string path = Path.Combine(Application.streamingAssetsPath, "data", "settings");

      string[] files = Directory.GetFiles(path, "settings*.json");

      foreach (string file in files) {


        string json = File.ReadAllText(file);

        JObject obj = JObject.Parse(json);

        foreach (JProperty jp in obj.Properties()) {
         settings[jp.Name.ToLower()] = jp.Value.ToString();

        }



        //foreach (var kvp in json.Children()) {

        //  Debug.Log(kvp + " = [" + kvp.Path + "]:[" + kvp.Children()[kvp.Path] + "]");


        //}
      }

    }

    public static string GetSpriteDirection(float px, float py, float nx, float ny) {
      float xx = nx - px;
      float yy = ny - py;

      if (xx < 0) {
        return World.WEST;
      } else if (xx > 0) {
        return World.EAST;
      } else if (yy > 0) {
        return World.NORTH;
      } else {
        return World.SOUTH;
      }



    }

    public static object Coalesce(params object[] objects) {
      foreach (object o in objects) {
        if (o != null) return o;
      }
      return null;
    }

    public static string GetLuaVariableName(string name) {
      string s = name;
      s = s.Replace(':', '_');


      return s;
    }


    public static string PadPair(int width, string property, string value, char padding = '.') {
      string output = "";

      string p = property;
      if (p.Length > width / 2) {
        p = p.Substring(0, width / 2);
      }

      string pad = "";

      string v = value;

      if (v.Length > width / 2) {
        v = v.Substring(0, width / 2);
      }

      while ((p + pad + v).Length < width) {
        pad += padding;
      }


      output = p + pad + v;

      //Debug.Log("padpair " + output + ", " + output.Length);

      return output;
    }

    public static string pad(int width, string padding, params string[] args) {
      string output = "";
      if (padding.Length > 1) {
        padding = padding.Substring(0, 1);
      }

      foreach (string s in args) {
        string ss = s;
        if (ss.Length > width) {
          ss = s.Substring(0, width);
        } else {
          while (ss.Length < width) {
            ss += padding;
          }
        }
        output += ss;
      }

      return output;
    }

    public static string getSpriteName(string name) {
      name = name.Replace(':', '_');



      return name;
    }

    public static float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget) {
      return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
    }

    public static JArray jsonGetArray(JObject o, string name) {
      if (o.Property(name) != null) {
        return (JArray)o[name];
      } else {
        return null;
      }
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
      return TaxiDistance(A.world_x, A.world_y, B.world_x, B.world_y);
    }

    public static float TaxiDistance(float x1, float y1, float x2, float y2) {

      return Mathf.Abs(x2 - x1) + Mathf.Abs(y2 - y1);
    }

    public static float Distance(Tile A, Tile B) {
      if (A == B) return 0;
      return Distance(A.world_x, A.world_y, B.world_x, B.world_y);
    }
    public static float Distance(float x1, float y1, float x2, float y2) {
      float d = Mathf.Sqrt(Mathf.Pow(x1 - x2, 2) + Mathf.Pow(y1 - y2, 2));


      return d;
    }

    public static void CopyTo(Stream src, Stream dest) {
      byte[] bytes = new byte[4096];

      int cnt;

      while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0) {
        dest.Write(bytes, 0, cnt);
      }
    }

    public static byte[] Zip(string str) {
      var bytes = Encoding.UTF8.GetBytes(str);

      using (var msi = new MemoryStream(bytes))
      using (var mso = new MemoryStream()) {
        using (var gs = new GZipStream(mso, CompressionMode.Compress)) {
          //msi.CopyTo(gs);
          CopyTo(msi, gs);
        }

        return mso.ToArray();
      }
    }

    public static string Unzip(byte[] bytes) {
      using (var msi = new MemoryStream(bytes))
      using (var mso = new MemoryStream()) {
        using (var gs = new GZipStream(msi, CompressionMode.Decompress)) {
          //gs.CopyTo(mso);
          CopyTo(gs, mso);
        }

        return Encoding.UTF8.GetString(mso.ToArray());
      }
    }

    public static Vector2 GetInstalledItemSpriteOffset(int w, int h) {

      Vector2 vec = new Vector2(((float)(w) / 2.0f) - 0.5f, ((float)(h) / 2.0f) - 0.5f);


      return vec;

    }

    public static Vector2 Spiral(float n) {



      float k = Mathf.Ceil((Mathf.Sqrt(n) - 1) / 2);
      float t = 2 * k + 1;
      float m = t * t;
      t = t - 1;
      if (n >= m - t) {
        return new Vector2(k - (m - n), -k);
      } else {
        m = m - t;
      }

      if (n >= m - t) {
        return new Vector2(-k, -k + (m - n));
      } else {
        m = m - t;
      }

      if (n >= m - t) {
        return new Vector2(-k + (m - n), k);
      } else {
        return new Vector2(k, k - (m - n - t));
      }
    }



    public static string Base64Encode(byte[] bytes) {
      return System.Convert.ToBase64String(bytes);
    }

    public static byte[] Base64Decode(string base64EncodedData, bool whatever) {
      return System.Convert.FromBase64String(base64EncodedData);

    }

    public static string Base64Encode(string plainText) {
      byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
      return System.Convert.ToBase64String(plainTextBytes);
    }

    public static string Base64Decode(string base64EncodedData) {
      byte[] base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
      return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }


    public static string TitleCase(string s) {
      s = s.ToUpper();
      string ss = "";
      if (s.Substring(0, 2) == "MC") {
        ss = "Mc";
        ss += s[2].ToString().ToUpper();
        ss += s.Substring(3).ToLower();
      } else {
        ss = s[0].ToString().ToUpper();
        ss += s.Substring(1).ToLower();
      }





      return ss;
    }

    public static bool fChance(float chance = 50.0f) {
      return UnityEngine.Random.Range(0.0f, 100.0f) < chance;
    }

    public static bool Chance(int chance = 50) {
      return UnityEngine.Random.Range(0, 100) < chance;
    }
  }
}