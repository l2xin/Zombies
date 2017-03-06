using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using System.Xml;

/// <summary>
/// nashnie
/// </summary>
public class MainEntry : MonoBehaviour
{
    private static MainEntry _instance;
    private static System.Action _nextFrameCall;
    private static System.Action _updateFrameList;
    private static System.Action _lateUpdateList;
    private bool isInit = false;
    private static Dictionary<string, List<Action<GameObject, string>>> requestingDependLoadList;

    public static MainEntry Instance
    {
        get
        {
            return _instance;
        }
    }

    public static void RunInUpdate(Action function)
    {
        _updateFrameList += function;
    }

    public static void CancelRunInUpdate(Action function)
    {
        if (_updateFrameList != null)
        {
            _updateFrameList -= function;
        }
    }

    public static void CancelAllRunInUpdate()
    {
        _updateFrameList = null;
        _lateUpdateList = null;
        _nextFrameCall = null;
    }

    public static void RunInLateUpdate(Action function)
    {
        _lateUpdateList += function;
    }

    public static void CancelRunInLateUpdate(Action function)
    {
        if (_lateUpdateList != null)
        {
            _lateUpdateList -= function;
        }
    }

    public static void RunInNextFrame(System.Action function)
    {
        _nextFrameCall += function;
    }

    public static void CancelInNextFrame(System.Action function)
    {
        if(_nextFrameCall != null)
        {
            _nextFrameCall -= function;
        }
    }

    public void StartLoadModule(string name, AssetType assetType, Action<GameObject, string> onLoadComplete)
    {
        StartLoad(name, assetType, onLoadComplete);
    }

    public void StartLoadNoDepend(string name, AssetType assetType, Action<AssetBundle, string> onLoadComplete)
    {
        string resName = platform.platformString + "/" + name;
        string abSuffix = AssetUtil.GetAssetSuffix(assetType);
        resName += abSuffix;
        if (AssetUtil.CheckCacheFileExists(name + abSuffix))
        {
            ResmgrNative.Instance.LoadAssetBundleFromCache(resName, name, (AssetBundle assetBundle, string s) =>
            {
                onLoadComplete(assetBundle, s);
                assetBundle.Unload(false);
            });
        }
        else
        {
            ResmgrNative.Instance.LoadFromStreamingAssets(resName, name, (WWW www, string s) =>
            {
                if (www != null && www.error == null)
                {
                    onLoadComplete(www.assetBundle, s);
                    www.assetBundle.Unload(false);
                    RunInNextFrame(() =>
                    {
                        www.Dispose();
                    });
                }
                else
                {
                    Debug.Log("load fail " + resName);
                    onLoadComplete(null, s);
                    RunInNextFrame(() =>
                    {
                        www.Dispose();
                    });
                }
            });
        }
    }

    public void StartLoadBytes(string name, Action<byte[], string> onLoadComplete)
    {
        string resName = platform.platformString + "/" + name + AssetUtil.bytesSuffix;
        if (AssetUtil.CheckCacheFileExists(name + AssetUtil.bytesSuffix))
        {
            ResmgrNative.Instance.LoadBytesFromCache(resName, name, onLoadComplete);
        }
        else
        {
            ResmgrNative.Instance.LoadFromStreamingAssets(resName, name, (WWW www, string s) =>
            {
                if (www != null && www.error == null)
                {
                    onLoadComplete(www.bytes, name);
                    RunInNextFrame(() =>
                    {
                        www.Dispose();
                    });
                }
            });
        }
    }

