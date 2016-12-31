using System.Collections.Generic;
using System.Linq;

namespace Completed {
    class PriorityQueue<P, V> {
        private SortedDictionary<float, Queue<V>> heap = new SortedDictionary<float, Queue<V>>();

        public void Enqueue(float priority, V value) {
            Queue<V> q;
            if (!heap.TryGetValue(priority, out q)) {
                q = new Queue<V>();
                heap.Add(priority, q);
            }
            q.Enqueue(value);
        }

        public V Dequeue() {
            KeyValuePair<float, Queue<V>> min = heap.First();
            V v = min.Value.Dequeue();
            if (min.Value.Count == 0)
                heap.Remove(min.Key);
            return v;
        }

        public bool IsEmpty {
            get { return !heap.Any(); }
        }
    }
}
