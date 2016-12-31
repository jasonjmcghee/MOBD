using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Completed {
	
	public class BoardManager : MonoBehaviour {

        public enum TileType {
            Wall, Floor, RoomWall, RoomFloor
        }

		public enum GameMode {
			Single, Multi, Test
		}

		public GameMode mode;

		public static int width = 150;
		public static int height = 150;
        public static Size MapSize = new Size(width, height);
		public Size map = BoardManager.MapSize;
        private SizeRange roomSize = new SizeRange(10, 25);
		public GameObject player, boardHolder, cellTile, unexploredTile;
		public GameObject dungeonFloor;
        public GameObject[] floorTiles, wallTiles, outerWallTiles;
        private Dictionary<Point, int> dungeonTileLookup = new Dictionary<Point, int>();
		private LayerMask unitMovement;			// Layer which does not block unit movement

		// Preprocess Step
		private List<Point> nonRoomFloorTileList = new List<Point>();
		private Dictionary<PointPair, List<Point>> manhattanPathLookup = new Dictionary<PointPair, List<Point>> ();
		private HashSet<PointPair> hasLineOfSight = new HashSet<PointPair> ();

        private TileType[][] tiles;
        private List<Room> rooms;

        private static BoardManager instance;

        private bool mapGenerated = false;

        private void Start() {
            if (!mapGenerated) {

				BoardManager.instance = this;
                boardHolder = new GameObject("BoardHolder");

                GenerateStage();

                CreateTileGameObjects();
                CreateOuterWallGameObjects();
                mapGenerated = true;

				unitMovement =  ~((1 << LayerMask.NameToLayer ("Floor")) | (1 << LayerMask.NameToLayer("Units")));
				//PreprocessLineOfSight ();
				//PreprocessAllManhattanPaths ();
            }
        }

        // All tiles are walls upon instantiation
        private void InitializeTiles() {
            this.tiles = GenerateNewTiles();
        }

        private TileType[][] GenerateNewTiles() {
            TileType[][] newTiles = new TileType[this.map.width][];
            for (int i = 0; i < newTiles.Length; i++) {
                newTiles[i] = new TileType[this.map.height];
            }
            return newTiles;
        }

		private void GenerateStage() {

			if (mode == GameMode.Test) {
				MapSize = new Size (50, 50);
				InitializeTiles ();
				for (int x = 0; x < tiles.Length; x++) {
					for (int y = 0; y < tiles [x].Length; y++) {
						SetFloorTile (x, y);
					}
				}
				Vector3 startingPosition = new Vector3 (this.map.width / 2, this.map.height / 2);
				Instantiate (this.player, startingPosition, Quaternion.identity);
				Loader.SetStartingPosition (startingPosition);
			} else {
				InitializeTiles ();
				if (mode == GameMode.Multi) {
					Point bottomLeft = GenerateCenterArena ();
					Size mapSize = new Size (this.map.width / 2, this.map.height / 2);
					this.rooms = SetRoomTiles (GenerateRooms (bottomLeft, mapSize), bottomLeft);
					GenerateMirrors ();
				} else {
					InitializeSinglePlayerGame ();
				}
			}

			dungeonFloor.transform.localScale = new Vector3 (this.map.width, this.map.height, 1);
			GameObject tileInstance = Instantiate(dungeonFloor, new Vector2(this.map.width / 2, this.map.height / 2), Quaternion.identity) as GameObject;
			tileInstance.transform.parent = boardHolder.transform;
        }

		private void InitializeSinglePlayerGame() {
			int percentChanceFloor = 30;
			for (int x = 0; x < tiles.Length; x++) {
				for (int y = 0; y < tiles[x].Length; y++) {
					if (Random.Range (0, 100) > (100 - percentChanceFloor)) {
						SetFloorTile (x, y);
					}
				}
			}
			List<Room> createdRooms = GenerateRooms (new Point (this.map.width - 1, this.map.height - 1), this.map);
			int index = Random.Range (0, createdRooms.Count);
			Point startPoint = createdRooms[index].entrance;
			this.rooms = SetRoomTiles(createdRooms, startPoint);
			GenerateCellularAutomata (4, 3, 10);
			this.rooms = SetRoomTiles(createdRooms, startPoint);

			Room startRoom = PlacePlayerInRandomRoom();
			Treasure.Generate (this.rooms, startRoom);
		}

        private Point GenerateCenterArena() {
            int r = Mathf.Min(this.map.width, this.map.height) / 16;
            Point center = new Point(this.map.width / 2, this.map.height / 2);
            int dx, dy;
            Point minPoint = new Point(center.x, center.y);
            for (int x = center.x - r + 1; x < center.x + r; x++) {
                for (int y = center.y - r + 1; y < center.y + r; y++) {
                    dx = center.x - x;
                    dy = center.y - y;
                    if ((dx * dx + dy * dy) <= r * r) {
                        SetFloorTile(x, y);
                        if (Mathf.Abs(x - y) < 2) {
                            if (minPoint.x > x) minPoint.x = x;
                            if (minPoint.y > y) minPoint.y = y;
                        }
                    }
                }
            }

            return minPoint;
        }

		private void GenerateCellularAutomata(int birthLimit, int deathLimit, int iterations) {

			for (int i = 0; i < iterations; i++) {
				for (int x = 0; x < tiles.Length; x++) {
					for (int y = 0; y < tiles [x].Length; y++) {
						int neighborsAlive = GetAdjacentFloor (new Point (x, y)).Count;

						if (IsFloorTile (x, y) && GetRoom(new Point(x, y)) == null) {
							if (neighborsAlive < deathLimit) {
								SetWallTile (x, y);
							} else {
								SetFloorTile (x, y);
							}
						} else if (IsWallTile (x, y)) {
							if (neighborsAlive > birthLimit) {
								SetFloorTile (x, y);
							} else {
								SetWallTile (x, y);
							}
						}
					}
				}
			}
		}
			

		private List<Room> GenerateRooms(Point maskOrigin, Size mapSize) {
            List<Room> createdRooms = new List<Room>();
            Point origin = new Point(0, 0);
			Func<Room> createRoom = () => Room.Create (roomSize, origin, mapSize, createdRooms, maskOrigin); 
			for (Room room = createRoom (); room != null; room = createRoom ()) {
				createdRooms.Add (room);
			}
				
			return createdRooms;
        }

        private List<Room> SetRoomTiles(List<Room> roomList, Point startPoint) {
            List<Point> walls = new List<Point>();
			float minDistance = startPoint.ManhattanDistance(new Point(0, 0));
            Point nearestEntrance = null;
            HashSet<Point> entrances = new HashSet<Point>();

            foreach (Room room in roomList) {
                walls = new List<Point>();
                for (int x = room.x; x < room.x + room.width; x++) {
                    for (int y = room.y; y < room.y + room.height; y++) {
                        if (x == room.x || y == room.y || x == room.x + room.width - 1 || y == room.y + room.height - 1) {
							if (room.entrance.Equals(new Point(x, y))) {
								SetFloorTile(x, y);
                            } else {
                                SetRoomWallTile(x, y);
                            }
                        } else {
                            SetRoomFloorTile(x, y);
                        }
                    }
                }

                foreach (Point wall in walls) {
                    SetRoomWallTile(wall.x, wall.y);
                }

				float dist = startPoint.ManhattanDistance(room.entrance);
                if (dist < minDistance) {
                    minDistance = dist;
                    nearestEntrance = room.entrance;
                }
            }

			List<Point> newPath = FastestRoute (startPoint, nearestEntrance);
			foreach (Point step in newPath) {
				SetFloorTile (step);
			}

            foreach (Room room in roomList) {
                int index = Random.Range(0, entrances.Count);
				newPath = FastestRoute(room.entrance, newPath[index]);
                foreach (Point step in newPath) {
                    SetFloorTile(step);
                }
				SetFloorTile (room.entryWay);
            }
            return roomList;
        }

		private List<Point> FastestRoute (Point start, Point target) {
			return start.FindPathAStar (target, GetNeighbors, Point.PointManhattanDistance);
		}

		private List<Point> DrunkenWalk (Point start, Point target, float threshold = 15) {
			float maxDistance = target.ManhattanDistance (start);
			List<Point> path = new List<Point> ();
			List<Point> neighbors;
			path.Add (start);

			float distance;
			HashSet<Point> seen = new HashSet<Point> ();
			for  (Point current = start; current != target; maxDistance = current.ManhattanDistance (target)) {
				neighbors = new List<Point> ();
				foreach (Point neighbor in GetNeighbors (current)) {
					distance = neighbor.ManhattanDistance (target);
					if (distance <= maxDistance || !seen.Contains(neighbor) && distance <= maxDistance + threshold) {
						neighbors.Add (neighbor);
					}
				}
				if (neighbors.Count < 1) {
					path.AddRange(current.FindPathAStar (target, GetNeighbors, Point.PointManhattanDistance));
					return path;
				}
				current = neighbors[Random.Range(0, neighbors.Count)];
				seen.Add (current);
			}
			return path;
		}

        private List<Point> GetNeighbors(Point point) {
            List<Point> neighbors = new List<Point>();
            if (IsWallOrFloor(point.x - 1, point.y)) neighbors.Add(new Point(point.x - 1, point.y));
            if (IsWallOrFloor(point.x + 1, point.y)) neighbors.Add(new Point(point.x + 1, point.y));
            if (IsWallOrFloor(point.x, point.y - 1)) neighbors.Add(new Point(point.x, point.y - 1));
            if (IsWallOrFloor(point.x, point.y + 1)) neighbors.Add(new Point(point.x, point.y + 1));

            return neighbors;
        }

        public static List<Point> GetAdjacentFloor(Point point) {
            List<Point> neighbors = new List<Point>();
            if (instance.IsAnyFloorTile(point.x - 1, point.y)) neighbors.Add(new Point(point.x - 1, point.y));
            if (instance.IsAnyFloorTile(point.x + 1, point.y)) neighbors.Add(new Point(point.x + 1, point.y));
            if (instance.IsAnyFloorTile(point.x, point.y - 1)) neighbors.Add(new Point(point.x, point.y - 1));
            if (instance.IsAnyFloorTile(point.x, point.y + 1)) neighbors.Add(new Point(point.x, point.y + 1));
			if (instance.IsAnyFloorTile(point.x - 1, point.y - 1)) neighbors.Add(new Point(point.x - 1, point.y - 1));
			if (instance.IsAnyFloorTile(point.x + 1, point.y + 1)) neighbors.Add(new Point(point.x + 1, point.y + 1));
			if (instance.IsAnyFloorTile(point.x + 1, point.y - 1)) neighbors.Add(new Point(point.x + 1, point.y - 1));
			if (instance.IsAnyFloorTile(point.x - 1, point.y + 1)) neighbors.Add(new Point(point.x - 1, point.y + 1));

            return neighbors;
        }

        private void GenerateMirrors() {
            for (int j = 0; j < this.tiles.Length / 2; j++) {
                this.tiles[this.tiles.Length - 1 - j] = (TileType[])this.tiles[j].Clone();
            }
            for (int i = 0; i < this.tiles.Length; i++) {
                for (int j = 0; j < this.tiles[i].Length / 2; j++) {
                    this.tiles[i][this.tiles[i].Length - 1 - j] = this.tiles[i][j];
                }
            }
        }

        private void CreateTileGameObjects() {
			int index;
			int wallIndex;
			Point point;
			for (int i = 0; i < this.tiles.Length; i++) {
				for (int j = 0; j < this.tiles[i].Length; j++) {
					point = new Point (i, j);
					index = DecideAndRecordFloorIndex3x3 (point);
					if (IsFloorTile (i, j) || IsRoomFloorTile (i, j)) {
						//InstantiateWithIndex (this.floorTiles, new Vector3 (i, j), index);
						if (IsFloorTile (i, j)) {
							nonRoomFloorTileList.Add (point);
						}
						//InstantiateTile (this.cellTile, new Vector3 (i, j));
						//InstantiateTile (this.unexploredTile, new Vector3 (i, j));
					} else if (IsWallTile (i, j) || IsRoomWallTile (i, j)) {
						wallIndex = ChooseWallTile (i, j);
						if (wallIndex != 4) {
							//InstantiateWithIndex (this.floorTiles, new Vector3 (i, j), index);
						}
						InstantiateWithIndex (this.wallTiles, new Vector3 (i, j), 4);
					}
				}
			}
        }

		private int ChooseWallTile(int i, int j) {

			bool hasLeft = IsAnyFloorTile (i - 1, j);
			bool hasRight = IsAnyFloorTile (i + 1, j);
			bool hasAbove = IsAnyFloorTile (i, j + 1);
			bool hasBelow  = IsAnyFloorTile (i, j - 1);

			bool isTopLeft = hasLeft && !hasRight && hasAbove && !hasBelow;
			bool isTop = !hasLeft && !hasRight && hasAbove && !hasBelow;
			bool isTopRight = !hasLeft && hasRight && hasAbove && !hasBelow;
			bool isLeft = hasLeft && !hasRight && !hasAbove && !hasBelow;

			bool isRight = !hasLeft && hasRight && !hasAbove && !hasBelow;
			bool isBottomLeft = hasLeft && !hasRight && !hasAbove && hasBelow;
			bool isBottom = !hasLeft && !hasRight && !hasAbove && hasBelow;
			bool isBottomRight = !hasLeft && hasRight && !hasAbove && hasBelow;

			bool isCenterOfFloor = hasLeft && hasRight && hasAbove && hasBelow;
			bool isMiddleOfFloorHorizontal = !hasLeft && !hasRight && hasAbove && hasBelow;
			bool isMiddleOfFloorVertical = hasLeft && hasRight && !hasAbove && !hasBelow;
			bool isTopLeftSingle = isTopLeft && IsAnyFloorTile (i + 1, j - 1);
			bool isTopRightSingle = isTopRight && IsAnyFloorTile (i - 1, j - 1);
			bool isBottomLeftSingle = isBottomLeft && IsAnyFloorTile (i + 1, j + 1);
			bool isBottomRightSingle = isBottomRight && IsAnyFloorTile (i - 1, j + 1);

			if (isCenterOfFloor) {
				return 13;
			} else if (isTopLeftSingle) {
				return 9;
			} else if (isTopRightSingle) {
				return 11;
			} else if (isBottomLeftSingle) {
				return 15;
			} else if (isBottomRightSingle) {
				return 17;
			} else if (isMiddleOfFloorHorizontal) {
				return 10;
			} else if (isMiddleOfFloorVertical) {
				return 12;
			} else if (isTopLeft) {
				return 0;
			} else if (isTopRight) {
				return 2;
			} else if (isBottomLeft) {
				return 6;
			} else if (isBottomRight) {
				return 8;
			} else if (isTop) {
				return 1;
			} else if (isLeft) {
				return 3;
			} else if (isRight) {
				return 5;
			} else if (isBottom) {
				return 7;
			} else {
				return 4;
			}
		}

		private int DecideAndRecordFloorIndex3x3(Point p) {
			int index = 0;
			int temp;
			if (this.dungeonTileLookup.TryGetValue(new Point(p.x + 1, p.y), out temp)) {
				index = 3 * (temp / 3) + ((temp + 2) % 3);
			} else if ( this.dungeonTileLookup.TryGetValue(new Point(p.x - 1, p.y), out temp)) {
				index = 3 * (temp / 3) + ((temp + 1) % 3);
			} else if (this.dungeonTileLookup.TryGetValue(new Point(p.x, p.y + 1), out temp)) {
				index = (temp + 3) % 9;
			} else if (this.dungeonTileLookup.TryGetValue(new Point(p.x, p.y - 1), out temp)) {
				index = (temp + 6) % 9;
			}
			this.dungeonTileLookup.Add(p, index);
			return index;
		}

        private void CreateOuterWallGameObjects() {
            Rect border = new Rect(-1f, map.width, map.height, -1f);
            CreateVerticalOuterWalls(border.left, border.bottom, border.top);
            CreateVerticalOuterWalls(border.right, border.bottom, border.top);

            CreateHorizontalOuterWalls(border.left + 1f, border.right - 1f, border.bottom);
            CreateHorizontalOuterWalls(border.left + 1f, border.right - 1f, border.top);
        }

        private void CreateVerticalOuterWalls (float x, float start, float end) {
            for (float y = start; y <= end; y++) InstantiateTiles(this.outerWallTiles, new Vector3(x, y));
        }

        private void CreateHorizontalOuterWalls(float start, float end, float y) {
            for (float x = start; x <= end; x++) InstantiateTiles(this.outerWallTiles, new Vector3(x, y));
        }

		private void PreprocessAllManhattanPaths() {
			manhattanPathLookup = Point.AllPairsBFS (nonRoomFloorTileList, point => {
				List<Point> neighbors = new List<Point>();
				if (BoardManager.IsFloor(point.x - 1, point.y)) neighbors.Add(new Point(point.x - 1, point.y));
				if (BoardManager.IsFloor(point.x + 1, point.y)) neighbors.Add(new Point(point.x + 1, point.y));
				if (BoardManager.IsFloor(point.x, point.y - 1)) neighbors.Add(new Point(point.x, point.y - 1));
				if (BoardManager.IsFloor(point.x, point.y + 1)) neighbors.Add(new Point(point.x, point.y + 1));
				return neighbors;
			});
			Debug.Log (manhattanPathLookup);
		}

		private bool LineOfSightBetweenVectors(Vector2 start, Vector2 target) {
			int targetX = Mathf.RoundToInt (target.x);
			int targetY = Mathf.RoundToInt(target.y);
			Vector2 delta = target - start;
			float currentX = start.x + delta.normalized.x;
			float currentY = start.y + delta.normalized.y;
			int roundedX = Mathf.RoundToInt(currentX);
			int roundedY = Mathf.RoundToInt(currentY);

			while (roundedX != targetX || roundedY != targetY) {
				if (!IsAnyFloorTile(roundedX, roundedY)) {
					return false;
				}

				currentX += delta.normalized.x;
				currentY += delta.normalized.y;
				roundedX = Mathf.RoundToInt(currentX);
				roundedY = Mathf.RoundToInt(currentY);
			}
			return IsAnyFloorTile (roundedX, roundedY);
		}

		private void PreprocessLineOfSight() {
			List<Vector2> vectors = new List<Vector2> ();
			foreach (Point point in nonRoomFloorTileList) {
				vectors.Add (point.AsVector());
			}
			for (int i = 0; i < nonRoomFloorTileList.Count; i++) {
				hasLineOfSight.Add (new PointPair (nonRoomFloorTileList[i], nonRoomFloorTileList[i]));
				for (int j = i + 1; j < nonRoomFloorTileList.Count - 1; j++) {
					if (LineOfSightBetweenVectors(vectors[i], vectors[j])) {
						hasLineOfSight.Add (new PointPair (nonRoomFloorTileList[i], nonRoomFloorTileList[j]));
					}
				}
			}
		}

        private void InstantiateTiles(GameObject[] prefabs, Vector3 position) {
            GameObject randomPrefab = prefabs[Random.Range(0, prefabs.Length)];
            GameObject tileInstance = Instantiate(randomPrefab, position, Quaternion.identity) as GameObject;
            tileInstance.transform.parent = boardHolder.transform;
        }

        private void InstantiateWithIndex(GameObject[] floorPrefabs, Vector3 position, int index) {
            GameObject tileInstance = Instantiate(floorPrefabs[index], position, Quaternion.identity) as GameObject;
            tileInstance.transform.parent = boardHolder.transform;
        }

        private void InstantiateTile(GameObject prefab, Vector3 position) {
            GameObject[] prefabs = { prefab };
            InstantiateTiles(prefabs, position);
        }

        private void SetWallTile(int x, int y) {
            this.tiles[Mathf.Clamp(x, 0, this.map.width - 1)][Mathf.Clamp(y, 0, this.map.height - 1)] = TileType.Wall;
        }

        private void SetFloorTile(int x, int y) {
            this.tiles[Mathf.Clamp(x, 0, this.map.width - 1)][Mathf.Clamp(y, 0, this.map.height - 1)] = TileType.Floor;
        }

        private void SetRoomWallTile(int x, int y) {
            this.tiles[Mathf.Clamp(x, 0, this.map.width - 1)][Mathf.Clamp(y, 0, this.map.height - 1)] = TileType.RoomWall;
        }

        private void SetRoomFloorTile(int x, int y) {
            this.tiles[Mathf.Clamp(x, 0, this.map.width - 1)][Mathf.Clamp(y, 0, this.map.height - 1)] = TileType.RoomFloor;
        }

        private void SetFloorTile(Point p) {
            SetFloorTile(p.x, p.y);
        }

        private bool InBounds(int x, int y) {
            return x >= 0 && y >= 0 && x < this.map.width && y < this.map.height;
        }

        private bool IsFloorTile(int x, int y) {
            return this.tiles[x][y] == TileType.Floor;
        }

		private bool IsWallTile(int x, int y) {
			return this.tiles[x][y] == TileType.Wall;
		}

		private bool IsRoomWallTile(int x, int y) {
			return this.tiles[x][y] == TileType.RoomWall;
		}

        private bool IsRoomFloorTile(int x, int y) {
            return this.tiles[x][y] == TileType.RoomFloor;
        }

        private bool IsWallOrFloor(int x, int y) {
            return InBounds(x, y) && (IsWallTile(x, y) || IsFloorTile(x, y));
        }

        private bool IsAnyFloorTile(int x, int y) {
            return InBounds(x, y) && (IsRoomFloorTile(x, y) || IsFloorTile(x, y));
        }

		private bool IsAnyWallTile(int x, int y) {
			return InBounds(x, y) && (IsRoomWallTile(x, y) || IsWallTile(x, y));
		}

		public static bool IsFloor(int x, int y) {
			return BoardManager.instance.IsAnyFloorTile(x, y);
		}

		public static bool IsInRoom(int x, int y) {
			return BoardManager.instance.IsRoomFloorTile(x, y);
		}

		public static Room GetRoom(Point p) {
			foreach (Room room in BoardManager.instance.rooms) {
				if (room.PointInRoom (p)) {
					return room;
				}
			}
			return null;
		}

		public static bool IsWall(int x, int y) {
			return BoardManager.instance.IsAnyWallTile(x, y);
		}

        private Room PlacePlayerInRandomRoom () {
            Room randomRoom = this.rooms[Random.Range(0, this.rooms.Count - 1)];
            int x = Random.Range(randomRoom.x + 1, randomRoom.x + randomRoom.width - 1);
            int y = Random.Range(randomRoom.y + 1, randomRoom.y + randomRoom.height - 1);
            Instantiate(this.player, new Vector3(x, y), Quaternion.identity);
            Loader.SetStartingPosition(new Vector3(x, y));
			return randomRoom;
        }

		public static List<Point> GetManhattanPath (Point start, Point target) {
			List<Point> path;
			if (BoardManager.instance.manhattanPathLookup.TryGetValue (new PointPair (start, target), out path)) {
				return path;
			}
			return new List<Point>();
		}

		public static bool HasLineOfSight (Point start, Point target) {
			return BoardManager.instance.hasLineOfSight.Contains (new PointPair (start, target));
		}
    }
}
