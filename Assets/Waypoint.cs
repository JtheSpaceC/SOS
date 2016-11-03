using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Waypoint : MonoBehaviour {

	public enum WaypointType {Extraction, Move, Escort, SearchAndDestroy, Follow, Support, Comms};
	public WaypointType waypointType;

	[HideInInspector] public Vector2[] waypoints;
	public GameObject zoneBoxAnimation;

	public enum BehaviourType {Constant, Checkpoint, ClearArea};
	[HideInInspector] public BehaviourType behaviourType;

	public bool destroyWhenReached = false;
	public UnityEvent OnReachedEvents;



	void OnTriggerEnter2D(Collider2D other)
	{
		AudioMasterScript.instance.PlayChime();
		OnReachedEvents.Invoke();
		if(destroyWhenReached)
			Destroy(gameObject);
	}

	void ReportActivity()
	{
		CameraTactical.reportedInfo = "Waypoint: ";
		CameraTactical.reportedInfo += StaticTools.SplitCamelCase(waypointType.ToString());

		CameraTactical.instance.pipCamera.orthographicSize = 10;
	}
}
