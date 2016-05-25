using UnityEngine;
using System.Collections;

public class Persistence : MonoBehaviour {


	void Awake()
	{
		DontDestroyOnLoad(this.gameObject);
	}
	
	void Update () 
	{
		if(Input.GetKeyDown(KeyCode.X))
		{
			ClickToPlay.instance.LoadScene("rts_CAG walkabout");
		}
	}

	void OnLevelWasLoaded()
	{
		if(CAGManager.instance != null)
		{
			CAGManager.instance.EnableLeaveSquadronHQ(true);
			Destroy(gameObject);

		}
	}
}
