using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// BloodBar
/// </summary>

public class BloodBar
{
    private GameObject bloodBar;
    public GameObject bloodBarChild;
    private GameObject bloodBarContainer;
    private GameObject bloodBarOfBar;
    private GameObject bloodBarBg;

    private RectTransform bloodBarOfBarRect;
    private RectTransform bloodBarOfBgRect;

    private RectTransform bloodBarRect;
    private Canvas bloodBarCanvas;
    private GameObject arrow;
    private RectTransform bossBlood;
    public Text nameTxt;
    private Quaternion directionArrowRot = Quaternion.identity;
    private const float directionArrowDis = 3;

    private Image countryFlag;

    private const int defaultBloodTileWidth = 30;
    private readonly int maxBloodTileCount = 5;
    private int singleBloodGird;
    private float bloodViewScale;
    private int defaultMaxBloodWidth;
    private Vector2 bloodSizeDelta = Vector2.one;
    private Vector3 bloodBarLocalScale = Vector3.one;

    public bool isHero = false;
    public CharacterInfo myInfo;
    public ulong id;
    private string characterName;
    private Vector2 pointInRectangle = Vector2.zero;
    private Vector3 pointInRectangleOf = Vector3.zero;

    private Vector3 exOffsetXLocalPosition = Vector3.zero;
    private Vector2 normalOffsetXLocalPosition = Vector3.zero;

    private Vector3 directionArrowOutOfScreenPos = new Vector3(5000, 5000, 5000);

    public VisibleChecker visibleChecker;

    public Transform hero;

    public Vector3 offsetY = Vector3.zero;

    public void InitHud()
    {
        singleBloodGird = myInfo.lifePoints / maxBloodTileCount;
        bloodViewScale = (float)myInfo.lifePoints / (defaultBloodTileWidth * maxBloodTileCount);
        bloodBar = PoolUtil.SpawnerGameObject(FightManager.config.roundOptions.bloodBar, PoolUtil.guiPoolName);
        bloodBar.transform.SetParent(UIManager.bloodBarParent.transform);
        bloodBar.transform.localScale = Vector3.one * 0.75f;
        bloodBar.SetActive(true);
        bloodBarCanvas = UIManager.bloodBarParent.GetComponent<Canvas>();
        bloodBarCanvas.worldCamera = UIManager.UICamera;

        bloodBarRect = bloodBar.GetComponent<RectTransform>();

        bloodBarChild = bloodBar.transform.Find("BloodBar").gameObject;
        bloodBarChild.SetActive(true);
        bloodBarContainer = bloodBarChild.transform.Find("Blood").gameObject;

        bloodBarBg = bloodBarContainer.transform.Find("BloodBg").gameObject;
        bloodBarOfBar = bloodBarContainer.transform.Find("Blood").gameObject;

        countryFlag = bloodBarChild.transform.Find("Flag").GetComponent<Image>();
        nameTxt = bloodBarChild.transform.Find("Name").GetComponent<Text>();

        bloodBarOfBarRect = bloodBarOfBar.GetComponent<RectTransform>();
        bloodBarOfBgRect = bloodBarBg.GetComponent<RectTransform>();
        nameTxt.text = FightManager.playerModelMap[id].name;

		if (isHero) {
			nameTxt.color = FightManager.config.myColor;
		} 
		else if(myInfo.camp == UFECamp.Camp1){
			nameTxt.color = FightManager.config.selfColor;
		}
		else
		{
			nameTxt.color = FightManager.config.opColor;
		}

        if (Global.isShowFlag && myInfo.countryId > 0)
        {
            countryFlag.gameObject.SetActive(true);
            countryFlag.sprite = GeneralUtils.GetCountryFlag(myInfo.countryId);
        }
        else
        {
            countryFlag.gameObject.SetActive(false);
        }
        myInfo.characterName = nameTxt.text;
        characterName = myInfo.characterName;
       
    }

    public void UpdateHUD()
    {
        if (bloodBar != null)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, hero.position + offsetY);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(bloodBarRect, screenPoint, bloodBarCanvas.worldCamera, out pointInRectangle);
            pointInRectangleOf.x = pointInRectangle.x;
            pointInRectangleOf.y = pointInRectangle.y;
            pointInRectangleOf.z = 0;
            bloodBarChild.transform.localPosition = pointInRectangleOf;

            if (visibleChecker.isVisible)
            {
                float bloodSizeDeltaX = maxBloodTileCount * defaultBloodTileWidth;
                bloodSizeDelta.x = bloodSizeDeltaX > 0 ? bloodSizeDeltaX + 1 : 0;
                bloodSizeDelta.y = bloodBarOfBgRect.sizeDelta.y;
                bloodBarOfBgRect.sizeDelta = bloodSizeDelta;

                bloodSizeDeltaX = myInfo.currentLifePoints / bloodViewScale;
                bloodSizeDelta.x = bloodSizeDeltaX > 0 ? bloodSizeDeltaX + 1 : 0;
                bloodSizeDelta.y = bloodBarOfBgRect.sizeDelta.y;
                bloodBarOfBarRect.sizeDelta = bloodSizeDelta;

                Vector3 bloodBarOfBarRectPos = Vector3.zero;
                bloodBarOfBarRectPos.x = -(bloodBarOfBgRect.sizeDelta.x - bloodBarOfBarRect.sizeDelta.x) / 2;
                bloodBarOfBarRect.transform.localPosition = bloodBarOfBarRectPos;
            }
        }
    }

    public void DespawnerBloodBar()
    {
        if (bloodBar != null)
        {
            PoolUtil.Despawner(bloodBar, PoolUtil.guiPoolName);
            bloodBar = null;
        }
    }
}
