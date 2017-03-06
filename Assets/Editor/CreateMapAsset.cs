using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Media;
using System.Collections.Generic;
using System.Xml;
using System.Runtime.InteropServices;
using UnityEngine.UI;

/// <summary>
/// nashnie
/// </summary>
public class CreateMapAsset 
{
    public static bool isCompressByZip = false;
    static List<string> paths = new List<string>();
    static List<string> files = new List<string>();
    static List<AssetBundleBuild> _buils;
    static string copyToGameTargetPath = "";
    static string copyToGameTargetPathKey = "CopyToGameTargetPathKey";
    private static Dictionary<string, List<string>> _atlasDic;
    private static List<cellModel> cellList;
    private static List<itemModel> itemList;
    private static readonly int serverMapWidth = 4000;
    private static readonly int serverMapGridWidth = 100;


    [MenuItem("Custom Tools/CreateSoundPrefab")]
    private static void CreateSoundsPrefab()
    {
        string resPath = AppDataPath + "/BuildAssets/Sounds";
        string prefabPath = resPath + "/Prefab";

        if (!Directory.Exists(prefabPath))
        {
            Directory.CreateDirectory(prefabPath);
        }

        DirectoryInfo directionInfo = new DirectoryInfo(resPath);
        foreach (FileInfo fileInfo in directionInfo.GetFiles("*.mp3", SearchOption.AllDirectories))
        {
            string allPath = fileInfo.FullName;
            string assetPath = allPath.Substring(allPath.IndexOf("assets"));
            AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            GameObject go = new GameObject(audioClip.name);
            go.AddComponent<AudioSource>().clip = audioClip;
            string singlePrefabPath = prefabPath + "/" + "Sound" + audioClip.name + ".prefab";
            singlePrefabPath = singlePrefabPath.Substring(singlePrefabPath.IndexOf("assets"));
            PrefabUtility.CreatePrefab(singlePrefabPath, go);
            GameObject.DestroyImmediate(go);
        }
    }

    private static cellModel GetCellModel(int x, int z)
    {
        for (int i = 0; i < cellList.Count; i++)
        {
            cellModel model = cellList[i];
            if(model.cellX == x && model.cellZ == z)
            {
                return model;
            }
        }
        cellModel temp = new cellModel();
        temp.cellX = x;
        temp.cellZ = z;
        temp.type = 0;
        return temp;
    }

    struct vortexModel
    {
        public string id;
        public string pos;
    }

    struct cellModel
    {
        public int cellX;
        public int cellZ;
        public int type;
    }

    struct itemModel
    {
        public int id;
        public float posX;
        public float posZ;
        public int type;
    }

    enum MapItemType
    {
        Mage=1,//职业道具
        MageAndBoom,//职业道具和炸弹等
        Coins,//金币
    }

    static void UpdateProgress(int progress, int progressMax, string desc)
    {
        string title = "Processing...[" + progress + " - " + progressMax + "]";
        float value = (float)progress / (float)progressMax;
        EditorUtility.DisplayProgressBar(title, desc, value);
    }

    static void Recursive(string path)
    {
        string[] names = Directory.GetFiles(path);
        string[] dirs = Directory.GetDirectories(path);
        foreach (string filename in names)
        {
            string ext = Path.GetExtension(filename);
            if (ext.Equals(".meta")) continue;
            files.Add(filename.Replace('\\', '/'));
        }
        foreach (string dir in dirs)
        {
            paths.Add(dir.Replace('\\', '/'));
            Recursive(dir);
        }
    }

    [MenuItem("Custom Tools/Build All")]
    public static void BuildAllOf()
    {
		Debug.Log ("BuildAllOf...");
        List<string> folderNameList = new List<string>();
        folderNameList.Add("GUI");
        folderNameList.Add("Fonts");
        folderNameList.Add("Items");
        folderNameList.Add("Module");
        folderNameList.Add("Effect");
        folderNameList.Add("Move");
        folderNameList.Add("Avatar");
        folderNameList.Add("Character");
        folderNameList.Add("Sounds/Prefab");
        //folderNameList.Add("ShopIcons/Prefab");
        //folderNameList.Add("SkillIcons/Prefab");
        BuildUnityStreamResource(folderNameList);
        BuildConf();
    }

    [MenuItem("Custom Tools/Build Character")]
    public static void BuildCharacter()
    {
        List<string> folderNameList = new List<string>();
        folderNameList.Add("Avatar");
        folderNameList.Add("Move");
        folderNameList.Add("Character");
     
        BuildResource(folderNameList);
    }

