using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class RTSDirector : MonoBehaviour {

	public static RTSDirector instance;

	[Tooltip ("This is used by the UI Resizer to check if it should be resizing sprites or not.")] public bool viewingStrategyMap = true;

	[Header("Timekeeping stuff")]
	float startingDay = 42;
	public float gameDay;
	float scratchTimer;
	public float secondsToDays = 120;
	public float gameSpeed = 1;

	public Slider daySlider;
	public Color timeNotSelectedColour;
	public Color timeSelectedColour;
	public Image[] timeButtonImages;
	public Text dayText;

	[Header("Currency")]
	public Text currencyText;
	[HideInInspector] public float creditsOwned;
	[HideInInspector] public float newCreditsPerPeriod;

	[Header("Other")]
	public string solarSystemName = "";

	[HideInInspector] public bool mouseIsOverSomething = false;

	public GameObject tooltipCanvas;
	Text tooltipText;


	void Awake () 
	{
		if (instance == null)
			instance = this;
		else
			Destroy (gameObject);

		tooltipText = tooltipCanvas.GetComponentInChildren<Text> ();

		gameDay = startingDay;
		ChangeGameSpeed (1);

		UpdateCurrencyDisplay ();
	}


	public void PostTooltip(string message, Vector3 where)
	{
		tooltipText.text = message;
		tooltipCanvas.transform.position = where;
		tooltipCanvas.gameObject.SetActive (true);
	}


	public void ChangeGameSpeed(int whatSpeed)
	{
		foreach (Image im in timeButtonImages)
			im.color = timeNotSelectedColour;

		gameSpeed = whatSpeed;

		switch (whatSpeed) 
		{
			default:
			Debug.LogError("wrong speed selected");
			break;
		case 0:
			timeButtonImages[0].color = timeSelectedColour;
			break;
		case 1:
			timeButtonImages[1].color = timeSelectedColour;
			break;
		case 10:
			timeButtonImages[2].color = timeSelectedColour;
			break;
		case 100:
			timeButtonImages[3].color = timeSelectedColour;
			break;
		}
	}
	

	void Update () 
	{
		if (Input.GetKeyDown (KeyCode.F12))
			SceneManager.LoadScene (1);
		if (Input.GetKeyDown (KeyCode.F11))
			SceneManager.LoadScene (0);

		if (!ClickToPlay.instance.paused)
			gameDay += Time.deltaTime * gameSpeed / secondsToDays;

		scratchTimer = gameDay;
		while (scratchTimer > 1)
			scratchTimer -= 1;

		daySlider.value = scratchTimer;
		dayText.text = "Day " + Mathf.FloorToInt (gameDay);

		if(Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Space))
		{
			ChangeGameSpeed(0);
		}
		if(Input.GetKeyDown(KeyCode.Alpha2))
		{
			ChangeGameSpeed(1);
		}
		if(Input.GetKeyDown(KeyCode.Alpha3))
		{
			ChangeGameSpeed(10);
		}
		if(Input.GetKeyDown(KeyCode.Alpha4))
		{
			ChangeGameSpeed(100);
		}
	}


	public void UpdateCurrencyDisplay()
	{
		//TODO: Draw currency info from save file
		currencyText.text = "c" + Mathf.FloorToInt (creditsOwned) + "\n" +
			"+c" + Mathf.FloorToInt (newCreditsPerPeriod) + "/per";
	}
}
