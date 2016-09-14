using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombatEncounter : MonoBehaviour {

	float targetDepth;
	public float zDepth = 10;
	public float speedToDepth = 5;
	public List<GameObject> fighters;
	List<GameObject> livingFighters = new List<GameObject>();

	float startTime;
	float z;
	Vector3 encounterPos;
	Vector3 fighterPos;

	int fightersAlive;
	bool bringingBackUp = false;


	void Start()
	{
		fightersAlive = fighters.Count;
		targetDepth = zDepth;
		StartCoroutine("MoveToDepth");
	}

	IEnumerator MoveToDepth()
	{
		startTime = Time.time;

		while(transform.position.z != targetDepth)
		{
			encounterPos = transform.position;
			if(!bringingBackUp)
			{
				z = Mathf.Lerp(0, zDepth, (Time.time - startTime)/speedToDepth);
			}
			else
			{
				z = Mathf.Lerp(zDepth, 0, (Time.time - startTime)/speedToDepth);
			}
			encounterPos.z = z;
			transform.position = encounterPos;
			yield return new WaitForEndOfFrame();
		}
	}
	
	void LateUpdate () 
	{
		//check if they're dead, if not move them until at depth
		if(!bringingBackUp)
		{
			for(int i = 0; i < fighters.Count; i++)
			{
				if(fighters[i].GetComponent<Health>().dead || fighters[i].GetComponent<AIFighter>().inRetreatState)
				{
					if(HowManyLeftAlive() == 1)
					{
						bringingBackUp = true;
						targetDepth = 0;
						StopCoroutine("MoveToDepth");
						StartCoroutine("MoveToDepth");
					}
				}
				else
				{

					//keep them on the layer
					fighterPos = fighters[i].transform.position;
					fighterPos.z = transform.position.z;
					fighters[i].transform.position = fighterPos;
				}
			}
		}
		//if so, bring the survivor up
		if(bringingBackUp)
		{
			for(int i = 0; i < livingFighters.Count; i++)
			{
				//keep them on the layer
				fighterPos = livingFighters[i].transform.position;
				fighterPos.z = transform.position.z;
				livingFighters[i].transform.position = fighterPos;
			}
		}



	}

	int HowManyLeftAlive()
	{
		int alive = 0;
		livingFighters.Clear();

		foreach(GameObject fighter in fighters)
		{
			if(!fighter.GetComponent<Health>().dead)
			{
				alive++;
				livingFighters.Add(fighter);
			}
		}
		return alive;
	}
}
