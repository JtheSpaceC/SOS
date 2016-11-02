using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RecolourMe))]

public class CargoTransport : TargetableObject {

	public Transform containersParent;
	int numContainers = 30;
	public float containersDistance = 1f;
	public Sprite[] singleContainers;
	public Sprite[] doubleContainers;

	bool lastSpriteWasDouble; //used so we don't put a container on top of a double container
	int randomSprite;

	[Tooltip("How many containers are on the transport, as a percentage of full.")]
	[Range(0,100)]
	public float fullPercentage = 50;


	void Start () 
	{
		SetUpContainers();
	}

	[ContextMenu("Set Up Containers")]
	public void SetUpContainers ()
	{
		numContainers = containersParent.childCount;

		float nextDistance = 0;

		//activate all container sprites and gameObjects
		for (int i = 0; i < numContainers; i++) 
		{
			containersParent.GetChild (i).GetComponent<SpriteRenderer>().enabled = true;
			containersParent.GetChild (i).gameObject.SetActive(true);
		}


		//ATTACH CONTAINERS
		for (int i = 0; i < numContainers; i++) 
		{
			//setPosition
			containersParent.GetChild (i).localPosition = new Vector3 (0, nextDistance, .2f);

			//CHOOSE CONTAINER
			containersParent.GetChild (i).GetComponent<SpriteRenderer> ().enabled = true; //REMOVE later;

			if(lastSpriteWasDouble)
			{
				containersParent.GetChild (i).GetComponent<SpriteRenderer> ().enabled = false;
				lastSpriteWasDouble = false;
			}
			//last container in a row can't be double
			else if((i + 1) == numContainers/2 || (i + 1) == numContainers)
			{
				containersParent.GetChild (i).GetComponent<SpriteRenderer> ().sprite = singleContainers [Random.Range (0, singleContainers.Length)];
			}
			else
			{
				randomSprite = Random.Range(0, (singleContainers.Length + doubleContainers.Length));
				if(randomSprite >= singleContainers.Length) //if the number is into the second group, subtract first group length and go with it
				{
					randomSprite -= singleContainers.Length;
					containersParent.GetChild (i).GetComponent<SpriteRenderer>().sprite = doubleContainers[randomSprite];
					lastSpriteWasDouble = true;
				}
				else //random number landed within length of the first group
				{
					containersParent.GetChild (i).GetComponent<SpriteRenderer> ().sprite = singleContainers [randomSprite];
				}
			}

			//choose next position
			if (i + 1 == numContainers / 2) 
			{
				nextDistance = 0;
			}
			else
				nextDistance -= containersDistance;
		}

		//Remove the blank container spaces (ie. those underneath doubles)
		for (int i = numContainers -1; i >= 0; i--)
		{
			if(!containersParent.GetChild (i).GetComponent<SpriteRenderer>().enabled) 
			{
				containersParent.GetChild (i).gameObject.SetActive(false);
			}
		}
	
		//DETACH CONTAINERS IF NOT FULLY FULL,
		int numberToRemove = Mathf.FloorToInt( ((100f-fullPercentage)/100f) * numContainers);

		for(int i = 0; i < numberToRemove; i++)
		{
			GameObject go = containersParent.GetChild(Random.Range(0, containersParent.childCount)).gameObject;
			if(go.activeSelf)
				go.SetActive(false);
			else
				i--;
		}
	}


}
