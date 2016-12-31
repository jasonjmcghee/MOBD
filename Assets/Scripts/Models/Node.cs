using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Completed {
    class Node {
        public Point point;
        public Node parent;
        float cost;

        public float Cost {
            get { return this.cost; }
        }

        public Node(Point p) : this(p, null) {
            this.point = p;
            this.parent = null;
        }

        public Node(Point p, Node prnt) {
            this.point = p;
            this.parent = prnt;
            this.cost = prnt == null ? 1 : prnt.cost + 1;
        }

        public Node Clone() {
            if (this.parent == null) {
                return new Node(this.point.Clone(), null);
            }
            return new Node(this.point.Clone(), this.parent.Clone());
        }

        public List<Point> GetPointsToNode() {
            Stack<Point> stack = new Stack<Point>();
            Node n = Clone();
            for (; n.parent != null; n = n.parent) {
                stack.Push(n.point);
            }
            stack.Push(n.point);
            return stack.ToList<Point>();
        }

		// Hash resolves to the point value
		public override string ToString() {
			return this.point.x + "," + this.point.y;
		}

		public override int GetHashCode() {
			return ToString().GetHashCode();
		}

		public override bool Equals(object obj) {
			return obj is Node && ((Node)obj).point.Equals(point);
		}
    }
}
