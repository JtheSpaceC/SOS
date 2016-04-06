using UnityEngine;
using System.Collections;

public class AICapital : MonoBehaviour {

	EnginesFighter myEngines;

	public GameObject target;


	void Awake () {
		myEngines = GetComponent<EnginesFighter> ();
		target = GameObject.FindGameObjectWithTag ("Player");
	}
	

	void Update () 
	{
		myEngines.LookAtTarget (target);
		myEngines.MoveToTarget (target, 0);
	}
}
