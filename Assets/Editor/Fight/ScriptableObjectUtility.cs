using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;

public static class ScriptableObjectUtility
{
    public static void CreateAsset<T> () where T : ScriptableObject
    {
        T asset = ScriptableObject.CreateInstance<T> ();
        
        string path = AssetDatabase.GetAssetPath (Selection.activeObject);
        if (path == "") 
        {
            path = "Assets";
        } 
        else if (Path.GetExtension (path) != "") 
        {
            path = path.Replace (Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
        }
		
        string fileName;
		if (asset is MoveInfo) {
			fileName = "New Move";
		}else if (asset is CharacterInfo) {
			fileName = "New Character";
		}else if (asset.GetType().ToString().Equals("AIInfo")) {
			fileName = "New AI Instructions";
		}else if (asset is GlobalInfo) {
			fileName = "UFE_Config";
		}else{
			fileName = typeof(T).ToString();
		}
        string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath (path + "/" + fileName + ".asset");
        
        AssetDatabase.CreateAsset (asset, assetPathAndName);
        
        AssetDatabase.SaveAssets ();
        EditorUtility.FocusProjectWindow ();
        Selection.activeObject = asset;
		
		if (asset is MoveInfo) {
			MoveEditorWindow.Init();
		}else if (asset is GlobalInfo) {
			GlobalEditorWindow.Init();
		}else if (asset.GetType().ToString().Equals("AIInfo")){
			FightManager.SearchClass("AIEditorWindow").GetMethod(
				"Init", 
				BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy,
				null,
				null,
				null
			).Invoke(null, new object[]{});
		}else if (asset is CharacterInfo) {
			CharacterEditorWindow.Init();
		}
		
    }
}
