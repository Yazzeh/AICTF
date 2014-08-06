using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class PlayerAI : Targetable
{

		// Properties
		public Targetable target = null;
		public PlayerAI helper = null;
		public PlayerAI predator = null;
		private List<PlayerAI> players;
		private List<PlayerAI> enemies;
		public List<Node> nodes;

		public bool home = true;
		public bool helping = false;
		public bool shimmy = false;
		public bool panic = false;

		public float bottom = 0.0f;
		public float sink = 0.0f;
		public float distanceMeasure;
		public int calls=0;
		public int maxcalls = 0;

		private float closeRadius = 0.65f;
		private float seekRange = 4.0f;
		private float dangerZone = 1.5f;
		private float safeZone;

		public AudioSource audioFreeze;
		public AudioSource audioUnfreeze;

		public AIState status;

	// States
	public enum AIState
		{
				WANDER,
				CHASE,
				HELP,
				FROZEN,
				GETFLAG,
				RETURNFLAG
		}

		// Use this for initialization
		new void Start ()
		{
				base.Start ();
				// Set scaling
				bottom = (gc.GetScale ()-1) * 0.24f;
				sink = Mathf.Round((bottom + (-0.3f * gc.GetScale()))*100)/100.0f;
				transform.localScale *= gc.GetScale ();
				maxA *= gc.GetScale ();
				maxV *= gc.GetScale ();
				closeRadius *= gc.GetScale ();
				safeZone = dangerZone + 0.5f;
				seekRange *= gc.GetScale ();
				dangerZone *= gc.GetScale ();
				safeZone *= gc.GetScale ();

				if (team == 1) {
						players = gc.redPlayers;
						enemies = gc.bluePlayers;
				} else {
						players = gc.bluePlayers;
						enemies = gc.redPlayers;
				}

				GameObject[] points = GameObject.FindGameObjectsWithTag("Node");
				foreach (GameObject n in points) {
						Node p = n.GetComponent<Node>();
						if (p.team == team)
								nodes.Add(p);
				}

				velocity = Vector3.zero;
				rotationSpeed = 0.75f;
				status = AIState.WANDER;

				AudioSource[] aSources = GetComponents<AudioSource> ();
				audioFreeze = aSources [0];
				audioUnfreeze = aSources [1];
		}

		// Update is called once per frame
		void Update ()
		{
		calls = 0;
				if (gc.state == GameController.GameState.PLAY) {
						if (status == AIState.WANDER)
								LocateTarget (seekRange);
						if (!panic)
								DoState ();
						if (status != AIState.FROZEN) {
								helper = null;
								transform.position = new Vector3(transform.position.x,bottom,transform.position.z);
								EscapeDanger (dangerZone);
								DoMovement ();
								//Debug.DrawRay (transform.position, this.transform.forward, Color.blue);
						}
						ForgetAboutIt();
				}
		if (calls > maxcalls)
						maxcalls = calls;
		}

		void DoState ()
		{
				switch (status) {
				case AIState.WANDER:
						{
								Wander ();
								break;
						}
				case AIState.CHASE:
						{
								// Check if there's still something to chase.
								if (target.tag == "Player"){
										PlayerAI e = (PlayerAI)target;
										// If target is not frozen and not home, or if it has the flag, chase.
										if ((e.status != AIState.FROZEN && !e.home) || e.status == AIState.RETURNFLAG) {
												Pursue ();
										} else {
												target = null;
												Align (new Vector3(Random.Range(-8,8),0,Random.Range(-5,5)));
												status = AIState.WANDER;
										}
								}
								break;
						}
				case AIState.HELP:
						{
								if (!helping) {
										// Go to nearest frozen ally and unfreeze them.
										float closest = float.MaxValue;
										foreach (PlayerAI p in players) {
												// If player is frozen and on the same team
												if (p.status == AIState.FROZEN) {
														// If no one is trying to help
														if (p.helper == null) {
																helping = true;
																// Check if closest
																float dist = TrueDistance (transform.position, p.transform.position);
																if (dist < closest) {
																		closest = dist;
																		if (target != null)
																				((PlayerAI)target).helper = null;
																		target = p;
																		p.helper = this;
																}
														} else {
																// If a helper is already assigned, check if you're closer than them
																if (TrueDistance (transform.position, p.transform.position) < TrueDistance (p.helper.transform.position, p.transform.position)) {
																		helping = true;
																		target = p;
																		p.helper = this;
																}
																else
																	status = AIState.WANDER;
														}
												}
										}
								}
								else
										if (((PlayerAI)target).status == AIState.FROZEN && ((PlayerAI)target).helper == this)
												SteeringArrive (target.transform.position);
										else
												status = AIState.WANDER;										
								break;
						}
				case AIState.FROZEN:
						{
								// Frozen to spot. Do nothing until unfrozen.
								transform.position = new Vector3(transform.position.x,sink,transform.position.z);
								target = null;
								helping = false;
								predator = null;
								velocity = Vector3.zero;
								
								if (helper != null)
										if (helper.target != this)
												helper = null;
								if (home)
										status = AIState.WANDER;
								break;
						}
				case AIState.GETFLAG:
						{
								// Getting the flag
								if (team == 1)
										target = gc.blueFlag;
								else
										target = gc.redFlag;
								
								SteeringArrive (target.transform.position);
								break;
						}
				case AIState.RETURNFLAG:
						{
								// Has flag, return home while evading / fleeing
								//if (home)
								//		gc.GameWon (this);
								float closest = float.MaxValue;
								Node close = null;
								foreach (Node n in nodes) {
										float dist = TrueDistance(transform.position,n.transform.position);
										if (dist < closest) {
												closest = dist;
												close = n;
										}
								}
								target = close;
								SteeringArrive (target.transform.position);
								break;
						}
				}
		}

		void ForgetAboutIt() {
				if (target != null && status == AIState.CHASE) {
						if (TrueDistance (transform.position, target.transform.position) > 2 * seekRange) {
								target = null;
								status = AIState.WANDER;
						}
				}
		}

		void DoMovement ()
		{				
				if (shimmy) {
						transform.Translate (velocity * Time.deltaTime);
						Debug.DrawRay(transform.position,velocity,Color.blue);
				}
				//else
						transform.Translate (Vector3.forward * velocity.magnitude * Time.deltaTime);
				shimmy = false;
		}

		float TrueDistance (Vector3 p1, Vector3 p2)
		{
				float xDis = Mathf.Abs (p1.x - p2.x);
				float zDis = Mathf.Abs (p1.z - p2.z);

				// If the distance on the x axis is longer than half the field, calculate the distance using toroid.
				if (xDis > gc.size.x) {
						// If the position is closer to the left
						if (p1.x <= 0)
								xDis = Mathf.Abs ((-gc.size.x - p1.x) - (gc.size.x - p2.x));
						else // Closer to right
								xDis = Mathf.Abs ((gc.size.x - p1.x) - (-gc.size.x - p2.x));
				}

				// If the distance on the z axis is longer than half the field, calculate the distance using toroid.
				if (zDis > gc.size.z) {
						// If the position is closer to the bottom
						if (p1.z <= 0)
								zDis = Mathf.Abs ((-gc.size.z - p1.z) - (gc.size.z - p2.z));
						else // Closer to top
								zDis = Mathf.Abs ((gc.size.z - p1.z) - (-gc.size.z - p2.z));
				}

				float closer = Vector3.Distance (p1, p2);
				float newDis = Mathf.Sqrt (Mathf.Pow (xDis, 2) + Mathf.Pow (zDis, 2));

				if (closer > newDis)
						closer = newDis;

				distanceMeasure = closer;
				return closer;
		}

		Vector3 TrueDirection (Vector3 source, Vector3 destination)
		{
				float xDir = destination.x - source.x;
				float zDir = destination.z - source.z;
		
				// If the distance on the x axis is longer than half the field, calculate the direction using toroid.
				if (Mathf.Abs (xDir) > gc.size.x) {
						// If the position is closer to the left
						if (source.x <= 0)
								xDir = (-gc.size.x - source.x) - (gc.size.x - destination.x);
						else // Closer to the right
								xDir = (gc.size.x - source.x) - (-gc.size.x - destination.x);
				}

				// If the distance on the z axis is longer than half the field, calculate the direction using toroid.
				if (Mathf.Abs (zDir) > gc.size.z) {
						// If the position is closer to the bottom
						if (source.z <= 0)
								zDir = (-gc.size.z - source.z) - (gc.size.z - destination.z);
						else // Closer to the top
								zDir = (gc.size.z - source.z) - (-gc.size.z - destination.z);
				}
				float euler = Vector3.Distance (source, destination);
				float toroid = Mathf.Sqrt (Mathf.Pow (xDir, 2) + Mathf.Pow (zDir, 2));

				// If Toroidal distance is less than euler distance
				if (toroid < euler) {
						// return Toroidal direction
						return (new Vector3 (xDir, 0, zDir));
				} else {
						Vector3 dist = destination - source;
						dist.y = 0;
						return dist;
				}
		}

		bool LocateTarget (float range)
		{
				// If the player has no target already, try to find one within range! Like... to help. Or... to touch inappropriately inside the other player's... area.
				if (target == null) {
						float closest = float.MaxValue;
						PlayerAI victim = null;
						foreach (PlayerAI e in enemies) {
								// If there is an enemy within seek range, get the closest one
								float dist = TrueDistance (e.transform.position, transform.position);
								if (dist <= range && !e.home) {
										if (dist < closest) {
												closest = dist;
												victim = e;
										}
								}
						}
						if (victim != null) {
								if (!victim.home && victim.status != AIState.FROZEN){
										target = victim;
										status = AIState.CHASE;
										return true;
								} else {
										target = null;
								}
						} else {
							target = null;
						}

						foreach (PlayerAI p in players) {
								if (p.status == AIState.FROZEN) {
										status = AIState.HELP;
										return true;
								}
						}
						return false;
				}
				return false;
		}

		void EscapeDanger (float range)
		{
				// If you're currently panicking and escaping, you need to clear the safezone!
				if (panic) {
						range = safeZone;
				}
				
				bool fleeing = false;
				float closest = float.MaxValue;
				PlayerAI close = null;
				// Locate threats!
				foreach (PlayerAI e in enemies) {
						float dist = TrueDistance(transform.position,e.transform.position);
						// If player is not home, and an enemy is within the dangerzone
						if (!home && dist <= range) {
								fleeing = true;
								panic = true;
								if (dist < closest) {
										closest = dist;
										close = e;
								}
						} 
				}
				if (fleeing) {
						predator = close;
						SteeringFlee (close);
				}
						
				panic = fleeing;
		}

		void Wander ()
		{
				Vector3 wanderPosition = transform.localPosition + 20 * Random.insideUnitSphere + transform.forward * 2;
				wanderPosition.y = bottom;

				CalculateVelocity (wanderPosition - transform.position);
				// Set new rotation angle due to targeting
				Quaternion targetRotation = Quaternion.LookRotation (velocity.normalized);
				transform.rotation = Quaternion.Slerp (transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
		}

		void Pursue ()
		{
		SteeringArrive (target.transform.position+target.velocity*(TrueDistance(transform.position,target.transform.position))/(gc.GetScale()*2.0f));
				//Debug.DrawLine(transform.position,target.transform.position,Color.cyan);
		}
	
		void SteeringArrive (Vector3 targetPosition)
		{
				Vector3 direction = TrueDirection (transform.position, targetPosition);
				Debug.DrawRay (transform.position, direction, Color.green);
				float distance = TrueDistance (transform.position, targetPosition);
				// If stationary or slow
				if (velocity.magnitude <= 0.05f) {
						// If small distance
						if (distance <= closeRadius) {
								// Move directly to target (forget looking)
								shimmy = true;
								CalculateVelocity (direction);
								
						} else {
								// If large distance, align before moving
								Debug.DrawRay(transform.position,direction,Color.white);
								
								if (Vector3.Angle(transform.forward,direction) > 10.0f) {
										Decelerate();
										Align (direction);
								} else {
										Align (direction);
										CalculateVelocity(direction);
								}							
						}
				
						// If moving
				} else {
						// If target is within a speed-dependent perception zone in front of it, continue moving with align towards target
						float angle = 180/((velocity.magnitude/gc.GetScale())*3.0f +1);
						Debug.DrawRay(transform.position,direction,Color.yellow);
						Debug.DrawRay (transform.position,transform.forward, Color.yellow);
						if (Vector3.Angle(transform.forward, direction) <= angle) {
								Align (direction);
								CalculateVelocity(direction);
						} else {
								// If target is outside of perception zone, stop moving
								if (velocity.magnitude > 0.05f) {
										// Decelerate
										Decelerate();
								}
						}
				}
		}

		void SteeringFlee (PlayerAI predator)
		{
				Vector3 direction = TrueDirection (transform.position, predator.transform.position) * (-1);
				float distance = TrueDistance (transform.position, predator.transform.position);
				// If small distance
				if (distance <= closeRadius) {
						// Move away directly
						shimmy = true;
						CalculateVelocity (direction);
				} else {
						// If large distance, stop to unalign from target before moving
						//if (velocity.magnitude > 0.05f) {
						//		Decelerate();
						//} else {
								if (Vector3.Angle (transform.forward, direction) <= 5.0f) {
										Align (direction);
										CalculateVelocity (direction);
								} else {
										Align(direction);
								}
						//}
				}
		}

		void Align (Vector3 direction)
		{
				// Set new rotation angle due to targeting
				direction.y = 0;
				Quaternion targetRotation = Quaternion.LookRotation (direction.normalized);
				transform.rotation = Quaternion.Slerp (transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
		}

		void OnTriggerEnter (Collider c)
		{
				// Check if hitting another player
				if (c.gameObject.name.Contains ("Player")) {
						PlayerAI otherPlayer = c.gameObject.GetComponent<PlayerAI> ();
						if (otherPlayer.team != team) {
								if (otherPlayer.status == AIState.RETURNFLAG) {
										if (team == 1)
												gc.redFlag.FlagReset();
										else
												gc.blueFlag.FlagReset();
								}
								if (!otherPlayer.home && status != AIState.FROZEN) {
										otherPlayer.status = AIState.FROZEN;
										otherPlayer.panic = false;
										if (!audioFreeze.isPlaying)					
												audioFreeze.Play ();
								}
						} else if (otherPlayer.status == AIState.FROZEN) {
								helping = false;
								otherPlayer.helper = null;
								status = AIState.WANDER;
								otherPlayer.status = AIState.WANDER;
								if (!audioUnfreeze.isPlaying)					
									audioUnfreeze.Play ();
						}

						// Check if hitting flag when it isnt taken
				} else if (c.gameObject.tag == "Flag") {
						Flag flag = c.gameObject.GetComponent<Flag> ();
						if (flag.team != team && !flag.taken) {
								status = AIState.RETURNFLAG;
						}
				}
		}

		void CalculateVelocity (Vector3 Direction)
		{
				calls++;
				Vector3 acceleration = Vector3.zero;
				Vector3 newVelocity = Vector3.zero;
				if (shimmy) {
						if (predator != null) {
								acceleration = maxA * Vector3.Normalize((-1)*TrueDirection(transform.position,predator.transform.position));
								newVelocity = velocity + (acceleration * time);
						}
						else {
								if (target != null) {
										acceleration = maxA * Vector3.Normalize((-1)*TrueDirection(transform.position,target.transform.position));
										newVelocity = velocity + (acceleration * time);
								}
						}
				} else {
						acceleration = maxA * Vector3.Normalize (Direction);
						newVelocity = velocity + (acceleration * time);
				}

				if (Vector3.Magnitude (newVelocity) > maxV)	
						newVelocity = maxV * Vector3.Normalize (newVelocity);
				velocity = newVelocity;
				velocity.y = 0;
		}

		void Decelerate() {
				if (!Mathf.Approximately (velocity.magnitude, 0.0f))
						velocity -= maxA*Vector3.Normalize(velocity);
				velocity.y = 0;
		}


	void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position,dangerZone);
		Gizmos.color = Color.green;
		//Gizmos.DrawWireSphere(transform.position,seekRange);
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(transform.position,closeRadius);
		Gizmos.color = Color.cyan;
		//Gizmos.DrawWireSphere(transform.position,safeZone);
	}
}
