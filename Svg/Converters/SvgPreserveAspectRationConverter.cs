using System;

using Svg.Converters.Svg;

namespace Svg.Converters
{
    //implementaton for preserve aspect ratio
    public sealed class SvgPreserveAspectRatioConverter : BaseConverter
    {
        public override object ConvertFromString(string value, Type targetType, SvgDocument document)
        {
            if (value == null)
            {
                return new SvgAspectRatio();
            }

            if (!(value is string))
            {
                throw new ArgumentOutOfRangeException("value must be a string.");
            }

            SvgPreserveAspectRatio eAlign = SvgPreserveAspectRatio.none;
            bool bDefer = false;
            bool bSlice = false;

            string[] sParts = (value as string).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int nAlignIndex = 0;
            if (sParts[0].Equals("defer"))
            {
                bDefer = true;
                nAlignIndex++;
                if (sParts.Length < 2)
                    throw new ArgumentOutOfRangeException("value is not a member of SvgPreserveAspectRatio");
            }

#if Net4
            if (!Enum.TryParse<SvgPreserveAspectRatio>(sParts[nAlignIndex], out eAlign))
                throw new ArgumentOutOfRangeException("value is not a member of SvgPreserveAspectRatio");
#else
            eAlign = (SvgPreserveAspectRatio)Enum.Parse(typeof(SvgPreserveAspectRatio), sParts[nAlignIndex]);
#endif

            nAlignIndex++;

            if (sParts.Length > nAlignIndex)
            {
                switch (sParts[nAlignIndex])
                {
                    case "meet":
                        break;
                    case "slice":
                        bSlice = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("value is not a member of SvgPreserveAspectRatio");
                }
            }
            nAlignIndex++;
            if (sParts.Length > nAlignIndex)
                throw new ArgumentOutOfRangeException("value is not a member of SvgPreserveAspectRatio");

            SvgAspectRatio pRet = new SvgAspectRatio(eAlign);
            pRet.Slice = bSlice;
            pRet.Defer = bDefer;
            return (pRet);
        }
    }
}
