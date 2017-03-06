using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ARPG follow camera controller.
/// Created By: Juandre Swart 
/// Email: info@3dforge.co.za
/// 
/// A Script for a follow style camera that allows rotation on the x axis and to zoom.
/// It contains code that makes objects transparent if they are between the camera and target.
/// The objects material needs to be a transparent shader so we can change the alpha value.
/// </summary>
public class ARPGFollowCameraController : MonoBehaviour {

	public Transform target;
	public float startingDistance = 10f; // Distance the camera starts from target object.
	public float maxDistance = 20f; // Max distance the camera can be from target object.
	public float minDistance = 3f; // Min distance the camera can be from target object.
	public float zoomSpeed = 20f; // The speed the camera zooms in.
	public float targetHeight = 2.0f; // The amount from the target object pivot the camera should look at.
	public float camRotationSpeed = 70;// The speed at which  the camera rotates.
	public float rotationDamping = 3.0f; // How fast it should rotate to target angles.
	public float camXAngle = 15.0f; // The camera x euler angle.
	public bool fadeObjects = false; // Enable objects of a certain layer to be faded.
	public List<int> layersToTransparent = new List<int>();	// The layers where we will allow transparency.
	public float alpha = 0.3f; // The alpha value of the material when player behind object.
		
	private Transform myTransform;
	private Transform prevHit;
	private float minCameraAngle = 0.0f;
	private float maxCameraAngle = 90.0f;
    private Vector3 targetHeightVec = Vector3.zero;

    void Start ()
    {
        if (target != null)
        {
            myTransform = transform;
            myTransform.position = target.position;
            targetHeightVec.y = -1 * targetHeight;
        }
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
        myTransform = transform;
        myTransform.position = target.position;
    }

    public void LateUpdate()
    { 
		if(target != null)
		{
            // Calculate the current rotation angles
            float wantedRotationAngle = target.eulerAngles.y;
            float currentRotationAngle = myTransform.eulerAngles.y;
            // Damp the rotation around the y-axis
            currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);
            // Convert the angle into a rotation
            Quaternion currentRotation = Quaternion.Euler(camXAngle, currentRotationAngle, 0);

            // Position Camera.
            myTransform.position = target.position;
            myTransform.position -= currentRotation * Vector3.forward * startingDistance + targetHeightVec;

            if (FightManager.hero != null &&
               FightManager.hero.shakeCamera &&
               FightManager.hero.shakeDensity > 0)
            {
                float shakeDensity = .2f * FightManager.hero.shakeDensity;
                float rnd = UnityEngine.Random.Range(-shakeDensity, shakeDensity);
                myTransform.position += new Vector3(0, rnd, 0);
            }

            Vector3 targetToLookAt = target.position;
            targetToLookAt.y += targetHeight;
            myTransform.LookAt(targetToLookAt);

            //Start checking if object between camera and target
            if (fadeObjects)
            {
                // Cast ray from camera.position to target.position and check if the specified layers are between them.
                Ray ray = new Ray(myTransform.position, (target.position - myTransform.position).normalized);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, maxDistance))
                {
                    Transform objectHit = hit.transform;
                    if (layersToTransparent.Contains(objectHit.gameObject.layer))
                    {
                        if (prevHit != null)
                        {
                            prevHit.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
                        }
                        if (objectHit.GetComponent<Renderer>() != null)
                        {
                            prevHit = objectHit;
                            // Can only apply alpha if this material shader is transparent.
                            prevHit.GetComponent<Renderer>().material.color = new Color(1, 1, 1, alpha);
                        }
                    }
                    else if (prevHit != null)
                    {
                        prevHit.GetComponent<Renderer>().material.color = new Color(1, 1, 1, 1);
                        prevHit = null;
                    }
                }
            }

            FightManager.UpdateHUD();
        }
    }
}
