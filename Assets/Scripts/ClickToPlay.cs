using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ClickToPlay : MonoBehaviour
{
	public static ClickToPlay instance;

	public bool pCanPause = true;
	public bool escCanQuit = true;
	public bool escGivesQuitMenu = true;
	[HideInInspector]public bool escMenuIsShown = false;
	public bool disablePlayerSelectButtonForMenu = true;
	public WeaponsPrimaryFighter playerShootScript; 
	public bool rCanRestart = true;
	public bool paused = false;
	public GameObject escScreen;
	public GameObject pauseScreen;
	public Image theSlidesCanvasImage;

	[Tooltip("Screens to turn off when going back to game from Esc menu, like options")] 
	public GameObject[] screensToDisableOnResume;

	[Header ("Images stuff")]
	public Image controlsImage;
	public Sprite controls1;
	public Sprite controls2;

	[Header ("For doing a Presentation or Slide Show")]
	public bool usingSlides = false;
	public Sprite[] slides;
	public int whichSlideToShow = 0;

	void Awake()
	{
		if(instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
		Cursor.visible = true;

		ChangeControlsImage();

	}

	void Start()
	{
		if (usingSlides)
			NextSlide(0);
	}

	void Update()
	{
		if (pCanPause && Input.GetKeyDown(KeyCode.P))
		{
			TogglePause();    
		}

		if (escCanQuit && Input.GetKeyDown(KeyCode.Escape))
			QuitGame();

		if (rCanRestart && Input.GetKeyDown(KeyCode.R))
		{
			RestartLevel();
		}

		if(escGivesQuitMenu && !escMenuIsShown && Input.GetButtonDown("Cancel"))
		{
			EscMenu();
		}
		else if(escGivesQuitMenu && escMenuIsShown && Input.GetButtonDown("Cancel"))
		{
			ResumeFromEscMenu();
		}
	}

	public void LoadScene(int whichScene)
	{
		SceneManager.LoadScene(whichScene);
	}
	public IEnumerator LoadScene(int whichScene, float blackoutTime)
	{
		Tools.instance.CommenceFadeout(blackoutTime);
		yield return new WaitForSeconds(blackoutTime);
		SceneManager.LoadScene(whichScene);
	}
	public void LoadScene(string whichScene)
	{
		SceneManager.LoadScene(whichScene);
	}

	public void TogglePause()
	{
		if (!paused)
		{
			Time.timeScale = 0;
			paused = true;
			Tools.instance.VibrationStop();
			if (pauseScreen != null)
				pauseScreen.SetActive(true);
		}
		else if (paused)
		{
			Time.timeScale = 1;
			paused = false;
			if (pauseScreen != null)
				pauseScreen.SetActive(false);
		}
	}

	public void EscMenu()
	{
		if (disablePlayerSelectButtonForMenu && GameObject.FindGameObjectWithTag("PlayerFighter") != null)
			playerShootScript.allowedToFire = false;

		CameraTactical.instance.canAccessTacticalMap = false;

		Tools.instance.VibrationStop();
		AudioMasterScript.instance.MuteSFX();
		escScreen.SetActive(true);
		Time.timeScale = 0;
		paused = true;

		escMenuIsShown = true;
	}

	public void ResumeFromEscMenu()
	{
		if (disablePlayerSelectButtonForMenu && GameObject.FindGameObjectWithTag("PlayerFighter") != null)
			StartCoroutine (playerShootScript.AllowedToFire ());

		foreach (GameObject screen in screensToDisableOnResume)
		{
			screen.SetActive(false);
		}

		CameraTactical.instance.canAccessTacticalMap = true;
		AudioMasterScript.instance.ZeroSFX();

		if(!CameraTactical.instance.mapIsShown)
		{
			Time.timeScale = 1;
			paused = false;
		}

		escMenuIsShown = false;
	}

	public void RestartLevel()
	{
		ResumeFromEscMenu ();
		Time.timeScale = 1;
		SceneManager.LoadScene (SceneManager.GetActiveScene().name);
	}

	public void QuitGame()
	{
		Tools.instance.VibrationStop();

		#if UNITY_STANDALONE
		//Quit the application
		Application.Quit();
		#endif

		#if UNITY_WEBPLAYER
		Destroy(GameObject.Find("_AUDIO MANAGER"));
		SceneManager.LoadScene(0);		
		#endif

		//If we are running in the editor
		#if UNITY_EDITOR
		//Stop playing the scene
		UnityEditor.EditorApplication.isPlaying = false;
		#endif		
	}

	public void NextSlide(int fwdOrBackInt)
	{
		whichSlideToShow += fwdOrBackInt;

		if (whichSlideToShow < 0)
			whichSlideToShow = 0;

		else if (whichSlideToShow == slides.Length)
			whichSlideToShow -= 1;

		theSlidesCanvasImage.sprite = slides[whichSlideToShow];
	}

	public void DestroyMusicManager()
	{
		Destroy(GameObject.FindGameObjectWithTag("MusicManager"));
	}

	public void ChangeControlScheme(int which)
	{
		switch(which)
		{
		case 1:
			try{
				PlayerPrefsManager.SetControllerStickBehaviourKey("StickRotates");
			}catch{};
			break;
		case 2:
			try{
				PlayerPrefsManager.SetControllerStickBehaviourKey("StickPoints");}catch{};
			break;
		default:
			Debug.Log("Invalid control scheme selected");
			break;
		}

		ChangeControlsImage ();
	}

	void ChangeControlsImage ()
	{
		Sprite imageToShow = PlayerPrefsManager.GetControllerStickBehaviourKey () == "StickPoints" ? controls2 : controls1;
		controlsImage.sprite = imageToShow;
	}
}