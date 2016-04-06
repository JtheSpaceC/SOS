using UnityEngine;
using System.Collections;

public class PlanetsAndMoons : RTSObject {

	[Header("For Orbital Information")]
	public bool doesOrbit = true;
	public Transform orbitsWhom;
	public float orbitalDistance = 10;
	public float daysForFullOrbit = 300;

	[Range(0,360)]
	public float initialPosition = 0;

	[Header("Other")]
	public float value = -1;

	float whatDay;
	float angle;	
	float xValue;
	float yValue;

	LineRenderer lr;

	[Header("Garrison info")]
	public bool canGarrison;
	public int numFighters = 3;
	public int numTroopers = 6;
	public GameObject[] garrisonDisplays;

	
	void Start()
	{
		BaseClassAwake ();

		lr = GetComponent<LineRenderer> ();
		if (lr != null)
		{
			DrawCircle (lr);
			lr.sortingLayerName = "Planets";
			lr.sortingOrder = -10;
		}
	}


	void LateUpdate () 
	{
		if(doesOrbit)
			CalculatePosition ();

		if (lr != null && transform.parent.gameObject != transform.root.gameObject)
			DrawCircle (lr);
	}


	public void CalculatePosition()
	{
		whatDay = RTSDirector.instance.gameDay;
		angle = initialPosition + (whatDay / daysForFullOrbit * 360);
		
		if(angle >0)
		{
			while (angle >= 360)
				angle -= 360;
		}
		else if(angle < 0)
		{
			while (angle <= -360)
				angle += 360;
		}
		
		//these are for clockwise orbits
		if(angle < 90 && angle >= 0)
		{
			UpperRightQuadrant(angle);
		}
		else if (angle >= 90 && angle < 180)
		{
			angle -= 90;
			
			BottomRightQuadrant(angle);
		}
		else if(angle >= 180 && angle < 270)
		{
			angle -= 180;
			
			BottomLeftQuadrant(angle);
		}
		else if(angle >= 270)
		{
			angle -= 270;
			
			UpperLeftQuadrant(angle);
		}
		//these are for anticlockwise orbits
		else if(angle > -90 && angle < 0)
		{
			angle = 90 - angle;
			UpperLeftQuadrant(angle);
		}
		else if(angle > -180 && angle <=-90)
		{
			angle = 180 - angle;
			BottomLeftQuadrant(angle);
		}
		else if(angle > -270 && angle <= -180)
		{
			angle = 270 - angle;
			BottomRightQuadrant(angle);
		}
		else if(angle > -360 && angle <= -270)
		{
			angle = 360 - angle;
			UpperRightQuadrant(angle);
		}
	}

	void UpperRightQuadrant(float whichAngle)
	{
		xValue = Mathf.Abs (Mathf.Tan (Mathf.Deg2Rad * whichAngle) * orbitalDistance);
		transform.position = (Vector2)orbitsWhom.position + (new Vector2 (xValue, orbitalDistance).normalized * orbitalDistance);
	}

	void BottomRightQuadrant(float whichAngle)
	{
		yValue = Mathf.Abs( Mathf.Tan (Mathf.Deg2Rad * whichAngle) * orbitalDistance);
		transform.position = (Vector2)orbitsWhom.position + (new Vector2 (orbitalDistance, -yValue).normalized * orbitalDistance);
	}

	void BottomLeftQuadrant(float whichAngle)
	{
		xValue = Mathf.Abs(Mathf.Tan (Mathf.Deg2Rad * whichAngle) * orbitalDistance);
		transform.position = (Vector2)orbitsWhom.position + (new Vector2 (-xValue, -orbitalDistance).normalized * orbitalDistance);
	}

	void UpperLeftQuadrant(float whichAngle)
	{
		yValue = Mathf.Abs( Mathf.Tan (Mathf.Deg2Rad * whichAngle) * orbitalDistance);
		transform.position = (Vector2)orbitsWhom.position + (new Vector2 (-orbitalDistance, yValue).normalized * orbitalDistance);
	}

	void DrawCircle(LineRenderer lr)
	{
		int i = 0;
		Vector3 endPos = Vector3.zero;

		for(float theta = 0; theta < (2 *  Mathf.PI); theta += 0.1f)
		{
			float x = orbitsWhom.transform.position.x + (orbitalDistance * Mathf.Cos(theta));
			float y = orbitsWhom.transform.position.y + (orbitalDistance * Mathf.Sin(theta));
			Vector3 pos = new Vector3(x, y, 0);
			
			lr.SetPosition(i, pos);

			if(i == 0)
				endPos = pos;
			i += 1;
		}
		lr.SetPosition (i, endPos);
	}

	void GetMyInfo()
	{
		if(value >= 0)
			RTSButtonManager.instance.valueText.text = "VALUE\n" +
			"c" + value;

		if (canGarrison) {
			RTSButtonManager.instance.SwitchAllGarrisonInfoOnOff (true);
		} else 
			RTSButtonManager.instance.SwitchAllGarrisonInfoOnOff (false);
	}
}
