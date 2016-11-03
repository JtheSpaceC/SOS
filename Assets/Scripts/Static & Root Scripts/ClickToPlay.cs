using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class ClickToPlay : MonoBehaviour
{
	public static ClickToPlay instance;

	public bool pCanPause = false;
	[SerializeField] bool escKeyCanQuitDirectly = false;
	 public bool escCanGiveQuitMenu = false;
	[HideInInspector]public bool escMenuIsShown = false;
	public bool disablePlayerSelectButtonForMenu = false;
	public WeaponsPrimaryFighter playerShootScript; 
	public bool rCanRestart = false;
	public bool paused = false;
	public GameObject escCanvas;
	public GameObject escMenuPanel;
	public GameObject pauseScreen;
	public Image theSlidesCanvasImage;
	public GameObject quitConfirmationWindow;

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

		if (escKeyCanQuitDirectly && Input.GetKeyDown(KeyCode.Escape))
			QuitGame();

		if (rCanRestart && Input.GetKeyDown(KeyCode.R))
		{
			RestartLevel();
		}

		if(escCanGiveQuitMenu && !escMenuIsShown && Input.GetButtonDown("Cancel"))
		{
			EscMenu();
		}
		else if(escCanGiveQuitMenu && escMenuIsShown && Input.GetButtonDown("Cancel"))
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
		Tools.instance.CommenceFade(0, blackoutTime, Color.clear, Color.black, true);
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
		//this next part prevents the player ship from firing if you 'Resume' from the Escape menu with a fire button (spacebar, A)
		if (disablePlayerSelectButtonForMenu && GameObject.FindGameObjectWithTag("PlayerFighter") != null && playerShootScript)
			playerShootScript.allowedToFire = false;

		if(InputManager.instance.inputFrom == InputManager.InputFrom.keyboardMouse)
			Cursor.visible = true;

		CameraTactical.instance.canAccessTacticalMap = false;

		Tools.instance.VibrationStop();
		AudioMasterScript.instance.MuteSFX();
		escCanvas.SetActive(true);
		escMenuPanel.SetActive(true);
		Tools.instance.blackoutPanel.color = Color.Lerp(Color.clear, Color.black, 0.8f);
		Tools.instance.MoveCanvasToFront(escCanvas.GetComponent<Canvas>());
		Tools.instance.AlterTimeScale(0);
		paused = true;

		escMenuIsShown = true;
	}

	public void ResumeFromEscMenu()
	{
		if (disablePlayerSelectButtonForMenu && GameObject.FindGameObjectWithTag("PlayerFighter") != null && playerShootScript)
			StartCoroutine (playerShootScript.AllowedToFire ());

		foreach (GameObject screen in screensToDisableOnResume)
		{
			screen.SetActive(false);
		}
		Tools.instance.blackoutPanel.color = Color.clear;

		CameraTactical.instance.canAccessTacticalMap = true;
		AudioMasterScript.instance.ZeroSFX();

		if(!CameraTactical.instance.mapIsShown)
		{
			Tools.instance.AlterTimeScale(1);
			paused = false;
		}

		escMenuIsShown = false;
		Tools.instance.MoveCanvasToRear(escCanvas.GetComponent<Canvas>());
	}

	public void RestartLevel()
	{
		ResumeFromEscMenu ();
		Time.timeScale = 1;
		SceneManager.LoadScene (SceneManager.GetActiveScene().name);
	}

	public void QuitGame()
	{
		if(Tools.instance)
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

	public void QuitGameWithWarning()
	{
		if(Tools.instance)
			Tools.instance.VibrationStop();

		quitConfirmationWindow.SetActive(true);
	}

	public void QuitToMainMenu()
	{
		if(Tools.instance)
			Tools.instance.VibrationStop();

		#if UNITY_EDITOR //if in the editor and we haven't added all the right scenes to the Build queue, this will just exit
		if(SceneManager.sceneCount < 2)
			UnityEditor.EditorApplication.isPlaying = false;		
		#endif

		Time.timeScale = 1;
		SceneManager.LoadScene(1);
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

	void ChangeControlsImage () //TODO: This is used in Demo level but is irrelevant in other scenes. Remove when done with Demo level
	{
		if(controlsImage == null)
			return;
		Sprite imageToShow = PlayerPrefsManager.GetControllerStickBehaviourKey () == "StickPoints" ? controls2 : controls1;
		controlsImage.sprite = imageToShow;
	}

	public void GoToWebsite(string website)
	{
		Application.OpenURL("http://"+ website);
	}

}