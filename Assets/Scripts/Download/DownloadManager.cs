using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Uri = System.Uri;
using System;


public class DownloadManager : MonoBehaviour
{
    private List<string> assetBundleDestroyList = new List<string>();
    private Dictionary<string, DelayDestroyAssetBundle> assetBundleDestroyDic = new Dictionary<string, DelayDestroyAssetBundle>();

    public string GetError(string url)
    {
        string name = url;
        if (!ConfigLoaded)
        {
            return null;
        }

        url = formatUrl(url);
        if (failedRequest.ContainsKey(url))
        {
            return failedRequest[url].error;
        }
        else
        {
            return null;
        }
    }

    public bool CheckResInBundleDict(string name)
    {
        return true;
    }

    /**
	 * Test if the url is already requested.
	 */
    public bool IsUrlRequested(string url)
    {
        if (!ConfigLoaded)
        {
            return isInBeforeInitList(url);
        }
        else
        {
            url = formatUrl(url);
            bool isRequested = isInWaitingList(url) || processingRequest.ContainsKey(url) || succeedRequest.ContainsKey(url) || failedRequest.ContainsKey(url);
            return isRequested;
        }
    }

    /**
	 * Get WWW instance of the url.
	 * @return Return null if the WWW request haven't succeed.
	 */
    public AssetBundle GetWWW(string url)
    {
        if (!ConfigLoaded)
            return null;

        url = formatUrl(url);

        if (succeedRequest.ContainsKey(url))
        {
            WWWRequest request = succeedRequest[url];
            prepareDependBundles(stripBundleSuffix(request.requestString));
            //Debug.Log("GetWWW " + url);
            return request.assetBundle;
        }
        else
            return null;
    }

    public IEnumerator WaitDownload(string url)
    {
        yield return StartCoroutine(WaitDownload(url, -1));
    }

    public IEnumerator WaitDownload(string url, int priority)
    {
        while (!ConfigLoaded)
        {
            yield return null;
        }
        if(string.IsNullOrEmpty(url))
        {
            yield return null;
        }

        WWWRequest request = new WWWRequest();
        request.requestString = url;
        request.url = formatUrl(url);
        request.priority = priority;
        download(request);

        while (isDownloadingWWW(request.url))
            yield return null;
    }

    public void StartDownload(string url)
    {
        StartDownload(url, -1);
    }

    public void StartDownload(string url, int priority)
    {
        WWWRequest request = new WWWRequest();
        request.requestString = url;
        request.url = url;
        request.priority = priority;

        if (!ConfigLoaded)
        {
            if (!isInBeforeInitList(url))
                requestedBeforeInit.Add(request);
        }
        else
            download(request);
    }

    public void StopDownload(string url)
    {
        if (!ConfigLoaded)
        {
            requestedBeforeInit.RemoveAll(x => x.url == url);
        }
        else
        {
            url = formatUrl(url);
            waitingRequests.RemoveAll(x => x.url == url);
        }
    }

    public void DisposeWWW(string url)
    {
        StopDownload(url);
        if (succeedRequest.ContainsKey(url))
        {
            WWWRequest www = succeedRequest[url];
            if (www.CheckDispose())
            {
                succeedRequest.Remove(url);
            }
        }
        if (failedRequest.ContainsKey(url))
        {
            failedRequest[url].Dispose();
            failedRequest.Remove(url);
        }
    }

    //RemoveRef
    public void TryDisposeDependBundle(string url)
    {
        string bundleName = url;
        List<string> dependencies = getDependList(bundleName);
        if (dependencies != null)
        {
            foreach (string dependBundle in dependencies)
            {
                string dependUrl = dependBundle;
                if (succeedRequest.ContainsKey(dependUrl))
                {
                    succeedRequest[dependUrl].RemoveRef();
                    if (succeedRequest[dependUrl].CheckDispose())
                    {
                        succeedRequest.Remove(dependUrl);
                    }
                }
            }
        }
    }

