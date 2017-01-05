using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class StratPOI : MonoBehaviour {

	public List<StratPOI> myConnections = new List<StratPOI>();

	/*public void MakeConnections(StratPOI other)
	{
		if(!other.myConnections.Contains(this))
		{
			myConnections.Add(other);
			other.myConnections.Add(this);
		}
	}*/

	public void DrawLines()
	{
		for(int i = 0; i < myConnections.Count; i++)
		{
			GameObject lineChild = new GameObject("Line Child "+i);
			lineChild.transform.SetParent(this.transform);
			LineRenderer lr = lineChild.AddComponent<LineRenderer>();

			lr.startWidth = 0.05f;
			lr.endWidth = 0.05f;

			if(myConnections[i] != null)
			{
				lr.SetPosition(0, transform.position);
				lr.SetPosition(1, myConnections[i].transform.position);
			}
		}
	}
}
