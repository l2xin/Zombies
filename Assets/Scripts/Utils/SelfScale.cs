﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SelfScale : MonoBehaviour 
{
	public Vector3 AngleSpeed = Vector3.zero;

	void Start()
	{
		AngleSpeed = Vector3.up * 60;
	}
	float Speedx = 0;
	float Speedy = 0;
	float Speedz = 0;
	void LateUpdate()
	{
		if(AngleSpeed.x != 0){Speedx = AngleSpeed.x * Time.deltaTime;}
		if(AngleSpeed.y != 0){Speedy = AngleSpeed.y * Time.deltaTime;}
		if(AngleSpeed.z != 0){Speedz = AngleSpeed.z * Time.deltaTime;}
		transform.Rotate (Speedx,Speedy,Speedz);
	}
}
