using UnityEngine;
using System.Collections;
using DG.Tweening;

public class TestTween : MonoBehaviour
{
    private RectTransform starRect;
    private GameObject star;

    // Use this for initialization
    void Start () {
        star = this.gameObject;
        starRect = transform.GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            LevelUp();
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            LevelDown();
        }
    }


    public void LevelUp()
    {
        starRect.localScale = new Vector2(0.3f, 0.3f);
        starRect.DOScale(1.0f, 0.3f).SetEase(Ease.OutBounce);
        star.SetActive(true);
    }

    public void LevelDown()
    {
        starRect.localScale = new Vector2(1f, 1f);
        starRect.DOScale(0f, 0.1f).SetEase(Ease.InBounce);
        star.SetActive(true);
    }
}
