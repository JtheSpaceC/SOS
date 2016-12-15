using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeTests : MonoBehaviour {
	
	public Character myCharacter;

	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.Keypad1))
			myCharacter.eyeballs.localPosition = myCharacter.lowerLeft;
		if(Input.GetKeyDown(KeyCode.Keypad2))
			myCharacter.eyeballs.localPosition = myCharacter.down;
		if(Input.GetKeyDown(KeyCode.Keypad3))
			myCharacter.eyeballs.localPosition = myCharacter.lowerRight;
		if(Input.GetKeyDown(KeyCode.Keypad4))
			myCharacter.eyeballs.localPosition = myCharacter.left;
		if(Input.GetKeyDown(KeyCode.Keypad5))
			myCharacter.eyeballs.localPosition = myCharacter.neutral;
		if(Input.GetKeyDown(KeyCode.Keypad6))
			myCharacter.eyeballs.localPosition = myCharacter.right;
		if(Input.GetKeyDown(KeyCode.Keypad7))
			myCharacter.eyeballs.localPosition = myCharacter.upperLeft;
		if(Input.GetKeyDown(KeyCode.Keypad8))
			myCharacter.eyeballs.localPosition = myCharacter.up;
		if(Input.GetKeyDown(KeyCode.Keypad9))
			myCharacter.eyeballs.localPosition = myCharacter.upperRight;
		
	}
}
