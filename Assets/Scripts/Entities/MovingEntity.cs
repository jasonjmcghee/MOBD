using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Completed
{
	public abstract class MovingEntity : MonoBehaviour {
        protected float speed = 1f;

        public float moveTime = 10f;
		private float inverseMoveTime;
		public static float dampTime = 0.05f;
        private Vector3 intendedVelocity = Vector3.zero;
		private Vector3 velocity = Vector3.zero;
		public LayerMask unitMovement;
		private Vector3 targetPosition;
		private Vector3 lastValidPosition;
        protected Animation animation;
        private Rigidbody2D body;

        // Pathing
        private List<Point> currentPath;
		private Vector2 pathEnd;


		// Misc
		private BoxCollider2D boxCollider;
		private Facing facing;
		private enum Facing {
			Right, Left
		}

		protected virtual void Start () {
			boxCollider = GetComponent <BoxCollider2D> ();
            body = GetComponent <Rigidbody2D> ();
			inverseMoveTime = 1f / moveTime;

			facing = Mathf.Round(transform.eulerAngles.y) == 0f ? Facing.Right : Facing.Left;
			targetPosition = transform.position;
            intendedVelocity = Vector2.zero;
			unitMovement =  ~((1 << LayerMask.NameToLayer ("Floor")) | (1 << LayerMask.NameToLayer("Units")));
            StartIdleAnimation();
        }

		protected void FixedUpdate() {
            if (!body.velocity.Equals(Vector3.zero)) {
                body.velocity = Vector3.zero;
            }
			Vector3 diff = targetPosition - transform.position;
			if (intendedVelocity.magnitude >= diff.magnitude) {
				transform.position += diff;
                intendedVelocity = Vector3.zero;
				OnSuccessfulStep ();
			} else if (intendedVelocity != Vector3.zero) {
				transform.position += intendedVelocity;
            }

            if (intendedVelocity.x > 0 && facing != Facing.Right) {
                transform.Rotate(new Vector3(transform.rotation.x, 180f));
                facing = Facing.Right;
            } else if (intendedVelocity.x < 0 && facing != Facing.Left) {
                transform.Rotate(new Vector3(transform.rotation.x, 180f));
                facing = Facing.Left;
            }
        }

        protected bool Move (Vector3 target) {
            StartWalkAnimation();
			pathEnd = target;
			currentPath = GetPathFor (target);
			//OptimizeCurrentPath ();
			Step ();
			return true;
		}
		
		protected void Step () {
			Vector2 start = transform.position;

			Vector2 newPos;
			if (currentPath.Count < 1 && CanMoveDirectlyBetween (Point.Create(transform.position), Point.Create(pathEnd))) {
				newPos = pathEnd;
			} else if (currentPath.Count > 0) {
				Point next = currentPath [0];
				newPos = next.AsVector ();
			} else {
				return;
			}
			Vector2 length = (newPos - start);

			int xDir = Mathf.RoundToInt (length.x == 0f ? 0 : Mathf.Sign(length.x));
			int yDir = Mathf.RoundToInt (length.y == 0f ? 0 : Mathf.Sign(length.y));
            RotateAsNeeded(xDir, yDir);

			targetPosition = newPos;
			Vector2 normDist = length.normalized;
            intendedVelocity = new Vector2 (normDist.x * speed * inverseMoveTime * Time.fixedDeltaTime,
				normDist.y * speed * inverseMoveTime * Time.fixedDeltaTime);
		}

		protected virtual void OnSuccessfulStep() {
            if (currentPath != null && currentPath.Count > 0) {
                currentPath.RemoveAt(0);
                if (currentPath.Count > 0) Step();
            } else {
                StartIdleAnimation();
            }
		}

		private void RotateAsNeeded(int xDir, int yDir) {
			bool shouldFlip = (facing == Facing.Left && xDir > 0) || (facing == Facing.Right && xDir < 0);
			if (shouldFlip) {
				GetComponent<Rigidbody2D>().angularVelocity = 0;
				transform.Rotate(new Vector3(transform.rotation.x, 180f));
				this.facing = this.facing == Facing.Left ? Facing.Right : Facing.Left;
			}
		}

		protected virtual List<Point> GetAdjacentFloor(Point point) {
			List<Point> neighbors = new List<Point>();
			if (BoardManager.IsFloor(point.x - 1, point.y)) neighbors.Add(new Point(point.x - 1, point.y));
			if (BoardManager.IsFloor(point.x + 1, point.y)) neighbors.Add(new Point(point.x + 1, point.y));
			if (BoardManager.IsFloor(point.x, point.y - 1)) neighbors.Add(new Point(point.x, point.y - 1));
			if (BoardManager.IsFloor(point.x, point.y + 1)) neighbors.Add(new Point(point.x, point.y + 1));
			return neighbors;
		}

		private List<Point> GetPathFor(Vector3 vTarget) {
			//currentPath = BoardManager.GetManhattanPath (Point.Create (transform.position), Point.Create(target));
			return FindPathTo(vTarget);
		}

		protected virtual List<Point> FindPathTo(Vector3 vTarget) {
			List<Point> path = new List<Point>();
			Point start = Point.Create (transform.position);
			Point target = Point.Create (vTarget);
			if (HasLineOfSightTo (vTarget)) {
				path.Add (target);
			} else {
				Room startingRoom = BoardManager.GetRoom (start);
				if (startingRoom != null) {
					start = startingRoom.entryWay;
				}
				Room targetRoom = BoardManager.GetRoom (target);
				if (targetRoom != null) {
					target = targetRoom.entryWay;
				}
				path = start.FindPathAStar (target, GetAdjacentFloor, Point.PointManhattanDistance);
				if (targetRoom != null) {
					path.Add (Point.Create (vTarget));
				}
				if (path != null && path.Count > 0 && startingRoom == null) {
					path.RemoveAt (0);
				}
			}
			return path;
		}

		private bool ShouldMove() {
			return HasLineOfSightTo (targetPosition);
		}

		private bool HasLineOfSightTo(Vector2 target) {
			boxCollider.enabled = false;
			RaycastHit2D hit = Physics2D.Linecast (transform.position, target, unitMovement);
			boxCollider.enabled = true;
			return hit.transform == null;
		}

		private bool LineOfSightBetween(Vector2 start, Vector2 target) {
			if (Point.Create (start).Equals (Point.Create (transform.position))) {
				return HasLineOfSightTo (target);
			}
			return Physics2D.Linecast (start, target, unitMovement).transform == null;
		}

		private bool CanMoveDirectlyBetween(Point start, Point target) {
			return BoardManager.HasLineOfSight (start, target);
		}

		private System.Func<Point, List<Point>> PointsWithLineOfSight(Dictionary<Point, int> pointLookup) {
			return new Func<Point, List<Point>> (point => {
				List<Point> points = new List<Point>();
				int index = pointLookup[point];

				if (index < currentPath.Count - 1) {
					points.Add(currentPath[index]);
				}

				for (int i = index + 1; i < currentPath.Count; i++) {
					if (CanMoveDirectlyBetween (point, currentPath[i])) {
						points.Add(currentPath[i]);
					}
				}
				return points;
			});
		}

		private void OptimizeCurrentPath () {
			// Only optimize if the current path is at least of length 2
			if (currentPath != null && currentPath.Count > 1) {
				Dictionary<Point, int> pointLookup = new Dictionary<Point, int> ();
				Point current = Point.Create (transform.position);
				pointLookup [current] = 0;

				for (int i = 0; i < currentPath.Count; i++) {
					pointLookup [currentPath [i]] = i + 1;
				}
				List<Point> newPath = current.FindPathAStar (currentPath [currentPath.Count - 1], PointsWithLineOfSight (pointLookup), Point.PointEuclideanDistance);
				if (newPath != null && newPath.Count > 0) {
					if (newPath [0].Equals (current)) {
						newPath.RemoveAt (0);
					}

					if (newPath.Count > 1) {
						if (CanMoveDirectlyBetween (current, newPath [1])) {
							newPath.RemoveAt (0);
						}
					}
					currentPath = newPath;
				}
			}
		}

        private void StartWalkAnimation() {
            animation.CrossFade("Move", 0.1f, PlayMode.StopAll);
        }

        private void StartIdleAnimation() {
            animation.CrossFade("Idle", 0.1f, PlayMode.StopAll);
        }

        // Old / Unused stuff

        // We raycast (while binary searching) to skip to the furthest possible!
        // Idea is good, but flawed. Not used for now.
        private int IndexOfFurthestDirectPointInCurrentPath(Point furthest, Vector2 start) {
			Point currentFurthest = furthest;
			Point pointStart = Point.Create(start);
			// Find last point in path that has line of sight
			currentPath.BinarySearch(null, new LambdaComparer<Point> ((_, point) => {
				if (!CanMoveDirectlyBetween (pointStart, point)) {
					return -1;
				} else {
					currentFurthest = point;
					return 1;
				}
			}));

			// Get index of last point in path that has line of sight
			int index = currentPath.BinarySearch (currentFurthest,  new LambdaComparer<Point> ((p, point) => {
				if (!CanMoveDirectlyBetween (pointStart, point)) {
					return -1;
				} else {
					if (p.Equals (point)) {
						return 0;
					}
					return 1;
				}
			}));
			return index != -1 ? index : 0;
		}
	}
}
