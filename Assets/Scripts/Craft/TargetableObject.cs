using UnityEngine;
using System.Collections.Generic;

public class TargetableObject : MonoBehaviour {

	[HideInInspector]public AICommander myCommander;
	[HideInInspector]public AICommander enemyCommander;
	[HideInInspector]public WarpDrive warpDrive;

	[HideInInspector] public Rigidbody2D myRigidbody;

	public enum WhichSide {Enemy, Ally};
	public WhichSide whichSide;
	[HideInInspector] public LayerMask friendlyFireMask;
	protected LayerMask potshotAtEnemiesMask;
	LayerMask potshotsPlusFriendliesMask;

	protected bool switchingState = true;
	protected bool completedState = false;

	public List<GameObject> myAttackers;	
	public int kills = 0;

	protected Vector3 targetLook;
	float firingAngle = 25;

	protected string myActivity;
	protected Vector3 warpOutLookAtPoint;
	protected Vector3 literalSpawnPoint;
	[HideInInspector] public Vector3 insertionPoint; //where to warp in to

	[HideInInspector] public GameObject myGui;

	public float accuracy = 1f;
	

	protected void SetUpSideInfo () {

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

		friendlyFireMask = myCommander.fighterFriendlyFireMask;
		potshotAtEnemiesMask = myCommander.fighterPotshotMask;
		potshotsPlusFriendliesMask = friendlyFireMask + potshotAtEnemiesMask;

		myCommander.myTurrets.Add(this.gameObject);
		enemyCommander.knownEnemyTurrets.Add(this.gameObject);
	}


	protected Vector2 PatrolPoint(Vector2 centralPoint, float distance)
	{
		return centralPoint + (Random.insideUnitCircle.normalized * distance);
	}


	protected bool TakePotshot(Transform forwardOfThisObject, float range)
	{
		
		RaycastHit2D hit = Physics2D.CircleCast((transform.position+(forwardOfThisObject.up*1.35f)),
		                                        0.75f, forwardOfThisObject.up, range, potshotsPlusFriendliesMask);
		
		if(hit.collider != null && StaticTools.IsInLayerMask(hit.collider.gameObject, potshotAtEnemiesMask))
		{
			return true;
		}		
		else
		{
			return false;
		}
	}

	
	protected bool ReadyAimFire(GameObject targetInQuestion, Transform forwardOfThisObject, float range)
	{
		if (targetInQuestion == null)
			return false;
		
		if(Vector2.Distance(transform.position, targetInQuestion.transform.position) > range)
		{
			return false; //target is out of range
		}
		
		targetLook = targetInQuestion.transform.position;
		
		Vector3 forward = forwardOfThisObject.up;
		
		//for Aiming
		if(targetInQuestion != null)
		{
			Vector2 targetDir = (targetLook - transform.position).normalized;

			if (AmILookingAt(targetLook, forward, firingAngle))
			{
				RaycastHit2D hit = Physics2D.CircleCast((forwardOfThisObject.position+(forward*1.35f)),
                            0.75f, targetDir, Vector2.Distance(transform.position,targetInQuestion.transform.position), friendlyFireMask);
				
				if(hit.collider != null && hit.collider.gameObject == this.gameObject)
				{
					Debug.Log ("ERROR: I CircleCast hit myself");
					return false;
				}
				else if(hit.collider != null)
				{
					return false;
				}
				else
				{
					return true; //target is in range with a clear line of fire. TRUE
				}
			}
			else 
			{
				return false;
			}
			
		}
		else return false;
	}


	protected bool AmILookingAt(Vector3 targetPosition, Vector3 forwardAngleToCheckAgainst, float angleTolerance)
	{
		Vector2 targetDir = (targetPosition - transform.position).normalized;
		float angle = Vector3.Angle(targetDir, forwardAngleToCheckAgainst);

		if (angle < angleTolerance)
		{
			return true;
		}
		else 
		{
			return false;
		}
	}


	/*protected void CheckAndAddTargetToCommanderList(AICommander myCommander, GameObject target)
	{
		if (whichSide == WhichSide.Ally && myCommander.knownEnemyFighters.Count == 0)
		{
			_battleEventManager.instance.firstClashCalled = false;
			Debug.Log("Is This Used Any more?"); //delete?
		}

		if((target.tag == "Fighter" ||  target.tag == "PlayerFighter")
			&& !myCommander.knownEnemyFighters.Contains(target))
		{
			if(!target.activeInHierarchy || target.GetComponent<HealthFighter>().dead)
			{
				Debug.Log(gameObject.name + " BIG FUCKING ERROR!!!!!!");
				return;
			}
			
			//TODO: Remove once I'm sure I haven't seen this in a while - Kevin 31/8/16
			Debug.LogError("This Should Never Be Called Anymore");
			myCommander.knownEnemyFighters.Add(target);

			if(whichSide == WhichSide.Ally)
				_battleEventManager.instance.CallFirstWingmanClash();
		}
		else if(target.tag == "Turret" && !myCommander.knownEnemyTurrets.Contains(target))
		{
			if(!target.activeInHierarchy || target.GetComponent<HealthTurret>().dead)
			{
				Debug.Log(gameObject.name + " BIG FUCKING ERROR!!!!!!");
				return;
			}	
			myCommander.knownEnemyTurrets.Add(target);
		}
		else 
		{
			if(!target.activeInHierarchy || 
			   ((target.tag == "Fighter" || target.tag == "PlayerFighter") && target.GetComponent<HealthFighter>().dead) ||
			   target.tag == "Turret" && target.GetComponent<HealthTurret>().dead)
			{
				myCommander.knownEnemyFighters.Remove(target);
				Debug.Log("Removed "+target+"(INVALID) from "+ myCommander.name+" KnownFighters List");
			}
		}
	}*/

	
	void AddSomeoneAttackingMe(GameObject attacker)
	{
		if(!myAttackers.Contains(attacker))
		{
			myAttackers.Add (attacker);
		}
		//CheckAndAddTargetToCommanderList (myCommander, attacker);
	}
	void RemoveSomeoneAttackingMe (GameObject attackerWhoDiedOrChangedTargets)
	{
		if(myAttackers.Contains(attackerWhoDiedOrChangedTargets))	
		{
			myAttackers.Remove (attackerWhoDiedOrChangedTargets);
		}
	}
	void AddKill()
	{
		kills++;
		myCommander.kills++;
		
		if(whichSide == WhichSide.Ally && tag != "PlayerFighter")
		{
			int roll = Random.Range(0,11);
			if(roll >= 3)
			{
				string[] killConfirms = new string[]{"Got him!", "One less to worry about!", "Boom! Tell your friends!", "Take that!",
					"That'll teach you!"};
				Subtitles.instance.PostSubtitle(killConfirms);
				_battleEventManager.instance.CallWingmanGotAKill();

				if(GetComponent<AIFighter>() && GetComponent<AIFighter>().myCharacterAvatarScript)
					GetComponent<AIFighter>().myCharacterAvatarScript.StartCoroutine("Speaking");
			}
		}
		if(tag == "PlayerFighter")
		{
			_battleEventManager.instance.CallPlayerGotKill();
			Tools.instance.StartCoroutine(Tools.instance.TextAnim(Tools.instance.killsText, Color.green, Color.white, 0.5f));
		}
	}
}
