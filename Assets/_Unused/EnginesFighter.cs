using UnityEngine;
using System.Collections;

namespace UnusedOldCode{

public class EnginesFighter : MonoBehaviour {

	[HideInInspector] public Rigidbody2D myRigidBody;

	Vector2 targetLook; 
	[HideInInspector]public Vector2 targetMove;
	Vector2 newTargetPosition;

	//for MOVING
	public float startSpeed = 0;
	public float normalAccelerationRate = 300;
	public float accelerationSpeed;

	public bool hasAfterburner = true;
	[Tooltip("Ship lengths (Unity Units) per second. Speedometer will read m/sec, which is 10 times this amount.")] 
	public float maxNormalVelocity = 7f;
	[Tooltip("Ship lengths (Unity Units) per second. Speedometer will read m/sec, which is 10 times this amount.")] 
	public float maxAfterburnerVelocity = 12f;

	[HideInInspector] public float maxVelocity;
	float speed;
	[HideInInspector] public Vector2 newMovementPosition;
	float powerToMainEngines;
	float angleToMovePos;

	//for ROTATION
	[Tooltip("Degrees per second")]
	public float rotatespeed = 144;

	//for FIRING SOLUTION
	float shooterToTargetDistance;
	float bulletToTargetTime;
	float shotSpeedAvg = 19f;

	//for DOCKING MANOEUVRES
	[HideInInspector] public bool linedUp = false;
	[HideInInspector] public bool secured = false;



	void Awake () 
	{
		myRigidBody = GetComponent<Rigidbody2D> ();
	}

	void Start()
	{
		maxVelocity = maxNormalVelocity;
		myRigidBody.velocity = transform.up * startSpeed;
		accelerationSpeed = normalAccelerationRate;
	}
	

	void Update () 
	{		
		//FOR CAPPING SPEED
		
		speed = myRigidBody.velocity.magnitude;
		
		if(speed >= maxVelocity)
		{
			myRigidBody.velocity = Vector3.Normalize(myRigidBody.velocity) * maxVelocity;
		}

		
	} // end of Update

	
	
	public void MoveToTarget (GameObject target, float constantForwardThrustProportion)
	{
		if (target == null || ClickToPlay.instance.paused)
			return;

		if (target.tag == "Fighter" || target.tag == "PlayerFighter") 
		{
			targetMove = target.transform.FindChild("Craft's Six").position;
			GetMovementSolution (target, targetMove, true, false);
		}
		else if(target.tag == "Turret")
		{
			targetMove = target.transform.FindChild("Craft's Six").position;
			GetMovementSolution (target, targetMove, false, false);
		}
		else
		{
			targetMove = target.transform.position;
			GetMovementSolution(target, targetMove, false, false);
		}

		#region Former Simple Engine Calculations
		/*
		//for normal thrust
		if(Vector2.Distance(transform.position, newMovementPosition) >.25f)
		{
			myRigidBody.AddForce ((newMovementPosition - (Vector2)transform.position).normalized * accelerationSpeed * Time.deltaTime); 
		}*/

		#endregion

		//New complex equation in this function
		ApplyThrust();

		//TODO: add attack patterns that send fighters away from their target at different rates
		//the higher this force, the less ships will stop and turn on the spot. They'll have wider turning circles
		//myRigidBody.AddForce (transform.up * accelerationSpeed * Time.deltaTime * constantForwardThrustProportion);

	}
	public void MoveToTarget(Vector2 waypoint, bool stopAtWaypoint)
	{
		if (ClickToPlay.instance.paused)
			return;

		GetMovementSolution (null, waypoint, false, stopAtWaypoint);

		if(Vector2.Distance(transform.position, newMovementPosition) >.25f)
		{
			float angle = Vector2.Angle ((Vector2)transform.position + myRigidBody.velocity.normalized, waypoint - (Vector2)transform.position);
			if(angle > 10)
			{
				myRigidBody.AddForce ((newMovementPosition - (Vector2)transform.position).normalized 
				                      * normalAccelerationRate * 4/4 * Time.deltaTime); 
			}
			else
			{
				myRigidBody.AddForce ((newMovementPosition - (Vector2)transform.position).normalized * accelerationSpeed * Time.deltaTime); 
			}
		}
	}

