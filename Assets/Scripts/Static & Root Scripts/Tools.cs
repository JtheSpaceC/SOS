using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;
using AssemblyCSharp;

public class Tools: MonoBehaviour
{
	public static Tools instance;

	public GameObject playerUI;
	public Transform[] avatarsPanelUI;
	int nextFreePanel = 0;

	[HideInInspector] public ObjectPoolerScript explosionPoolerScript;
	[HideInInspector] public ObjectPoolerScript explosionMiniPoolerScript;
	[HideInInspector] public ObjectPoolerScript asteroidPoofPoolerScript;
	[HideInInspector] public ObjectPoolerScript asteroidPoofBigPoolerScript;

	[HideInInspector] public CombatAsteroidsStyle combatAsteroidsStyleScript;

	public Environment environments;
	public ShipStats shipStats;
	public Icons icons;

	public Text killsText;

	public Slider ammoRemainingSlider;
	public Text ammoRemainingText;
	public Slider barrelTempSlider;
	[HideInInspector] public AudioSource barrelTempAudio;
	[HideInInspector] public Image barrelTempFillImage;
	public Text missilesRemainingText;

	public Slider nitroRemainingSlider;
	public Text nitroRemainingText;

	public Canvas blackoutCanvas;
	public Image blackoutPanel;
	float fadeDelay;
	float fadeDuration;
	Color fadeFromColour;
	Color fadeToColour;
	public Color whiteOutColour;
	List<FadeStats> fadeStatsList = new List<FadeStats>();

	public Toggle allowVibrationToggleSwitch;
	bool allowVibrationThisSession = true;

	public Toggle useHintsToggleSwitch;
	[HideInInspector] public bool useHintsThisSession = true;

	[HideInInspector] public enum WaypointTypes {Extraction, Move, Escort, SearchAndDestroy, Follow, Support, Comms};

	[Header("Waypoint Prefabs")]
	public GameObject waypointPrefab;

	public Color avatarAwarenessFlashColour;
	public Color avatarHitFlashColour;

	[HideInInspector] public float normalFixedDeltaTime;

	[Space()]

	[Tooltip ("If a camera were to follow a ship, what stuff should it normally see?")]
	public LayerMask normalCameraViewingLayers;

	[HideInInspector] public List<string> callsignsInUse = new List<string>();
	[HideInInspector] public List<string> fullNamesInUse = new List<string>();


	void Awake()
	{
		if(instance == null)
		{
			instance = this;
		}
		else 
			Destroy(gameObject);

		Application.runInBackground = true;

		explosionPoolerScript = GameObject.FindGameObjectWithTag ("ObjectPooler").transform.FindChild ("Explosion Pooler").GetComponent<ObjectPoolerScript> ();
		explosionMiniPoolerScript = GameObject.FindGameObjectWithTag ("ObjectPooler").transform.FindChild ("ExplosionMini Pooler").GetComponent<ObjectPoolerScript> ();
		asteroidPoofPoolerScript = GameObject.Find ("asteroidPoof Pooler").GetComponent<ObjectPoolerScript> ();
		asteroidPoofBigPoolerScript = GameObject.Find ("asteroidPoofBig Pooler").GetComponent<ObjectPoolerScript> ();

		combatAsteroidsStyleScript = FindObjectOfType<CombatAsteroidsStyle>();

		if(GameObject.FindGameObjectWithTag("PlayerFighter") == null)
		{
			if(playerUI != null)
				playerUI.SetActive(false);

			BGScroller[] scrollers = Camera.main.GetComponentsInChildren<BGScroller>();
			foreach(BGScroller scroller in scrollers)
			{
				scroller.basedOnPlayer = false;
			}
		}

		if(PlayerPrefsManager.GetHintsKey() == "On")
		{
			useHintsThisSession = true;
			if(useHintsToggleSwitch)
				useHintsToggleSwitch.isOn = true;
		}
		else if(PlayerPrefsManager.GetHintsKey() == "Off")
		{
			useHintsThisSession = false;
			if(useHintsToggleSwitch)
				useHintsToggleSwitch.isOn = false;
		}
		else
		{
			Debug.Log("Something went wrong");
		}


		if (PlayerPrefsManager.GetVibrateKey() == "true")
		{
			allowVibrationThisSession = true;
			if(allowVibrationToggleSwitch)
				allowVibrationToggleSwitch.isOn = true;
		}
		else if(PlayerPrefsManager.GetVibrateKey() == "false")
		{
			try{
			allowVibrationThisSession = false;
			allowVibrationToggleSwitch.isOn = false;
			}catch{}
		}
		else if(PlayerPrefsManager.GetVibrateKey() == "")
		{
			allowVibrationThisSession = allowVibrationToggleSwitch.isOn;

			if(allowVibrationThisSession)
				PlayerPrefsManager.SetVibrateKey("true");
			else
				PlayerPrefsManager.SetVibrateKey("false");
		}
		else
		{
			print(PlayerPrefsManager.GetVibrateKey());
			Debug.LogError("Vibrate Key wasn't blank, true, or false. Something has set it incorrectly. Resetting..");
			PlayerPrefsManager.SetVibrateKey("");
		}

		barrelTempAudio = barrelTempSlider.GetComponent<AudioSource>();
		barrelTempFillImage = barrelTempSlider.transform.FindChild("Fill Area/Fill").GetComponent<Image>();

		AudioMasterScript.instance.StopAllCoroutines();
		AudioMasterScript.instance.ZeroSFX();

		normalFixedDeltaTime = Time.fixedDeltaTime;

		callsignsInUse = new List<string>();

	}//end of AWAKE

