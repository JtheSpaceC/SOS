using UnityEngine;
using System.Collections;

public class ExecutionOrderExample : MonoBehaviour {

	bool updated = false;
	bool fixedUpdated = false;
	bool lateUpdated = false;


	void Awake()
	{
		print("Awake");
	}

	void OnEnable()
	{
		print("OnEnable");
	}

	void Start () 
	{
		print("Start");
	}
	
	void Update () 
	{
		if(!updated)
		{
			updated = true;
			print("Updated");
		}
	}
		
	void FixedUpdate()
	{
		if(!fixedUpdated)
		{
			fixedUpdated = true;
			print("fixedUpdated");
		}
	}

	void LateUpdate()
	{
		if(!lateUpdated)
		{
			lateUpdated = true;
			print("lateUpdated");
		}
	}

	void OnDisable()
	{
		print("OnDisable");
	}

	void OnDestroy()
	{
		print("OnDestroy");
	}
}
