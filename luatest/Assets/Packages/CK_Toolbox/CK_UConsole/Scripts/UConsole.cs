namespace CardboardKeep {
  using UnityEngine;
  using System.Collections;
  using System.Collections.Generic;
  using System.Reflection;
  using UnityEngine.UI;
  /// <summary>
  /// UConsole - A Valve-style in-game runtime command console for Unity games
  /// Author: Calum Spring
  /// E-mail: calum@cardboardkeep.com
  /// Creation Date: 7 November 2014
  /// Last Update: 26 April 2015
  /// License: You may modify these works and package them in a game release, commercial or otherwise, but you may not redistribute or resell this code, with or without modifications. This code is copyright Cardboard Keep PTY LTD and is protected by the Unity Asset Store commercial license (http://unity3d.com/legal/as_terms)
  /// Usage: Drag the Console prefab into your scene, press tilde (~) to activate, add functions in the UConsoleCommands script to make them callable from the console. Requires Unity 4.6 or higher due to use of uGUI.
  /// </summary>
  public class UConsole : MonoBehaviour {
    // Static instance
    public static UConsole instance;

    // Components
    UConsoleCommands commands;
    CanvasGroup canvas;
    InputField inputField;
    Text suggestions, eventLog;

    // Complex data
    List<MethodInfo> methods = new List<MethodInfo>();
    BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

    // Simple data
    string oldInputText, inputMethod;
    [HideInInspector]
    public string inputArgument; // <- public so it can be accessed by UConsoleCommands
    public string[] args;
    bool on;

    void Start() {
      instance = this;

      commands = GetComponent<UConsoleCommands>();
      canvas = GetComponent<CanvasGroup>();
      inputField = transform.Find("InputField").GetComponent<InputField>();
      suggestions = transform.Find("ConsoleSuggestions").GetComponent<Text>();
      eventLog = transform.Find("EventLog").GetComponent<Text>();

      methods.AddRange(commands.GetType().GetMethods(flags));
      methods.Sort((x, y) => string.Compare(x.Name, y.Name)); // Order alphabetically
                                                              /*foreach(MethodInfo m in methods) // Enable to print a list of grabbed methods
                                                                  Debug.Log(m.Name);*/
      eventLog.text = "";

      // Subtle deactivation
      inputField.DeactivateInputField();
      inputField.enabled = false;
      canvas.alpha = 0;
    }

    void Update() {
      if (on) {
        // clear leading ` characters due to ~/` key being pressed to open console
        if (inputField.text.StartsWith("`"))
          inputField.text = "";
        // if the input string has changed, re-compare to functions
        if (oldInputText != inputField.text) {
          suggestions.text = "";

          string[] splitInput = inputField.text.Split();
          inputMethod = splitInput[0];
          if (splitInput.Length > 1) {
            inputArgument = splitInput[1];
            args = new string[splitInput.Length - 1];
            for (int i = 1; i < splitInput.Length; i += 1) {
              args[i - 1] = splitInput[i];
            }

          }

          foreach (MethodInfo m in methods) {
            if (m.Name.Contains(inputMethod)) {
              suggestions.text += ReadableMethodInfo(m); // Change to ComplexMethodInfo if you prefer
            }
          }
          oldInputText = inputField.text;
        }

        if (Input.GetKeyDown(KeyCode.Tab)) { // Autocomplete and submit the top suggestion
          inputMethod = inputField.text = suggestions.text.Split()[0];
          ConsoleOnSubmit();
        }

        if (Input.GetKeyDown(KeyCode.BackQuote))
          Deactivate();
      } else {
        if (Input.GetKeyDown(KeyCode.BackQuote))
          Activate();
      }
    }

    public void Activate() {
      on = true;
      inputField.enabled = true;
      inputField.ActivateInputField();
      inputField.Select();
      canvas.alpha = 1;
      //commands.SendMessage("GameSpecificActivate", SendMessageOptions.DontRequireReceiver);
      commands.Invoke("GameSpecificActivate", 0);
    }

    public void Deactivate() {
      on = false;
      inputField.DeactivateInputField();
      inputField.enabled = false;
      canvas.alpha = 0;
      //commands.SendMessage("GameSpecificDeactivate", SendMessageOptions.DontRequireReceiver);
      commands.Invoke("GameSpecificDeactivate", 0);
    }

    static public void NewEvent(string text) {
      instance.eventLog.text += text + "\n";
    }

    public void ConsoleValueChange(string input) {
      // if the input string has changed, re-compare to functions
      suggestions.text = "";
      foreach (MethodInfo m in methods) {
        if (m.Name.Contains(input)) {
          suggestions.text += ReadableMethodInfo(m);
        }
      }
    }

    public void ConsoleOnSubmit() {
      if (suggestions.text != "" && inputMethod != "") {
        try {
          commands.Invoke(inputMethod, 0);
        } catch {
          NewEvent("Failed to invoke function, please ensure the function exists and the argument you provided is valid.");
        }
      }
      Deactivate();
    }

    string ReadableMethodInfo(MethodInfo m) {
      string info = m.Name;
      // Display each parameter
      info += " <i>";
      foreach (ParameterInfo pi in m.GetParameters())
        info += pi.Name + " ";
      info += "</i>\n";
      return info;
    }

    string ComplexMethodInfo(MethodInfo m) {
      string info = m.Name;
      // Display each parameter
      info += " <i>(";
      foreach (ParameterInfo pi in m.GetParameters())
        info += " " + pi.ParameterType.ToString() + " " + pi.Name + " ";
      info += ")</i>\n";
      return info;
    }
  }
}