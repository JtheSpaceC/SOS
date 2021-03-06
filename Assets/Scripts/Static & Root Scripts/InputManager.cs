﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class InputManager : MonoBehaviour {

	public static InputManager instance;

	[Tooltip("Enables Game Restart if no input for X time.")]
	public bool conventionMode = false;
	public float restartTime = 90;
	float restartTimer;

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

	[HideInInspector] public bool DpadUpDown;
	[HideInInspector] public bool DpadDownDown;
	[HideInInspector] public bool DpadLeftDown;
	[HideInInspector] public bool DpadRightDown;

	public delegate void MenuActivated();
	public static event MenuActivated menuActivated;

	public delegate void MenuDeactivated();
	public static event MenuDeactivated menuDeactivated;

	public List<CanvasGroup> canvasGroupsToggled = new List<CanvasGroup>();
	CanvasGroup[] cgs;

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

	#region MenuButtons
	void OnEnable()
	{
		menuActivated += TurnOffBlockRaycasts;
		menuDeactivated += TurnOnBlockRaycasts;
	}
	void OnDisable()
	{
		menuActivated -= TurnOffBlockRaycasts;
		menuDeactivated -= TurnOnBlockRaycasts;
	}

	public void CallMenuActivated()
	{
		menuActivated();
	}
	public void CallMenuDeactivaed()
	{
		menuDeactivated();
	}
	void TurnOffBlockRaycasts() //when turning ON menu
	{
		if(inputFrom == InputFrom.keyboardMouse)
		{
			return;
		}
		cgs = FindObjectsOfType<CanvasGroup>();

		foreach(CanvasGroup cg in cgs)
		{
			if(cg.tag == "ToggleableMenu")
			{
				cg.blocksRaycasts = false;
				canvasGroupsToggled.Add(cg);
			}
		}
	}
	void TurnOnBlockRaycasts() //when turning OFF menu
	{
		foreach(CanvasGroup cg in canvasGroupsToggled)
		{
			if(cg.tag == "ToggleableMenu")
			{
				cg.blocksRaycasts = true;
			}
		}
		canvasGroupsToggled.Clear();
	}
	#endregion

	void Update()
	{
		//If we start using the mouse, deactivate the selections that the buttons had set up, and just go by mouse input
		if(inputFrom == InputFrom.keyboardMouse && EventSystem.current.IsPointerOverGameObject())
		{
			EventSystem.current.SetSelectedGameObject(null);
		}		

		if(conventionMode)
		{
			if(Input.anyKey || !Mathf.Approximately(Input.GetAxis("Gamepad Left Horizontal"), 0) || 
				!Mathf.Approximately(Input.GetAxis("Gamepad Triggers"), 0) || DpadDownDown)
			{
				restartTimer = 0;
			}

			restartTimer += Time.unscaledDeltaTime;

			if(restartTimer >= restartTime)
			{
				Tools.instance.AlterTimeScale(1);
				if(!ClickToPlay.instance.paused)
				{
					ClickToPlay.instance.escCanGiveQuitMenu = false;
					FindObjectOfType<CameraTactical>().enabled = false;
					StartCoroutine(ClickToPlay.instance.LoadScene(0, 1));
				}
				else 
				{
					Tools.instance.AlterTimeScale(1);
					ClickToPlay.instance.LoadScene(0);
				}

				//REMOVE: Too specific to just the demo level to be in here
				if(FindObjectOfType<DemoAndTutorialLevel>())
					FindObjectOfType<DemoAndTutorialLevel>().ClearCheckpoints();

				try{FindObjectOfType<Analytics_Demo1>().PlayerWalkedAwayFromConsole();}
				catch{Debug.Log("No Analytics script hooked up to Demo Restart.");}
			}
		}

		if(inputFrom == InputFrom.controller) //if we're on controller, do the checks for keyboard
		{
			if(!Mathf.Approximately(Input.GetAxis("Mouse X"), 0) || !Mathf.Approximately(Input.GetAxis("Mouse Y"), 0) ||
				Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
				Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)||
				Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Space)||
				Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Q) ||
				Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) ||
				Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftShift))
			{
				ChangeTo(InputFrom.keyboardMouse);
				restartTimer = 0;
			}

			//check for DpadInput
			//first reset anything that was down on previous frame
			if(Input.GetAxisRaw("Dpad Vertical") == 0 || DpadUpDown || DpadDownDown)
			{
				DpadUpDown = false;
				DpadDownDown = false;
			}
			if(Input.GetAxisRaw("Dpad Horizontal") == 0 || DpadLeftDown || DpadRightDown)
			{
				DpadLeftDown = false;
				DpadRightDown = false;
			}

			if(Input.GetAxisRaw("Dpad Vertical") > 0)
			{
				DpadUpDown = true;
			}
			else if(Input.GetAxisRaw("Dpad Vertical") < 0)
			{
				DpadDownDown = true;
			}
			if(Input.GetAxisRaw("Dpad Horizontal") < 0)
			{
				DpadLeftDown = true;
			}
			else if(Input.GetAxisRaw("Dpad Horizontal") > 0)
			{
				DpadRightDown = true;
			}
		}
		else if(inputFrom == InputFrom.keyboardMouse) //if we're on key/mouse, do checks for gamepad
		{
			if (!Mathf.Approximately(Input.GetAxis("Gamepad Left Horizontal"), 0) || 
				!Mathf.Approximately(Input.GetAxis("Gamepad Left Vertical"), 0) ||
				!Mathf.Approximately(Input.GetAxis("Gamepad Right Horizontal"), 0) ||
				!Mathf.Approximately(Input.GetAxis("Gamepad Right Vertical"), 0)||
				!Mathf.Approximately(Input.GetAxis("Dpad Vertical"), 0)||
				!Mathf.Approximately(Input.GetAxis("Dpad Horizontal"), 0)||
				!Mathf.Approximately(Input.GetAxis("Gamepad Triggers"), 0) ||
				Input.GetKeyDown(KeyCode.JoystickButton0)||
				Input.GetKeyDown(KeyCode.JoystickButton1)||
				Input.GetKeyDown(KeyCode.JoystickButton2)||
				Input.GetKeyDown(KeyCode.JoystickButton3)||
				Input.GetKeyDown(KeyCode.JoystickButton4)||
				Input.GetKeyDown(KeyCode.JoystickButton5)||
				Input.GetKeyDown(KeyCode.JoystickButton6)||
				Input.GetKeyDown(KeyCode.JoystickButton7) )
			{
				ChangeTo(InputFrom.controller);
				restartTimer = 0;
			}
		}

	}//end of UPDATE()

	public void ChangeTo(InputFrom newType)
	{
		if(inputFrom == newType)
			return;

		inputFrom = newType;

		if(newType == InputFrom.keyboardMouse)
		{
			Cursor.visible = true;
			EventSystem.current.SetSelectedGameObject(null);
		//	RadioCommands.instance.SwitchHelperButtons("keyboard"); 
			TurnOnBlockRaycasts();
		}
		else
		{
			Cursor.visible = false;

			if(FindObjectOfType<Button>())
				EventSystem.current.SetSelectedGameObject(FindObjectOfType<Button>().gameObject);
		//	RadioCommands.instance.SwitchHelperButtons("gamepad");
			TurnOffBlockRaycasts();
		}
	}

}
