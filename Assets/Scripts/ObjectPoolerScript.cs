using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPoolerScript : MonoBehaviour {

    //PUT SOMETHING LIKE THIS IN ANOTHER SCRIPT
    /*
    GameObject obj1 = shotPoolerScript.current.GetPooledObject();
			
			//not needed if Will Grow is true
			//if (obj1 == null) return;
			
			obj1.transform.position = position;
			obj1.transform.rotation = rotation;
			obj1.SetActive(true);
	*/
	public ObjectPoolerScript current;

	public GameObject pooledObject; //could be enemies, bullets, etc
	public int pooledAmount = 20;
	public bool willGrow = true;
	
	public List <GameObject> pooledObjects;
	
	void Awake()
	{
		current = this;
	}
	
	void Start () 
	{
		pooledObjects = new List<GameObject> ();
		for (int i=0; i <pooledAmount; i++) 
		{
			GameObject obj = (GameObject)Instantiate(pooledObject);
			obj.name = pooledObject.name;
			obj.SetActive(false);
			pooledObjects.Add(obj);

			//put all instances into a parent for hierarchy
			obj.transform.parent = transform;
		}
	}
	
	public GameObject GetPooledObject()
	{
		for (int i=0; i < pooledObjects.Count; i++) 
		{
			if(!pooledObjects[i].activeInHierarchy)
			{
				return pooledObjects[i];
			}
		}
		
		if (willGrow) 
		{
			GameObject obj = (GameObject)Instantiate(pooledObject);
			obj.name = pooledObject.name;
			obj.SetActive(false);
			pooledObjects.Add(obj);
			
			//put all instances into a parent for hierarchy
			obj.transform.parent = transform;
			return obj;
		}
		
		return null;
	}

}//Mono
