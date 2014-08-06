using UnityEngine;
using System.Collections;

public class Flag : Targetable
{
		private Vector3 initialPos;
		private float bottom;
		private Quaternion initialRot;
		public bool taken;
		public PlayerAI carrier = null;

		// Use this for initialization
		new void Start ()
		{
				base.Start();
				bottom = (gc.GetScale ()-1) * 0.24f;
				transform.localScale *= gc.GetScale ();
				taken = false;
				initialPos = transform.position;
				initialPos.y = bottom;
				transform.position = initialPos;
				initialRot = transform.localRotation;
		}

		public void FlagReset ()
		{
				transform.position = initialPos;
				transform.rotation = initialRot;
				taken = false;
				carrier = null;
		}

		void OnTriggerEnter(Collider c)
		{
				if (c.gameObject.name.Contains("Player")){
						PlayerAI player = c.gameObject.GetComponent<PlayerAI>();
						if (player.team != team && !taken){
								taken = true;
								carrier = player;
								carrier.status = PlayerAI.AIState.RETURNFLAG;
						}
				}
				if (c.gameObject.tag == "Side") {
						HomeBase hb = c.gameObject.GetComponent<HomeBase>();
						if (hb.team != team) {
								gc.GameWon(carrier);
						}
				}
		}

		void onTriggerExit(Collider c) {

		}
	
		// Update is called once per frame
		void Update ()
		{
				if (carrier != null) {
						if (carrier.status != PlayerAI.AIState.FROZEN) {
								transform.position = carrier.transform.position;
								transform.rotation = carrier.transform.rotation;
								carrier.status = PlayerAI.AIState.RETURNFLAG;
						}
						else
								FlagReset();
				}
		}
}
