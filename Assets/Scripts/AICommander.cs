﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AICommander : MonoBehaviour {

	public enum WhichSide {Enemy, Ally};
	public WhichSide whichSide;

	public List<GameObject> myFighters;
	public List<GameObject> myBombers;
	public List<GameObject> myTransports;
	public List<GameObject> myAssaultShuttles;
	public List<GameObject> myCapShips;
	public List<GameObject> myTurrets;

	public List<GameObject> knownEnemyFighters;
	public List<GameObject> knownEnemyBombers;
	public List<GameObject> knownEnemyTransports;
	public List<GameObject> knownEnemyTurrets;

	public int kills = 0;
	public int losses = 0;

	public string[] squadronNames;
	int squadronsNamedAlready = 0;


	void Awake()
	{
		if(whichSide == WhichSide.Ally && GameObject.FindGameObjectWithTag("PlayerFighter") !=null)
		{
			myFighters.Add(GameObject.FindGameObjectWithTag("PlayerFighter"));
		}
	}

	public GameObject ClosestTarget(List<GameObject> whichTargetList, Vector2 positionOfAsker)
	{
		if (whichTargetList.Count == 0)
			return null;

		float closestDist = Mathf.Infinity;
		GameObject resultingTarget = null;

		foreach(GameObject target in whichTargetList)
		{
			float thisDist = Vector2.Distance(target.transform.position, positionOfAsker);

			if(thisDist < closestDist)
			{
				resultingTarget = target;
				closestDist = thisDist;
			}
		}
		return resultingTarget;
	}

	public void RequestOrders(AIFighter askerScript)
	{
		if(knownEnemyFighters.Count >0)
		{
			askerScript.ChangeToNewState(askerScript.combatStates, new float[]{1});
		}
		else if(knownEnemyTurrets.Count > 0)
		{
			askerScript.ChangeToNewState(askerScript.combatStates, new float[]{1});
		}
		else
		{
			askerScript.ChangeToNewState(askerScript.normalStates, new float[]{1});
		}
	}

	public Vector2 AverageForcesPosition()
	{
		Vector2 avgPos = Vector2.zero;
		int i = 0;

		foreach(GameObject fighter in myFighters)
		{
			i++;
			avgPos += (Vector2)fighter.transform.position;
		}
		foreach(GameObject turret in myTurrets)
		{
			i++;
			avgPos += (Vector2)turret.transform.position;
		}

		if (i == 0)
			return Vector2.zero;
		else 
			return (avgPos /= i);
	}

	public string RequestSquadronName()
	{
		string name = squadronNames[squadronsNamedAlready];
		squadronsNamedAlready = (squadronsNamedAlready == squadronNames.Length - 1) ? 0 : squadronsNamedAlready + 1;
		return name;
	}

	public void RemoveFromMyAttackersWhenDead(GameObject theDeadAttacker)
	{
		foreach (GameObject myCraft in myFighters) 
		{
			myCraft.SendMessage ("RemoveSomeoneAttackingMe", theDeadAttacker);
		}
	}

	public void CallFullRetreat()
	{
		foreach(GameObject fighter in myFighters)
		{
			fighter.GetComponent<AIFighter>().orders = AIFighter.Orders.FullRetreat;
			fighter.GetComponent<AIFighter>().currentState = AIFighter.StateMachine.Retreat;
		}
	}

}//Mono
