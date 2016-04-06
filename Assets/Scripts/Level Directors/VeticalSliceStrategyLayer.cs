using UnityEngine;
using System.Collections;

public class VeticalSliceStrategyLayer : MonoBehaviour {

	public static VeticalSliceStrategyLayer instance;



	void Awake () 
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
			Debug.LogError("There were two _Directors. Destroying one.");
		}

	}
	

	void Update () 
	{
	
	}


}//Mono
