using System;

namespace Completed {
	public class PointPair {

		Point one;
		Point two;

		public PointPair (Point a, Point b) {
			one = a;
			two = b;
		}

		// Paths are equal in both directions between points
		public override bool Equals (object obj) {
			PointPair pp = (PointPair)obj;
			return (pp.one.Equals (this.one) && pp.two.Equals (this.two))
				|| (pp.one.Equals (this.two) && pp.two.Equals (this.one));
		}

		public override int GetHashCode () {
			return one.GetHashCode () ^ two.GetHashCode ();
		}
	}
}

