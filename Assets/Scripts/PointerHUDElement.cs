using UnityEngine;
using UnityEngine.UI;

public class PointerHUDElement : MonoBehaviour {

	public Transform sourceObject;
	public Transform target;

	[Tooltip("For if the target is a point, not a Transform")] 
	public Vector2 targetWP;

	public Image arrowPointerImage;
	public Image mainImage;
	public Image distanceTextPanel;
	public Text distanceText;

	[Tooltip("Does the image fade if you're near it. Set -1 for 'no'. " +
		"Otherwise greater numbers mean greater fade (stay under camera ortho size).")]
	public float fadeBuffer = -1;

	Color mainImageColor;

	public Sprite[] numberImages;

	Vector3 pos;
	Vector2 directionToTarget;
	Vector3 centreScreen;

	void Awake()
	{
		if (target == null)
		{			
			if(transform.root.gameObject != this.gameObject)
				target = transform.root;
		}
		if(sourceObject == null)
			sourceObject = Camera.main.transform;

		transform.rotation = Quaternion.Euler (Vector3.zero);

		centreScreen = new Vector3 (0.5f, 0.5f, -Camera.main.transform.position.z);
	}

	void ChooseImage(string name)
	{
		if(name.EndsWith(" 1"))
		{
			mainImage.sprite = numberImages[1];
		}
		else if(name.EndsWith(" 2"))
		{
			mainImage.sprite = numberImages[2];
		}
		else if(name.EndsWith("3"))
		{
			mainImage.sprite = numberImages[3];
		}
		else if(name.EndsWith("4"))
		{
			mainImage.sprite = numberImages[4];
		}
		else if(name.EndsWith("5"))
		{
			mainImage.sprite = numberImages[5];
		}
		else if(name.EndsWith("6"))
		{
			mainImage.sprite = numberImages[6];
		}
		else if(name.EndsWith("7"))
		{
			mainImage.sprite = numberImages[7];
		}
		else if(name.EndsWith("8"))
		{
			mainImage.sprite = numberImages[8];
		}
		else if(name.EndsWith("9"))
		{
			mainImage.sprite = numberImages[9];
		}
		else if(name.EndsWith("10"))
		{
			mainImage.sprite = numberImages[10];
		}
		else if(name.EndsWith("11"))
		{
			mainImage.sprite = numberImages[11];
		}
		else if(name.EndsWith("12"))
		{
			mainImage.sprite = numberImages[12];
		}
		else
		{
			//it's a waypoint image. Leave it alone, probably
		}
	}

	void Start()
	{
		//TripleTextSize ();
		ChooseImage (transform.root.name);
	}

	[ContextMenu("TripleFontSize")]
	void TripleTextSize()
	{
		distanceText.fontSize = distanceText.fontSize * 3;
	}


	void LateUpdate () 
	{
		if(target != null)
		{
			pos = Camera.main.WorldToViewportPoint (target.position);
			targetWP = target.position;
		}
		else
		{
			pos = Camera.main.WorldToViewportPoint (targetWP);
		}


		//turn on arrow part of pointer if the target is off our screen
		if(pos.x < 0 || pos.x > 1 || pos.y <0 || pos.y > 1)
		{
			arrowPointerImage.enabled = true;
			distanceTextPanel.enabled = true;

			//rotate arrow to point from player to target
			Vector3 dir = (Vector3)targetWP - sourceObject.position; 
			float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90;
			Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
			transform.rotation = q;

			distanceText.text = string.Format("{0:n0}", Vector2.Distance(sourceObject.position, targetWP)*10) + "m";

			//keep the arrow on screen
			directionToTarget.x = (targetWP.x - sourceObject.position.x);
			directionToTarget.y = (targetWP.y - sourceObject.position.y);
			directionToTarget = directionToTarget.normalized * 0.4f;

			transform.position = Camera.main.ViewportToWorldPoint(centreScreen + (Vector3)directionToTarget);
		}
		else //turn off arrow part
		{
			arrowPointerImage.enabled = false;
			distanceTextPanel.enabled = false;
			transform.rotation = Quaternion.identity;
			distanceText.text = "";

			//keep the arrow on screen
			pos.x = Mathf.Clamp01(pos.x);
			pos.y = Mathf.Clamp01(pos.y);
			transform.position = Camera.main.ViewportToWorldPoint(pos);

			float distToTarget = Vector2.Distance(targetWP, (Vector2) Camera.main.transform.position);

			if(fadeBuffer >= 0 && distToTarget < Camera.main.orthographicSize)
			{
				mainImageColor = mainImage.color;
				mainImageColor.a = Mathf.Clamp((distToTarget - fadeBuffer)/Camera.main.orthographicSize, 0, 1);
				mainImage.color = mainImageColor;
			}
		}
	}
}
