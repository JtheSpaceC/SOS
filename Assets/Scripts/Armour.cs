using UnityEngine;
using System.Collections;

public class Armour : Health {

	SpriteExploder spriteExploderScript;
	public bool isGibbed = false;

	public int numberOfCuts = 3;
	public float objRadius = 1;


	void Awake()
	{
		spriteExploderScript = GameObject.FindGameObjectWithTag ("SpriteExploder").GetComponent<SpriteExploder> ();	
	}
	
	void Update () 
	{
		/*if (Input.GetKeyDown (KeyCode.R) && gameObject.name.StartsWith("armour 1"))
			FractureThenScatter ();*/
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if(other.tag == "Bomb")
		{
			other.GetComponent<Missile>().Explode();
			Tools.instance.SpawnExplosion(other.gameObject, other.transform.position + (other.transform.up * 0.5f), false);

			FractureThenScatter();
		}
	}

	public void FractureThenScatter()
	{
		if(!isGibbed)
		{
			spriteExploderScript.Fracture(this.gameObject, numberOfCuts, objRadius);
		}
		else 
		{
			Collider2D[] scatterObjects = Physics2D.OverlapCircleAll 
				(transform.position, 2, LayerMask.GetMask(LayerMask.LayerToName(this.gameObject.layer)));
			foreach(Collider2D scatterObj in scatterObjects)
			{
				scatterObj.transform.SetParent(Tools.instance.destructionBin);
				Rigidbody2D objRB = scatterObj.GetComponent<Rigidbody2D>();
				objRB.isKinematic = false;
				objRB.AddForce(Random.insideUnitCircle.normalized * 25, ForceMode2D.Force);
				objRB.AddTorque(Random.Range(-200, 200));
				
				scatterObj.GetComponent<Collider2D>().enabled = false;

				scatterObj.gameObject.AddComponent<FadeAndDestroyMesh>(); 

			}
		}
	}
}
