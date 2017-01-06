using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StratZoneGenerator : MonoBehaviour {

	public GameObject poiPrefab;
	public GameObject zoneParent;
	public List<StratPOI> allPointsOfInterest;

	public int minPointsOfInterest;
	public int maxPointsOfInterest;
	public int minConnections;
	public int maxConnections;

	int numConnections;
	List<List<StratPOI>> listOfGroups = new List<List<StratPOI>>();
	List<StratPOI> checkedPOIs = new List<StratPOI>();
	bool hasMarchedFurther = false;

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

			allPointsOfInterest.Add(newStratPOI);

			//push them apart if needs be
			float maxDist = 0.25f;
			if(allPointsOfInterest.Count > 1)
			{
				while(Vector2.Distance(newPOI.transform.position, AbsoluteNearestPOI(newStratPOI).transform.position) < 1.5f)
				{
					newPOI.transform.position = Random.insideUnitCircle.normalized * maxDist;
					maxDist += .25f;
				}
			}

			//TODO: destroy any that are too far outside the bounds
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
		for(int i = 0; i < allPointsOfInterest.Count; i++)
		{
			if(allPointsOfInterest[i].myConnections.Count == 0)
			{
				StratPOI poiToAdd = AbsoluteNearestPOI(allPointsOfInterest[i]);

				if(poiToAdd != null)
				{
					if(!allPointsOfInterest[i].myConnections.Contains(poiToAdd))
						allPointsOfInterest[i].myConnections.Add(poiToAdd);
					if(!poiToAdd.myConnections.Contains(allPointsOfInterest[i]))
						poiToAdd.myConnections.Add(allPointsOfInterest[i]);
				}
			}
		}

		//find isolated regions and Connect them
		StartCoroutine(ConnectIsolatedZones()); //this also draws the lines

		#endregion
	}

	IEnumerator ConnectIsolatedZones ()
	{
		while (MarchToFindIsolatedGroups () > 1)//also creates listOfGroups
		 {
			float shortestDistance = Mathf.Infinity;
			List<StratPOI> closestGroup = null;
			List<StratPOI> secondClosestGroup = null;
			List<List<StratPOI>> groupsToCheck = listOfGroups;
			//find the group nearest the origin point
			foreach (List<StratPOI> list in groupsToCheck) 
			{
				Vector3 averagePosition = Vector3.zero;
				//get average position of group
				foreach (StratPOI poi in list) 
				{
					averagePosition += poi.transform.position;
				}
				averagePosition /= list.Count;

				float distance = Vector2.Distance (averagePosition, Vector2.zero);
				if (distance < shortestDistance) 
				{
					closestGroup = list;
					shortestDistance = distance;
				}
			}
			//find the second nearest group
			groupsToCheck.Remove (closestGroup);
			shortestDistance = Mathf.Infinity;
			foreach (List<StratPOI> list in groupsToCheck) 
			{
				Vector3 averagePosition = Vector3.zero;
				//get average position of group
				foreach (StratPOI poi in list) 
				{
					averagePosition += poi.transform.position;
				}
				averagePosition /= list.Count;

				float distance = Vector2.Distance (averagePosition, Vector2.zero);
				if (distance < shortestDistance) 
				{
					secondClosestGroup = list;
					shortestDistance = distance;
				}
			}
			//find the nearest pair of POIs, one from each group
			shortestDistance = Mathf.Infinity;
			StratPOI[] closestPair = new StratPOI[2];
			foreach (StratPOI firstPOI in closestGroup) 
			{
				foreach (StratPOI secondPOI in secondClosestGroup) 
				{
					float distance = Vector2.Distance (firstPOI.transform.position, secondPOI.transform.position);
					if (distance < shortestDistance) 
					{
						shortestDistance = distance;
						closestPair = new StratPOI[]{firstPOI,secondPOI};
					}
				}
			}
			//connect them
			closestPair [0].myConnections.Add (closestPair [1]);
			closestPair [1].myConnections.Add (closestPair [0]);

			//Draw the lines
			for(int i = 0; i < allPointsOfInterest.Count; i++)
			{
				allPointsOfInterest[i].DrawLines();
			}

			yield return new WaitForSeconds(.9f);
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
		allPointsOfInterest.Clear();
		listOfGroups.Clear();

		StratPOI.index = 0;
	}

	StratPOI nearestPOI(StratPOI startingPOI)
	{
		float nearestDistance = Mathf.Infinity;
		StratPOI nearestResult = null;

		for(int i = 0; i < allPointsOfInterest.Count; i++)
		{
			if(allPointsOfInterest[i] != startingPOI)
			{
				if(Vector2.Distance(startingPOI.transform.position, allPointsOfInterest[i].transform.position) < nearestDistance)
				{
					if(!startingPOI.myConnections.Contains(allPointsOfInterest[i]) && !allPointsOfInterest[i].myConnections.Contains(startingPOI))
					{
						if(allPointsOfInterest[i].myConnections.Count < numConnections)
						{
							nearestDistance = Vector2.Distance(startingPOI.transform.position, allPointsOfInterest[i].transform.position);
							nearestResult = allPointsOfInterest[i];
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

		for(int i = 0; i < allPointsOfInterest.Count; i++)
		{
			if(allPointsOfInterest[i] != startingPOI)
			{
				if(Vector2.Distance(startingPOI.transform.position, allPointsOfInterest[i].transform.position) < nearestDistance)
				{
					nearestDistance = Vector2.Distance(startingPOI.transform.position, allPointsOfInterest[i].transform.position);
					nearestResult = allPointsOfInterest[i];
				}
			}
		}
		return nearestResult;
	}

	int MarchToFindIsolatedGroups()
	{
		checkedPOIs.Clear();
		hasMarchedFurther = false;
		listOfGroups.Clear();
		int isolatedGroups = 0;

		foreach(StratPOI poi in allPointsOfInterest.ToArray()) //look at every POI in the map
		{
			//see if it's contained in a check already. Only execute the march if it hasn't
			if(!checkedPOIs.Contains(poi))
			{
				List<StratPOI> thisGroup = new List<StratPOI>();

				//increase the number of isolated groups.
				isolatedGroups ++;
				thisGroup.Clear();

				//add POI to Checked and to a new Group immediately.
				checkedPOIs.Add(poi);

				thisGroup.Add(poi);
				hasMarchedFurther = true;

				//for each of its connections, recursively check their connections
				while(hasMarchedFurther)
				{
					hasMarchedFurther = false;
					List<StratPOI> tempGroup = new List<StratPOI>();

					foreach(StratPOI connectedPOI in thisGroup)
					{
						foreach(StratPOI doubleConnectedPOI in connectedPOI.myConnections)
						{
							if(!checkedPOIs.Contains(doubleConnectedPOI))
							{
								hasMarchedFurther = true;

								checkedPOIs.Add(doubleConnectedPOI);
								tempGroup.Add(doubleConnectedPOI);
								break;
							}
						}
					}
					thisGroup.AddRange(tempGroup);
				}
				
				//add thisGroup to the list of groups
				listOfGroups.Add(thisGroup);
			}
		}
		//return how many individual groups there were
		return isolatedGroups;
	}
}
