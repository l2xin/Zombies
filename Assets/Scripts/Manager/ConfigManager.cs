using System;
using System.Xml;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
/// <summary>
///  nashnie
///  预加载解析配置以及一些UI资源
/// </summary>
public class ConfigManager : SingletonInstance<ConfigManager>
{
    private const string configPath = "Conf/";
    private const string xmlPath = "Xml/";
    public List<RandomName> randomNameList;
    public List<RandomName> randomFaceNameList;

    public List<CountryConf> countryConfList;
    public List<BuffConf> buffConfList;
    private Dictionary<int, string> cityMap;
	private Dictionary<string, Vector3> GameMap;
    public List<BillBoardConf> billBoardConf;

    private float currentLoadedConf = 0;
    private float totalLoadedConf = 0;

    private float currentLoadedDepend = 0;
    private float totalLoadedDepend = 0;

    private float currentLoadedDepend2 = 0;
    private float totalLoadedDepend2 = 0;

    private float progress = 0f;
    public ShaderList shaderList;
    public GlobalInfo globalConfig;
    public RuntimeAnimatorController runtimeAnimatorController;
    public GameObject joystick;
    public Dictionary<int, string> countryFlagConfMap;
    public List<ShopItemConf> shopItemConfList;
    private List<RankConf> rankConfList;
    private ProgressStep currentProgressStep = ProgressStep.parseConfig;
    public bool isDownloadManagerReady = false;

    public List<MapGridConf> mapGridConfList;
    public List<SkillConf> skillList;
    private static readonly int serverMapGridWidth = 100;
    private static readonly int serverMapWidth = 4000;

    public float Progress
    {
        get
        {
            if (totalLoadedConf <= 0)
            {
                return 0f;
            }
            return (currentLoadedConf + currentLoadedDepend + currentLoadedDepend2) / (totalLoadedConf + totalLoadedDepend + totalLoadedDepend2);
        }
    }

    public ProgressStep ProgressStep
    {
        get
        {
            return currentProgressStep;
        }
    }

    public void Setup()
    {
        ParseXml("dirtywordconfig", ParseDirtyWordXml);

        currentProgressStep = ProgressStep.parseConfig;
    }

    private void LoadDownloadManagerConf()
    {
        MainEntry.Instance.StartCoroutine(ParseDownloadManagerConf());
    }

    private IEnumerator ParseDownloadManagerConf()
    {
        totalLoadedConf++;
        isDownloadManagerReady = false;
        do
        {
            isDownloadManagerReady = DownloadManager.Instance.ConfigLoaded;
            yield return null;

        } while (isDownloadManagerReady == false);
        currentLoadedConf++;
        LoadShaders();
    }

    private void LoadGlobalConfig()
    {
        List<string> dependlist = DownloadManager.Instance.getDependList("globalconfig.asset");
        totalLoadedDepend = dependlist.Count;
        MainEntry.Instance.StartLoadAssetBundle("globalconfig", AssetType.asset, onLoadGlobalConfigComplete);
        SetInterval.Start(CheckUfeconfigDepend, 0.05f);
        totalLoadedConf++;

        dependlist = DownloadManager.Instance.getDependList("commoneffect.asset");
        totalLoadedDepend2 = dependlist.Count;
    }

    private void CheckUfeconfigDepend()
    {
        currentLoadedDepend = DownloadManager.Instance.getBundleDependenciesReadyCount("ufe_config.asset");
    }

    private void CheckCommonEffectDepend()
    {
        currentLoadedDepend2 = DownloadManager.Instance.getBundleDependenciesReadyCount("commoneffect.asset");
    }

    private void LoadShaders()
    {
        totalLoadedConf++;

        currentProgressStep = ProgressStep.unzipRes;
        MainEntry.Instance.StartLoad("shaderlist", AssetType.prefab, onLoadShadersComplete);
    }

    private void onLoadShadersComplete(GameObject arg1, string arg2)
    {
        currentLoadedConf++;
        shaderList = arg1.GetComponent<ShaderList>();
        Shader.WarmupAllShaders();
        LoadGlobalConfig();
    }

    private void onLoadGlobalConfigComplete(AssetBundle arg1, string arg2)
    {
        currentLoadedConf++;
        currentLoadedDepend = totalLoadedDepend;
        SetInterval.Clear(CheckUfeconfigDepend);
        UnityEngine.Object res = arg1.LoadAsset("globalconfig");
        globalConfig = GameObject.Instantiate(res) as GlobalInfo;

        LoadCommonEffect();
    }

    private void LoadCommonEffect()
    {
        MainEntry.Instance.StartLoadAssetBundle("commoneffect", AssetType.asset, onLoadCommonEffectComplete);
        SetInterval.Start(CheckCommonEffectDepend, 0.05f);
        totalLoadedConf++;
    }

    private void onLoadCommonEffectComplete(AssetBundle arg1, string arg2)
    {
        currentLoadedConf++;
        currentLoadedDepend2 = totalLoadedDepend2;
        SetInterval.Clear(CheckCommonEffectDepend);
        UnityEngine.Object res = arg1.LoadAsset("commoneffect");
        GlobalInfo commoneffect = GameObject.Instantiate(res) as GlobalInfo;
        globalConfig.roundOptions = commoneffect.roundOptions;
        globalConfig.hitOptions = commoneffect.hitOptions;

        PreloadJoystick();
    }

