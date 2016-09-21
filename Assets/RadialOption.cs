using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RadialOption : MonoBehaviour {

	Image myImage;

	public bool selected = false;
	public float realRotation;


	void Start()
	{
		myImage = GetComponent<Image>();
		realRotation = GetComponent<RectTransform>().rotation.z;
		if(realRotation != 180)
			realRotation *= -1; //this just corrects it into a clockwise 360 that's easier to think about
	}

	void Update()
	{
		if(!selected)
			myImage.color = Color.white;
		else
			myImage.color = Color.red;

	}
}
