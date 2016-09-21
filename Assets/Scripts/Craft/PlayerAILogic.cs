using UnityEngine;
using System.Collections;

public class PlayerAILogic : FighterFunctions {

	public PlayerAILogic instance;

	[HideInInspector]public HealthFighter healthScript;
	[HideInInspector]public PlayerFighterMovement engineScript;
	[HideInInspector]public WeaponsPrimaryFighter shootScript;
	[HideInInspector]public WeaponsSecondaryFighter missilesScript;
	[HideInInspector]public Dodge dodgeScript;
	
	public enum Orders {FighterSuperiority, Patrol, Escort, RTB, NA}; //set by commander
	public Orders orders;
	
	public GameObject target;
	public GameObject radialOptionPrefab;
	public GameObject radialDivideBarPrefab;
	public GameObject radialGuideArrowPrefab;

	bool radialMenuShown = false;
	RadialOption selectedOption;
	GameObject radialGuideArrow;
	Vector2 cursorPos;
	Vector3 screenCentre;
	float keyboardLedRotation; //for rotatin on the radial menu with arrow keys
	bool takeDPadInput = true;


	void Awake()
	{
		if(instance == null)
		{
			instance = this;
		}
		else
		{
			Debug.LogError("There were 2 PlayerAILogic scripts");
			Destroy(gameObject);
			return;
		}

		healthScript = GetComponent<HealthFighter> ();
		engineScript = GetComponent<PlayerFighterMovement> ();
		shootScript = GetComponentInChildren<WeaponsPrimaryFighter> ();
		missilesScript = GetComponentInChildren<WeaponsSecondaryFighter> ();
		dodgeScript = GetComponentInChildren<Dodge>();
		myRigidbody = GetComponent<Rigidbody2D>();

		if(transform.FindChild("Effects/GUI"))
		{
			myGui = transform.FindChild("Effects/GUI").gameObject;
		}

		SetUpSideInfo();

		//Friendly Commander script automatically adds player to known craft
		enemyCommander.knownEnemyFighters.Add (this.gameObject); //TODO; AI Commander instantly knows all enemies. Make more complex
	}

	void Start()
	{
		screenCentre = Tools.instance.radialMenuCanvas.transform.position;
	}

	IEnumerator DPadInputWait()
	{
		takeDPadInput = false;
		yield return new WaitForSecondsRealtime(0.4f);
		takeDPadInput = true;
	}