	void OnDisable()
	{
		VibrationStop();
		AlterTimeScale(1);
	}
		

	public void CommenceFade(float delay, float dur, Color fromColour, Color toColour, bool cancelOldCoroutine)
	{
		if(cancelOldCoroutine)
		{
			StopCoroutine("FadeScreen");
			fadeStatsList.Clear();
		}

		FadeStats newFadeStats = new FadeStats();
		newFadeStats.fadeDelay = delay;
		newFadeStats.fadeDuration = dur;
		newFadeStats.fadeFromColour = fromColour;
		newFadeStats.fadeToColour = toColour;
		fadeStatsList.Add(newFadeStats);

		StartCoroutine("FadeScreen");
		MoveCanvasToFront(blackoutPanel.GetComponentInParent<Canvas>());
	}
	IEnumerator FadeScreen()
	{
		FadeStats myFadeStats = fadeStatsList[fadeStatsList.Count-1];
		yield return new WaitForSeconds(myFadeStats.fadeDelay);

		float fadeStartTime = Time.time;
		blackoutPanel.color = myFadeStats.fadeFromColour;

		while(blackoutPanel.color != myFadeStats.fadeToColour)
		{
			blackoutPanel.color = 
				Color.Lerp(myFadeStats.fadeFromColour, myFadeStats.fadeToColour, (Time.time - fadeStartTime)/myFadeStats.fadeDuration);
			yield return new WaitForEndOfFrame();
		}
		fadeStatsList.Remove(myFadeStats);
	}


	public void SpawnExplosion (GameObject go, Vector2 where, bool inheritVelocity) 
	{				
		GameObject obj = explosionPoolerScript.current.GetPooledObject ();
		
		obj.transform.position = where;
		obj.transform.rotation = go.transform.rotation;
		obj.SetActive (true);
		if (inheritVelocity)
		{	
			if(go.tag == "Turret")
			{
				obj.GetComponent<Rigidbody2D> ().velocity = go.transform.parent.parent.GetComponent<Rigidbody2D> ().velocity;
			}
			else
			{
				obj.GetComponent<Rigidbody2D> ().velocity = go.GetComponent<Rigidbody2D> ().velocity;
			}
		}
		else 
			obj.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
	}

	public void SpawnExplosionMini(GameObject go, float radius)
	{
		GameObject obj = explosionMiniPoolerScript.current.GetPooledObject ();
		
		obj.transform.position = go.transform.position + ((Vector3)Random.insideUnitCircle * radius);
		obj.transform.rotation = go.transform.rotation;
		obj.SetActive (true);
	}

