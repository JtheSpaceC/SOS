using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour {

	public static CameraShake instance;
	
	[HideInInspector] public bool isShaking = false;
	GameObject CamPosOnPlayer;
	float offset;
	Vector2 newPosition;
	
	[HideInInspector] public float _amplitude = 0.1f;


	void Awake()
	{
		offset = transform.position.z;
	}
	
	void Start ()
	{
		instance = this;
	}
	
	public void Shake(float amplitude, float duration)
	{
		_amplitude = amplitude;
		CancelInvoke();
		isShaking = true;
		Invoke("StopShaking",duration);
	}
	
	void StopShaking()
	{
		isShaking = false;
		//transform.position = new Vector3(transform.position.x, transform.position.y, -50);
		//transform.localPosition = Vector3.Lerp (transform.position, Camera.main.transform.position + offset, smoothTime * Time.deltaTime);
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(isShaking)
		{
			newPosition = Random.insideUnitCircle * _amplitude;
			transform.position = new Vector3(transform.position.x + newPosition.x, transform.position.y + newPosition.y, offset);
		}
	}
}//Mono