    public AssetBundle TryGetBundle(string bundleName)
    {
        string bundleUrl = bundleName;
        if (succeedRequest.ContainsKey(bundleUrl))
        {
            List<string> dependencies = getDependList(bundleName);
            if (dependencies != null)
            {
                foreach (string dependBundle in dependencies)
                {
                    string dependUrl = dependBundle;
                    if (succeedRequest.ContainsKey(dependUrl))
                    {
                        succeedRequest[dependUrl].AddRef();
                    }
                }
            }
            return succeedRequest[bundleUrl].assetBundle;
        }
        return null;
    }

    /**
	 * This function will stop all request in processing.
	 */
    public void StopAll()
    {
        requestedBeforeInit.Clear();
        waitingRequests.Clear();

        foreach (WWWRequest request in processingRequest.Values)
            request.Dispose();
    }

    /**
	 * Check if the config files downloading finished.
	 */
    public bool ConfigLoaded
    {
        get
        {
            return _manifest != null;
        }
    }

    /**
	 * Get list of the built bundles. 
	 * Before use this, please make sure ConfigLoaded is true.
	 */
    public BundleData[] BuiltBundles
    {
        get
        {
            if (bundles == null)
                return null;
            else
                return bundles.ToArray();
        }
    }

    private AssetBundleManifest _manifest;

    void Awake()
    {
        instance = this;
        _manifest = null;
        initRootUrl();
        string platformManifest = platform.platformString;
        MainEntry.Instance.StartLoadNoDepend(platformManifest, AssetType.none, onLoadPlatformManifest);
    }

    private void onLoadPlatformManifest(AssetBundle ab, string name)
    {
        _manifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
    }

    List<string> newFinisheds = new List<string>();
    List<string> newFaileds = new List<string>();
    private readonly int downloadRetryTime = 3;
    private readonly int downloadThreadsCount = 3;

    void Update()
    {
        if (!ConfigLoaded)
        {
            return;
        }
        newFinisheds.Clear();
        newFaileds.Clear();
        for (int i = 0; i < processingRequestList.Count; i++)
        {
            string requestUrl = processingRequestList[i];
            WWWRequest request = processingRequest[requestUrl];
            if (request.error != null)
            {
                if (request.triedTimes - 1 < downloadRetryTime)
                {
                    request.CreatWWW();
                }
                else
                {
                    newFaileds.Add(request.url);
                    Debug.Log("Download " + request.url + " failed for " + request.triedTimes + " times.\nError: " + request.error);
                }
            }
            else if (request.isDone)
            {
                newFinisheds.Add(request.url);
            }
        }

        // Move complete bundles out of downloading list
        for (int i = 0; i < newFinisheds.Count; i++)
        {
            string finishedUrl = newFinisheds[i];
            succeedRequest.Add(finishedUrl, processingRequest[finishedUrl]);
            //var bundle = processingRequest[finishedUrl].www.assetBundle;
            processingRequest.Remove(finishedUrl);
            processingRequestList.Remove(finishedUrl);
        }

        // Move failed bundles out of downloading list
        for (int i = 0; i < newFaileds.Count; i++)
        {
            string finishedUrl = newFaileds[i];
            if (!failedRequest.ContainsKey(finishedUrl))
                failedRequest.Add(finishedUrl, processingRequest[finishedUrl]);
            processingRequest.Remove(finishedUrl);
            processingRequestList.Remove(finishedUrl);
        }
        // Start download new bundles
        int waitingIndex = 0;
        while (processingRequest.Count < downloadThreadsCount &&
               waitingIndex < waitingRequests.Count)
        {
            WWWRequest curRequest = waitingRequests[waitingIndex++];
            //bool canStartDownload = curRequest.bundleData == null || isBundleDependenciesReady( curRequest.name );
            bool canStartDownload = isBundleDependenciesReady(curRequest.url);
            if (canStartDownload)
            {
                waitingRequests.Remove(curRequest);
                curRequest.CreatWWW();
                processingRequest.Add(curRequest.url, curRequest);
                processingRequestList.Add(curRequest.url);
            }
        }
        if (assetBundleDestroyList.Count > 0)
        {
            string url = assetBundleDestroyList[0];
            DelayDestroyAssetBundle delayDestroyAssetBundle = assetBundleDestroyDic[url];
            AssetBundle ab = delayDestroyAssetBundle.ab;
            assetBundleDestroyList.RemoveAt(0);
            assetBundleDestroyDic.Remove(url);
            ab.Unload(false);
            ab = null;
            delayDestroyAssetBundle = null;
        }
    }

