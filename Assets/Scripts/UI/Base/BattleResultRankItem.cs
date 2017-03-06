using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BattleResultRankItem : MonoBehaviour
{
    [HideInInspector]
    public Transform rankIcon;
    [HideInInspector]
    public GameObject CampBg1;
    [HideInInspector]
    public GameObject CampBg2;
    [HideInInspector]
    public GameObject NormalBg;
    [HideInInspector]
    public Text rankTxt;
    [HideInInspector]
    public Text nameTxt;
    [HideInInspector]
    public Text countryTxt;
    [HideInInspector]
    public Image countryFlag;
    [HideInInspector]
    public Text coinTxt;
    [HideInInspector]
    public Text totalKillTxt;
    [HideInInspector]
    public Text comboKillTxt;
    [HideInInspector]
    public Button followBtn;
    [HideInInspector]
    public GameObject SelfIcon;
    [HideInInspector]
    public UIPresentTool AlphaTool;

    void Awake()
    {
        rankIcon = transform.FindChild("TRoot/RankIcon");

        rankTxt = transform.FindChild("TRoot/RankIcon/Icon4/Num").GetComponent<Text>();
        nameTxt = transform.FindChild("TRoot/NameTxt").GetComponent<Text>();
        countryTxt = transform.FindChild("TRoot/CountryTxt").GetComponent<Text>();
        countryFlag = transform.FindChild("TRoot/CountryFlag").GetComponent<Image>(); 
        coinTxt = transform.FindChild("TRoot/CoinTxt").GetComponent<Text>();
        totalKillTxt = transform.FindChild("TRoot/TotalKillTxt").GetComponent<Text>();
        comboKillTxt = transform.FindChild("TRoot/ComboKillTxt").GetComponent<Text>();
        followBtn = transform.FindChild("TRoot/FollowBtn").GetComponent<Button>();
        SelfIcon = transform.FindChild("TRoot/SelfIcon").gameObject;
        CampBg1 = transform.FindChild("TRoot/BlueBg").gameObject;
        CampBg2 = transform.FindChild("TRoot/RedBg").gameObject;
        NormalBg = transform.FindChild("TRoot/NormalBg").gameObject;
    }
}