using UnityEngine;
using System.Collections;

public class CameraResizer : MonoBehaviour {

	Camera myCam;

	void Start () 
	{
		myCam = GetComponent<Camera>();

		if(Screen.width / 16 != Screen.height / 9)
		{
			float currentRatio = (float)Screen.width/Screen.height;
			myCam.orthographicSize /= currentRatio;
			myCam.orthographicSize *= (float)16/9;
		}
	}
	

}
