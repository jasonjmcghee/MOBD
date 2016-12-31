using System.Collections.Generic;
using UnityEngine;

namespace Completed
{
	public class Unexplored : MonoBehaviour {
		private Renderer rend;
		private Vector3 pos;
		private static Dictionary<Point, Unexplored> UnexploredLookup = new Dictionary<Point, Unexplored>();

		void Start() {
			rend = GetComponent<Renderer>();
			pos = GetComponent<Transform>().position;
			UnexploredLookup.Add(Point.Create(pos), this);
			rend.enabled = false;
		}

		void Reveal() {
			rend.enabled = false;
		}

		public static void Reveal(Point point) {
			Unexplored value;
			if (UnexploredLookup.TryGetValue(point, out value)) {
				value.Reveal();
			}
		}
	}
}