    private void PreloadJoystick()
    {
        totalLoadedConf++;
        MainEntry.Instance.StartLoad("hudplayercontroller", AssetType.prefab, onLoadJoystick);
    }

    private void onLoadJoystick(GameObject uiPrefabObj, string nameTag)
    {
        currentLoadedConf++;
        joystick = uiPrefabObj;
    }

    private void ParseDirtyWordXml(string obj)
    {
        ParseDirtyWordConfig(obj);
    }

    private const int defaultRandomNameIndexGap = 1000;
    public List<object> GetRandomName()
    {
        bool isFaceName = UnityEngine.Random.Range(0, 1f) > 0.5f;
        List<object> randomNameList = new List<object>();
        int index = 0;
        RandomName randomName1 = null;
        RandomName randomName2 = null;

        if (ConfigManager.Instance.randomFaceNameList != null)
        {
            if (isFaceName)
            {
                index = UnityEngine.Random.Range(0, ConfigManager.Instance.randomFaceNameList.Count);
                randomName1 = ConfigManager.Instance.randomFaceNameList[index];
                randomNameList.Add(index);
                randomNameList.Add(defaultRandomNameIndexGap);
            }
            else
            {
                index = UnityEngine.Random.Range(0, ConfigManager.Instance.randomNameList.Count);
                randomName1 = ConfigManager.Instance.randomNameList[index];
                randomNameList.Add(index);
                index = UnityEngine.Random.Range(0, ConfigManager.Instance.randomNameList.Count);
                randomName2 = ConfigManager.Instance.randomNameList[index];
                randomNameList.Add(index);
            }
            string name = "";
            if (randomName1.isFaceName)
            {
                name = randomName1.firstName;
            }
            else
            {
                name = randomName1.firstName + randomName2.secondName;
            }
            randomNameList.Add(name);
        }
        else
        {
            randomNameList.Add("");
        }
        return randomNameList;
    }

    public string GetRandomName(int randomIndex, int randomIndex2)
    {
        if (FightManager.gameMode == GameMode.VersusMode || FightManager.gameMode == GameMode.TrainingRoom)
        {
            string name = "";
            RandomName randomName1 = null;
            RandomName randomName2 = null;
            if (randomIndex2 != defaultRandomNameIndexGap)
            {
                randomName1 = ConfigManager.Instance.randomNameList[randomIndex];
                randomName2 = ConfigManager.Instance.randomNameList[randomIndex2];
                name = randomName1.firstName + randomName2.secondName;
            }
            else
            {
                randomName1 = ConfigManager.Instance.randomFaceNameList[randomIndex];
                name = randomName1.firstName;
            }
            return name;
        }
        else
        {
            List<object> randomNameList = GetRandomName();
            return (string)randomNameList[randomNameList.Count - 1];
        }
    }


    private void ParseBuffConf(byte[] bytes)
    {
        buffConfList = new List<BuffConf>();
        int len = System.BitConverter.ToInt32(bytes, 0);
        BinayUtil binayUtil = new BinayUtil(bytes);
        for (int i = 0; i < len; i++)
        {
            BuffConf buffConf = new BuffConf();
            buffConf.Read(bytes, binayUtil);
            if (buffConf.id > 0)
            {
                buffConfList.Add(buffConf);
            }
        }
    }

    public BuffConf GetBuffConf(int id)
    {
        for (int i = 0; i < buffConfList.Count; i++)
        {
            BuffConf buffConf = buffConfList[i];
            if (buffConf.id == id)
            {
                return buffConf;
            }
        }
        return null;
    }

    public string GetCountry(int id)
    {
        for (int i = 0; i < countryConfList.Count; i++)
        {
            CountryConf countryConf = countryConfList[i];
            if (countryConf.id == id)
            {
                return countryConf.country;
            }
        }
        return null;
    }

    private void ParseXml(string name, Action<string> parseXml)
    {
        totalLoadedConf++;
        MainEntry.Instance.StartLoadXml(name, (string text, string tag) =>
        {
            parseXml(text);
            currentLoadedConf++;
            if(currentLoadedConf >= totalLoadedConf)
            {
                LoadDownloadManagerConf();
            }
        });
    }

    private void ParseRandomNameConf(byte[] bytes)
    {
        randomNameList = new List<RandomName>();
        randomFaceNameList = new List<RandomName>();
        int len = System.BitConverter.ToInt32(bytes, 0);
        BinayUtil binayUtil = new BinayUtil(bytes);
        for (int i = 0; i < len; i++)
        {
            RandomName conf = new RandomName();
            conf.Read(bytes, binayUtil);
            if(conf.isFaceName)
            {
                randomFaceNameList.Add(conf);
            }
            else
            {
                randomNameList.Add(conf);
            }
        }
    }

    private DirtyWordConfig _dirtyWordConfig;

    public DirtyWordConfig DirtyWordConfig
    {
        get
        {
            return _dirtyWordConfig;
        }
    }

    public void ParseDirtyWordConfig(string text)
    {
        _dirtyWordConfig = new DirtyWordConfig();
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(text);
        XmlNode xmlnode = xmlDoc.SelectSingleNode("Config");
        DirtyWordConfig.Parse(xmlnode);
    }

}