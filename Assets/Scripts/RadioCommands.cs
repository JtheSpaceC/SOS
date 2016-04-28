using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RadioCommands : MonoBehaviour {

	public static RadioCommands instance;

	public bool canAccessRadio = true;
	[HideInInspector] public int radioButtonPresses = 0;
	
	public Button button1;
	public Text button1Text;
	public Button button2;
	public Text button2Text;
	public Button button3;
	public Text button3Text;
	public Button button4;
	public Text button4Text;
	public Image DpadImage;
	public Text centralText;

	Button remainingButton;

	Button[] all4Buttons;
	[HideInInspector] public Button[] activeButtons;

	[HideInInspector] public bool buttonsShown = false;
	[HideInInspector] public bool criticalMessage = false;

	bool tookDpadInput = false;
	[HideInInspector] public float timer;
	public float buttonIdleFadeTime = 3.5f;

	[HideInInspector] public bool upPressed = false;
	[HideInInspector] public bool downPressed = false;
	[HideInInspector] public bool leftPressed = false;
	[HideInInspector] public bool rightPressed = false;

	string previousRadioCode = "";
	string radioCode = "";

	SquadronLeader playerSquadLeadScript;
	[HideInInspector] public GameObject communicatingGameObject;

	[Header("Helper Buttons")]
	public Image helperImage1;
	public Image helperImage2;
	public Image helperImage3;
	public Image helperImage4;
	public Sprite helperImageKeyboard1;
	public Sprite helperImageKeyboard2;
	public Sprite helperImageKeyboard3;
	public Sprite helperImageKeyboard4;
	public Sprite helperImageGamepad1;
	public Sprite helperImageGamepad2;
	public Sprite helperImageGamepad3;
	public Sprite helperImageGamepad4;
	Color helperImageStartColour;
	string channelTextString;


	void Awake()
	{
		if (instance == null)
			instance = this;
		else 
		{
			Destroy(gameObject);
			Debug.LogError("There were two radio command scripts. Deleting One");
			return;
		}

		all4Buttons = new Button[]{button1, button2, button3, button4};

	}


	void Start () 
	{
		playerSquadLeadScript = GameObject.FindGameObjectWithTag ("PlayerFighter").GetComponentInChildren<SquadronLeader> ();
		helperImageStartColour = helperImage1.color;
		channelTextString = button1Text.text;
	}
	


	void Update () 
	{
		//comms don't work if paused
		if (ClickToPlay.instance.paused || !canAccessRadio)
			return;

		//this is the fade out time for button inactivity
		if(buttonsShown && !criticalMessage)
			timer += Time.deltaTime;

		if(buttonsShown && timer >= buttonIdleFadeTime)
		{
			Invoke ("ResetRadioOptions",0.7f);
			FadeOutButtons(activeButtons, false);
		}

		//for handling input

		if( !tookDpadInput && (Mathf.Abs(Input.GetAxis("Orders Vertical")) > 0.5f || Input.GetAxis("Orders Horizontal") != 0
		    || Input.GetButtonDown("Orders Up") || Input.GetButtonDown("Orders Down")
		    || Input.GetButtonDown("Orders Right")|| Input.GetButtonDown("Orders Left")))
		{
			tookDpadInput = true;
			if(Input.GetAxis("Orders Vertical") > 0 || Input.GetButtonDown("Orders Up"))
			{
				upPressed  = true;
			}
			else if(Input.GetAxis("Orders Vertical") < 0 || Input.GetButtonDown("Orders Down"))
			{
				downPressed = true;
			}
			else if (Input.GetAxis("Orders Horizontal") > 0 || Input.GetButtonDown("Orders Right"))
			{
				rightPressed = true;
			}
			else if (Input.GetAxis("Orders Horizontal") < 0 || Input.GetButtonDown("Orders Left"))
			{
				leftPressed = true;
			}
		}
		else if(tookDpadInput && (Input.GetAxis("Orders Vertical") == 0 && Input.GetAxis("Orders Horizontal") == 0
		                          && !Input.GetButtonDown("Orders Up") && !Input.GetButtonDown("Orders Down")
		                          && !Input.GetButtonDown("Orders Right")&& !Input.GetButtonDown("Orders Left")))
		{
			ZeroInput();
			tookDpadInput = false;
		}

		//for the actual orders

		if(!buttonsShown && (upPressed || rightPressed || downPressed || leftPressed))
		{
			button1.GetComponentInChildren<Text> ().text = "SQUADRON";
			button2.GetComponentInChildren<Text> ().text = "";
			button3.GetComponentInChildren<Text> ().text = "TACTICAL";
			button4.GetComponentInChildren<Text> ().text = "";

			TurnOnRadio();
		}
		else if(buttonsShown && (upPressed || rightPressed || downPressed || leftPressed))
		{
			timer = 0;
			InterpretRadioOption(upPressed, rightPressed, downPressed, leftPressed);
			ZeroInput();
		}
	}

	public void TurnOnRadio()
	{
		ZeroInput();
		/*ResetRadioOptions();*/	centralText.text = "RADIO"; radioCode = "RADIO";

		activeButtons = new Button[]{button1, button3};
		FadeInButtons(activeButtons, false);
		CheckRemainingButtonsForWrongActivity();
	}

	public void ContextualTurnOnRadio(string centreText, string[] options, bool isCriticalMessage)
	{
		ZeroInput ();
		ResetRadioOptions ();

		centralText.text = centreText;
		button1Text.text = options [0];
		button2Text.text = options [1];
		button3Text.text = options [2];
		button4Text.text = options [3];
		FadeInButtons (activeButtons, false);
		CheckRemainingButtonsForWrongActivity();

		criticalMessage = isCriticalMessage;

		radioCode = centreText;
	}

	void InterpretRadioOption(bool up, bool right, bool down, bool left)
	{
		if (up && CheckIsButtonActive(button1)) 
		{
			previousRadioCode = radioCode;
			radioCode += " " + button1Text.text;
			//FadeInButtons(new Button[]{button2, button3, button4}, true);
		} 
		else if (right && CheckIsButtonActive(button2)) 
		{
			previousRadioCode = radioCode;
			radioCode += " " + button2Text.text;
			//FadeInButtons(new Button[]{button1, button3, button4}, true);
		}
		else if (down && CheckIsButtonActive(button3)) 
		{
			previousRadioCode = radioCode;
			radioCode += " " + button3Text.text;
			//FadeInButtons(new Button[]{button1, button2, button4}, true);
		} 
		else if (left && CheckIsButtonActive(button4)) 
		{
			previousRadioCode = radioCode;
			radioCode += " " + button4Text.text;
			//FadeInButtons(new Button[]{button1, button2, button3}, true);
		}
		else 
			return;

		DoRadioOption (radioCode);
	
		if(buttonsShown)
			FadeInButtons (activeButtons, true);
	}

	public void DoRadioOption(string code)
	{
		radioButtonPresses ++;

		switch (code)
		{
		default: Debug.LogError("Radio code error! " + code);
			radioCode = previousRadioCode;
			break;

		case "RADIO SQUADRON":
			centralText.text = "SQUADRON";
			button1Text.text = "FORM UP";
			button2Text.text = "ENGAGE AT WILL";
			button3Text.text = "COVER ME";
			button4Text.text = "DISENGAGE";
			activeButtons = all4Buttons;
			break;

		case "RADIO TACTICAL":
			centralText.text = "TACTICAL";
			button1Text.text = "SEND BACKUP";
			button2Text.text = "SUPPORT";
			button3Text.text = "EXTRACTION";
			button4Text.text = "REPORT IN";
			activeButtons = all4Buttons;
			break;

		case "RADIO SQUADRON FORM UP":
			button2Text.text = "";
			button3Text.text = "";
			button4Text.text = "";

			FadeOutButtons(new Button[]{button2, button3, button4}, true);
			remainingButton = button1;
			Invoke("FadeOutRemainingButtons", 1);

			playerSquadLeadScript.FormUp();
			break;

		case "RADIO SQUADRON ENGAGE AT WILL":
			button1Text.text = "";
			button3Text.text = "";
			button4Text.text = "";
			
			FadeOutButtons(new Button[]{button1, button3, button4}, true);
			remainingButton = button2;
			Invoke("FadeOutRemainingButtons", 1);

			playerSquadLeadScript.EngageAtWill();
			break;

		case "RADIO SQUADRON COVER ME":
			button1Text.text = "";
			button2Text.text = "";
			button4Text.text = "";
			
			FadeOutButtons(new Button[]{button1, button2, button4}, true);
			remainingButton = button3;
			Invoke("FadeOutRemainingButtons", 1);

			playerSquadLeadScript.CoverMe();
			break;

		case "RADIO SQUADRON DISENGAGE":
			button1Text.text = "";
			button2Text.text = "";
			button3Text.text = "";
			
			FadeOutButtons(new Button[]{button1, button2, button3}, true);
			remainingButton = button4;
			Invoke("FadeOutRemainingButtons", 1);

			playerSquadLeadScript.Disengage();

			break;
		case "RADIO TACTICAL SEND BACKUP":
			button2Text.text = "";
			button3Text.text = "";
			button4Text.text = "";
			
			FadeOutButtons(new Button[]{button2, button3, button4}, true);
			remainingButton = button1;
			Invoke("FadeOutRemainingButtons", 1);

			PMCMisisonSupports.instance.SendBackup();
			break;
			
		case "RADIO TACTICAL SUPPORT":
			button1Text.text = "";
			button3Text.text = "";
			button4Text.text = "";

			FadeOutButtons(new Button[]{button1, button3, button4}, true);
			remainingButton = button2;
			Invoke("FadeOutRemainingButtons", 1);

			PMCMisisonSupports.instance.CallSupportMenu();
			break;
			
		case "RADIO TACTICAL EXTRACTION":
			button1Text.text = "";
			button2Text.text = "";
			button4Text.text = "";
			
			FadeOutButtons(new Button[]{button1, button2, button4}, true);
			remainingButton = button3;
			Invoke("FadeOutRemainingButtons", 1);

			PMCMisisonSupports.instance.Extraction(playerSquadLeadScript.mate01);
			break;
			
		case "RADIO TACTICAL REPORT IN":
			button1Text.text = "";
			button2Text.text = "";
			button3Text.text = "";
			
			FadeOutButtons(new Button[]{button1, button2, button3}, true);
			remainingButton = button4;
			Invoke("FadeOutRemainingButtons", 1);

			//TODO: finish the report in feature
			Subtitles.instance.PostHint(new string[] {"<Feature not fully implemented yet>"});
			playerSquadLeadScript.ReportIn();

			break;

		case "DOCKING DOCK":
			buttonsShown = false;
			remainingButton = button1;
			Invoke("FadeOutRemainingButtons", 1);

			_battleEventManager.instance.CallPlayerBeganDocking();

			communicatingGameObject.GetComponent<AITransport>().reelingInPlayerGroup = true;
			communicatingGameObject.GetComponent<AITransport>().sentRadioMessageToPlayerGroup = false;
			communicatingGameObject.GetComponent<AITransport>().ChangeToNewState(AITransport.StateMachine.reelingInPassengers);
			break;

		case "CANCEL DOCK? YES":
			button3Text.text = "";

			FadeOutButtons(new Button[]{button3}, true);
			remainingButton = button1;
			Invoke("FadeOutRemainingButtons", 1);

			communicatingGameObject.GetComponent<AITransport>().ReleaseFighters(true);
			break;

		case "CANCEL DOCK? NO":
			button1Text.text = "";

			FadeOutButtons(new Button[]{button1}, true);
			remainingButton = button3;
			Invoke("FadeOutRemainingButtons", 1);

			break;

		case "LEAVE? LEAVE":
			button1Text.text = "";

			FadeOutButtons(new Button[] {button1}, true);
			remainingButton = button3;
			Invoke("FadeOutRemainingButtons", 1);

			communicatingGameObject.GetComponent<AITransport>().ChangeToNewState(AITransport.StateMachine.warpOut);
			break;

		case "LEAVE? CANCEL":
			button3Text.text = "";
			
			FadeOutButtons(new Button[]{button3}, true);
			remainingButton = button1;
			Invoke("FadeOutRemainingButtons", 1);
			
			communicatingGameObject.GetComponent<AITransport>().ReleaseFighters(true);
			break;
		}
	}

	//functional functions
	void FadeInButtons(Button[] buttonsToFade, bool excludeCentre)
	{
		CancelInvoke ();
		timer = 0;
		buttonsShown = true;

		foreach (Button button in buttonsToFade)
		{
			button.GetComponent<Animator>().SetTrigger("FadeIn");
		}
		if(!excludeCentre)
		{
			DpadImage.GetComponent<Animator> ().SetTrigger ("FadeIn");
		}
	}
	void FadeOutButtons(Button[] buttonsToFade, bool excludeCentre)
	{
		timer = 0;
		buttonsShown = false;
		criticalMessage = false;

		foreach (Button button in buttonsToFade)
		{
			button.GetComponent<Animator>().SetTrigger("FadeOut");
		}
		if(!excludeCentre)
		{
			DpadImage.GetComponent<Animator> ().SetTrigger ("FadeOut");
		}
	}
	void FadeOutRemainingButtons()
	{
		buttonsShown = false;
		remainingButton.GetComponent<Animator>().SetTrigger("FadeOut");
		DpadImage.GetComponent<Animator> ().SetTrigger ("FadeOut");
		Invoke ("ResetRadioOptions",0.7f);
	}
	void ResetRadioOptions()
	{
		centralText.text = "RADIO";
		button1.GetComponentInChildren<Text> ().text = channelTextString;
		button2.GetComponentInChildren<Text> ().text = channelTextString;
		button3.GetComponentInChildren<Text> ().text = channelTextString;
		button4.GetComponentInChildren<Text> ().text = channelTextString;
		radioCode = "RADIO";
	}
	void ZeroInput()
	{
		upPressed = false;
		downPressed = false;
		leftPressed = false;
		rightPressed = false;
	}
	bool CheckIsButtonActive(Button whichButton)
	{
		for (int i = 0; i< activeButtons.Length; i++)
		{
			if(activeButtons[i] == whichButton)
			{
				return true;
			}
		}
		return false;
	}
	void CheckRemainingButtonsForWrongActivity()
	{
		foreach(Button but in all4Buttons)
		{
			bool foundActiveButton = false;

			foreach(Button butt in activeButtons)
			{
				if (but == butt) 
				{
					foundActiveButton = true;
					break;
				}
			}
			if(!foundActiveButton && but.GetComponent<Image>().color.a != 0)
			{
				but.GetComponent<Animator>().SetTrigger("FadeOut");
			}
		}
	}

	public void SwitchHelperButtons(string inputMethod)
	{
		if(inputMethod == "gamepad")
		{
			helperImage1.sprite = helperImageGamepad1;
			helperImage2.sprite = helperImageGamepad2;
			helperImage3.sprite = helperImageGamepad3;
			helperImage4.sprite = helperImageGamepad4;
			helperImage1.color = Color.white;
			helperImage2.color = Color.white;
			helperImage3.color = Color.white;
			helperImage4.color = Color.white;
		}
		else if(inputMethod == "keyboard")
		{
			helperImage1.sprite = helperImageKeyboard1;
			helperImage2.sprite = helperImageKeyboard2;
			helperImage3.sprite = helperImageKeyboard3;
			helperImage4.sprite = helperImageKeyboard4;
			helperImage1.color = helperImageStartColour;
			helperImage2.color = helperImageStartColour;
			helperImage3.color = helperImageStartColour;
			helperImage4.color = helperImageStartColour;
		}
		else 
			Debug.LogError("Wrong String for Radio Commands Switching Helper Buttons");
	}

	void OnDisable()
	{
		CancelInvoke ();
	}
}//MONO
