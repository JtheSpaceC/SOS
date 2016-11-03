using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombatAsteroidsStyle : MonoBehaviour {

	public List<GameObject> itemsInZone = new List<GameObject>();
	public List<GameObject> itemsToRemove = new List<GameObject>();
	float minX;
	float maxX;
	float minY;
	float maxY;

	BoxCollider2D boxCol;
	Vector3 newPosition;
	TrailRenderer[] shipTrails;
	Health healthScript;


	void Awake()
	{
		boxCol = GetComponent<BoxCollider2D>();
	}

	void OnEnable()
	{
		GetComponentInParent<CameraControllerFighter>().cameraBehaviour = CameraControllerFighter.CameraBehaviour.AsteroidsBox;

		float zPos = -Camera.main.transform.position.z;

		minX = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, zPos)).x;
		maxX = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, zPos)).x;
		minY = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, zPos)).y;
		maxY = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, zPos)).y;	

		boxCol.transform.localPosition = new Vector3(0, 0, zPos);
		Vector2 boxSize = boxCol.size;
		boxSize.x = maxX - minX;
		boxSize.y = maxY - minY;
		boxCol.size = boxSize;
		boxCol.enabled = true;
	}

	void OnDisable()
	{
		boxCol.enabled = false;
		if(GetComponentInParent<CameraControllerFighter>())
			GetComponentInParent<CameraControllerFighter>().cameraBehaviour = CameraControllerFighter.CameraBehaviour.Normal;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if(!this.enabled)
			return;
		
		if(!itemsInZone.Contains(other.gameObject))
		{
			itemsInZone.Add(other.gameObject);
		}
	}

	void Update()
	{
		ClearTheList();
		for(int i = 0; i < itemsToRemove.Count; i++)
		{
			itemsInZone.Remove(itemsToRemove[i]); 
		}

		//clear every frame. It's a temp list to adjust the shipsInCombat list
		itemsToRemove.Clear();

		//if there's only the player left, return camera to normal
	/*	if(itemsInZone.Count == 1 && itemsInZone[0].tag == "PlayerFighter")
		{
			this.enabled = false;
			return;
		}*/

		//check up on all ships that are locked in, and keep them in the box
		for(int i = 0; i < itemsInZone.Count; i++)
		{
			newPosition = itemsInZone[i].transform.position;

			if(!isInsideBounds(newPosition))
			{
				//check if we're outside the screen, and snap back in if that's the case
				if(itemsInZone[i].transform.position.x < minX)
					newPosition.x = maxX;
				else if(itemsInZone[i].transform.position.x > maxX)
					newPosition.x = minX;
				
				if(itemsInZone[i].transform.position.y < minY)
					newPosition.y = maxY;
				else if(itemsInZone[i].transform.position.y > maxY)
					newPosition.y = minY;

				shipTrails = itemsInZone[i].GetComponentsInChildren<TrailRenderer>();

				if(shipTrails != null)
				{
					foreach(TrailRenderer trail in shipTrails)
					{
						StartCoroutine(ReactivateTrailRenderer(trail, trail.time));
						trail.time = 0;
					}
				}
				itemsInZone[i].transform.position = newPosition;
			}

			//Prepare to remove if inactive or dead
			if(!itemsInZone[i].activeInHierarchy)
				itemsToRemove.Add(itemsInZone[i]);

			healthScript = itemsInZone[i].GetComponent<Health>();

			if(healthScript != null && healthScript.dead)
				itemsToRemove.Add(itemsInZone[i]);
		}
	}//end of UPDATE

	bool isInsideBounds(Vector2 pos)
	{
		if(pos.x < minX || pos.x > maxX || pos.y < minY || pos.y > maxY)
			return false;
		else return true;
	}

	IEnumerator ReactivateTrailRenderer(TrailRenderer trail, float t)
	{
		yield return new WaitForEndOfFrame();
		trail.time = t;		
	}

	void ClearTheList()
	{
		List<int> ats = new List<int>();

		for(int i = 0; i < itemsInZone.Count; i++)
		{
			if(itemsInZone[i] == null)
			{
				ats.Add(i);
			}
		}

		for(int j = 0; j < ats.Count; j++)
		{
			itemsInZone.RemoveAt(ats[j]);
		}
	}
}
