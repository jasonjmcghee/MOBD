using UnityEngine;
using System.Collections.Generic;

namespace Completed {
    public class Room {

        public int x;
        public int y;
        public int width;
        public int height;
        public Point entrance;
		public Point entryWay;

        public Room(Point origin, Size size) {
            this.x = origin.x;
            this.y = origin.y;
            this.width = size.width;
            this.height = size.height;
        }

        public static Room Create(SizeRange sizeRange, Point boundsOrigin, Size bounds, List<Room> rooms, Point maskOrigin) {

            Room room = null;
            int count = 0;
            int maxCount = 100;
            for (bool success = false; success != true && count < maxCount; success = room.CanPlaceRoom(rooms)) {
                room = GenerateRoom(sizeRange, boundsOrigin, bounds, maskOrigin);
                count++;
            }

            if (count == maxCount) {
				return null;
            }

			List<Point> possibleEntrances = new List<Point>();
			for (int x = room.x; x < room.x + room.width; x++) {
				for (int y = room.y; y < room.y + room.height; y++) {
					if (x == room.x || y == room.y || x == room.x + room.width - 1 || y == room.y + room.height - 1) {
						if (!room.IsCorner(x, y)) {
							possibleEntrances.Add(new Point(x, y));
						}
					}
				}
			}

			room.entrance = possibleEntrances[Random.Range(0, possibleEntrances.Count)].Clone();
			room.entryWay = room.FindEntryWay ();
			possibleEntrances = null;

            return room;

        }

        public static Room GenerateRoom(SizeRange sizeRange, Point boundsOrigin, Size bounds, Point maskOrigin) {
            int width = sizeRange.width.Random;
            int height = sizeRange.height.Random;

            int x, y;
            do {
                x = Random.Range(boundsOrigin.x + width / 2, bounds.width - width - 1);
                y = Random.Range(boundsOrigin.y + height / 2, bounds.height - height - 1);
            } while (x + width >= maskOrigin.x && y + height >= maskOrigin.y);

            return new Room(new Point(x, y), new Size(width, height));
        }

        public bool Overlaps(Room room) {
            if (this.x > room.x + room.width || room.x > this.x + this.width) {
                return false;
            } else if (this.y > room.y + room.height || room.y > this.y + this.height) {
                return false;
            }
            return true;
        }

		public bool PointInRoom(Point p) {
			return p.x > this.x && p.x < this.x + this.width - 1 && p.y > this.y && p.y < this.y + this.height - 1;
		}

        public bool CanPlaceRoom(List<Room> rooms) {
            foreach (Room room in rooms) {
                if (Overlaps(room)) {
                    return false;
                }
            }
            return true;
        }

        public bool IsCorner(int i, int j) {
            Point bottomLeft = new Point(this.x, this.y);
            Point bottomRight = new Point(this.x + this.width - 1, this.y);
            Point topLeft = new Point(this.x, this.y + this.height - 1);
            Point topRight = new Point(this.x + this.width - 1, this.y + this.height - 1);
            return (new Point(i, j)).EqualsAny(bottomLeft, bottomRight, topLeft, topRight);
        }

		public Point FindEntryWay() {
			if (this.entrance.x == this.x) {
				return new Point (this.entrance.x + 1, this.entrance.y);
			} else if (this.entrance.x == this.x + this.width - 1) {
				return new Point (this.entrance.x - 1, this.entrance.y);
			} else if (this.entrance.y == this.y) {
				return new Point (this.entrance.x, this.entrance.y + 1);
			} else {
				return new Point (this.entrance.x, this.entrance.y - 1);
			}
		}

		public override bool Equals (object obj) {
			return ((Room)obj).x == this.x && ((Room)obj).y == this.y;
		}

		public override int GetHashCode() {
			return ToString().GetHashCode();
		}

		public override string ToString ()
		{
			return string.Format (this.x + "," + this.y + "-" + this.width + "x" + this.height);
		}
    }
}
