using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(MoveInfo))]
[CanEditMultipleObjects]
public class MoveEditor : Editor {
	public override void OnInspectorGUI(){
		if (GUILayout.Button("Open Move Editor")) 
			MoveEditorWindow.Init();
		
	}
}