using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {

	public static InputManager instance;

	public enum InputFrom {controller, keyboardMouse};
	public InputFrom inputFrom;

	[Header("Images. 0 for gamepad, 1 for keyboard")]
	public Sprite imgGamepadA;
	public Sprite imgGamepadB;
	public Sprite imgGamepadX;
	public Sprite imgGamepadY;
	public Sprite imgGamepadLB;
	public Sprite imgGamepadRB;
	public Sprite imgGamepadLT;
	public Sprite imgGamepadRT;
	public Sprite imgGamepadBack;
	public Sprite imgGamepadStart;
	public Sprite imgGamepadLS;
	public Sprite imgGamepadRS;
	public Sprite imgGamepadDpad;


	void Awake()
	{
		transform.SetParent(null);

		if(instance == null)
			instance = this;
		else
		{
			Debug.Log("Were two Input Managers. Deleting one.");
			Destroy(gameObject);
			return;
		}

		inputFrom = InputFrom.keyboardMouse;
	}

	void Update()
	{
		if(!Mathf.Approximately(Input.GetAxis("Mouse X"), 0) || !Mathf.Approximately(Input.GetAxis("Mouse Y"), 0) ||
			Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
			Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)||
			Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Space))
		{
			ChangeTo(InputFrom.keyboardMouse);
		}

		else if (!Mathf.Approximately(Input.GetAxis("Gamepad Left Horizontal"), 0) || 
			!Mathf.Approximately(Input.GetAxis("Gamepad Left Vertical"), 0) ||
			!Mathf.Approximately(Input.GetAxis("Gamepad Right Horizontal"), 0) ||
			!Mathf.Approximately(Input.GetAxis("Gamepad Right Vertical"), 0)||
			!Mathf.Approximately(Input.GetAxis("Orders Vertical"), 0)||
			!Mathf.Approximately(Input.GetAxis("Orders Horizontal"), 0))
		{
			ChangeTo(InputFrom.controller);
		}
	}

	public void ChangeTo(InputFrom newType)
	{
		if(inputFrom == newType)
			return;

		inputFrom = newType;

		if(newType == InputFrom.keyboardMouse)
		{
			Cursor.visible = true;
			RadioCommands.instance.SwitchHelperButtons("keyboard"); 
		}
		else
		{
			Cursor.visible = false;
			RadioCommands.instance.SwitchHelperButtons("gamepad");
		}
	}

}
