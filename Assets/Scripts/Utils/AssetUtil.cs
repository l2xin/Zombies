using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;

/**
 * Author: nashnie
 * 
 **/

class AssetUtil
{
    public const string UIAtlasPath = "Module/Atlas/";

    public const string UIPrefabPath = "Module/";
    //.xml
    public static string xmlSuffix = ".xml";
    //.bytes
    public static string bytesSuffix = ".bytes";
    //.assetBundle
    public static string audioSuffix = ".assetBundle";
    //.assetBundle
    public static string abSuffix = ".assetBundle";
    //.prefab
    public static string prefabSuffix = ".prefab";
    //.png
    public static string pngSuffix = ".png";
    //.asset
    public static string assetSuffix = ".asset";
    //.controller
    public static string controller = ".controller";
    //.controller
    public static string mask = ".mask";
    //.lua
    public static string luaSuffix = ".lua";
    //.mp3
    public static string soundSuffix = ".mp3";

    //folder name
    public static string DependBundles = "DependBundles/";

    public static string LightMap = "Map/";

    // 对象标签 Edit by Tom
    public const string ENEMY_TAG   = "EnemyTag";
    public const string PLAYER_TAG  = "PlayerTag";

    private static string _xmlPath = "Xml/";
    private static string _uiPath = "Module/Prefabs/UIPanel/";

    private static string _streamingAsset;
    public static string StreamingAssetPath
    {
        get
        {
            //初始化本地URL
            #if UNITY_ANDROID && !UNITY_EDITOR
                _streamingAsset = Application.streamingAssetsPath;
                //Android 比较特别
            #else
                _streamingAsset = "file://" + Application.streamingAssetsPath;
                //此url 在 windows 及 WP IOS  可以使用   
            #endif

            return _streamingAsset;
        }
    }

    public static string GetAssetSuffix(AssetType assetType)
    {
        string assetSuffix = "";
        switch (assetType)
        {
            case AssetType.prefab:
                assetSuffix = AssetUtil.prefabSuffix;
                break;
            case AssetType.png:
                assetSuffix = AssetUtil.pngSuffix;
                break;
            case AssetType.asset:
                assetSuffix = AssetUtil.assetSuffix;
                break;
            case AssetType.controller:
                assetSuffix = AssetUtil.controller;
                break;
            case AssetType.mask:
                assetSuffix = AssetUtil.mask;
                break;
            case AssetType.sound:
                assetSuffix = AssetUtil.soundSuffix;
                break;
            case AssetType.none:
                assetSuffix = "";
                break;
            default:
                Debug.LogError("GetAssetSuffix error...");
                break;
        }
        return assetSuffix;
    }

    public static GameObject Load(string url, Vector3 pos, Quaternion quaternion)
    {
        UnityEngine.Object obj = Resources.Load(url, typeof(UnityEngine.Object)) as UnityEngine.Object;
        GameObject go = GameObject.Instantiate(obj, pos, quaternion) as GameObject;
        return go;
    }

    public static string LocalUrl
    {
        get
        {
            return ResmgrNative.Instance.localurl;
        }
    }

    public static string CacheUrl
    {
        get
        {
            return ResmgrNative.Instance.cacheurl;
        }
    }

    public static Boolean CheckCacheFileExists(string name)
    {
        if (Global.IsOpenHotUpdate)
        {
            LocalVersion.ResState state = ResmgrNative.Instance.GetFileState(name);
            if (state == LocalVersion.ResState.ResState_UseLocal)
            {
                return false;
            }
            else if (state == LocalVersion.ResState.ResState_UseDownloaded)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    public static string GetXML(string name)
    {
        return _xmlPath + name;
    }

    public static string GetUI(string name)
    {
        return _uiPath + name;
    }
}