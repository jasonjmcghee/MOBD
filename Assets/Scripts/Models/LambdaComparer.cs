using System;
using System.Collections.Generic;

namespace Completed {
	public class LambdaComparer<T> : IComparer<T> {
		private Func<T, T, int> comparer;
		public LambdaComparer(Func<T, T, int> comparer) {
			this.comparer = comparer;
		}

		public static IComparer<T> Create(Func<T, T, int> comparer) {
			return new LambdaComparer<T>(comparer);
		}
		public int Compare(T x, T y) {
			return comparer(x, y);
		}
	}
}

