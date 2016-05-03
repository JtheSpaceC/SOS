using UnityEngine;
using System.Collections;

public class TacticalObject : MonoBehaviour {

	public GameObject whoDoIRepresent;
	

	public void SelectMe()
	{
		CameraTactical.instance.rtsCamControls.SetAutoMoveTarget (whoDoIRepresent.transform.position);
		InputManager.instance.inputFrom = InputManager.InputFrom.controller;

		CameraTactical.instance.pipCamera.transform.position = 
			new Vector3 (whoDoIRepresent.transform.position.x, whoDoIRepresent.transform.position.y, -50);
		//CameraTactical.instance.fancyBackground.transform.position = whoDoIRepresent.transform.position;
	}
}
