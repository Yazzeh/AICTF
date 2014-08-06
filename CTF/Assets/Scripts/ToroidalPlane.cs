using UnityEngine;
using System.Collections;

public class ToroidalPlane : MonoBehaviour
{
		public float offset = 0.1f;
		private float x = 0.0f;
		private float z = 0.0f;
		public GameController gc;

		// Use this for initialization
		void Start ()
		{
				gc = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameController>();
		}

		void OnTriggerExit (Collider other)
		{ 
				x = other.transform.position.x;
				z = other.transform.position.z;

				if (other.transform.position.x < -(gc.size.x-.01f)) //left
						x = (-1*(other.transform.position.x+offset));
				if (other.transform.position.x > (gc.size.x-.01f)) // right
						x = (-1*(other.transform.position.x-offset));

				if (other.transform.position.z < -(gc.size.z-.01f)) //bottom
						z= (-1*(other.transform.position.z+offset));
				if (other.transform.position.z > (gc.size.z-.01f)) // top
						z= (-1*(other.transform.position.z-offset));

				other.transform.position = new Vector3 (x, 0, z); 
		}
	
		// Update is called once per frame
		void Update ()
		{
	
		}
}