    bool isBundleDependenciesReady(string bundleName)
    {
        List<string> dependencies = getDependList(bundleName);
        foreach (string dependBundle in dependencies)
        {
            string url = dependBundle;
            if (!succeedRequest.ContainsKey(url))
                return false;
        }

        return true;
    }

    public int getBundleDependenciesReadyCount(string bundleName)
    {
        int sucessed = 0;
        List<string> dependencies = getDependList(bundleName);
        foreach (string dependBundle in dependencies)
        {
            string url = dependBundle;
            if (succeedRequest.ContainsKey(url))
            {
                sucessed++;
            }
        }
        return sucessed;
    }


    void prepareDependBundles(string bundleName)
    {
        List<string> dependencies = getDependList(bundleName);
        foreach (string dependBundle in dependencies)
        {
            string dependUrl = dependBundle;
            if (succeedRequest.ContainsKey(dependUrl))
            {
#pragma warning disable 0168
                var assetBundle = succeedRequest[dependUrl].assetBundle;
#pragma warning restore 0168
            }
        }
    }

    // This private method should be called after init
    void download(WWWRequest request)
    {
        request.url = formatUrl(request.url);
        string bundleName = stripBundleSuffix(request.requestString);
        if (isDownloadingWWW(request.url) || succeedRequest.ContainsKey(request.url))
        {
            List<string> dependlist = getDependList(bundleName);
            foreach (string bundle in dependlist)
            {
                WWWRequest dependRequest = GetWWWRequest(bundle);
                if (dependRequest != null)
                {
                    dependRequest.AddRef();
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogError(bundleName + " 依赖资源出现bug " + bundle);
#endif
                }
            }
            return;
        }

        if (isBundleUrl(request.url))
        {
            List<string> dependlist = getDependList(bundleName);
            foreach (string bundle in dependlist)
            {
                string bundleRequestStr = bundle;
                string bundleUrl = formatUrl(bundleRequestStr);
                if (!processingRequest.ContainsKey(bundleUrl) && !succeedRequest.ContainsKey(bundleUrl))
                {
                    WWWRequest dependRequest = new WWWRequest();
                    //dependRequest.bundleData = bundleDict[bundle];
                    //dependRequest.bundleBuildState = buildStatesDict[bundle];
                    dependRequest.requestString = bundleRequestStr;
                    dependRequest.url = bundleUrl;
                    addRequestToWaitingList(dependRequest);
                }
                WWWRequest dependRequestT = GetWWWRequest(bundle);
                dependRequestT.AddRef();
            }

            //request.bundleData = bundleDict[bundleName];
            //request.bundleBuildState = buildStatesDict[bundleName];
            if (request.priority == -1)
            {
                request.priority = 0;
            }
            addRequestToWaitingList(request);
        }
        else
        {
            if (request.priority == -1)
            {
                request.priority = 0;
            }
            addRequestToWaitingList(request);
        }
    }

    private WWWRequest GetWWWRequest(string bundle)
    {
        string bundleRequestStr = bundle;
        string bundleUrl = formatUrl(bundleRequestStr);

        if (processingRequest.ContainsKey(bundleUrl))
        {
            return processingRequest[bundleUrl];
        }
        else if (succeedRequest.ContainsKey(bundleUrl))
        {
            return succeedRequest[bundleUrl];
        }
        else
        {
            foreach (WWWRequest request in waitingRequests)
            {
                if (request.url == bundleUrl)
                {
                    return request;
                }
            }
        }
        return null;
    }

    bool isInWaitingList(string url)
    {
        foreach (WWWRequest request in waitingRequests)
        {
            if (request.url == url)
            {
                return true;
            }
        }
        return false;
    }

    void addRequestToWaitingList(WWWRequest request)
    {
        if (succeedRequest.ContainsKey(request.url) || isInWaitingList(request.url))
        {
            return;
        }
        if(string.IsNullOrEmpty(request.url))
        {
            return;
        }
        int insertPos = waitingRequests.FindIndex(x => x.priority < request.priority);
        insertPos = insertPos == -1 ? waitingRequests.Count : insertPos;
        waitingRequests.Insert(insertPos, request);
    }

