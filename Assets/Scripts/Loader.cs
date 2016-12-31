using UnityEngine;
using System.Collections;

namespace Completed
{
    public class Loader : MonoBehaviour {
        private static Loader instance;
        public GameObject gameManager;
		public GameObject playerLight;

        public static float dampTime = 0.05f;
        private static Vector3 velocity = Vector3.zero;

        public GameObject target;
        public Vector3 offset;
        Vector3 targetPos;

		// Camera pan vars
		private static float inverseScrollSpeed = 1;
        private static Vector2 scrollArea = new Vector2(Screen.width / 6, Screen.height / 6);
        private static Vector3 startingPosition;
        private static bool shouldSetPosition = false;


		private static bool hasApplicationFocus = false;
		public static bool HasApplicationFocus {
			get { return Loader.hasApplicationFocus; }
		}

		private static int cameraHeight;
		private static Size cameraSize; 
		private bool positionInitialized = false;

        private static Size MapSize {
            get { return BoardManager.MapSize; }
        }

		private static bool cameraMoving = false;
		public static bool CameraMoving {
			get { return cameraMoving; }
		}

		public int floorLayer;

        void Start() {
            Loader.instance = this;
            AnimatedSpriteManager asm = new AnimatedSpriteManager();
            targetPos = transform.position;

			cameraHeight = Mathf.RoundToInt(2f * Camera.main.orthographicSize);
			cameraSize = new Size(cameraHeight * Screen.width / Screen.height, cameraHeight); 
			floorLayer = LayerMask.NameToLayer ("Floor");
			playerLight = (GameObject) Instantiate (playerLight, Vector2.zero, Quaternion.identity);
        }

		void OnApplicationFocus(bool hasFocus) {
			hasApplicationFocus = hasFocus;
		}

		void Update() {
			if (Input.GetMouseButton (1)) {
				HandleMouseAction ();
			} else if (Input.GetMouseButton (0)) {
				HandleMouseSelect ();
			}
		}

		void LateUpdate() {
			
		}

        void FixedUpdate() {

			if (hasApplicationFocus) {
				//if (Input.GetKey (KeyCode.Space)) {
				Vector3 centeredAtCenterOfPlayer = Player.Position;
				centeredAtCenterOfPlayer.x -= 0.5f;
				centeredAtCenterOfPlayer.y -= 0.5f;
				centeredAtCenterOfPlayer.z = -10;
				/* transform.position = ClampCameraTargetPosition (centeredAtCenterOfPlayer) */
				Vector3 diff = transform.position - centeredAtCenterOfPlayer;
				float smooth = Mathf.Max (0.05f, Mathf.Log(diff.magnitude) / 2);
				transform.position = Vector3.Lerp (transform.position, centeredAtCenterOfPlayer, smooth * Time.deltaTime);
				playerLight.transform.position = centeredAtCenterOfPlayer + new Vector3 (0, 0, -20);

				//} else if (!(Input.GetMouseButton (0) || Input.GetMouseButton (1))) {
				//	cameraMoving = MoveCameraOnEdgeHover ();	
				//}
			}

			/*if (shouldSetPosition) {
				transform.position = startingPosition;
				shouldSetPosition = false;
			}*/
        }

        public static void SetStartingPosition(Vector3 position) {
			startingPosition = ClampCameraTargetPosition (position);
            shouldSetPosition = true;
        }

        private static void FindStartingPosition() {
            Vector3 position = startingPosition;
            Transform transform = Loader.instance.transform;
            Vector3 posNoZ = transform.position;
            posNoZ.z = position.z;

            Vector3 delta = position - posNoZ;
            Vector3 destination = transform.position + delta;
			transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime * Time.deltaTime);
        }

        void Awake () {
			Application.targetFrameRate = 200;
			//Check if a GameManager has already been assigned to static variable GameManager.instance or if it's still null
			if (GameManager.instance == null)
				
				//Instantiate gameManager prefab
				Instantiate(gameManager);
		}

        bool MoveCameraOnEdgeHover () {

            float mPosX = Input.mousePosition.x;
            float mPosY = Input.mousePosition.y;
			Vector3 delta = Vector3.zero;
			float modifier = 1;

			if (mPosX < 0 || mPosX > Screen.width || mPosY < 0 || mPosY > Screen.width) {
				return false;
			}

            if (mPosX < scrollArea.x && mPosX > 0) {
				//modifier = 1 - mPosX / scrollArea.x;
				delta += Vector3.left * modifier;
            } else if (mPosX >= Screen.width - scrollArea.x && mPosX <= Screen.width) {
				//modifier = 1 - ((float)Screen.width - mPosX) / scrollArea.x;
				delta += Vector3.right * modifier;
            }

            if (mPosY < scrollArea.y && mPosY > 0) {
				//modifier = (1 - mPosY / scrollArea.y);
				delta += Vector3.down * modifier;
            } else if (mPosY >= Screen.height - scrollArea.y && mPosY <= Screen.height) {
				//modifier = 1 - ((float)Screen.height - mPosY) / scrollArea.y;
				delta += Vector3.up * modifier;
            }

			if (delta != Vector3.zero) {
				Vector3 target = transform.position + delta.normalized;
				target.x = Mathf.Clamp (target.x, cameraSize.width / 2 - 1, MapSize.width - cameraSize.width / 2);
				target.y = Mathf.Clamp (target.y, cameraSize.height / 2 - 1, MapSize.height - cameraSize.height / 2);
				transform.position = Vector3.SmoothDamp(transform.position, target, ref velocity, dampTime);
				return true;
            }
			return false;
        }

		void HandleMouseAction () {
			Vector2 origin = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.zero);
			if (hit.transform != null) {
				if (hit.transform.gameObject.layer == floorLayer) {
					// Instantiate (mouseMovePrefab, Input.mousePosition, Quaternion.identity);
				} else if (/* collides with loot */false) {
					// Instantiate (collectLootPrefab, Input.mousePosition, Quaternion.identity);
				} else if (/* collides with enemy */false) {
					// Instantiate (attackEnemyPrefab, Input.mousePosition, Quaternion.identity);
				} else if (/* has active ability selected */false) {
					// Deselect active ability
				}
			}
		}

		void HandleMouseSelect () {
			if (/* has no active ability selected */true) {
				// Display metadata for clickable
			} else if (/* has active ability selected */false) {
				// Use active ability at location
			}
		}

		static Vector3 ClampCameraTargetPosition(Vector3 target) {
			return new Vector3 (
				Mathf.Clamp (target.x, cameraSize.width / 2 - 1, MapSize.width - cameraSize.width / 2),
				Mathf.Clamp (target.y, cameraSize.height / 2 - 1, MapSize.height - cameraSize.height / 2),
				Camera.main.transform.position.z);
		}
    }
}