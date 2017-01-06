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
	List<List<StratPOI>> listOfGroups = new List<List<StratPOI>>();
	List<StratPOI> checkedPOIs = new List<StratPOI>();
	bool hasMarchedFurther = false;
	List<StratPOI> thisGroup = new List<StratPOI>();

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
				while(Vector2.Distance(newPOI.transform.position, AbsoluteNearestPOI(newStratPOI).transform.position) < 1.5f)
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
				StratPOI poiToAdd = AbsoluteNearestPOI(pointsOfInterest[i]);

				if(poiToAdd != null)
				{
					if(!pointsOfInterest[i].myConnections.Contains(poiToAdd))
						pointsOfInterest[i].myConnections.Add(poiToAdd);
					if(!poiToAdd.myConnections.Contains(pointsOfInterest[i]))
						poiToAdd.myConnections.Add(pointsOfInterest[i]);
				}
			}
		}

		//find and connect isolated regions
		print("Isolated Groups: "+ MarchToFindIsolatedGroups());

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

	StratPOI AbsoluteNearestPOI(StratPOI startingPOI)
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

	int MarchToFindIsolatedGroups()
	{
		listOfGroups.Clear();
		checkedPOIs.Clear();
		hasMarchedFurther = false;
		int isolatedGroups = 0;

		foreach(StratPOI poi in pointsOfInterest.ToArray()) //look at every POI in the map
		{
			//see if it's contained in a check already. Only execute the march if it hasn't
			if(!checkedPOIs.Contains(poi))
			{
				//increase the number of isolated groups.
				isolatedGroups ++;
				thisGroup.Clear();

				//add POI to Checked and to a new Group immediately.
				checkedPOIs.Add(poi);
				thisGroup.Add(poi);

				//add all its connections to Checked and the same Group
				foreach(StratPOI connectedPOI in poi.myConnections)
				{
					checkedPOIs.Add(connectedPOI);
					thisGroup.Add(connectedPOI);
					hasMarchedFurther = true;
				}

				//for each of its connections, recursively check their connections
				while(hasMarchedFurther)
				{
					hasMarchedFurther = false;

					foreach(StratPOI doubleConnectedPOI in thisGroup)
					{
						if(!checkedPOIs.Contains(doubleConnectedPOI))
						{
							hasMarchedFurther = true;

							checkedPOIs.Add(doubleConnectedPOI);
							thisGroup.Add(doubleConnectedPOI);
						}
					}
				}

				//add thisGroup to the list of groups
				print("This group is " + thisGroup.Count);
				listOfGroups.Add(thisGroup);
			}
		}

		//return how many individual groups there were
		return isolatedGroups;

		//should then connect all group members who are closest to the centre to each other
	}
}
