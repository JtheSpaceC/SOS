using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyTrace : MonoBehaviour {

	void Awake()
	{
		print(name);
	}

	void OnDestroy()
	{
		Debug.LogError("DESTROYED " + name);
	}
}
