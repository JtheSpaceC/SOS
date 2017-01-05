using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StratZoneGenerator : MonoBehaviour {

	public GameObject poiPrefab;
	public GameObject zoneParent;
	public List<StratPOI> pointsOfInterest;

	public int minPointsOfInterest;
	public int maxPointsOfInterest;
	public int minConnections;
	public int maxConnections;


	public void GenerateNewZone()
	{
		zoneParent = new GameObject();// Instantiate(new GameObject());
		zoneParent.name = "Zone";

		//create the POIs
		int chosenNumPointsOfInterest = Random.Range(minPointsOfInterest, maxPointsOfInterest);
		for(int i = 0; i < chosenNumPointsOfInterest; i++)
		{
			GameObject newPOI = Instantiate(poiPrefab, Random.insideUnitCircle * 5, Quaternion.identity, zoneParent.transform);
			pointsOfInterest.Add(newPOI.GetComponent<StratPOI>());
		}

		//add a few connections.
		for(int i = 0; i < pointsOfInterest.Count; i++)
		{
			int numConnections = Random.Range(minConnections, maxConnections+1);
			for(int j = 0; j < numConnections; j++)
			{
				pointsOfInterest[i].myConnections.Add(nearestPOI(pointsOfInterest[i]));
			}
		}

		//Draw the lines
		for(int i = 0; i < pointsOfInterest.Count; i++)
		{
			pointsOfInterest[i].DrawLines();
		}
	}

	public void ClearZone()
	{
		if(zoneParent != null)
		{
			DestroyImmediate(zoneParent);
		}
		else if(GameObject.Find("Zone"))
			DestroyImmediate(GameObject.Find("Zone"));
		pointsOfInterest.Clear();
	}

	StratPOI nearestPOI(StratPOI startingPOI)
	{
		float nearestDistance = Mathf.Infinity;
		StratPOI nearestResult = null;

		for(int i = 0; i < pointsOfInterest.Count; i++)
		{
			if(pointsOfInterest[i] != startingPOI)
			{
				if(Vector2.Distance(startingPOI.transform.position, pointsOfInterest[i].transform.position) < nearestDistance)
				{
					if(!startingPOI.myConnections.Contains(pointsOfInterest[i]))
					{
						nearestDistance = Vector2.Distance(startingPOI.transform.position, pointsOfInterest[i].transform.position);
						nearestResult = pointsOfInterest[i];
					}
				}
			}
		}

		return nearestResult;
	}
}
