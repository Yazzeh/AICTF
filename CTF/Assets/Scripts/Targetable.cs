using UnityEngine;
using System.Collections;

public class Targetable : MonoBehaviour {

	public int team;
	public TargetType type;

	// Movement
	public Vector3 velocity;
	public float rotationSpeed;
	public float time;
	public float maxV;
	public float maxA;

	public GameController gc;

	public enum TargetType {
		FLAG,
		PLAYER,
		BASE,
		NODE
	}

	public void Start()
	{
		gc = GameObject.FindGameObjectWithTag ("GameController").GetComponent<GameController> ();

		time = 0.25f;
		maxV = 1.0f;
		maxA = maxV/15.0f;
	}
}
