using UnityEngine;

public class ParallaxLayer : MonoBehaviour {

	public Vector3 realLocalPosition;
	Vector3 imageProjectPosition;

	[Tooltip("0 puts it on the player layer, -1 at the camera's eye (above player), and 1 at infinite distance.")]
	[Range(-1,1)] public float movement_resistance = 1f; 


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
