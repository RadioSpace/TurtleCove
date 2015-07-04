using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNX.VectorGraphics
{
    
    [Serializable]    
    public struct GLine : IEquatable<GLine>
    {
        double sx;
        double ex;

    

        double sa;
        double ea;

        /// <summary>
        /// a line for graphic vector stuff
        /// </summary>
        /// <param name="_sx">the starting x positino</param>
        /// <param name="_ex">the end x position</param>
        /// <param name="_sy">the starting y position</param>
        /// <param name="_ey">the ending y position</param>
        /// <param name="_sa">the the starting amount</param>
        /// <param name="_ea">the ending amount</param>        
        public GLine(double _sx, double _ex,  double _sa, double _ea)
        {
            sx = _sx;
            ex = _ex;    
            sa = _sa;
            ea = _ea;
        }


        public double getAmount(double u)
        {
            return ((ea - sa) * u) + sa;
        }

        public double getPosition(double u)
        {
            return ((ex - sx) * u) + sx;
        }
        
        public bool Equals(GLine other)
        {
            return other.sx == sx && other.ex == ex && other.sa == sa;
        }

        public override bool Equals(object obj)
        {
            if (obj is GLine)
            {
                return Equals((GLine)obj);
            }
            else return false;
        }

        public static bool operator==(GLine a, GLine b)
        {
            return a.Equals(b);
        }
        public static bool operator !=(GLine a, GLine b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash + 31 * sx.GetHashCode();
            hash = hash + 31 * ex.GetHashCode();
            return hash + 31 * sa.GetHashCode();            
        }
    }
}
