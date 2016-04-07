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

	Rigidbody2D targetRB;
	Vector2 dockingLookPos;


	void Awake()
	{
		healthScript = GetComponent<HealthTransport> ();
		engineScript = GetComponent<EnginesFighter> ();
		warpDrive = GetComponentInChildren<WarpDrive>();
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

		warpDrive.warpBubble.enabled = false;
		dockingLookPos = Vector2.zero;

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
			if(dockTarget.parent != null)
				targetRB = dockTarget.transform.parent.transform.parent.transform.parent.GetComponent<Rigidbody2D>();
			else
			{
				targetRB = dockTarget.GetComponent<Rigidbody2D>();
				dockingLookPos = (Vector2)(dockTarget.position - transform.position).normalized;
			}
		}
		targetPosition = dockTarget.position;

		if(dockingLookPos != Vector2.zero)
			engineScript.LookAtTarget((Vector2)transform.position + dockingLookPos);
		else
			engineScript.LookAtTarget(targetPosition + (Vector2)dockTarget.up * 5);
		
		MatchTargetVelocity((Vector2)dockTarget.position, targetRB, Vector2.zero);

		if(Vector2.Distance(transform.position, targetPosition) < 0.2f)
		{
			dockTimer += Time.deltaTime;
			if(dockTimer >= dockTime)
			{
				Subtitles.instance.PostSubtitle(new string[] {name + " has docked with " + dockTarget.transform.root.name});

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
			}
		}
		else 
			dockTimer = 0;
	}

	void Docked()
	{
		
	}

	void ReleaseFromDock()
	{
		transform.SetParent(null);
		myRigidbody.isKinematic = false;
	}
}
