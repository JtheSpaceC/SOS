using UnityEngine;
using System.Collections;

public class DestroyAfterTime : MonoBehaviour {

	[Tooltip("Use this to destroy a parent or other object.")]
	public bool setInactiveInsteadOfDestroy = false;
	public bool destroyDifferentObject = false;
	public GameObject[] otherObjects;

	public float delayTime = 1;

	void OnEnable () 
	{
		Invoke ("DestroyObject", delayTime);
	}
	

	void DestroyObject () 
	{
		if(destroyDifferentObject && !setInactiveInsteadOfDestroy)
		{
			foreach(GameObject obj in otherObjects)
			{
				Destroy(obj);
			}
		}
		else if(!destroyDifferentObject && !setInactiveInsteadOfDestroy)
		{
			Destroy(gameObject);
		}

		else if(destroyDifferentObject && setInactiveInsteadOfDestroy)
		{
			foreach(GameObject obj in otherObjects)
			{
				obj.SetActive(false);
			}
		}
		else if(!destroyDifferentObject && setInactiveInsteadOfDestroy)
		{
			gameObject.SetActive(false);
		}
	}

	void OnDisable()
	{
		CancelInvoke ();
	}
}
