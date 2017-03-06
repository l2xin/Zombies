using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogNotice : BaseUI
{
	private GameObject TRoot;
	private Text TitleName;
	private Text BtnName;

	private GameObject PICTURE;
	private GameObject WORDS;
	private Image PictureBoard;
	private Text WordsBoard;

	List<BillBoardConf> Notice = new List<BillBoardConf>();
	private BillBoardConf CurrentNotice;
	bool visible = false;
	bool Action = false;
	void Awake()
	{
        TRoot = gameObject;
        TitleName = transform.Find("BG1/TitleWord").gameObject.GetComponent<Text>();
        BtnName = transform.Find("BG1/Button-OK/Text").gameObject.GetComponent<Text>();
        Button btn = transform.Find("BG1/Button-OK").gameObject.GetComponent<Button>();
        btn.onClick.AddListener(OK);
        PICTURE = transform.Find("BG1/BG2/PictureBoard").gameObject;
        WORDS = transform.Find("BG1/BG2/WordsBoard").gameObject;
        PictureBoard = transform.Find("BG1/BG2/PictureBoard/Viewport/Picture").gameObject.GetComponent<Image>();
        WordsBoard = transform.Find("BG1/BG2/WordsBoard/Viewport/Words").gameObject.GetComponent<Text>();
        this.Open ();
        isNeedCache = false;

        // *** skip,  第一版没有此界面资源
        this.gameObject.SetActive(false);

    }

	void LateUpdate()
	{
		if(!visible){return;}
		if(transform.localScale.y != 1)
		{
			transform.localScale = Vector3.SmoothDamp (transform.localScale, Vector3.one, ref V, 0.2f);
		}
		if(Action){return;}
		if (ShowIndex < Notice.Count) 
		{
			Show (Notice [ShowIndex]);
		}
		else
		{
			Close ();
		}

	}
	Vector3 V = Vector3.zero;
	int ShowIndex = 0;
	public void Open()
	{
		ShowIndex = 0;
		Notice = ConfigManager.Instance.billBoardConf;
		TRoot.SetActive (true);
		visible = true;

		this.OK ();
	}

	public void Close()
	{
		visible = false;
		TRoot.SetActive (false);
	}

	public void OK()
	{		
		transform.localScale = Vector3.one * 0.8f;
		Action = false;
		WORDS.SetActive (false);
		PICTURE.SetActive (false);
	}
	public enum NoticeType
	{
		Words = 1,
		Picture = 2,
	}
	public void Show(BillBoardConf data)
	{
//		Debug.Log ("-------------------------------------------------------->"+(NoticeType)data.boardType);
		// 1.文字 2.图片
		switch ((NoticeType)data.boardType) 
		{
		case NoticeType.Words:
			
			WordsBoard.supportRichText = true;
			WordsBoard.text = CheckSpecWords (data.words);
//			WordsBoard.text = "<color=#ff00ff>测试文字</color>";
			WORDS.SetActive (true);
			break;
			case NoticeType.Picture:
			PICTURE.SetActive (true);
			break;
		}
		TitleName.text = data.titleName;
		BtnName.text = data.buttonName;
		Action = true;
		ShowIndex++;
	}
	public string CheckSpecWords(string words)
	{
		words = words.Replace ("[#","<color=#");
		words = words.Replace ("[-","</color");
		words = words.Replace (']','>');
		return words;
	}
}