	public void SpawnAsteroidPoof(Vector2 where)
	{
		GameObject obj =  asteroidPoofPoolerScript.current.GetPooledObject();
		obj.transform.position = where;
		obj.SetActive (true);
	}

	public void SpawnAsteroidPoofBig(Vector2 where, float scale, Vector2 velocity)
	{
		GameObject obj =  asteroidPoofBigPoolerScript.current.GetPooledObject();
		obj.transform.position = where;
		obj.transform.localScale = new Vector3(scale/3, scale/3, scale/3);
		obj.SetActive (true);

		obj.GetComponent<Rigidbody2D>().velocity = velocity;
	}

	public void CreateWaypoint(WaypointTypes wpType, Vector2[] positions)
	{
		if(wpType == WaypointTypes.Extraction || wpType == WaypointTypes.SearchAndDestroy || wpType == WaypointTypes.Move)
		{
			GameObject wp = Instantiate(waypointPrefab, positions[0], Quaternion.identity) as GameObject;
			wp.GetComponent<PointerHUDElement>().targetWP = positions[0];
			wp.GetComponent<PointerHUDElement>().myWaypointType = wpType;
			wp.GetComponent<PointerHUDElement>().Awake();
		}
	}

	public void CreateWaypoint(WaypointTypes wpType, Transform target)
	{
		if(wpType == WaypointTypes.Follow)
		{
			
		}
		else if(wpType == WaypointTypes.Escort || (wpType == WaypointTypes.Comms))
		{
			GameObject wp = Instantiate(waypointPrefab, target.position, Quaternion.identity) as GameObject;
			wp.GetComponent<PointerHUDElement>().target = target;
			wp.GetComponent<PointerHUDElement>().myWaypointType = wpType;
			wp.GetComponent<PointerHUDElement>().Awake();
		}
	}

	public void ClearWaypoints()
	{
		PointerHUDElement[] wayPoints = FindObjectsOfType<PointerHUDElement>();

		foreach(PointerHUDElement wp in wayPoints)
		{
			wp.gameObject.SetActive(false);
		}
	}

	public bool CheckTargetIsRetreating(GameObject targetToCheck, GameObject theCaller, string whereWasItCalled)
	{		
		if (targetToCheck == null)
		{
			Debug.LogError("ERROR: " + theCaller.name + " checked if a NULL target was retreating. " + whereWasItCalled);
			return false;
		}
		else if(targetToCheck.tag == "FormationPosition")
		{
			Debug.LogError("ERROR: " + theCaller.name + " checked if a FORMATION was retreating. ");
			return false;
		}
		else if(targetToCheck.tag == "PlayerFighter" || targetToCheck.tag == "Turret")
		{
			return false;
		}
		else if(targetToCheck.GetComponent<AIFighter>().currentState == AIFighter.StateMachine.Evade || 
			targetToCheck.GetComponent<AIFighter>().currentState == AIFighter.StateMachine.Retreat)
		{
			return true;
		}
		else 
			return false;		
	}

	public bool CheckTargetIsLegit(GameObject target)
	{
		if (target == null)
			return false;

		if (target.tag == "FormationPosition" && target.activeInHierarchy)
			return true;

		else if(!target.activeInHierarchy)
		{
			//	if(target != null)
			//Debug.Log(gameObject.name + ": "+ target.name + " IS NOT LEGIT!");

			return false;
		}
		else if((target.tag == "Fighter" || target.tag == "PlayerFighter") 
			&& target.GetComponent<HealthFighter>().dead)
		{
			return false;
		}
		else if(target.tag == "Turret" && target.GetComponent<HealthTurret>().dead)
		{
			return false;
		}
		else
		{
			//Debug.Log(gameObject.name + ": "+ target.name + " is LEGIT!");
			return true;
		}
	}

	public IEnumerator TextAnim(Text givenText, Color flashColour, Color returnColour, float givenTime)
	{
		givenText.color = flashColour;
		yield return new WaitForSeconds(givenTime);
		givenText.color = returnColour;
	}
		

