using UnityEngine;
using System.Collections;

public class PatrolMission : MonoBehaviour {

	Vector2 nav1 = new Vector2(50, 100);
	Vector2 nav2 = new Vector2(-100, 100);
	Vector2 nav3 = new Vector2(-150, 0);
	Vector2 nav4 = new Vector2(0, 0);


	void Start () 
	{
		Tools.instance.CreateWaypoint(Waypoint.WaypointType.SearchAndDestroy, new Vector2[]{nav1, nav2, nav3, nav4}, 10);
	}

}
