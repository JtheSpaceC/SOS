using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class MainMenu : MonoBehaviour {

	public GameObject quitConfirmationWindow;
	public GameObject eventSystem;

	void Awake()
	{
		EventSystem[] es = FindObjectsOfType<EventSystem>();
		if(es.Length > 1)
		{
			Destroy(eventSystem);
		}
	}

	public void GoToWebsite(string website)
	{
		Application.OpenURL("http://"+ website);
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


	public void NewGame()
	{
		if(FindObjectOfType<DemoSelfPlayingLevel>() != null)
		{
			FindObjectOfType<DemoSelfPlayingLevel>().LoadDemo();
			AudioMasterScript.instance.FadeChannel("Master vol", 0, 0, 0.3f);
			this.gameObject.SetActive(false);
		}
	}

	void OnDisable()
	{
		quitConfirmationWindow.SetActive(false);
	}
}
