using UnityEngine;
using System.Collections.Generic;
using System;

namespace Completed {
    public class Cell : MonoBehaviour {

        private Renderer rend;
        private Vector3 pos;
        private static Dictionary<Point, Cell> CellLookup = new Dictionary<Point, Cell>();
        private bool pathFound = false;
        private static List<Point> currentHighlightedPath = null;
		private bool underPlayer = false;

        // Use this for initialization
        void Start() {
            rend = GetComponent<Renderer>();
            pos = GetComponent<Transform>().position;
            CellLookup.Add(Point.Create(pos), this);
        }

        // Update is called once per frame
        void Update() {
            
        }

        void OnMouseEnter() {
			if (!Loader.HasApplicationFocus || Loader.CameraMoving) return;
            Show();
        }

        void OnMouseExit() {
			if (!Loader.HasApplicationFocus) return;
            Hide();
            pathFound = false;
        }

        void OnMouseOver() {
			if (!Loader.HasApplicationFocus) return;
			//if (Player.IsMoving || Loader.CameraMoving) return;
            if (Input.GetMouseButtonDown(1)) {
                HideCurrentPath();
            } else if (Input.GetMouseButton(0)) {
                if (!pathFound) {
                    HideCurrentPath();
                    //currentHighlightedPath = Player.FindPathFor(pos);
                    //Player.HighlightPath(currentHighlightedPath);
                    pathFound = true;
                }
			} else if (pathFound) {
                pathFound = false;
                HideCurrentPath();
                Show();
            }
        }

        void Show() {
            if (!rend.enabled && pos != Player.position) rend.enabled = true;
        }

        void Hide() {
            rend.enabled = false;
        }

		void ToggleUnderPlayer() {
			this.underPlayer = !this.underPlayer;
		}

        public static void HighlightPath(List<Point> path) {
            foreach (Point point in path) {
                Cell value;
                if (CellLookup.TryGetValue(point, out value)) {
                    value.Show();
                }
            }
        }

        public static void HideCellAt(Point point) {
			TellCell (point, "Hide");
        }


		public static void EnteredCell (Point p) {
			TellCell (p, "ToggleUnderPlayer");
		}

		public static void ExitedCell (Point p) {
			TellCell (p, "ToggleUnderPlayer");
		}

		private static void TellCell(Point point, String method) {
			Cell cell = GetCellAt (point);
			if (cell != null) {
				cell.Invoke (method, 0);
			}
		}

		private static Cell GetCellAt(Point point) {
			Cell value;
			if (CellLookup.TryGetValue(point, out value)) {
				return value;
			}
			return null;
		}

        public static void HideCurrentPath() {
            if (currentHighlightedPath != null) {
                foreach (Point point in currentHighlightedPath) {
                    HideCellAt(point);
                }
                currentHighlightedPath = null;
            }
        }
    }
}
