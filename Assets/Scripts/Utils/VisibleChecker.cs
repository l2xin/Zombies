using UnityEngine;
using System.Collections;

public class VisibleChecker : MonoBehaviour
{
    public bool isVisible = false;

    void OnBecameVisible()
    {
        isVisible = true;
    }

    void OnBecameInvisible()
    {
        isVisible = false;
    }
}