    bool isDownloadingWWW(string url)
    {
        foreach (WWWRequest request in waitingRequests)
            if (request.url == url)
                return true;

        return processingRequest.ContainsKey(url);
    }

    bool isInBeforeInitList(string url)
    {
        foreach (WWWRequest request in requestedBeforeInit)
        {
            if (request.url == url)
                return true;
        }

        return false;
    }

    public List<string> getDependList(string bundle)
    {
        if (!ConfigLoaded)
        {
            Debug.LogError("getDependList() should be call after download manger inited");
            return null;
        }
        string[] allDependencies = _manifest.GetAllDependencies(bundle);
        List<string> res = new List<string>(allDependencies);
        return res;
    }

    BuildPlatform getRuntimePlatform()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            return BuildPlatform.Standalones;
        }
        else if (Application.platform == RuntimePlatform.OSXWebPlayer ||
                Application.platform == RuntimePlatform.WindowsWebPlayer)
        {
            return BuildPlatform.WebPlayer;
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            return BuildPlatform.IOS;
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            return BuildPlatform.Android;
        }
        else
        {
            return BuildPlatform.Standalones;
        }
    }

    void initRootUrl()
    {
        curPlatform = getRuntimePlatform();
    }

    string formatUrl(string urlstr)
    {
        Uri url;
        if (!isAbsoluteUrl(urlstr))
        {
            /*			url = new Uri(new Uri(downloadRootUrl + '/'), urlstr);*/
            return urlstr;
        }
        else
        {
            url = new Uri(urlstr);
        }
        return url.AbsoluteUri;
    }

    bool isAbsoluteUrl(string url)
    {
        Uri result;
        return Uri.TryCreate(url, System.UriKind.Absolute, out result);
    }

    bool isBundleUrl(string url)
    {
        //return string.Compare(Path.GetExtension(url), "." + bmConfiger.bundleSuffix, System.StringComparison.OrdinalIgnoreCase)  == 0;
        return true;
    }

    string stripBundleSuffix(string requestString)
    {
        //return requestString.Substring(0, requestString.Length - bmConfiger.bundleSuffix.Length - 1);
        return requestString;
    }

    // Members
    List<BundleData> bundles = null;

    string downloadRootUrl = null;
    BuildPlatform curPlatform;

    // Request members
    Dictionary<string, WWWRequest> processingRequest = new Dictionary<string, WWWRequest>();
    List<string> processingRequestList = new System.Collections.Generic.List<string>();
    Dictionary<string, WWWRequest> succeedRequest = new Dictionary<string, WWWRequest>();
    Dictionary<string, WWWRequest> failedRequest = new Dictionary<string, WWWRequest>();
    List<string> noResRequests = new List<string>();
    List<WWWRequest> waitingRequests = new List<WWWRequest>();
    List<WWWRequest> requestedBeforeInit = new List<WWWRequest>();

    static DownloadManager instance = null;
    static string manualUrl = "";

    /**
	 * Get instance of DownloadManager.
	 * This prop will create a GameObject named Downlaod Manager in scene when first time called.
	 */
    public static DownloadManager Instance
    {
        get
        {
            return instance;
        }
    }

    class WWWRequest
    {
        public string requestString = "";
        public string url = "";
        public int triedTimes = 0;
        public int priority = 0;
        public string name = "";
        public AssetBundle assetBundle = null;
        public byte[] bytes;
        public string txt;
        public string tag = "";
        public bool isDone = false;
        public string error = null;
        public bool isBytesLoad = false;
        public bool isTxtLoad = false;
        private string PathTag = "";
        private WWW www = null;
        private int refCount = 0;

        public void AddRef()
        {
            refCount++;
        }

        public void RemoveRef()
        {
            refCount--;
        }

        public bool CheckDispose()
        {
            if (refCount <= 0)
            {
                bool checkDestroyWWW = MainEntry.Instance.CheckDestroyWWW(url);
                if (checkDestroyWWW)
                {
                    Dispose();
                    return true;
                }
                else
                {
                    Main.RunInNextFrame(() =>
                    {
                        if (www != null)
                        {
                            www.Dispose();
                            www = null;
                        }
                    });
                    return false;
                }
            }
            else
            {
                Main.RunInNextFrame(() =>
                {
                    if (www != null)
                    {
                        www.Dispose();
                        www = null;
                    }
                });
            }
            return false;
        }

        public void CreatWWW()
        {
            name = url;
            triedTimes++;

            PathTag = platform.platformString + "/";
            string remoteUrl = System.IO.Path.Combine(ResmgrNative.Instance.cacheurl, PathTag + url);

            if (DownloadManager.instance.assetBundleDestroyDic.ContainsKey(url))
            {
                DelayDestroyAssetBundle delayDestroyAssetBundle = DownloadManager.instance.assetBundleDestroyDic[url];
                LoadAssetBundleFromCache(delayDestroyAssetBundle.ab, url);
                DownloadManager.instance.assetBundleDestroyDic.Remove(url);
                DownloadManager.instance.assetBundleDestroyList.Remove(url);
                SetTimeout.Clear(delayDestroyAssetBundle.function);
                delayDestroyAssetBundle = null;
            }
            else if (AssetUtil.CheckCacheFileExists(url))
            {
                CreateRemote();
            }
            else
            {
                CreateLocal();
            }
        }

        private void CreateLocal()
        {
            triedTimes++;
            ResmgrNative.Instance.LoadFromStreamingAssets(PathTag + url, "local", LoadFromStreamingAssets);
        }

        private void CreateRemote()
        {
            triedTimes++;
            if (isBytesLoad)
            {
                ResmgrNative.Instance.LoadBytesFromCache(PathTag + url, "remote", LoadAssetBundleFromCache);
            }
            else if (isTxtLoad)
            {
                ResmgrNative.Instance.LoadStringFromCache(PathTag + url, "remote", LoadAssetBundleFromCache);
            }
            else
            {
                ResmgrNative.Instance.LoadAssetBundleFromCache(PathTag + url, "remote", LoadAssetBundleFromCache);
            }
        }

        private void LoadAssetBundleFromCache(string txt, string tag)
        {
            this.txt = txt;
            this.tag = tag;
            isDone = true;
        }

        private void LoadAssetBundleFromCache(byte[] bytes, string tag)
        {
            this.bytes = bytes;
            this.tag = tag;
            isDone = true;
        }

        private void LoadAssetBundleFromCache(AssetBundle ab, string tag)
        {
            this.assetBundle = ab;
            this.tag = tag;
            isDone = true;
            Debug.Log("LoadAssetBundleFromCache " + tag);
        }

        private void LoadFromStreamingAssets(WWW www, string tag)
        {
            this.www = www;
            this.tag = tag;
            error = www.error;
            isDone = www.isDone;
            if (error == null && isDone)
            {
                if (error == null)
                {
                    if (isBytesLoad)
                    {
                        this.bytes = www.bytes;
                    }
                    else if (isTxtLoad)
                    {
                        this.txt = www.text;
                    }
                    else
                    {
                        this.assetBundle = www.assetBundle;
                    }
                }

            }
            else
            {
                Debug.Log("LoadFromStreamingAssets failed.WWW has not completed the download yet.");
            }
        }

        public void Dispose()
        {
            Main.RunInNextFrame(() =>
            {
                if (www != null)
                {
                    www.Dispose();
                    www = null;
                }
            });
            if (refCount <= 0)
            {
                if (this.assetBundle != null)
                {
                    DelayDestroyAssetBundle delayDestroyAssetBundle = new DownloadManager.DelayDestroyAssetBundle();
                    delayDestroyAssetBundle.ab = assetBundle;
                    delayDestroyAssetBundle.url = url;
                    delayDestroyAssetBundle.function = AddAssetBundleToList;
                    DownloadManager.instance.assetBundleDestroyDic.Add(url, delayDestroyAssetBundle);
                    SetTimeout.Start(AddAssetBundleToList, 1f);
                }
            }
        }

        private void AddAssetBundleToList()
        {
            if (DownloadManager.instance.assetBundleDestroyDic.ContainsKey(url))
            {
                DownloadManager.instance.assetBundleDestroyList.Add(url);
                this.assetBundle = null;
            }
        }

        public float progress
        {
            get;
            set;
        }
    }

    class DelayDestroyAssetBundle
    {
        public AssetBundle ab = null;
        public string url = "";
        public Action function = null;
    }
}