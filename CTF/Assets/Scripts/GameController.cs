using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class GameController : MonoBehaviour
{

		// Properties
		public GameObject redPlayer;
		public GameObject bluePlayer;
		public Flag redFlag;
		public Flag blueFlag;
		public List<PlayerAI> redPlayers;
		public List<PlayerAI> bluePlayers;
		public Vector3 size = new Vector3(8.8f,0,5.0f);
		private bool gameOver = false;
		public float scale = 1f;
		public int TEAMSIZE = 15;
		public float bestOutOf;
		private int[] score = {0,0};
		public GameState state;

		Circles circle1;
		Circles circle2;

		private AudioSource audioScore;
		private AudioSource audioWin;

		public enum GameState
		{
				PLAY,
				WON
		}

	public float GetScale()
	{
		return scale;
		}

		// Use this for initialization
		void Start ()
		{
				// Generate players
				redPlayers = new List<PlayerAI> ();
				bluePlayers = new List<PlayerAI> ();
				for (int i = 0; i<TEAMSIZE; i++) {
						float posX = Random.Range (-8.0f, -0.5f);
						float posZ = Random.Range (-4.5f, 4.5f);
						Vector3 pos = new Vector3 (posX, 0.0f, posZ);
						redPlayers.Add((Instantiate (redPlayer, pos, Quaternion.Euler(0,Random.Range(0,360),0)) as GameObject).GetComponent<PlayerAI>());
						posX = Random.Range (0.5f, 8.0f);
						posZ = Random.Range (-4.5f, 4.5f);
						pos = new Vector3 (posX, 0.0f, posZ);
						bluePlayers.Add((Instantiate (bluePlayer, pos, Quaternion.Euler(0,Random.Range(0,360),0)) as GameObject).GetComponent<PlayerAI>());
				}
				circle1 = GameObject.FindGameObjectWithTag ("Circle1").GetComponent<Circles> ();
				circle2 = GameObject.FindGameObjectWithTag ("Circle2").GetComponent<Circles> ();

				AudioSource[] aSources = GetComponents<AudioSource> ();
				audioScore = aSources [0];
				audioWin = aSources [1];

				// Play the game!
				state = GameState.PLAY;


		}
	
		// Update is called once per frame
		void Update ()
		{
				if (state == GameState.PLAY) {
						// Check if a player on each team is chasing the flags. If not, choose one at random.
						if (!FlagClaimed (blueFlag)) {
								PlayerAI redguy = redPlayers [Random.Range (0, TEAMSIZE - 1)];
								if (redguy.status != PlayerAI.AIState.FROZEN)
										redguy.status = PlayerAI.AIState.GETFLAG;
						}
						if (!FlagClaimed (redFlag)) {
								PlayerAI blueguy = bluePlayers [Random.Range (0, TEAMSIZE - 1)];
								if (blueguy.status != PlayerAI.AIState.FROZEN)
										blueguy.status = PlayerAI.AIState.GETFLAG;
						}
				}
				circle1.Setup (redFlag.transform.position,1.0f*scale,Color.red,Color.red);
				circle2.Setup (blueFlag.transform.position,1.0f*scale,Color.cyan,Color.cyan);

		}

		bool FlagClaimed (Flag flag)
		{
				List<PlayerAI> playerList;
				if (flag.team == 1)
						playerList = bluePlayers;
				else
						playerList = redPlayers;

				if (!flag.taken) {
						foreach (PlayerAI p in playerList) {
								// If a player is getting the flag, we're good.
								if (p.status == PlayerAI.AIState.GETFLAG) {
										return true;
								}
						}
				} else {
						foreach (PlayerAI p in playerList) {
								if (p.status == PlayerAI.AIState.GETFLAG)
										p.status = PlayerAI.AIState.WANDER;
						}
						return true;
				}
				return false;
		}

		public void GameWon (PlayerAI winner)
		{
				state = GameState.WON;
				score [winner.team - 1] ++;
				if (score [0] < (int)(bestOutOf / 2) + 1 && score [1] < (int)(bestOutOf / 2) + 1) {
						audioScore.Play ();
						ResetGame ();
				}
				else {
						audioWin.Play ();
						gameOver = true;
				}
		}

		void ResetGame ()
		{
				redPlayers.Clear ();
				bluePlayers.Clear ();
				redFlag.FlagReset ();
				blueFlag.FlagReset ();
				GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
				for (int i = 0; i < players.Length; i++) {
						Destroy (players[i]);
				}
				circle1.Reset ();
				circle2.Reset ();
				Start ();
		}

		void RestartGame() {
				score[0] = 0;
				score[1] = 0;
				gameOver = false;
				ResetGame ();
		}

		void OnGUI ()
		{
				GUI.Label (new Rect (10, 10, 50, 20), "Red: " + score [0]);
				GUI.Label (new Rect (Screen.width - 60, 10, 50, 20), "Blue: " + score [1]);
				int first = score[0];
				int second = score[1];
				if (gameOver) {
						string winner = "Red";
						if (score[0] < score[1]) {
								winner = "Blue";
								first = score[1];
								second = score[0];
						}

						GUI.Label (new Rect (Screen.width / 2 - 37, Screen.height / 2 - 30, 75, 20), "Game Over!");
						GUI.Label (new Rect (Screen.width / 2 - 50, Screen.height / 2 - 10, 100, 20), winner + " Wins " + first + " - " + second + "!");
						if (GUI.Button (new Rect (Screen.width / 2 - 100, Screen.height / 2 + 50, 200, 30), "Restart Game!")) {
								RestartGame();
						}
				}
		}
	
}