using UnityEngine;
using System.Collections;

public class Minefield : MonoBehaviour {

	public GameObject minePrefab;
	public float mineDistanceFromEachOther = 8f;

	[Tooltip("How far from their specific point might they be. Keep small.")]
	public float variation = 1.5f;

	public enum Shape {Circle, Square};
	public Shape shape;

	public float radius = 200;

	bool staggerLine;
	float staggerDistance;


	void Start () 
	{
		
		for(float i = transform.position.x - radius; i < transform.position.x + radius; i += mineDistanceFromEachOther)
		{
			if(staggerLine)
				staggerDistance = mineDistanceFromEachOther /2f;
			else 
				staggerDistance = 0f;

			for(float j = transform.position.y - radius + staggerDistance; j < transform.position.y + radius + staggerDistance; j += mineDistanceFromEachOther)
			{
				if(shape == Shape.Circle)
				{
					if(Vector3.Distance(new Vector3(i, j, transform.position.z), transform.position) < radius)
					{
						Instantiate(minePrefab, 
							(Vector3)Random.insideUnitCircle*variation + new Vector3(i, j, minePrefab.transform.position.z), 
							Quaternion.identity, this.transform);
					}
				}
				else					
					Instantiate(minePrefab, new Vector3(i, j, minePrefab.transform.position.z), Quaternion.identity, this.transform);
			}

			staggerLine = !staggerLine;
		}


		print(transform.childCount);
	}
	

}
