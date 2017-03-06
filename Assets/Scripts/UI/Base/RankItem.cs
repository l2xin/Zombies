using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;

public class RankItem : MonoBehaviour
{
    [HideInInspector]
    public GameObject Icon1;
    [HideInInspector]
    public GameObject Icon2;
    [HideInInspector]
    public GameObject Icon3;
    [HideInInspector]
    public Text Nums;
    [HideInInspector]
    public Text PlayerName;
    [HideInInspector]
    public Text RankScore;
    [HideInInspector]
    public Image CountryFlag;
    private Transform selfBg;
    private Transform normalBg;
    public bool isActive = false;
    private ulong id;
    private int rankIndex = -1;
    private int num = -1;
    private int countryId = -1;
    public UFECamp camp;

    private readonly Color myColor = new Color(0, 255f / 255f, 0);
    private readonly Color selfColor = new Color(32f / 255f, 179f / 255f, 255f / 255f, 255f / 255f);
    private readonly Color opColor = new Color(253f / 255f, 105f / 255f, 122f / 255f, 255f / 255f);

    public ulong Id
    {
        get
        {
            return id;
        }

        set
        {
            if (id != value)
            {
                id = value;
                bool isHero = id == LoginManager.playerId;
                if(isHero)
                {
                    if(selfBg != null)
                    {
                        selfBg.gameObject.SetActive(true);
                    }
                    if(normalBg != null)
                    {
                        normalBg.gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (selfBg != null)
                    {
                        selfBg.gameObject.SetActive(false);
                    }
                    if (normalBg != null)
                    {
                        normalBg.gameObject.SetActive(true);
                    }
                }
                Color color = myColor;
                if (camp == UFECamp.Camp1)
                {
                    color = selfColor;
                }
                else
                {
                    color = opColor;
                }
                string characterName = FightManager.GetPlayerShortName(id);
                switch (camp)
                {
                    case UFECamp.Camp1:
                    case UFECamp.Camp2:
                        PlayerName.text = characterName;
                        PlayerName.color = color;
                        RankScore.color = color;
                        Nums.color = color;
                        gameObject.SetActive(true);
                        break;
                    default:
                        gameObject.SetActive(false);
                        break;
                }

            }
        }
    }

    public int RankIndex
    {
        get
        {
            return rankIndex;
        }
        set
        {
            if (rankIndex != value)
            {
                rankIndex = value;
                UpdateRankItem(rankIndex);
                switch (camp)
                {
                    case UFECamp.Camp1:
                    case UFECamp.Camp2:
                        Nums.text = rankIndex.ToString();
                        gameObject.SetActive(true);
                        break;
                    default:
                        gameObject.SetActive(false);
                        break;
                }

            }
        }
    }

    public int Num
    {
        get
        {
            return num;
        }

        set
        {
            if (num != value)
            {
                num = value;
                switch (camp)
                {
                    case UFECamp.Camp1:
                    case UFECamp.Camp2:
                        if(RankScore != null)
                        {
                            RankScore.text = num.ToString();
                        }
                        break;
                    default:
                        gameObject.SetActive(false);
                        break;
                }

            }
        }
    }

    public int CountryId
    {
        get
        {
            return countryId;
        }

        set
        {
            if(countryId != value)
            {
                countryId = value;
                if(countryId > 0 && Global.isShowFlag)
                {
                    CountryFlag.sprite = GeneralUtils.GetCountryFlag(countryId);
                    CountryFlag.gameObject.SetActive(true);
                }
                else
                {
                    CountryFlag.gameObject.SetActive(false);
                }
            }
        }
    }

    private void UpdateRankItem(int rankHeroIndex)
    {
        switch (rankHeroIndex)
        {
            case 1:
                Icon1.SetActive(true);
                Icon2.SetActive(false);
                Icon3.SetActive(false);
                Nums.gameObject.SetActive(false);
                break;
            case 2:
                Icon1.SetActive(false);
                Icon2.SetActive(true);
                Icon3.SetActive(false);
                Nums.gameObject.SetActive(false);
                break;
            case 3:
                Icon1.SetActive(false);
                Icon2.SetActive(false);
                Icon3.SetActive(true);
                Nums.gameObject.SetActive(false);
                break;
            default:
                Icon1.SetActive(false);
                Icon2.SetActive(false);
                Icon3.SetActive(false);
                Nums.gameObject.SetActive(true);
                Nums.text = "<color=#00FF00FF>" + (rankHeroIndex + 1) + "</color>";
                break;
        }
    }

    void Awake()
    {
        Icon1 = transform.Find("RankIcon/Icon1").gameObject;
        Icon2 = transform.Find("RankIcon/Icon2").gameObject;
        Icon3 = transform.Find("RankIcon/Icon3").gameObject;
        Nums = transform.Find("RankIcon/Num").GetComponent<Text>();
        PlayerName = transform.Find("PlayerName").GetComponent<Text>();
        RankScore = transform.Find("RankScore").GetComponent<Text>();
        CountryFlag = transform.Find("Flag").GetComponent<Image>();
        selfBg = transform.FindChild("Self");
        normalBg = transform.FindChild("Normal");
    }
}
