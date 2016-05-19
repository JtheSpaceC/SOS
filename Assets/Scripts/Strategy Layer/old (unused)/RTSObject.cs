using UnityEngine;
using System.Collections;

public class RTSObject : MonoBehaviour {

	protected bool shouldGrow = true;

	[HideInInspector] public Vector3 normalScale;
	[HideInInspector] public Vector3 grownScale;
	[HideInInspector] public Vector3 shrunkScale;
	protected float growRate = 2;
	
	protected GameObject[] myChildren;

	SpriteRenderer sr;

	[TextArea(10,10)]
	public string desciption;



	void Start()
	{
		BaseClassAwake ();
	}

	protected void BaseClassAwake()
	{		
		sr = GetComponent<SpriteRenderer> ();

		if(name == RTSDirector.instance.solarSystemName)
		{
			shouldGrow = false;
		}
		else 
		{
			normalScale = transform.localScale;
			grownScale = new Vector3 (transform.localScale.x * growRate, transform.localScale.y * growRate, 1);
			shrunkScale = new Vector3 (transform.localScale.x / growRate, transform.localScale.y / growRate, 1);
		}
		
		myChildren = new GameObject[transform.childCount];
		
		for (int i = 0; i < transform.childCount; i++)
		{
			myChildren[i] = transform.GetChild(i).gameObject;
		}

	}

	
	void OnMouseEnter()
	{
		if (RTSButtonManager.instance.menusAreShown || ClickToPlay.instance.paused)
			return;

		ShowObjectTooltip ();
	}
	void OnMouseExit()
	{
		if (RTSButtonManager.instance.menusAreShown || ClickToPlay.instance.paused)
			return;

		HideObjectTooltip ();
	}
	void OnMouseDown()
	{
		if (RTSButtonManager.instance.menusAreShown || ClickToPlay.instance.paused)
			return;
		ShowObjectInfo ();
	}

	void ShowObjectTooltip()
	{
		if (RTSDirector.instance.mouseIsOverSomething)
		{
			return;
		}
		else 
			RTSDirector.instance.mouseIsOverSomething = true;

		if(shouldGrow)
		{
			transform.localScale = new Vector3 
				(transform.localScale.x * growRate, transform.localScale.y * growRate, transform.localScale.z * growRate);

			sr.sortingOrder += 10;
			
			foreach (GameObject child in myChildren)
			{
				child.transform.localScale = new Vector3 
					(child.transform.localScale.x / growRate, child.transform.localScale.y / growRate, child.transform.localScale.z / growRate);
			}
		}
		RTSDirector.instance.PostTooltip (name, transform.position);
	}
	void HideObjectTooltip()
	{
		RTSDirector.instance.mouseIsOverSomething = false;
		if(shouldGrow)
		{
			transform.localScale = new Vector3 
				(transform.localScale.x / growRate, transform.localScale.y / growRate, transform.localScale.z / growRate);
			sr.sortingOrder -= 10;
			
			foreach (GameObject child in myChildren)
			{
				child.transform.localScale = new Vector3 
					(child.transform.localScale.x * growRate, child.transform.localScale.y * growRate, child.transform.localScale.z * growRate);
			}
		}
		RTSDirector.instance.tooltipCanvas.SetActive(false);
	}
	void ShowObjectInfo()
	{
		HideObjectTooltip ();
		RTSButtonManager.instance.ShowObjectInfo (this.gameObject);
	}
    void HideObjectInfo()
	{
		RTSButtonManager.instance.HideAllMenuAndInfoScreens ();
	}
}
