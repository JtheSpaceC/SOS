using UnityEngine;
using UnityEngine.EventSystems;

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
		if(conventionMode)
		{
			if(Input.anyKey)
				restartTimer = 0;

			restartTimer += Time.fixedDeltaTime;

			if(restartTimer >= restartTime)
			{
				Time.timeScale = 1;
				if(!ClickToPlay.instance.paused)
				{
					ClickToPlay.instance.escGivesQuitMenu = false;
					FindObjectOfType<CameraTactical>().enabled = false;
					StartCoroutine(ClickToPlay.instance.LoadScene(0, 1));
				}
				else 
				{
					Time.timeScale = 1;
					ClickToPlay.instance.LoadScene(0);
				}

				try{FindObjectOfType<Analytics_Demo1>().PlayerWalkedAwayFromConsole();}
				catch{Debug.Log("No Analytics script hooked up to Demo Restart.");}
			}
		}

		if(inputFrom == InputFrom.controller) //if we're on controller, do the checks for keyboard
		{
			if(!Mathf.Approximately(Input.GetAxis("Mouse X"), 0) || !Mathf.Approximately(Input.GetAxis("Mouse Y"), 0) ||
				Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
				Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)||
				Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Space))
			{
				ChangeTo(InputFrom.keyboardMouse);
				restartTimer = 0;
			}
		}
		else if(inputFrom == InputFrom.keyboardMouse) //if we're on key/mouse, do checks for gamepad
		{
			if (!Mathf.Approximately(Input.GetAxis("Gamepad Left Horizontal"), 0) || 
				!Mathf.Approximately(Input.GetAxis("Gamepad Left Vertical"), 0) ||
				!Mathf.Approximately(Input.GetAxis("Gamepad Right Horizontal"), 0) ||
				!Mathf.Approximately(Input.GetAxis("Gamepad Right Vertical"), 0)||
				!Mathf.Approximately(Input.GetAxis("Orders Vertical"), 0)||
				!Mathf.Approximately(Input.GetAxis("Orders Horizontal"), 0))
			{
				ChangeTo(InputFrom.controller);
				restartTimer = 0;
			}
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
		//	RadioCommands.instance.SwitchHelperButtons("keyboard"); 
		}
		else
		{
			Cursor.visible = false;
		//	RadioCommands.instance.SwitchHelperButtons("gamepad");
		}
	}

}