	void ApplyThrust()
	{
		#region New Complex Engine Calculations

		//Note: Based on main engines having 6-10 power, and front and side thrusters having x2 1Power thrusters per side
		powerToMainEngines = 1;

		angleToMovePos = Vector2.Angle(transform.up, newMovementPosition - (Vector2)transform.position);
		if(Vector3.Cross(transform.up, (newMovementPosition - (Vector2)transform.position)).z > 0)
			angleToMovePos = 360 - angleToMovePos;

		//First see if reverse Thrust is needed
		if(angleToMovePos > 90 && angleToMovePos < 270)
		{
			myRigidBody.AddForce(-transform.up * 0.2f * accelerationSpeed * Time.deltaTime);
			powerToMainEngines = 0;
		}

		//Then see if we need lateral thrust, first left side firing, then right
		if(angleToMovePos > 5 && angleToMovePos < 175)
		{
			myRigidBody.AddForce(transform.right * 0.2f * accelerationSpeed * Time.deltaTime);
			powerToMainEngines -= 0.2f;
		}
		else if(angleToMovePos > 185 && angleToMovePos < 355)
		{
			myRigidBody.AddForce(-transform.right * 0.2f * accelerationSpeed * Time.deltaTime);
			powerToMainEngines -= 0.2f;
		}

		//Lastly, see if we should thrust forward with whatever power is left
		if(powerToMainEngines > 0 && Vector2.Angle(transform.up, newMovementPosition - (Vector2)transform.position) <30)
		{
			myRigidBody.AddForce(transform.up * powerToMainEngines * accelerationSpeed * Time.deltaTime);
		}

		Debug.DrawLine(transform.position, newMovementPosition, Color.red);

		#endregion
	}
	
	
	void GetMovementSolution(GameObject target, Vector2 moveToWhere, bool hasRigidbody2D, bool stopAtWaypoint)
	{	
		if(hasRigidbody2D)
		{
			newMovementPosition = moveToWhere + (target.GetComponent<Rigidbody2D>().velocity *1) 
				- (myRigidBody.velocity *1);
			//AdjustForAfterburner(myRigidBody,target.GetComponent<Rigidbody2D>()); 
		}
		else if(target != null && target.tag == "FormationPosition")
		{
			newMovementPosition = moveToWhere + (target.transform.parent.parent.parent.GetComponent<Rigidbody2D>().velocity *1) 
				- (myRigidBody.velocity *1);
			//AdjustForAfterburner(myRigidBody,target.transform.parent.parent.parent.GetComponent<Rigidbody2D>()); 
		}
		else if(target != null && target.tag == "Turret")
		{
			newMovementPosition = moveToWhere + (target.transform.parent.parent.GetComponent<Rigidbody2D>().velocity *1) 
				- (myRigidBody.velocity *1);
			//AdjustForAfterburner(myRigidBody,target.transform.parent.parent.parent.GetComponent<Rigidbody2D>()); 
		}
		else if(target == null)
		{
			if(stopAtWaypoint)
				newMovementPosition = moveToWhere - (myRigidBody.velocity * myRigidBody.velocity.magnitude);
			else
				newMovementPosition = moveToWhere - myRigidBody.velocity;
		}
		else
		{ 
			//so this is a target with no rigidbody which isn't a formation position, which shouldn't happen
			newMovementPosition = moveToWhere;
			Debug.LogError ("ERROR: MOVE TO WHERE. "+ gameObject.name + " to " + target.name);
			//GetComponent<AIFighter>().target.SendMessage("RemoveSomeoneAttackingMe", this.gameObject, SendMessageOptions.DontRequireReceiver); 
			//GetComponent<AIFighter>().target = null;
		}
	}

	public void LookAtTarget(GameObject target)
	{
		if (target != null)
		{
			if (target.layer == LayerMask.NameToLayer("PMCFighters") || target.layer == LayerMask.NameToLayer("EnemyFighters")
			    || target.tag == "Turret")		
			{
				GetFiringSolution (target);
				targetLook = newTargetPosition;
			}
		}
		else if(targetMove != Vector2.zero)
		{
			targetLook = targetMove;
		}
		else 
			targetLook = transform.up;
		
		Vector3 dir = (Vector3)targetLook - transform.position; 
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
		Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
		
		if(!System.Single.IsNaN(angle) && !ClickToPlay.instance.paused)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, q, (Time.deltaTime * rotatespeed));
			//transform.rotation = Quaternion.Slerp(transform.rotation, q, (Time.deltaTime * rotatespeed));
		}
	}


	public void LookAtTarget(Vector2 waypoint)
	{
		Vector3 dir = (Vector3)waypoint - transform.position; 
		float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
		Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
		
		if(!System.Single.IsNaN(angle) && !ClickToPlay.instance.paused)
		{
			transform.rotation = Quaternion.RotateTowards(transform.rotation, q, (Time.deltaTime * rotatespeed));
		}
	}

	
	void GetFiringSolution(GameObject target)
	{
		if(target != null)
		{
			shooterToTargetDistance = (newTargetPosition - (Vector2)transform.position).magnitude;
			
			bulletToTargetTime = shooterToTargetDistance / shotSpeedAvg;
			
			if(System.Single.IsNaN(bulletToTargetTime))
			{
				newTargetPosition = new Vector2 (target.transform.position.x, target.transform.position.y);
				Debug.Log("Rotation Error Overcome by " + gameObject.name + " targeting " + target.name);
			}
			else
			{
				if(target.GetComponent<Rigidbody2D>())
				{
					newTargetPosition = (Vector2)target.transform.position + 
						(target.GetComponent<Rigidbody2D>().velocity*bulletToTargetTime) - 
							(myRigidBody.velocity*bulletToTargetTime ); //this was the ideal
				}
				else
				{
					newTargetPosition = (Vector2)target.transform.position + 
						(target.transform.parent.parent.GetComponent<Rigidbody2D>().velocity*bulletToTargetTime) 
							- (myRigidBody.velocity*bulletToTargetTime );
				}
			}
		}
		else
		{
			Debug.Log("ERROR: Target was NULL. Firing solution becomes (0,0)");
			newTargetPosition = Vector2.zero;
		}
	}


	void AdjustForAfterburner(Rigidbody2D shipRB, Rigidbody2D targetRB)
	{
		/*if (!hasAfterburner)
			return;

		float angle = Vector2.Angle (shipRB.velocity.normalized, targetRB.velocity.normalized);
		if(angle > 45)
		{
			accelerationSpeed = normalAccelerationRate * 1.5f;
			print ("angle > 45, using afterburner");
			return;
		}
		else 
		{
			if(targetRB.velocity.magnitude > shipRB.velocity.magnitude / 1.25f)
			{
				accelerationSpeed = normalAccelerationRate * 1.5f;
				print ("same direction, but target is faster, using afterburner");
				return;
			}
		}
		accelerationSpeed = normalAccelerationRate;
		print ("normal acceleration");*/
		//accelerationSpeed = normalAccelerationRate * 1.5f;
	}
}//Mono
}