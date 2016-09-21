using UnityEngine;

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

	bool radialMenuShown = false;
	RadialOption selectedOption;
	Vector2 cursorPos;
	Vector3 screenCentre;

	
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


	void Update()
	{
		if(!radialMenuShown && (Input.GetKeyDown(KeyCode.Q) || (Input.GetAxis("Orders Vertical")) > 0.5f))
		{
			Tools.instance.StopCoroutine("FadeScreen");
			Tools.instance.MoveCanvasToFront(Tools.instance.blackoutCanvas);
			Tools.instance.MoveCanvasToFront(Tools.instance.radialMenuCanvas);
			Tools.instance.blackoutPanel.color = Color.Lerp (Color.black, Color.clear, 0.1f);
			AudioMasterScript.instance.masterMixer.SetFloat("Master vol", -15f);
			Tools.instance.AlterTimeScale(0.1f);

			PopulateRadialMenuOptions();
			radialMenuShown = true;
		}
		else if(radialMenuShown && (Input.GetKeyDown(KeyCode.Q) || (Input.GetAxis("Orders Vertical")) > 0.5f))
		{
			Tools.instance.MoveCanvasToRear(Tools.instance.blackoutCanvas);
			Tools.instance.MoveCanvasToRear(Tools.instance.radialMenuCanvas);
			Tools.instance.blackoutPanel.color = Color.clear;
			AudioMasterScript.instance.masterMixer.SetFloat("Master vol", 0f);
			Tools.instance.AlterTimeScale(1f);
			while(Tools.instance.radialMenuCanvas.transform.childCount > 0)
			{
				DestroyImmediate(Tools.instance.radialMenuCanvas.transform.GetChild(0).gameObject);
			}
			radialMenuShown =false;
		}

		if(radialMenuShown)
		{
			cursorPos = new Vector2(Input.GetAxis("Horizontal"), -Input.GetAxis("Vertical")).normalized;

			if(cursorPos != Vector2.zero)
			{
				if(selectedOption)
					selectedOption.selected = false;

				RaycastHit2D hit = Physics2D.Raycast((Vector2)screenCentre, cursorPos, 500f, LayerMask.GetMask("UI"));

				if(hit.collider != null && hit.collider.GetComponent<RadialOption>())
				{					
					selectedOption = hit.collider.GetComponent<RadialOption>();
					selectedOption.selected = true;
				}
			}
			else if(selectedOption != null){
				selectedOption.selected = false;
				selectedOption = null;
			}
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
