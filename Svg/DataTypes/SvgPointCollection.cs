using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Globalization;

namespace Svg
{
    /// <summary>
    /// Represents a list of <see cref="SvgUnits"/> used with the <see cref="SvgPolyline"/> and <see cref="SvgPolygon"/>.
    /// </summary>
    //[TypeConverter(typeof(SvgPointCollectionConverter))]
    public class SvgPointCollection : List<SvgUnit>
    {
        public override string ToString()
        {
            string ret = "";
            foreach (var unit in this)
            {
                ret += unit.ToString() + " ";
            }

            return ret;
        }
    }
}
