using UnityEngine;

public class DialogManager : HxSingleMono<DialogManager>
{
	private DialogNotice mNotice;
	private PlayerGuide mGuide;
	public Camera MyCamera;
    private bool isShow = false;

    public void ShowGuideOrNotice()
    {
        int isFirstGame = PlayerPrefs.GetInt("FirstGame");
        if (isFirstGame == 0)
        {
            if(mGuide != null)
            {
                mGuide.Open();
            }
        }
        else
        {
            if(mNotice != null)
            {
                mNotice.Open();
            }
        }
        isShow = true;
    }

    public void HideGuideOrNotice()
    {
        if(isShow)
        {
            isShow = false;
            if (mGuide != null)
            {
                mGuide.Close();
            }
            if (mNotice != null)
            {
                mNotice.Close();
            }
        }
    }

	private void Add(GameObject target)
	{
		if(target == null){Debug.LogError (target+" is nor exist !!!");return;}
		target.transform.SetParent(UIManager.canvas != null ? UIManager.canvas.transform : null,false);
	}

	private void Add(GameObject target,Canvas parent)
	{
		if(target == null){Debug.LogError (target+" is nor exist !!!");return;}
		target.transform.parent = parent.transform;
	}
}