    public void StartLoadLua(string name, string folderName, Action<byte[], string> onLoadComplete)
    {
        string resName = "";
        string lowerName = name.ToLower();
        if (lowerName.EndsWith(AssetUtil.luaSuffix))
        {
            resName = folderName + "/" + name;
        }
        else
        {
            resName = folderName + "/" + name + AssetUtil.luaSuffix;
        }
        if (AssetUtil.CheckCacheFileExists(resName))
        {
            ResmgrNative.Instance.LoadBytesFromCache(platform.platformString + "/" + resName, folderName, onLoadComplete);
        }
        else
        {
            ResmgrNative.Instance.LoadFromStreamingAssets(platform.platformString + "/" + resName, folderName, (WWW www, string s) =>
            {
                if (www != null && www.error == null)
                {
                    onLoadComplete(www.bytes, folderName);
                    RunInNextFrame(() =>
                    {
                        www.Dispose();
                    });
                }
            });
        }
    }

    public void StartLoadXml(string name, Action<string, string> onLoadComplete)
    {
        string resName = name + AssetUtil.xmlSuffix;
        if (AssetUtil.CheckCacheFileExists(resName))
        {
            ResmgrNative.Instance.LoadStringFromCache(platform.platformString + "/" + resName, "", onLoadComplete);
        }
        else
        {
            ResmgrNative.Instance.LoadFromStreamingAssets(platform.platformString + "/" + resName, "", (WWW www, string s) =>
            {
                if (www != null && www.error == null)
                {
                    onLoadComplete(www.text, name);
                    RunInNextFrame(() =>
                    {
                        www.Dispose();
                    });
                }
            });
        }
    }

    public void StartLoadTexture(string name, Action<Sprite, string> onLoadComplete)
    {
        StartLoad(name, AssetType.prefab, (GameObject go, string tag) =>
        {
            if(go != null)
            {
                Image image = go.GetComponent<Image>();
                onLoadComplete(image.sprite, name);
                image.sprite = null;
                GameObject.Destroy(go);
            }
        });
    }

    public void StartLoad(int id, AssetType assetType, Action<GameObject, string> onLoadComplete = null)
    {
        StartLoad(id.ToString(), assetType, onLoadComplete);
    }

    public bool CheckResInBundleDict(string name)
    {
        return DownloadManager.Instance.CheckResInBundleDict(name);
    }

    public void StartLoad(string name, AssetType assetType, Action<GameObject, string> onLoadComplete = null)
    {
        if (CheckResInBundleDict(name))
        {
            List<Action<GameObject, string>> onLoadCompleteList = null;
            if (requestingDependLoadList.ContainsKey(name) == false)
            {
                onLoadCompleteList = new List<Action<GameObject, string>>();
                requestingDependLoadList.Add(name, onLoadCompleteList);
            }
            else
            {
                onLoadCompleteList = requestingDependLoadList[name];
            }
            onLoadCompleteList.Add(onLoadComplete);
            StartCoroutine(StartLoadBy(name, assetType, onLoadComplete));
        }
        else
        {
            if (Global.IsDebug && string.IsNullOrEmpty(name) == false)
            {
                Debug.Log("StartLoadBy name " + name + " load fail.(CheckResInBundleDict False.)");
            }
            if (onLoadComplete != null)
            {
                onLoadComplete(null, name);
            }
        }
    }

    public IEnumerator StartLoadAB(string name, AssetType assetType, Action<AssetBundle, string> onLoadComplete = null)
    {
        string getAssetSuffix = AssetUtil.GetAssetSuffix(assetType);
        yield return StartCoroutine(DownloadManager.Instance.WaitDownload(name + getAssetSuffix));
        var bundle = DownloadManager.Instance.GetWWW(name + getAssetSuffix);
        if (bundle != null)
        {
            if (onLoadComplete != null)
            {
                onLoadComplete(bundle, name);
            }
            DownloadManager.Instance.DisposeWWW(name + getAssetSuffix);
        }
    }

    public bool CheckDestroyWWW(string url)
    {
        if(cacheBundleList.IndexOf(url) >= 0)
        {
            return false;
        }
        return true;
    }

    public void StartLoadAssetBundle(string name, AssetType assetType, Action<AssetBundle, string> onLoadComplete = null)
    {
        StartCoroutine(StartLoadAB(name, assetType, onLoadComplete));
    }