	void Update()
	{
		if(ClickToPlay.instance.paused)
			return;

		//ACTIVATE the Radial menu
		if(!radialMenuShown && 
			(Input.GetKeyDown(KeyCode.Q) || 
				((Input.GetAxis("Orders Vertical")) > 0.5f) && takeDPadInput ) )
		{
			StartCoroutine("DPadInputWait");
			Tools.instance.StopCoroutine("FadeScreen");
			Tools.instance.MoveCanvasToFront(Tools.instance.blackoutCanvas);
			Tools.instance.MoveCanvasToFront(Tools.instance.radialMenuCanvas);
			Tools.instance.blackoutPanel.color = Color.Lerp (Color.black, Color.clear, 0.1f);
			AudioMasterScript.instance.masterMixer.SetFloat("Master vol", -15f);
			Tools.instance.AlterTimeScale(0.1f);
			TogglePlayerControl(true, false, false, false);

			PopulateRadialMenuOptions();
			radialMenuShown = true;
		}

		//DEACTIVATE the Radial menu
		else if(radialMenuShown && 
			(Input.GetKeyDown(KeyCode.Q) || (Input.GetAxis("Orders Vertical")) > 0.5f && takeDPadInput))
		{
			StartCoroutine("DPadInputWait");
			Tools.instance.MoveCanvasToRear(Tools.instance.blackoutCanvas);
			Tools.instance.MoveCanvasToRear(Tools.instance.radialMenuCanvas);
			Tools.instance.blackoutPanel.color = Color.clear;
			AudioMasterScript.instance.masterMixer.SetFloat("Master vol", 0f);
			Tools.instance.AlterTimeScale(1f);
			TogglePlayerControl(true, true, true, true);

			ClearRadialMenu();

			radialMenuShown =false;
		}

		//OPERATE the Radial menu
		if(radialMenuShown)
		{
			//if using controller
			if(InputManager.instance.inputFrom == InputManager.InputFrom.controller)
			{
				//cursor pos is notional (made up), but the guide arrow object uses it
				cursorPos = new Vector2(Input.GetAxis("Gamepad Left Horizontal"), Input.GetAxis("Gamepad Left Vertical")).normalized;

				if(cursorPos != Vector2.zero)
				{
					if(selectedOption)
						selectedOption.selected = false;

					var variable = Vector2.Angle(Vector2.up, cursorPos);

					if(cursorPos.x < 0)
						variable = 360 - variable;

					radialGuideArrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -variable));
					SendRay();
				}
			}
			//if using keyboard
			else
			{
				if(Input.GetKey(KeyCode.LeftArrow))
					keyboardLedRotation -= 6;
				if(Input.GetKey(KeyCode.RightArrow))
					keyboardLedRotation += 6;

				keyboardLedRotation = keyboardLedRotation % 360;

				radialGuideArrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -keyboardLedRotation));
				SendRay();
			}
		}
	}
	void SendRay()
	{
		RaycastHit2D hit = Physics2D.Raycast((Vector2)screenCentre, radialGuideArrow.transform.up, 500f, LayerMask.GetMask("UI"));
		Debug.DrawLine((Vector2)screenCentre, screenCentre + (radialGuideArrow.transform.up * 500), Color.blue);

		if(hit.collider != null && hit.collider.GetComponent<RadialOption>())
		{					
			selectedOption = hit.collider.GetComponent<RadialOption>();
			selectedOption.selected = true;
		}
		else if(selectedOption)
		{
			selectedOption.selected = false;
			selectedOption = null;
		}
	}

	void PopulateRadialMenuOptions()
	{
		int radialOptions = Random.Range(2, 6);
		float degreesEach = 360f/radialOptions;
		float rotation = 0;

		//throw up the options
		for(int i = 0; i < radialOptions; i++)
		{
			GameObject newOption = Instantiate(radialOptionPrefab) as GameObject;
			newOption.transform.SetParent(Tools.instance.radialMenuCanvas.transform);
			newOption.transform.localPosition = Vector3.zero;
			newOption.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));

			rotation += degreesEach;
		}
		rotation = degreesEach/2;

		//throw up dividing bars
		for(int i = 0; i < radialOptions; i++)
		{
			GameObject newDivider = Instantiate(radialDivideBarPrefab) as GameObject;
			newDivider.transform.SetParent(Tools.instance.radialMenuCanvas.transform);
			newDivider.transform.localPosition = Vector3.zero;
			newDivider.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotation));

			rotation += degreesEach;
		}

		//make the guide arrow
		radialGuideArrow = Instantiate(radialGuideArrowPrefab) as GameObject;
		radialGuideArrow.transform.SetParent(Tools.instance.radialMenuCanvas.transform);
		radialGuideArrow.transform.localPosition = Vector3.zero;

		keyboardLedRotation = 0;
		radialGuideArrowPrefab.SetActive(true);
	}

	void ClearRadialMenu()
	{
		while(Tools.instance.radialMenuCanvas.transform.childCount > 0)
		{
			DestroyImmediate(Tools.instance.radialMenuCanvas.transform.GetChild(0).gameObject);
		}
	}


	public void TogglePlayerControl(bool healthScriptenabled, bool engineScriptEnabled, bool dodgeScriptEnabled, bool shootScriptEnabled)
	{
		healthScript.enabled = healthScriptenabled;
		engineScript.enabled = engineScriptEnabled;
		dodgeScript.enabled = dodgeScriptEnabled;
		shootScript.enabled = shootScriptEnabled;
	}


	void TargetDestroyed()
	{
		if(myAttackers.Contains(target))
		{
			myAttackers.Remove(target);
		}
		if(myCommander.knownEnemyFighters.Contains(target))
		{
			myCommander.knownEnemyFighters.Remove(target);
		}
		target = null;
	}

	void ReportActivity()
	{
		CameraTactical.reportedInfo = this.name + "\n" + "(You)\n";

		if ((float)healthScript.health / healthScript.maxHealth < (0.33f)) 
			CameraTactical.reportedInfo += "Heavily Damaged";		
		else if (healthScript.health == healthScript.maxHealth) 
			CameraTactical.reportedInfo += "Fully Functional";		
		else
			CameraTactical.reportedInfo += "Damaged";
	}

	public void CallDumpAwarenessMana(int howMany)
	{
		StartCoroutine(dodgeScript.DumpPlayerAwarenessMana(howMany));
	}
}//Mono
