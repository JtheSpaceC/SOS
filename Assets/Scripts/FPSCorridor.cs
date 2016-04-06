using UnityEngine;
using System.Collections;

public class FPSCorridor : MonoBehaviour {

	public enum FPSState {Stopped, Walking, Running, Shooting};
	public FPSState fpsState;

	public float walkSpeed = 2;
	public float runSpeed = 3.5f;

	public Transform[] corridorMovement;
	public float minCorridorScale = 0.1f;
	public float maxCorridorScale = 10f;
	Vector3 newScale;


	void Start()
	{
		/*//space out the corridor movement sections evenly
		for(int i = 0; i < corridorMovement.Length; i++)
		{
			float appropriatePosition = ((float)i+1)/corridorMovement.Length;

			newScale.x = maxCorridorScale * appropriatePosition * appropriatePosition;

			newScale.y = newScale.x;
			newScale.z = newScale.x;

			corridorMovement[i].transform.localScale = newScale;
		}*/
	}


	void Update () 
	{
		if(fpsState == FPSState.Walking)
		{
			MoveForward (walkSpeed);
		}
		else if(fpsState == FPSState.Running)
		{
			MoveForward(runSpeed);
		}
	}

	void MoveForward(float moveSpeed)
	{
		for(int i = 0; i < corridorMovement.Length; i++)
		{
			newScale = corridorMovement[i].localScale;

			newScale.x += moveSpeed * (corridorMovement[i].localScale.x  / maxCorridorScale) * Time.deltaTime;

			if(newScale.x > maxCorridorScale)
				newScale.x = minCorridorScale + .01f;

			newScale.y = newScale.x;
			newScale.z = newScale.x;

			corridorMovement[i].localScale = newScale;
		}
	}
}