    public IEnumerator StartLoadBy(string name, AssetType assetType, Action<GameObject, string> onLoadComplete = null)
    {
        string getAssetSuffix = AssetUtil.GetAssetSuffix(assetType);
        yield return StartCoroutine(DownloadManager.Instance.WaitDownload(name + getAssetSuffix));
        var bundle = DownloadManager.Instance.GetWWW(name + getAssetSuffix);
        if (bundle != null)
        {
            if (requestingDependLoadList.ContainsKey(name))
            {
                List<Action<GameObject, string>> onLoadCompleteList = requestingDependLoadList[name];
                for (int i = 0; i < onLoadCompleteList.Count; i++)
                {
                    Action<GameObject, string> onLoadCompleteTemp = onLoadCompleteList[i];
                    UnityEngine.Object bundleObject = bundle.LoadAsset(name, typeof(GameObject));
                    if (bundleObject != null)
                    {
                        GameObject go = Instantiate(bundleObject) as GameObject;
                        AutoCheckDependBundle autoCheckDependBundle = go.AddComponent<AutoCheckDependBundle>();
                        autoCheckDependBundle.url = name;
                        onLoadCompleteTemp(go, name);
                    }
                    else
                    {
                        Debug.Log("StartLoadBy name " + name + " load failed.");
                        onLoadCompleteTemp(null, name);
                    }
                }
                requestingDependLoadList.Remove(name);
            }
            DownloadManager.Instance.DisposeWWW(name + getAssetSuffix);
        }
    }

    private List<string> cacheBundleList = new List<string>();
    public List<ServerIPAndPort> serverIPAndPortList = new List<ServerIPAndPort>();
    private float fillAmount;
    private AsyncOperation async;
    private ProgressStep currentProgressStep = ProgressStep.loadScene;

    //保留主程序
    void Awake()
    {
        _instance = this;
        GameObject.DontDestroyOnLoad(gameObject);

        //GA.StartWithAppKeyAndChannelId("57bec887e0f55a3e4c004907", VersionManager.channel);

        AlertUI.Setup();
        UIManager.Instance.SetUp();

        requestingDependLoadList = new Dictionary<string, List<Action<GameObject, string>>>();
        cacheBundleList.Add("fonts");
    }