    [MenuItem("Custom Tools/Build UIModule")]
    public static void BuildUIModule()
    {
        List<string> folderNameList = new List<string>();
        folderNameList.Add("GUI");
        folderNameList.Add("Fonts");
        folderNameList.Add("Module");
        BuildUnityStreamResource(folderNameList);
    }

    [MenuItem("Custom Tools/Build ConfAndXml")]
    public static void BuildConf()
    {
        //CopyResTo("Conf");
        //CopyResTo("Xml");
    }

    public static void BuildUnityStreamResource(List<string> folderNameList, bool isExtension = true)
    {
        _buils = new List<AssetBundleBuild>();
        List<AssetBundleBuild> _buils2 = new List<AssetBundleBuild>();
        _atlasDic = new Dictionary<string, List<string>>();
        string resPath = AppDataPath + "/StreamingAssets/";
        for (int i = 0; i < folderNameList.Count; i++)
        {
            string folderName = folderNameList[i];
            Caching.CleanCache();
            //string dataPath = AppDataPath + "/Resources/" + folderName + "/";
            string dataPath = AppDataPath + "/BuildAssets/" + folderName + "/";
            files.Clear();
            Recursive(dataPath);

            if(folderName == "Fonts")
            {
                AssetBundleBuild buid = new AssetBundleBuild();
                List<string> assetNameList = new List<string>();
                foreach (string name in files)
                {
                    FileInfo fileInfo = new FileInfo(name);
                    if (fileInfo.Name.ToLower().EndsWith(".meta"))
                    {
                        continue;
                    }
                    string assetNames = "Assets/BuildAssets/" + folderName + "/" + fileInfo.Name;
                    assetNameList.Add(assetNames);
                }
                string[] assetNameStrList = assetNameList.ToArray();
                buid.assetNames = assetNameStrList;
                buid.assetBundleName = "fonts";
                _buils2.Add(buid);
            }
            else if (folderName == "SkillIcons")
            {
                List<string> assetNameList = new List<string>();
                foreach (string name in files)
                {
                    FileInfo fileInfo = new FileInfo(name);
                    string assetName = fileInfo.Name.Split(new char[] { '.' })[0];
                    if (fileInfo.Name.ToLower().EndsWith(".meta"))
                    {
                        continue;
                    }
                    else if(fileInfo.Name.ToLower().EndsWith(".png"))
                    {
                        /*string pngPrefabPath = fileInfo.Directory.FullName + "/" + assetName + ".prefab";
                        if (File.Exists(pngPrefabPath) == false)
                        {
                            GameObject go = new GameObject(assetName);
                            Image image = go.AddComponent<Image>();
                            UnityEngine.Sprite sprite = AssetDatabase.LoadAssetAtPath(fileInfo.FullName, typeof(Sprite)) as Sprite;
                            image.sprite = sprite;
                        }*/
                        continue;
                    }
                    AssetBundleBuild buid = new AssetBundleBuild();
                    string assetNames = "Assets/BuildAssets/" + folderName + "/" + fileInfo.Name;
                    buid.assetNames = new string[] { assetNames };
                    if (isExtension)
                    {
                        buid.assetBundleName = fileInfo.Name;
                    }
                    else
                    {
                        string assetBundleName = fileInfo.Name.Split(new char[] { '.' })[0];
                        buid.assetBundleName = assetBundleName;
                    }
                    _buils2.Add(buid);
                }
            }
            else
            {
                int max = files.Count;
                int current = 0;
                foreach (string name in files)
                {
                    FileInfo fileInfo = new FileInfo(name);
                    if (fileInfo.Name.ToLower().EndsWith(".meta"))
                    {
                        continue;
                    }
                    current++;
                    UpdateProgress(current, max, fileInfo.Name);

                    string assetNames = "Assets/BuildAssets/" + folderName + "/" + fileInfo.Name;

                    if (folderName == "GUI")
                    {
                        CheckPackTag(assetNames);
                    }
                    else
                    {
                        AssetBundleBuild buid = new AssetBundleBuild();
                        buid.assetNames = new string[] { assetNames };
                        if (isExtension)
                        {
                            buid.assetBundleName = fileInfo.Name;
                        }
                        else
                        {
                            string assetBundleName = fileInfo.Name.Split(new char[] { '.' })[0];
                            buid.assetBundleName = assetBundleName;
                        }
                        _buils2.Add(buid);
                    }
                }
            }
        }

        foreach (string atlasName in _atlasDic.Keys)
        {
            AssetBundleBuild buid = new AssetBundleBuild();
            buid.assetNames = _atlasDic[atlasName].ToArray();
            buid.assetBundleName = "atlas_" + atlasName;
            _buils.Add(buid);
        }
        _buils.AddRange(_buils2);

        string targetPath = resPath + platform.platformString + "/";
        UnityEditor.BuildTarget target = platform.GetBuildTargets()[platform.platformString];
        AssetBundleBuild[] buildArr = _buils.ToArray();
        BuildPipeline.BuildAssetBundles(targetPath, buildArr, BuildAssetBundleOptions.None, target);

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    private static bool CheckPackTag(string path)
    {
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti != null && ti.spritePackingTag != "")
        {
            string atlasTag = ti.spritePackingTag;
            List<string> texturePathList;
            if (_atlasDic.ContainsKey(atlasTag))
            {
                texturePathList = _atlasDic[atlasTag];
            }
            else
            {
                texturePathList = new List<string>();
                _atlasDic.Add(atlasTag, texturePathList);
            }
            if (!texturePathList.Contains(path))
                texturePathList.Add(path);
            return true;
        }
        return false;
    }

