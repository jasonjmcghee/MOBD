using System.Collections.Generic;
using UnityEngine;

namespace Completed {
    public class Point {
        public int x;
        public int y;

        public Point() { }

        public Point(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj) {
            return obj is Point && Equals((Point)obj);
        }

        public bool Equals(Point p) {
            return this.x == p.x && this.y == p.y;
        }

        public bool EqualsAny(params Point[] ps) {
            foreach (Point p in ps) {
                if (Equals(p)) {
                    return true;
                }
            }
            return false;
        }

        public Point Clone() {
            return new Point(this.x, this.y);
        }

        public override string ToString() {
            return this.x + "," + this.y;
        }

        public override int GetHashCode() {
            return ToString().GetHashCode();
        }

        public float ManhattanDistance(Point p) {
            return Mathf.Abs(p.x - this.x) + Mathf.Abs(p.y - this.y);
        }

		public List<Point> FindPathToPoint(Point entrance, System.Func<Point, List<Point>> PossibleSteps, System.Func<Point, Point, float> heuristic) {
            //System.Func<Node, bool> firstFloor = node => IsFloorTile(node.point) && !node.point.Equals(start);
            //System.Func<Node, bool> entranceFound = node => node.point.Equals(entrance);

			return FindPathAStar(entrance, PossibleSteps, heuristic);
        }

        private List<Point> FindFloorWithBFS(System.Func<Node, bool> IsStopNode, System.Func<Point, List<Point>> PossibleSteps) {
            HashSet<Point> visited = new HashSet<Point>();
            Queue<Node> queue = new Queue<Node>();
            Node current = new Node(this);
            queue.Enqueue(current);
            while (queue.Count != 0) {
                current = queue.Dequeue();
                if (IsStopNode(current)) {
                    queue = null;
                    visited = null;
                    return current.GetPointsToNode();
                }
                visited.Add(current.point);
                foreach (Point point in PossibleSteps(current.point)) {
                    if (!visited.Contains(point)) {
                        queue.Enqueue(new Node(point, current));
                    }
                }
            }

            queue = null;
            visited = null;
            return new List<Point>();
        }

		public void BFS(System.Action<Point> Action, System.Func<Point, List<Point>> PossibleSteps) {
			HashSet<Point> visited = new HashSet<Point>();
			Queue<Node> queue = new Queue<Node>();
			Node current = new Node(this);
			queue.Enqueue(current);
			Action(current.point);
			visited.Add(current.point);
			while (queue.Count != 0) {
				current = queue.Dequeue();
				foreach (Point point in PossibleSteps(current.point)) {
					if (!visited.Contains(point)) {
						queue.Enqueue(new Node(point, current));
						visited.Add(point);
						Action(point);
					}
				}
			}

			queue = null;
			visited = null;
		}

		public List<Point> FindPathAStar(Point target, System.Func<Point, List<Point>> PossibleSteps, System.Func<Point, Point, float> heuristic) {
            HashSet<Point> finished = new HashSet<Point>();
			PriorityQueue<float, Point> queue = new PriorityQueue<float, Point>();
			Dictionary<Point, float> dist = new Dictionary<Point, float> ();
			Dictionary<Point, Point> parent = new Dictionary<Point, Point> ();

			Point current = this;
			dist[current] = 0;
			float cost;
			bool foundScore;

            queue.Enqueue(0f, this);
            while (!queue.IsEmpty) {
                current = queue.Dequeue();
                if (current.Equals(target)) {
					List<Point> path = new List<Point> ();
					if (parent.Count > 0) {
						for (Point p = current; parent.ContainsKey (p); p = parent [p]) {
							path.Insert (0, p);
						}
					}

					path.Insert (0, this);
					return path;
                }

				finished.Add(current);
				List<Point> points = PossibleSteps (current);

				foreach (Point point in points) {
                    if (!finished.Contains(point)) {
						foundScore = dist.ContainsKey (point);
						cost = dist [current] + heuristic(current, point);
						if (!foundScore || cost < dist [point]) {
							dist [point] = cost;
							parent [point] = current;
							queue.Enqueue (cost + heuristic(target, point), point);
						}
					}
                }
            }

            return new List<Point>();
        }

		public static Dictionary<PointPair, List<Point>> AllPairsBFS(List<Point> nonRoomFloors, System.Func<Point, List<Point>> PossibleSteps) {

			PointPair pointPair;
			Dictionary<PointPair, List<Point>> pathLookup = new Dictionary<PointPair, List<Point>> ();
			List<Point> path = new List<Point> ();

			foreach (Point currentStart in nonRoomFloors) {
				HashSet<Point> finished = new HashSet<Point>();
				Queue<Point> queue = new Queue<Point>();
				Dictionary<Point, Point> parent = new Dictionary<Point, Point> ();
				Point current = currentStart;

				queue.Enqueue(current);
				finished.Add (current);
				while (queue.Count > 0) {
					current = queue.Dequeue();

					path = new List<Point> ();
					pointPair = new PointPair (currentStart, current);
					if (!pathLookup.TryGetValue (pointPair, out path)) {
						path = new List<Point> ();
						if (parent.Count > 0) {
							for (Point p = current; parent.ContainsKey (p); p = parent [p]) {
								path.Insert (0, p);
							}
							path.Insert (0, current);
						}
						pathLookup.Add (pointPair, path);
					}

					List<Point> points = PossibleSteps (current);

					foreach (Point point in points) {
						if (!finished.Contains(point)) {
							parent [point] = current;
							queue.Enqueue (point);
						}
						finished.Add(point);
					}
				}
			}
			return pathLookup;
		}

        public static Point Create(Vector3 point) {
            return new Point(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y));
        }

        public Vector3 AsVector() {
            return new Vector3(this.x, this.y);
        }

		private float EuclideanDistance(Point target) {
			return Mathf.Sqrt(Mathf.Pow(this.x - target.x, 2) + Mathf.Pow(this.y - target.y, 2));
		}

		public static float PointManhattanDistance(Point a, Point b) {
			return a.ManhattanDistance(b);
		}

		public static float PointEuclideanDistance(Point a, Point b) {
			return a.EuclideanDistance(b);
		}
    }
}
