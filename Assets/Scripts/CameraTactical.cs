using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CameraTactical : MonoBehaviour {

	public static CameraTactical instance;

	public static string reportedInfo = "";

	[HideInInspector] public RTSCamera rtsCamControls;
	public AICommander pmcCommander;
	public AICommander enemyCommander;

	AICommander currentlyViewedCommanderShips;

	public bool canAccessTacticalMap = false;
	[HideInInspector]public bool mapIsShown = false;
	[HideInInspector] public int mapUses = 0;


	public GameObject playerFighterUI;

	public GameObject gridlinePrefab;
	public GameObject radarObjects;
	public int gridLineDistance = 100;

	public int gridsWide = 50;
	public int gridsHigh = 50;

	public Transform pipDisplay;
	public Text pipText;
	public Image pipTextBgImage;

	Vector3 pipDisplayScale;
	Vector3 pipDisplayScaleAtStart;
	public Camera pipCamera;
	Vector2 pipPosition;

	GameObject normalRadar;
	Camera tacticalCamera;
	Vector3 cameraPositionAtStart;

	GameObject player;

	public Transform tacticalCursor;
	public GameObject tacticalCanvas;
	public Transform allCraftListParent;
	public GameObject singleCraftListPrefab;
	public Text switchListText;

	List<GameObject> craftListItems = new List<GameObject>(); //the panels to display a craft in the list
	List<GameObject> craftToShow = new List<GameObject>();
	[HideInInspector]public Vector3 offset;
	RaycastHit2D hit;
	Collider2D lastHit;
	public LayerMask raycastMask;
	public LineRenderer lr1;


	void Awake()
	{
		if (instance == null)
			instance = this;
		else{
			Destroy(gameObject);
			Debug.LogError("There were two tactical cameras");
		}

		tacticalCamera = GetComponent<Camera> ();
		offset = tacticalCamera.transform.position;
		rtsCamControls = GetComponent<RTSCamera> ();
		normalRadar = GameObject.Find ("Camera (Radar)");
		player = GameObject.FindGameObjectWithTag ("PlayerFighter");

		DrawMapLines();
	}


	void Start()
	{
		Invoke ("AllowTacCam", 0.5f);

		pipDisplayScaleAtStart = pipDisplay.localScale;
	}

	void AllowTacCam()
	{
		if(ClickToPlay.instance != null && !ClickToPlay.instance.paused)
			canAccessTacticalMap = true;
	}


	void Update(){

		if(mapIsShown)
			Cursor.visible = false;

		if (Input.GetButtonDown ("Tactical Map") || (tacticalCamera.enabled == true && Input.GetKeyUp(KeyCode.JoystickButton1)))
		{
			ToggleTacticalCam();
		}

		if(tacticalCamera.enabled)
		{		
			hit = Physics2D.Raycast(tacticalCursor.transform.position, Vector2.zero, 100, raycastMask);
			
			if (hit.collider != null && hit.collider != lastHit) 
			{
				lastHit = hit.collider;

				pipDisplay.gameObject.SetActive(true);

				if(hit.collider.transform.parent != null)
				{
					pipCamera.transform.position = hit.collider.transform.parent.position + offset;

					if(hit.collider.transform.parent.tag != "Asteroid")
					{
						lr1.enabled = true;
						lr1.SetPosition(0, pipCamera.transform.position - offset);	

							if((Vector3)hit.collider.GetComponentInParent<EnginesFighter>().targetMove != Vector3.zero)
								lr1.SetPosition(1, (Vector3)hit.collider.GetComponentInParent<EnginesFighter>().targetMove);
							else lr1.enabled = false;
					}
					hit.collider.transform.parent.SendMessage("ReportActivity");	
				}

				pipText.text = reportedInfo;
				pipTextBgImage.enabled = true;
			}
			else if(hit.collider == null)
			{
				pipDisplay.gameObject.SetActive(false);
				pipText.text = "";
				pipTextBgImage.enabled = false;
				lr1.enabled = false;

				lastHit = null;
			}
		}

		//for switching ship lists
		if (Input.GetKeyDown (KeyCode.JoystickButton4) || Input.GetKeyDown (KeyCode.JoystickButton5)) 
		{
			SwitchShipList();
		}
	}


	public void ToggleTacticalCam()
	{
		if (!canAccessTacticalMap)
			return;

		if(normalRadar != null)
			normalRadar.SetActive (!normalRadar.activeSelf);

		tacticalCanvas.SetActive (!tacticalCanvas.activeSelf);

		playerFighterUI.SetActive (!playerFighterUI.activeSelf);

		tacticalCamera.enabled = !tacticalCamera.enabled;
		rtsCamControls.enabled = !rtsCamControls.enabled;
		tacticalCursor.gameObject.SetActive (!tacticalCursor.gameObject.activeSelf);

		foreach (ParallaxLayer parallaxScript in Object.FindObjectsOfType(typeof(ParallaxLayer)))
		{
			parallaxScript.enabled = !parallaxScript.enabled;
		}


		if(player != null)
		{
			transform.position = player.transform.position + new Vector3(0,0,-50);
		}
		ClickToPlay.instance.TogglePause();


		//if we're turning off the map, reset all radar sig sizes and parallax positions
		if(!tacticalCamera.enabled)
		{
			foreach (UI_resizer resizeScript in Object.FindObjectsOfType(typeof(UI_resizer)))
			{
				resizeScript.ResetSize();
			}
			rtsCamControls.ResetToDefaultSizes();
			AudioMasterScript.instance.ZeroSFX();

			if(InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
				Cursor.visible = true;
		}
		else //if we're turning it on
		{
			AudioMasterScript.instance.MuteSFX();
			mapUses ++;
			//SetUpShipLists(pmcCommander);
		}

		mapIsShown = !mapIsShown;
	}


	void DrawMapLines()
	{
		for(int i = 0; i < gridsWide; i++)
		{
			for(int j = 0; j < gridsHigh; j++)
			{
				float posX = (-gridsWide/2 + i) * gridLineDistance;
				float posY = (-gridsHigh/2 + j) * gridLineDistance;

				GameObject go = Instantiate(gridlinePrefab, new Vector3(posX, posY, 0), Quaternion.identity) as GameObject;
				go.transform.parent = radarObjects.transform;
			}
		}
	}

	public void SwitchShipList()
	{
		if (currentlyViewedCommanderShips == pmcCommander)
			SetUpShipLists (enemyCommander);
		else if (currentlyViewedCommanderShips == enemyCommander)
			SetUpShipLists (pmcCommander);
	}

	void SetUpShipLists(AICommander whichCommander)
	{
		currentlyViewedCommanderShips = whichCommander;

		if (whichCommander == pmcCommander)
			switchListText.text = "View Enemy Ships";
		else if (whichCommander == enemyCommander)
			switchListText.text = "View PMC Ships";

		for(int i=0; i< craftListItems.Count; i++)
		{
			craftListItems[i].SetActive(false);
		}

		craftToShow.Clear ();
		craftToShow.AddRange (whichCommander.myFighters);
		craftToShow.AddRange (whichCommander.myBombers);
		craftToShow.AddRange (whichCommander.myTransports);
		craftToShow.AddRange (whichCommander.myCapShips);

		if (craftToShow.Count <= 0)
			return;

		//if there aren't enough buttons created for the list length, make more
		if(craftListItems.Count < craftToShow.Count)
		{
			int howManyMore = craftToShow.Count - craftListItems.Count;
			for(int i=0; i < howManyMore; i++)
			{
				GameObject newListPanel = Instantiate(singleCraftListPrefab) as GameObject;
				newListPanel.transform.SetParent(allCraftListParent, false);
				craftListItems.Add(newListPanel);
				newListPanel.SetActive(false);
			}
		}

		//set up the buttons to reference a craft each
		for(int i=0; i< craftToShow.Count; i++)
		{
			craftListItems[i].GetComponent<TacticalObject>().whoDoIRepresent = craftToShow[i];
			craftListItems[i].SetActive(true);
			craftListItems[i].GetComponentInChildren<Text>().text = craftToShow[i].name;
		}

		//set up explicit button navigation and select first item
		Navigation[] navs = new Navigation[craftListItems.Count];

		if(craftListItems.Count >1)
		{
			for(int i=0; i< craftToShow.Count; i++)
			{
				craftListItems[i].GetComponentInChildren<Animator>().SetTrigger("Normal");

				navs[i] = craftListItems[i].GetComponentInChildren<Button> ().navigation;

				if(i == 0)
				{
					navs[i].selectOnUp = craftListItems[craftToShow.Count-1].GetComponentInChildren<Button>();
					navs[i].selectOnDown = craftListItems[i+1].GetComponentInChildren<Button>();
				}
				else if(i == craftToShow.Count-1)
				{
					navs[i].selectOnDown = craftListItems[0].GetComponentInChildren<Button>();
					navs[i].selectOnUp = craftListItems[i-1].GetComponentInChildren<Button>();
				}
				else
				{
					navs[i].selectOnUp = craftListItems[i-1].GetComponentInChildren<Button>();
					navs[i].selectOnDown = craftListItems[i+1].GetComponentInChildren<Button>();
				}
				craftListItems[i].GetComponentInChildren<Button>().navigation = navs[i];
			}
		}
		if(craftListItems.Count != 0)
		{
			craftListItems [0].GetComponentInChildren<Button> ().Select ();
			craftListItems [0].GetComponentInChildren<Animator> ().SetTrigger("Highlighted");
		}

	}//end of SetUpShipLists


	void LateUpdate()
	{
		if (InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
		{
			tacticalCursor.position = tacticalCamera.ScreenToWorldPoint (Input.mousePosition) - offset;
		}
		else
		{
			tacticalCursor.position = tacticalCamera.transform.position - offset;
		}

		pipDisplayScale = pipDisplayScaleAtStart * tacticalCamera.orthographicSize / rtsCamControls.defaultZoom; 
		pipDisplay.localScale = pipDisplayScale;

		pipPosition.x = tacticalCamera.transform.position.x + (tacticalCamera.orthographicSize * Screen.width/Screen.height);
		pipPosition.y = tacticalCamera.transform.position.y + tacticalCamera.orthographicSize;
		pipDisplay.position = pipPosition;

	}
}//Mono
