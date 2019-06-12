using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AutomaticResize))]
public class AutomaticVerticalSizeEditor : Editor
{

	public override void OnInspectorGUI(){
		base.OnInspectorGUI();

		//DrawDefaultInspector();
		if (GUILayout.Button("Resize")) {
			((AutomaticResize)target).AdjustSize();
		}


	}
}
