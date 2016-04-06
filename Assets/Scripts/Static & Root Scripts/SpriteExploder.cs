using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpriteExploder : MonoBehaviour {

	public static SpriteExploder instance;

	public GameObject target; //for testing, always find and explode this object when mouse is clicked (anywhere)
	public LayerMask debrisMask;
	public int cuts = 3;
	public float force = 30;
	public ForceMode2D forceMode;

	public float objectRadius = 0.5f;
	public float explosionRadius = 1;

	public bool createShadows = true;

	public Transform destroyBin;
	public Transform dontDestroyBin;
	public float binPurgeTime = 30;
	

	void Awake()
	{
		if(instance == null)
		{
			instance = this;
		}
		else 
			Destroy(gameObject);
	}

	void Start()
	{
		InvokeRepeating ("ClearBin", binPurgeTime, binPurgeTime);
	}


	public void Explode(GameObject explodeObj, int numCuts, float objRadius)
	{
		Vector2 parentVelocity = explodeObj.tag == "Turret"? 
			explodeObj.transform.parent.parent.GetComponent<Rigidbody2D>().velocity : explodeObj.GetComponent<Rigidbody2D> ().velocity;
		explodeObj.layer = LayerMask.NameToLayer("Debris");
		explodeObj.tag = explodeObj.tag == "PlayerFighter"? explodeObj.tag: "Debris";

		List<SpriteSlicer2DSliceInfo> info = new List<SpriteSlicer2DSliceInfo> ();

		//Cut by specifying random points around the object
		for(int i=0; i<numCuts; i++)
		{
			Vector3 offset1 = (Vector3)Random.insideUnitCircle.normalized * objRadius/4; //brought it in tighter to make sure a line would pass close to the ship's centre
			Vector3 offset2 = (Vector3)Random.insideUnitCircle.normalized * objRadius;

			Debug.DrawLine(explodeObj.transform.position + offset1, explodeObj.transform.position + offset2, Color.green, 2); //testing

			//the lines drawn needed to fully intersect the collider, so now that the points were inside the object's radius, I lengthened them out
			Vector3 worldPointLengthened1 = explodeObj.transform.position + offset1 
				+ (offset1 - offset2).normalized*3;
			Vector3 worldPointLengthened2 = explodeObj.transform.position + offset2
				+ (offset2 - offset1).normalized*3;

			Debug.DrawLine(worldPointLengthened1, worldPointLengthened2, Color.red, 1); //testing

			SpriteSlicer2D.SliceAllSprites(worldPointLengthened1, worldPointLengthened2, false, ref info, debrisMask); //false meant won't destroy original, but set inactive instead/ 
			//the ref info is required for the Destroy bool to be taken

			foreach(SpriteSlicer2DSliceInfo inf in info)
			{
				if(inf != info[0])
					inf.SlicedObject.transform.parent = destroyBin;

				//these are the larger, legit fragments
				foreach(GameObject childObject in inf.ChildObjects)
				{                    
					childObject.transform.parent = destroyBin;
					childObject.GetComponent<Collider2D>().isTrigger = true;
				}
			}
			//this is the one original object
			explodeObj.SetActive (true);
			explodeObj.GetComponent<SpriteRenderer>().enabled = false;
			explodeObj.GetComponent<Collider2D>().enabled = false;
		}

		//Apply an explosion force to the pieces
		Collider2D[] gibs = Physics2D.OverlapCircleAll (explodeObj.transform.position, explosionRadius, debrisMask); 

		foreach(Collider2D gib in gibs)
		{
			if(gib.GetComponent<Rigidbody2D>() != null)
			{
				if(createShadows)
				{
					CreateShadows(gib.gameObject, explodeObj.transform.position);
				}
				gib.transform.parent = explodeObj.transform;

				gib.GetComponent<Collider2D>().enabled = false; //I just want them to float instead of bounce off anything

				Rigidbody2D gibRB = gib.GetComponent<Rigidbody2D>();
				gibRB.velocity = parentVelocity;

				gibRB.AddForce(Random.insideUnitCircle.normalized * force, forceMode); 
				gibRB.AddTorque(Random.Range(-force*8, force*8)); 
				gib.gameObject.AddComponent<FadeAndDestroyMesh>();

			}
		}
	}



	public void Fracture(GameObject explodeObj, int numCuts, float objRadius)
	{
		string originalLayer = LayerMask.LayerToName (explodeObj.layer);
		explodeObj.layer = LayerMask.NameToLayer("Debris");

		List<SpriteSlicer2DSliceInfo> info = new List<SpriteSlicer2DSliceInfo> ();
		
		//Cut by specifying random points around the object
		for(int i=0; i<numCuts; i++)
		{
			Vector3 offset1 = (Vector3)Random.insideUnitCircle.normalized * objRadius/4; //brought it in tighter to make sure a line would pass close to the ship's centre
			Vector3 offset2 = (Vector3)Random.insideUnitCircle.normalized * objRadius;
			
			Debug.DrawLine(explodeObj.transform.position + offset1, explodeObj.transform.position + offset2, Color.green, 2); //testing
			
			//the lines drawn needed to fully intersect the collider, so now that the points were inside the object's radius, I lengthened them out
			Vector3 worldPointLengthened1 = explodeObj.transform.position + offset1 
				+ (offset1 - offset2).normalized*3;
			Vector3 worldPointLengthened2 = explodeObj.transform.position + offset2
				+ (offset2 - offset1).normalized*3;
			
			Debug.DrawLine(worldPointLengthened1, worldPointLengthened2, Color.red, 1); //testing
			
			SpriteSlicer2D.SliceAllSprites(worldPointLengthened1, worldPointLengthened2, false, ref info, debrisMask); //false meant won't destroy original, but set inactive instead/ 
			//the ref info is required for the Destroy bool to be taken
			
			foreach(SpriteSlicer2DSliceInfo inf in info)
			{
				if(inf != info[0])
					inf.SlicedObject.transform.parent = destroyBin;
				
				//these are the larger, legit fragments
				foreach(GameObject childObject in inf.ChildObjects)
				{                    
					childObject.transform.parent = destroyBin;
					childObject.GetComponent<Collider2D>().isTrigger = true;
				}
			}
			//this is the one original object
			explodeObj.SetActive (true);
			explodeObj.GetComponent<SpriteRenderer>().enabled = false;
			explodeObj.GetComponent<Collider2D>().enabled = false;
		}
		
		//Apply properties to the pieces
		Collider2D[] gibs = Physics2D.OverlapCircleAll (explodeObj.transform.position, explosionRadius, debrisMask); 

		//this will fix where armour segments are scaled at -1 in the X, which I use for having a single asset appear mirrored on both sides
		int invert = explodeObj.transform.localScale.x < 0 ? -1 : 1;

		foreach(Collider2D gib in gibs)
		{
			if(gib.GetComponent<Rigidbody2D>() != null)
			{
				gib.transform.parent = explodeObj.transform;
				gib.gameObject.layer = LayerMask.NameToLayer(originalLayer);
				gib.gameObject.AddComponent<Armour>();
				gib.GetComponent<Armour>().isGibbed = true;

				Vector3 myScale = gib.transform.localScale;
				myScale.x /= 1.05f * invert;
				myScale.y /= 1.05f;
				myScale.z /= 1.05f;

				gib.transform.localScale = myScale;
			} 
		}
		if(invert == -1)
		{
			Vector3 parentScale = explodeObj.transform.localScale;
			parentScale.x *= invert;
			explodeObj.transform.localScale = parentScale;
		}
	}



	void CreateShadows(GameObject gib, Vector3 parentPos)
	{
		//gib.transform.position -= (parentPos - gib.transform.position).normalized * 0.01f;

		//TODO: create shadows
		/*Material mat = gib.GetComponent<MeshRenderer> ().materials[0];
		GameObject gibShadow = new GameObject ();
		gibShadow.transform.parent = gib.transform;
		gibShadow.transform.localScale = new Vector3 (1.1f, 1.1f, 1);
		MeshRenderer shadowRenderer = gibShadow.AddComponent<MeshRenderer> ();
		shadowRenderer.material = mat;
		shadowRenderer.sortingLayerName = "Fighters";
		shadowRenderer.sortingOrder = -10;
*/
	}

	void ClearBin()
	{
		while(destroyBin.childCount > 0)
		{
			DestroyImmediate(destroyBin.GetChild(0).gameObject);
		}
	}
}