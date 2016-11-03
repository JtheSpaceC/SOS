using UnityEngine;

[RequireComponent(typeof(RecolourMe))]

public class CargoTransport : TargetableObject {

	SpriteRenderer myRenderer;

	public bool isDamaged = false;
	public Transform containersParent;
	int numContainers = 30;
	public float containersDistance = 1f;
	public Sprite[] singleContainers;
	public Sprite[] doubleContainers;
	public Sprite[] singleContainersDamaged;
	public Sprite[] doubleContainersDamaged;
	public Sprite mainShip;
	public Sprite mainShipDamaged;

	bool lastSpriteWasDouble; //used so we don't put a container on top of a double container
	int randomSpriteInt;

	[Tooltip("How many containers are on the transport, as a percentage of full.")]
	[Range(0,100)]
	public float fullPercentage = 50;

	[Tooltip("This should be the second lowest point on the vertical for container attachments")]
	public float lowestDoubleContainerAllowed = -18f;


	void Start () 
	{
		if(!isDamaged)
			SetUpContainers();
	}

	[ContextMenu("Set Up Containers")]
	public void SetUpContainers ()
	{
		myRenderer = GetComponent<SpriteRenderer>();
		myRenderer.sprite = mainShip;
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
				randomSpriteInt = Random.Range(0, (singleContainers.Length + doubleContainers.Length));
				if(randomSpriteInt >= singleContainers.Length) //if the number is into the second group, subtract first group length and go with it
				{
					randomSpriteInt -= singleContainers.Length;
					containersParent.GetChild (i).GetComponent<SpriteRenderer>().sprite = doubleContainers[randomSpriteInt];
					lastSpriteWasDouble = true;
				}
				else //random number landed within length of the first group
				{
					containersParent.GetChild (i).GetComponent<SpriteRenderer> ().sprite = singleContainers [randomSpriteInt];
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
	}//end of SETUPCONTAINERS()

	[ContextMenu("Wreck Ship")]
	public void WreckShip ()
	{
		isDamaged = true;
		myRenderer = GetComponent<SpriteRenderer>();
		myRenderer.sprite = mainShipDamaged;

		Transform[] children = containersParent.GetComponentsInChildren<Transform>();

		Sprite newSprite;

		foreach(Transform child in children)
		{
			if(child.GetComponent<SpriteRenderer>())
			{
				newSprite = singleContainersDamaged[Random.Range(0, singleContainersDamaged.Length)];

				/*if(child.localPosition.y < lowestDoubleContainerAllowed) //then this can't be a double
				{
					newSprite = singleContainersDamaged[Random.Range(0, singleContainersDamaged.Length)];
				}
				else //can be from double or single destroyed group
				{
					randomSpriteInt = Random.Range(0, (singleContainersDamaged.Length + doubleContainersDamaged.Length));
					if(randomSpriteInt >= singleContainersDamaged.Length) //if the number is into the second group, subtract first group length and go with it
					{
						randomSpriteInt -= singleContainersDamaged.Length;
						newSprite = doubleContainersDamaged[randomSpriteInt];
					}
					else //random number landed within length of the first group
					{
						newSprite = singleContainersDamaged [randomSpriteInt];
					}
				}*/
				child.GetComponent<SpriteRenderer>().sprite = newSprite;
			}
		}
	}


}
