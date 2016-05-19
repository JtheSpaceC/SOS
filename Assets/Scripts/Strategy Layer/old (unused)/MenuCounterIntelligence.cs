using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuCounterIntelligence : MonoBehaviour {

	public static MenuCounterIntelligence instance;

	public bool counterIntelCentreActive = false;
	public GameObject blockerButton;


	void Awake () 
	{
		if (instance == null) 
		{
			instance = this;
		}
		else 
		{
			Debug.LogError("Duplicate instances " + gameObject.name);
			Destroy (gameObject);
		}

		if (counterIntelCentreActive)
			blockerButton.SetActive (false);
	}
	

	void Update () {
	
	}
}
