using UnityEngine;
using System.Collections;

public class HomeBase : Targetable {

	// Use this for initialization
	new void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerExit(Collider c)
	{
		if (c.gameObject.name.Contains("Player"))
				c.gameObject.GetComponent<PlayerAI> ().home = (c.gameObject.GetComponent<PlayerAI> ().team != team);
	}
}
