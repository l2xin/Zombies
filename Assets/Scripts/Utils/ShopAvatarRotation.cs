using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ShopAvatarRotation : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public float roundLength = 200;

    private Vector3 startDragPoint;
    private Vector3 endDragPoint;

//    private float startYRotation;
    private Vector3 startRotation = new Vector3(0, 180, 0);
    private Vector3 endRotation;

    private IList<GameObject> objs;
    private Transform _avatar;

	// Use this for initialization
	void Start () {
	    _avatar = this.transform.FindChild("Avatar");
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void setObjsRotation(Vector3 v) {
        foreach (Transform obj in _avatar) {
            obj.localRotation = Quaternion.Euler(v);
        }
    }

    public void OnBeginDrag(PointerEventData eventData) {
        startDragPoint = eventData.pressPosition;
    }

    public void OnDrag(PointerEventData eventData) {
        var currentDragPoint = eventData.position;
        var deltaX = currentDragPoint.x - startDragPoint.x;
        var currentRotation = startRotation.y - deltaX*360/roundLength;
        setObjsRotation(new Vector3(startRotation.x, currentRotation, startRotation.z));
    }

    public void OnEndDrag(PointerEventData eventData) {
        startDragPoint = eventData.position;
    }
}
