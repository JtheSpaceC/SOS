using UnityEngine;

public class ParallaxLayer : MonoBehaviour {

	//NOTE: This requires the object to be a child of a more stationary parent in order to keep a relative point in space

	public Vector3 realLocalPosition;
	Vector3 imageProjectPosition;

	[Tooltip("0 puts it on the player layer, -1 at the camera's eye (above player), and 1 at infinite distance.")]
	[Range(-1,1)] public float movement_resistance = 1f; 

	[Tooltip("Not used for anything. Just notes")]
	[TextArea]
	public string[] notes;


	void Awake () 
	{
		realLocalPosition = transform.localPosition;
	}

	void OnEnable()
	{
		SetProjectedImageLocation ();
	}

	void OnDisable()
	{
		transform.localPosition = realLocalPosition;
	}
	

	void LateUpdate () 
	{
		SetProjectedImageLocation ();
	}

	void SetProjectedImageLocation()
	{
		imageProjectPosition = realLocalPosition + (Camera.main.transform.position - transform.parent.position) * movement_resistance;
		imageProjectPosition.z = realLocalPosition.z;
		transform.localPosition = imageProjectPosition;	
	}
}
