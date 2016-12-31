using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;	//Allows us to use UI.
using UnityEngine.SceneManagement;		//Allows us to use SceneManager
using System.Linq;


namespace Completed {
	public class Player : MonoBehaviour {
		public float restartLevelDelay = 1f;		//Delay time in seconds to restart level.
		
        private Animation animation;
        private Rigidbody2D body;
		private LayerMask unitMovement;			// Layer which does not block unit movement

        public static Vector3 position;
		private Vector3 velocity = Vector3.zero;

		private Facing facing;
		private enum Facing {
			Right, Left
		}

        private static bool isMoving = false;
        public static bool IsMoving {
            get { return isMoving; }
        }

        public static bool shouldStep = false;

        public static bool playerFound = false;

		private static Player instance = null;

		public static Vector3 Position {
			get { 
				if (Player.instance != null) {
					return Player.instance.body.position;
				}
				return Vector3.zero;
			}
		}

		private float playerSpeed = 10f;

		private static Vector3 targetPosition;

        void Start () {
            body = GetComponent<Rigidbody2D>();

			Player.instance = this;
			unitMovement =  ~((1 << LayerMask.NameToLayer ("Floor")) | (1 << LayerMask.NameToLayer("Units")));
            AnimatedSprite playerSprite = new AnimatedSprite();
            playerSprite.SetRenderers(GetComponentsInChildren<SpriteRenderer>());
            gameObject.AddComponent<Animation>();
            animation = GetComponent<Animation>();

            Transform[] transforms = GetComponentsInChildren<Transform>();
            AnimatedSpriteManager.SetIdleAnimation(animation, transforms);
            AnimatedSpriteManager.SetMoveAnimation(animation, transforms);
            animation.Play("Idle");
        }

        void Update() {
			Vector2 tempVelocity = Vector2.zero;
			if (Input.GetKey (KeyCode.W)) {
				tempVelocity = Vector2.up;
			} else if (Input.GetKey (KeyCode.S)) {
				tempVelocity = Vector2.down;
			}

			if (Input.GetKey (KeyCode.A)) {
				tempVelocity += Vector2.left;
			} else if (Input.GetKey (KeyCode.D)) {
				tempVelocity += Vector2.right;
			}

			if (!Player.isMoving && tempVelocity != Vector2.zero) {
                StartWalkAnimation();
                Player.isMoving = true;
            } else if (Player.isMoving && tempVelocity == Vector2.zero) {
                StartIdleAnimation();
                Player.isMoving = false;
            }

            if (body.IsTouchingLayers (unitMovement)) {
				velocity.x = tempVelocity.x * playerSpeed;
				velocity.y = tempVelocity.y * playerSpeed;
			} else {
				velocity.x = tempVelocity.normalized.x * playerSpeed;
				velocity.y = tempVelocity.normalized.y * playerSpeed;
			}
		}

		void FixedUpdate() {
			body.velocity = velocity;
			Vector2 currentMousePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
			if (currentMousePosition.x > body.position.x && facing != Facing.Right) {
				transform.Rotate(new Vector3(transform.rotation.x, 180f));
				facing = Facing.Right;
			} else if (currentMousePosition.x < body.position.x && facing != Facing.Left) {
				transform.Rotate(new Vector3(transform.rotation.x, 180f));
				facing = Facing.Left;
			}
		}

		void OnSuccessfulStep() {
			//Unexplored.Reveal (Point.Create(next));
			//Cell.HideCellAt (Point.Create(next));
		}
		
		private void OnTriggerEnter2D (Collider2D other) {
			if (other.tag == "Exit") {
				Invoke ("Restart", restartLevelDelay);
				enabled = false;
			}
		}

		private void OnTriggerExit2D (Collider2D other) {
		}
		
		private void Restart () {
			SceneManager.LoadScene (0);
		}

		private void RotatePlayerAsNeeded(int xDir, int yDir) {
			bool shouldFlip = (facing == Facing.Left && xDir > 0) || (facing == Facing.Right && xDir < 0);
			if (shouldFlip) {
				GetComponent<Rigidbody2D>().angularVelocity = 0;
				transform.Rotate(new Vector3(transform.rotation.x, 180f));
				this.facing = this.facing == Facing.Left ? Facing.Right : Facing.Left;
			}
		}

        private void StartWalkAnimation() {
            animation.CrossFade("Move", 0.1f, PlayMode.StopAll);
        }

        private void StartIdleAnimation() {
            animation.CrossFade("Idle", 0.1f, PlayMode.StopAll);
        }
    }


}