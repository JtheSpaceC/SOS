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
		if(CAGDirector.instance != null)
		{
			CAGDirector.instance.EnableLeaveSquadronHQ(true);
			Destroy(gameObject);
		}
	}
}
