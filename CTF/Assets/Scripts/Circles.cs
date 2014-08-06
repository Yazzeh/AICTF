using UnityEngine;
using System.Collections;

public class Circles : MonoBehaviour {

	private Vector3 location;
	private float radius;
	private Color color1;
	private Color color2;
	private bool toggle;

	private float theta_scale;
	private int size;
	LineRenderer lineRenderer;

	// Use this for initialization
	void Start () {
			theta_scale = 2.0f;             //Set lower to add more points
			size = (int)((2.0f * Mathf.PI) / theta_scale)+1; //Total number of points in circle.
			lineRenderer = gameObject.AddComponent<LineRenderer>();
			lineRenderer.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (toggle)
				DrawCircles ();
	}

	public void Setup(Vector3 l, float r, Color c1, Color c2)
	{
		location = l;
		radius = r;
		color1 = c1;
		color2 = c2;
		toggle = true;
		lineRenderer.enabled = true;
		}

	public void Reset()
	{
		toggle = false;
		lineRenderer.enabled = false;
		}

	void DrawCircles(){
		lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		lineRenderer.SetColors(color1,color2);
		lineRenderer.SetWidth(0.05f, 0.05f);
		lineRenderer.SetVertexCount(size);

		int i = 0;
		for(float theta = 0.0f; theta < 2.0f * Mathf.PI; theta += theta_scale) {
			float x = radius*Mathf.Cos(theta);
			float z = radius*Mathf.Sin(theta);
			
			Vector3 pos = new Vector3(x, 0.0f, z)+location;
			lineRenderer.SetPosition(i, pos);
			i+=1;
		}
	}
}
