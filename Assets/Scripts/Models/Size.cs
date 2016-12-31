using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Completed {

    public class Size {
        public int width;
        public int height;

        public Size(int w, int h) {
            this.width = w;
            this.height = h;
        }

        public int Area {
            get { return this.width * this.height; }
        }
    }
}
