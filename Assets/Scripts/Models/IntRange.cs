using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Completed {

    // Using Serializable allows us to embed a class with sub properties in the inspector.
    [Serializable]
    public class IntRange {
        public int minimum;
        public int maximum;

        public IntRange(int min, int max) {
            this.minimum = min;
            this.maximum = max;
        }

        public int Random {
            get { return UnityEngine.Random.Range(this.minimum, this.maximum); }
        }
    }
}
