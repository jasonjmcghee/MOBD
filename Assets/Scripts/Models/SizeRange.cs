using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Completed {

    public class SizeRange {
        public IntRange width;
        public IntRange height;

        public SizeRange(int min, int max) {
            this.width = new IntRange(min, max);
            this.height = new IntRange(min, max);
        }

        public SizeRange(IntRange w, IntRange h) {
            this.width = w;
            this.height = h;
        }
    }
}
