using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


namespace Completed
{
	using System.Collections.Generic;
	using UnityEngine.UI;

	
	public class GameManager : MonoBehaviour
	{
		public static GameManager instance = null;
		[HideInInspector] public bool playersTurn = true;

		private BoardManager boardScript;
		private List<Enemy> enemies;
		private bool enemiesMoving;	
		private bool doingSetup = true;
		
		void Awake() {
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Destroy(gameObject);
            }
			
			DontDestroyOnLoad(gameObject);
			enemies = new List<Enemy>();
			boardScript = GetComponent<BoardManager>();
		}

        public void AddEnemyToList(Enemy script) {
			enemies.Add(script);
		}
		
		public void GameOver() {
			enabled = false;
		}
	}
}

