using UnityEngine;
using System.Collections;

public class Fleet : RTSObject {


	void Start () 
	{
		BaseClassAwake ();
	}
	
	void GetMyInfo()
	{
		RTSButtonManager.instance.valueText.text = "";
		RTSButtonManager.instance.SwitchAllGarrisonInfoOnOff (false);
	}
}//Mono