    public static void BuildResource(List<string> folderNameList, bool isExtension = true)
    {
        _buils = new List<AssetBundleBuild>();
        string resPath = AppDataPath + "/StreamingAssets/";
        for (int i = 0; i < folderNameList.Count; i++)
        {
            string folderName = folderNameList[i];
            Caching.CleanCache();
            //string dataPath = AppDataPath + "/Resources/" + folderName + "/";
            string dataPath = AppDataPath + "/BuildAssets/" + folderName + "/";
            files.Clear();
            Recursive(dataPath);

            int max = files.Count;
            int current = 0;
            foreach (string name in files)
            {
                FileInfo fileInfo = new FileInfo(name);
                if (fileInfo.Name.ToLower().EndsWith(".meta"))
                {
                    continue;
                }
                current++;
                UpdateProgress(current, max, fileInfo.Name);

                //string assetNames = "Assets/Resources/" + folderName + "/" + fileInfo.Name;
                string assetNames = "Assets/BuildAssets/" + folderName + "/" + fileInfo.Name;
                AssetBundleBuild buid = new AssetBundleBuild();
                buid.assetNames = new string[] { assetNames };
                if(isExtension)
                {
                    buid.assetBundleName = fileInfo.Name;
                }
                else
                {
                    string assetBundleName = fileInfo.Name.Split(new char[] { '.' })[0];
                    buid.assetBundleName = assetBundleName;
                }

                _buils.Add(buid);
            }
        }
        string targetPath = resPath + platform.platformString + "/";
        UnityEditor.BuildTarget target = platform.GetBuildTargets()[platform.platformString];
        AssetBundleBuild[] buildArr = _buils.ToArray();
        BuildPipeline.BuildAssetBundles(targetPath, buildArr, BuildAssetBundleOptions.None, target);

        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
    }

    public static void BuildResource2(string folderName)
    {
        string resPath = AppDataPath + "/StreamingAssets/";
        string targetRootPath = resPath + platform.platformString + "/" + folderName + "/";
        string dataPath = AppDataPath + "/Resources/" + folderName + "/";
        ExportSingleBatchExecute(dataPath, targetRootPath);
    }

    static void ExportSingleBatchExecute(string importPath, string exportPath, string directoryName = "")
    {
        string[] fileList = Directory.GetFiles(importPath + directoryName);
        string[] directoryList = Directory.GetDirectories(importPath + directoryName);

        foreach (string file in fileList)
        {
            if (file.Contains(".meta") == false)
            {
                Object asset = AssetDatabase.LoadMainAssetAtPath("Assets" + file.Remove(0, Application.dataPath.Length).Replace("\\", "/"));
                string directoryPath = (exportPath + directoryName).Replace("\\", "/");

                if (Directory.Exists(directoryPath) == false)
                {
                    Directory.CreateDirectory(directoryPath);
                }

                UnityEditor.BuildTarget target = platform.GetBuildTargets()[platform.platformString];
                BuildPipeline.BuildAssetBundle(asset, new Object[] { asset }, directoryPath + "/" + asset.name + ".assetBundle", CurrentBuildAssetOpts, target);
            }
        }

        foreach (string directory in directoryList)
        {
            ExportSingleBatchExecute(importPath, exportPath, directory.Remove(0, importPath.Length));
        }
    }


    static string AppDataPath
    {
        get { return Application.dataPath.ToLower(); }
    }

    private static BuildAssetBundleOptions CurrentBuildAssetOpts
    {
        get
        {
            return (isCompressByZip == false ? 0 : BuildAssetBundleOptions.UncompressedAssetBundle) | 
                    BuildAssetBundleOptions.DeterministicAssetBundle |
                    BuildAssetBundleOptions.CollectDependencies;
        }
    }
}
