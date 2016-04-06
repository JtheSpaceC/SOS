using UnityEngine;
using System.Collections;

public class MiningFacility : PlanetsAndMoons {

	void GetMyInfo()
	{
		if (canGarrison) {
			RTSButtonManager.instance.SwitchAllGarrisonInfoOnOff (true);
		} else 
			RTSButtonManager.instance.SwitchAllGarrisonInfoOnOff (false);
	}

}//Mono
