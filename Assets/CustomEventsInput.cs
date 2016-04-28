using UnityEngine;
using UnityEngine.EventSystems;

public class CustomEventsInput : MonoBehaviour
{
	private GameObject currentButton;
	private AxisEventData currentAxis;
	//timer
	public float timeBetweenInputs = 0.15f; //in seconds
	private float timer = 0;

	void Update()
	{
		if(timer <= 0)
		{
			currentAxis = new AxisEventData (EventSystem.current);
			currentButton = EventSystem.current.currentSelectedGameObject;

			if (Input.GetAxis("Gamepad Left Vertical") > 0) // move up
			{
				currentAxis.moveDir = MoveDirection.Up;
				ExecuteEvents.Execute(currentButton, currentAxis, ExecuteEvents.moveHandler);
			}
			else if (Input.GetAxis("Gamepad Left Vertical") < 0) // move down
			{
				currentAxis.moveDir = MoveDirection.Down;
				ExecuteEvents.Execute(currentButton, currentAxis, ExecuteEvents.moveHandler);
			}
			else if (Input.GetAxis("Gamepad Left Horizontal") > 0) // move right
			{
				currentAxis.moveDir = MoveDirection.Right;
				ExecuteEvents.Execute(currentButton, currentAxis, ExecuteEvents.moveHandler);
			}
			else if (Input.GetAxis("Gamepad Left Horizontal") < 0) // move left
			{
				currentAxis.moveDir = MoveDirection.Left;
				ExecuteEvents.Execute(currentButton, currentAxis, ExecuteEvents.moveHandler);
			}
			timer = timeBetweenInputs;
		}

		//timer counting down
		timer -= Time.fixedDeltaTime;

	}
}