using UnityEngine;
using System.Collections.Generic;

namespace Completed
{
	public class Treasure : MonoBehaviour {
		public static void Generate(List<Room> rooms, Room start) {
			foreach (Room room in rooms) {
				if (room.Equals (start)) {
					GenerateForStartRoom (start);
				} else {
					Generate (room);
				}
			}
		}

		public static void Generate(Room room) {
			// Standard room stuff
		}

		public static void GenerateForStartRoom(Room room) {
			// Start room stuff
		}
	}
}
