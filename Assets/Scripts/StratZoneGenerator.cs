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

	int numConnections;

	public void GenerateNewZone()
	{
		zoneParent = new GameObject();// Instantiate(new GameObject());
		zoneParent.name = "Zone";

		//fix if max numbers are below min numbers
		maxPointsOfInterest = Mathf.Clamp(maxPointsOfInterest, minPointsOfInterest, 999);
		minConnections = Mathf.Clamp(minConnections, 0, maxPointsOfInterest-1);
		maxConnections = Mathf.Clamp(maxConnections, 0, maxPointsOfInterest-1);

		//create the POIs
		int chosenNumPointsOfInterest = Random.Range(minPointsOfInterest, maxPointsOfInterest+1);

		for(int i = 0; i < chosenNumPointsOfInterest; i++)
		{
			GameObject newPOI = Instantiate(poiPrefab, Random.insideUnitCircle * 5, Quaternion.identity, zoneParent.transform);
			StratPOI newStratPOI = newPOI.GetComponent<StratPOI>();

			pointsOfInterest.Add(newStratPOI);

			//push them apart if needs be
			float maxDist = 0.5f;
			if(pointsOfInterest.Count > 1)
			{
				while(Vector2.Distance(newPOI.transform.position, absoluteNearestPOI(newStratPOI).transform.position) < 1.5f)
				{
					newPOI.transform.position = Random.insideUnitCircle.normalized * maxDist;
					maxDist += .5f;
				}
			}
		}

		//MAKE CONNECTIONS
		#region old way of connections
		//add a few connections.
		/*for(int i = 0; i < pointsOfInterest.Count; i++)
		{
			numConnections = Random.Range(minConnections, maxConnections+1);

			for(int j = 0; j < numConnections; j++)
			{
				if(pointsOfInterest[i].myConnections.Count < numConnections)
				{
					StratPOI poiToAdd = nearestPOI(pointsOfInterest[i]);
					if(poiToAdd != null)
						pointsOfInterest[i].myConnections.Add(poiToAdd);
				}
			}
		}*/

		//prevent isolated points (ensure all can connect)
		//TODO: above
		#endregion

		#region new way
		//link everyone to their nearest
		for(int i = 0; i < pointsOfInterest.Count; i++)
		{
			if(pointsOfInterest[i].myConnections.Count == 0)
			{
				StratPOI poiToAdd = absoluteNearestPOI(pointsOfInterest[i]);
				if(!pointsOfInterest[i].myConnections.Contains(poiToAdd))
					pointsOfInterest[i].myConnections.Add(poiToAdd);
				if(!poiToAdd.myConnections.Contains(pointsOfInterest[i]))
					poiToAdd.myConnections.Add(pointsOfInterest[i]);
			}
		}

		#endregion

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
					if(!startingPOI.myConnections.Contains(pointsOfInterest[i]) && !pointsOfInterest[i].myConnections.Contains(startingPOI))
					{
						if(pointsOfInterest[i].myConnections.Count < numConnections)
						{
							nearestDistance = Vector2.Distance(startingPOI.transform.position, pointsOfInterest[i].transform.position);
							nearestResult = pointsOfInterest[i];
						}
					}
				}
			}
		}
		if(nearestResult != null)
			nearestResult.myConnections.Add(startingPOI);

		return nearestResult;
	}

	StratPOI absoluteNearestPOI(StratPOI startingPOI)
	{
		float nearestDistance = Mathf.Infinity;
		StratPOI nearestResult = null;

		for(int i = 0; i < pointsOfInterest.Count; i++)
		{
			if(pointsOfInterest[i] != startingPOI)
			{
				if(Vector2.Distance(startingPOI.transform.position, pointsOfInterest[i].transform.position) < nearestDistance)
				{
					nearestDistance = Vector2.Distance(startingPOI.transform.position, pointsOfInterest[i].transform.position);
					nearestResult = pointsOfInterest[i];
				}
			}
		}
		return nearestResult;
	}
}