	public IEnumerator WhiteScreenFlash(float flashDuration)
	{
		blackoutPanel.color = whiteOutColour;
		float flashStartTime = Time.time;

		while(blackoutPanel.color != Color.clear)
		{
			blackoutPanel.color = Color.Lerp(whiteOutColour, Color.clear, (Time.time - flashStartTime)/flashDuration);
			yield return new WaitForEndOfFrame();
		}
	}

	public IEnumerator ImageFlashToClear(Image flashImage, Color flashColour, float flashTime)
	{
		flashImage.color = flashColour;
		float startTime = Time.time;

		while(flashImage.color != Color.clear)
		{
			flashImage.color = Color.Lerp(flashColour, Color.clear, (Time.time - startTime)/flashTime);
			yield return new WaitForEndOfFrame();
		}
	}

	public void VibrateController(PlayerIndex playerIndex, float leftIntensity, float rightIntensity, float duration)
	{
		if(!allowVibrationThisSession || InputManager.instance.inputFrom != InputManager.InputFrom.controller)
			return;
		
		//we run the Coroutine from here (instead of directly) because if the caller dies in the game, the vibration may never finish (Vibration Stop())
		StartCoroutine(VibrationStart(playerIndex, leftIntensity, rightIntensity, duration));
	}

	IEnumerator VibrationStart(PlayerIndex playerIndex, float leftIntensity, float rightIntensity, float duration)
	{
		GamePad.SetVibration(playerIndex, leftIntensity, rightIntensity);
		yield return new WaitForSeconds(duration);
		VibrationStop();
	}

	public void VibrationStop()
	{
		GamePad.SetVibration(0, 0, 0);
	}

	public void ToggleVibration()
	{
		allowVibrationThisSession = allowVibrationToggleSwitch.isOn;
		string value = allowVibrationThisSession == false? "false": "true";
		PlayerPrefsManager.SetVibrateKey(value);
	}

	public void ToggleHints()
	{
		useHintsThisSession = useHintsToggleSwitch.isOn;
		string value = useHintsThisSession == false? "false": "true";
		PlayerPrefsManager.SetHintsKey(value);
	}

	public Transform NextFreeAvatarsPanelUI()
	{
		Transform selectedPanel = avatarsPanelUI[nextFreePanel];
		nextFreePanel ++;
		return selectedPanel;
	}

	public void MoveCanvasToFront(Canvas subjectCanvas)
	{
		List<Canvas> allRelevantCanvases = GetAllCanvases();
		int frontmostCanvas = 0;
		for(int i = 0; i < allRelevantCanvases.Count; i++)
		{
			if(allRelevantCanvases[i].sortingOrder > frontmostCanvas)
				frontmostCanvas = allRelevantCanvases[i].sortingOrder;
		}
		subjectCanvas.sortingOrder = frontmostCanvas +1;
	}
	public void MoveCanvasToRear(Canvas subjectCanvas)
	{
		List<Canvas> allRelevantCanvases = GetAllCanvases();
		int rearmostCanvas = 0;
		for(int i = 0; i < allRelevantCanvases.Count; i++)
		{
			if(allRelevantCanvases[i].sortingOrder < rearmostCanvas)
				rearmostCanvas = allRelevantCanvases[i].sortingOrder;
		}
		subjectCanvas.sortingOrder = rearmostCanvas -1;
	}
	List<Canvas> GetAllCanvases()
	{
		Canvas[] allCanvases = FindObjectsOfType<Canvas>();
		List<Canvas> screenspaceCanvases = new List<Canvas>();
		foreach(Canvas canvas in allCanvases)
		{
			if(canvas.renderMode != RenderMode.WorldSpace)
				screenspaceCanvases.Add(canvas);
		}
		return screenspaceCanvases;
	}

	public void AlterTimeScale(float newTimescale)
	{
		Time.timeScale = newTimescale;
		Time.fixedDeltaTime = normalFixedDeltaTime * newTimescale;
	}

}

class FadeStats
{
	public float fadeDelay;
	public float fadeDuration;
	public Color fadeFromColour;
	public Color fadeToColour;
}