using UnityEngine;
using System.Collections;

public class AIAssaultShuttle : SupportShipFunctions {

	public enum StateMachine {Holding, Retrieving, Docking, Docked, Moving, WarpIn, WarpOut};
	[Header("States")]

	public StateMachine currentState;
	public StateMachine previousState;

	public Transform dockTarget;
	public Vector2 targetPosition;

	float dockTimer;
	public float dockTime = 1.5f;


	void Awake()
	{
		healthScript = GetComponent<HealthTransport> ();
		engineScript = GetComponent<EnginesFighter> ();
		myRigidbody = GetComponent<Rigidbody2D>();

		if (whichSide == WhichSide.Ally)
		{
			myCommander = GameObject.FindGameObjectWithTag("AIManager").transform.FindChild("PMC Commander").GetComponent<AICommander> ();
			enemyCommander = GameObject.FindGameObjectWithTag("AIManager").transform.FindChild("Enemy Commander").GetComponent<AICommander> ();
		}
		else if (whichSide == WhichSide.Enemy)
		{
			myCommander = GameObject.FindGameObjectWithTag("AIManager").transform.FindChild("Enemy Commander").GetComponent<AICommander> ();
			enemyCommander = GameObject.FindGameObjectWithTag("AIManager").transform.FindChild("PMC Commander").GetComponent<AICommander> ();
		}

		myCommander.myAssaultShuttles.Add (this.gameObject);
		foreach(GameObject turret in myTurrets)
		{
			if(!myCommander.myTurrets.Contains(turret))
			{
				myCommander.myTurrets.Add(turret);
			}
		}
	}


	void Update()
	{
		if(currentState == StateMachine.Holding)
		{
			HoldPosition();
		}
		else if(currentState == StateMachine.Moving)
		{
			if(!completedState)
				MoveToPosition(targetPosition);
			else
				ChangeToNewState(StateMachine.Holding);
		}
		else if(currentState == StateMachine.Docking)
		{
			Docking();
		}
		else if(currentState == StateMachine.WarpOut)
		{
			warpDrive.WarpOut();
		}
	}

	public void ChangeToNewState(StateMachine newState)
	{
		previousState = currentState;

		warpBubble.enabled = false;

		if(newState != StateMachine.Docked)
			transform.SetParent(null);

		currentState = newState;
		switchingState = true;
		completedState = false;
	}

	void Docking()
	{
		if(switchingState)
		{
			switchingState = false;
		}
		targetPosition = dockTarget.position;

		engineScript.LookAtTarget(targetPosition + (Vector2)dockTarget.up * 5);
		engineScript.MoveToTarget(targetPosition, true);

		if(Vector2.Distance(transform.position, targetPosition) < 0.1f)
		{
			dockTimer += Time.deltaTime;
			if(dockTimer >= dockTime)
			{
				transform.position = dockTarget.position;
				myRigidbody.velocity = Vector2.zero;
				Subtitles.instance.PostSubtitle(new string[] {name + " has docked with " + dockTarget.transform.root.name});
<<<<<<< HEAD

				if(dockTarget.tag == "PlayerEVA" || dockTarget.tag == "PilotEVA")
				{
					dockTarget.SetParent(transform);
					dockTarget.gameObject.SetActive(false);

					if(dockTarget.tag == "PlayerEVA")
					{
						_battleEventManager.instance.CallPlayerRescued();
					}
					else
						ChangeToNewState(StateMachine.Docked);					
				}
				else
				{
					transform.parent = dockTarget;
					transform.position = dockTarget.position;
					myRigidbody.velocity = Vector2.zero;
					myRigidbody.isKinematic = true;
					ChangeToNewState(StateMachine.Docked);
				}
=======
				transform.parent = dockTarget;
				ChangeToNewState(StateMachine.Docked);
>>>>>>> parent of ac76538... EVA Pilot, New Warp script. Assault Shuttle work.
			}
		}
		else 
			dockTimer = 0;
	}

	void Docked()
	{
		
	}
}
