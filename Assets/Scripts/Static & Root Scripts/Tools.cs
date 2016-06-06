using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using XInputDotNetPure;


public class Tools: MonoBehaviour
{
	public static Tools instance;

	public GameObject playerUI;
	public GameObject avatarsPanelUI;

	public ObjectPoolerScript explosionPoolerScript;
	public ObjectPoolerScript explosionMiniPoolerScript;

	public Text killsText;

	public Slider ammoRemainingSlider;
	public Text ammoRemainingText;
	public Text missilesRemainingText;

	public Slider nitroRemainingSlider;
	public Text nitroRemainingText;

	public GameObject debugCircle1;
	public GameObject debugCircle2;

	public Image blackoutPanel;
	public bool commenceFadeIn = false;
	public bool commenceFadeout = false;
	float fadeInStartTime = 0;
	float fadeoutStartTime = 0;
	public float fadeInTime = 2f;
	public float fadeOutTime = 3f;

	bool camCurrentlyOnHitSlowdown = false;

	public Color whiteOutColour;

	public Toggle allowVibrationToggleSwitch;
	bool allowVibrationThisSession = true;

	public Toggle useHintsToggleSwitch;
	[HideInInspector] public bool useHintsThisSession = true;

	[HideInInspector] public enum WaypointTypes {Extraction, Move, SearchAndDestroy, Follow, Support};

	[Header("Waypoint Prefabs")]
	public GameObject waypointPrefab;

	public Color avatarAwarenessFlashColour;
	public Color avatarHitFlashColour;


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

		AudioMasterScript.instance.StopAllCoroutines();
		AudioMasterScript.instance.ZeroSFX();
	}


	void Update()
	{
		//for screen FADEIN
		if(commenceFadeIn == true)
		{
			if(fadeInStartTime == 0)
				fadeInStartTime = Time.time;
			
			blackoutPanel.color = Color.Lerp(Color.black, Color.clear, ((Time.time - fadeInStartTime)/fadeInTime));
			if(blackoutPanel.color == Color.black)
			{
				commenceFadeout = false;
				fadeoutStartTime = 0;
			}
		}

		//for screen FADEOUT
		if(commenceFadeout == true)
		{
			if(fadeoutStartTime == 0)
				fadeoutStartTime = Time.time;
			
			blackoutPanel.color = Color.Lerp(Color.clear, Color.black, ((Time.time - fadeoutStartTime)/fadeOutTime));
			if(blackoutPanel.color == Color.black)
			{
				commenceFadeout = false;
				fadeoutStartTime = 0;
			}
		}
	}

	public void CommenceFadeIn()
	{
		commenceFadeIn = true;
		commenceFadeout = false;
	}

	public void CommenceFadeout(float newTime)
	{
		Tools.instance.blackoutPanel.GetComponentInParent<Canvas> ().sortingOrder = 10;

		commenceFadeIn = false;
		commenceFadeout = true;

		fadeOutTime = newTime;
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

	public void CreateWaypoint(WaypointTypes wpType, Vector2 position)
	{
		if(wpType == WaypointTypes.Extraction)
		{
			GameObject wp = Instantiate(waypointPrefab, position, Quaternion.identity) as GameObject;
			wp.GetComponent<PointerHUDElement>().targetWP = position;
		}
	}

	public void CreateWaypoint(WaypointTypes wpType, Transform target)
	{
		if(wpType == WaypointTypes.Follow)
		{
			
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

	public IEnumerator TextAnim(Text givenText, Color flashColour, Color returnColour, float givenTime)
	{
		givenText.color = flashColour;
		yield return new WaitForSeconds(givenTime);
		givenText.color = returnColour;
	}


	public IEnumerator HitCamSlowdown()
	{
		if(!camCurrentlyOnHitSlowdown)
		{		
			camCurrentlyOnHitSlowdown = true;
			Time.timeScale = 0.75f;

			for(int i = 0; i < 20; i++)
			{
				yield return new WaitForEndOfFrame();
			}

			Time.timeScale = 1;
			camCurrentlyOnHitSlowdown = false;
			StopCoroutine(HitCamSlowdown());
		}
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

	[ContextMenu("Clear Player Prefs")]
	public void ClearPlayerPrefs()
	{
		PlayerPrefs.DeleteAll();
	}
}