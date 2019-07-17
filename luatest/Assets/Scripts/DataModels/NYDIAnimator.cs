using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoYouDoIt.DataModels {
  public class NYDIAnimator {
    public Dictionary<string, NYDIAnimation> animations;
    public int index { get; private set; } = 0;
    public float timer { get; private set; } = 0;
    public string currentSprite = "";
    public string name;
    public NYDIAnimationFrame currentFrame = null;
    public NYDIAnimation currentAnimation = null;
    public bool running { get; private set; } = false;
    public bool valid { get; private set; } = false;
    public bool active { get; private set; } = false;
    public bool changed { get; private set; } = false;

    public NYDIAnimator(Dictionary<string, NYDIAnimation> animations) {
      this.animations = animations;
      if (animations.Count > 0) {
        valid = true;
      }
    }

    public bool Set(string name) {
      if (name == null) {
        currentAnimation = null;
        active = false;
        return true;
      }
      if (animations.ContainsKey(name)) {
        //if (currentAnimation == null || (currentAnimation != null && currentAnimation.name != name)) {
          active = true;
          currentAnimation = animations[name];
          index = 0;
          currentFrame = currentAnimation.frames[index];
          currentSprite = currentFrame.sprite;
          timer = currentFrame.Time;
          running = true;
          changed = true;
        //}
        return true;
      }
      running = false;
      currentAnimation = null;
      active = false;
      return false;
    }



    public void Update(float deltaTime) {
      changed = false;
      if (currentAnimation != null) {
        running = true;
        changed = true;
        if (currentFrame == null) {
          index = 0;
          currentFrame = currentAnimation.frames[index];
          timer = currentFrame.Time;
          currentSprite = currentFrame.sprite;
        }

        timer -= deltaTime;



        if (timer <= 0) {
          index = (index + 1) % currentAnimation.frames.Length;
          currentFrame = currentAnimation.frames[index];
          currentSprite = currentFrame.sprite;
          timer = currentFrame.Time;
          changed = true;
        }
      } else {
        currentSprite = null;
        running = false;
        changed = true;
        active = false;

      }





    }
  }
}
