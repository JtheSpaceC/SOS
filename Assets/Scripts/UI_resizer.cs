using UnityEngine;
using System.Collections;

public class UI_resizer : MonoBehaviour {

	[Tooltip("In the strategy layer, the planets have lineRenderers that go here")] 
	public LineRenderer[] allLines;

	Camera mainCam;
	float startOrthoSize;
	Vector3 startImageScale;

	public float startLineThickness = 0.2f;

	float newProportion;
	float previousOrthoSize;


	void Start () 
	{
		if (GameObject.Find ("Camera (Tactical Map)") != null) {
			mainCam = GameObject.Find ("Camera (Tactical Map)").GetComponent<Camera> ();
		}
		else 
		{
			mainCam = Camera.main;
		}

		startOrthoSize = mainCam.orthographicSize;
		previousOrthoSize = startOrthoSize;
		startImageScale = transform.localScale;

		allLines = GetComponentsInChildren<LineRenderer> ();
	}


	void Update () 
	{
		if(CameraTactical.instance && !CameraTactical.instance.mapIsShown)
		{
			return;
		}
		
		if (mainCam.orthographicSize != previousOrthoSize) 
		{
			newProportion = mainCam.orthographicSize / startOrthoSize;
			transform.localScale = new Vector3 (startImageScale.x * newProportion, startImageScale.y * newProportion, startImageScale.z * newProportion);

			foreach (LineRenderer lr in allLines) 
			{
				lr.SetWidth (startLineThickness * newProportion, startLineThickness * newProportion);
			}
			previousOrthoSize = mainCam.orthographicSize;
		}
	}

	public void ResetSize()
	{
		transform.localScale = startImageScale;
	}
}//Mono
