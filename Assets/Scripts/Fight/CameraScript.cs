using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// CameraScript
/// </summary>
public class CameraScript : MonoBehaviour
{
    public ARPGFollowCameraController arpgFollowCameraController;
    float CurrentScale = 1;
    float TargetScale = 1;
    public float ScaleFactor = 0.6f;
    float v = 0;

    void Awake()
    {
        arpgFollowCameraController = Camera.main.gameObject.AddComponent<ARPGFollowCameraController>();
        arpgFollowCameraController.startingDistance = 7f;
        arpgFollowCameraController.maxDistance = 40f;
        arpgFollowCameraController.targetHeight = 0.5f;
        arpgFollowCameraController.camXAngle = 58;
        Camera.main.fieldOfView = 55f;
    }

    void LateUpdate()
	{
		if(CurrentScale != TargetScale)
		{
			CurrentScale = Mathf.SmoothDamp (CurrentScale, TargetScale, ref v, 0.4f);
			arpgFollowCameraController.startingDistance = 18 * CurrentScale;
		}
    }

    public void SetTarget(Transform target)
    {
        arpgFollowCameraController.SetTarget(target);
        arpgFollowCameraController.LateUpdate();
    }
	
	public void SetDistanceScale(float targetScale)
	{
		TargetScale = targetScale * ScaleFactor;
		if(TargetScale < 1)
		{
			TargetScale = 1;
		}
	}
}