    IEnumerator Start()
    {
        LoadingBarPanel.Instance.UpdateTitleBy(0);
        //从本地读取解析 
        string mainXml = AssetUtil.StreamingAssetPath + "/Main" + AssetUtil.xmlSuffix;
        WWW www = new WWW(mainXml);
        yield return www;

        XmlDocument configDoc = new XmlDocument();
        XmlElement elementXml;
        XmlNodeList nodeList;
        configDoc.LoadXml(www.text);
        nodeList = configDoc.SelectSingleNode("root").ChildNodes;
        for (int i = 0; i < nodeList.Count; i++)
        {
            elementXml = (XmlElement)nodeList[i];
            if (elementXml.Name == "content")
            {
                Global.IsDebug = elementXml.GetAttribute("isDebug") == "1";
                Global.IsOpenHotUpdate = elementXml.GetAttribute("isOpenHotUpdate") == "1";
                Global.IsOpenFPSCounter = elementXml.GetAttribute("isOpenFPSCounter") == "1";
                
                Global.cdnURL = elementXml.GetAttribute("cdnURL");
                Global.localCdnURL = elementXml.GetAttribute("localCdnURL");
                GeneralUtils.TryParseBool(elementXml.GetAttribute("isShowProjectilePath"), out Global.isShowProjectilePath);
            }
            else if (elementXml.Name == "serverIpAndPort")
            {
                ServerIPAndPort serverIpAndPort = new ServerIPAndPort();
                serverIpAndPort.serverName = elementXml.GetAttribute("name");
                serverIpAndPort.ip = elementXml.GetAttribute("ip");
                serverIpAndPort.port = elementXml.GetAttribute("port");
                serverIPAndPortList.Add(serverIpAndPort);
            }
        }

        //从CDN上读取解析 IP port hotUpdateURL homeURL
        if(Global.IsDebug)
        {
            www = new WWW(Global.localCdnURL + "Main" + AssetUtil.xmlSuffix);
        }
        else
        {
            www = new WWW(Global.cdnURL + "Main" + AssetUtil.xmlSuffix);
        }
        yield return www;
        if (www.error == null)
        {
            configDoc = new XmlDocument();
            configDoc.LoadXml(www.text);
            nodeList = configDoc.SelectSingleNode("root").ChildNodes;

            for (int i = 0; i < nodeList.Count; i++)
            {
                elementXml = (XmlElement)nodeList[i];
                if (elementXml.Name == "content")
                {
                    VersionManager.remoteVersion = elementXml.GetAttribute("ver");
                    VersionManager.homeUrl = elementXml.GetAttribute("homeURL");
                    GeneralUtils.TryParseBool(elementXml.GetAttribute("isShowFlag"), out Global.isShowFlag);
                    GeneralUtils.TryParseBool(elementXml.GetAttribute("isShowMicphone"), out Global.IsShowMicphone);
                    Global.maintenanceNotice = elementXml.GetAttribute("maintenanceNotice");
                }
                else if (elementXml.Name == "randomIP")
                {
                    int randomLoginIPIndex = UnityEngine.Random.Range(0, elementXml.ChildNodes.Count);
                    XmlElement node = (XmlElement)elementXml.ChildNodes[randomLoginIPIndex];
                }
            }
        }
        else
        {
            Debug.Log("load cdnURL Main xml error " + www.error + " url, " + (Global.cdnURL + "Main" + AssetUtil.xmlSuffix));
        }
        
        currentProgressStep = ProgressStep.loadScene;
        async = SceneManager.LoadSceneAsync("Zombies");
        while (async.isDone == false)
        {
            fillAmount = async.progress;
            yield return null;
        }
        fillAmount = 1.0f;
   
        if (Global.IsDebug)
        {
            Global.isShowFlag = true;
            Global.IsShowMicphone = true;
        }

        gameObject.AddComponent<DownloadManager>();

        currentProgressStep = ProgressStep.parseConfig;
        ConfigManager.Instance.Setup();
        TimeManager.Setup();
        AudioSourceManager.Instance.Setup();
    }

    private void OnQuitGame()
    {
        Application.Quit();
    }

	void Update ()
    {
        if (isInit)
        {
            if (_updateFrameList != null)
            {
                _updateFrameList.Invoke();
            }
            if (_nextFrameCall != null)
            {
                Delegate[] list = _nextFrameCall.GetInvocationList();
                foreach (System.Action function in list)
                {
                    _nextFrameCall -= function;
                    function.Invoke();
                }
            }
        }
        else
        {
            if (currentProgressStep == ProgressStep.loadScene)
            {
                LoadingBarPanel.Instance.UpdateTitleBy(fillAmount, currentProgressStep);
            }
            else
            {
                LoadingBarPanel.Instance.UpdateTitleBy(ConfigManager.Instance.Progress, ConfigManager.Instance.ProgressStep);
                if (ConfigManager.Instance.Progress >= 1f &&
                    ConfigManager.Instance.isDownloadManagerReady)
                {
                    isInit = true;
                    gameObject.AddComponent<FightManager>();
                    FightManager.OnModuleShowOrHideHandler += onModuleShowOrHideHandler;
                    FightManager.StartMainMenuScreen(0f);
                }
            }
        }
	}

    private void onModuleShowOrHideHandler(bool isShow, UIType type)
    {
        if(type == UIType.MainMenuScreen)
        {
            FightManager.OnModuleShowOrHideHandler -= onModuleShowOrHideHandler;
            if (isShow)
            {
                LoadingBarPanel.Instance.Hide();
            }
        }
    }

    void LateUpdate()
    {
        if (_lateUpdateList != null)
        {
            _lateUpdateList.Invoke();
        }
    }

    private string GetDeviceInfo()
    {
        string dev = "";
        return dev;
    }
}