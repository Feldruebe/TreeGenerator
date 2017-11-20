using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeGeneratorLib.Wrappers
{
    public interface IColor
    {
        byte A { get; set; }

        byte R { get; set; }

        byte G { get; set; }

        byte B { get; set; }

        void ToHsv(out float h, out float s, out float v);
    }
}
