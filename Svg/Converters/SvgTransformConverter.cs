using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Svg.Converters.Svg;
using Svg.Transforms;

namespace Svg.Converters
{
    internal class SvgTransformConverter : BaseConverter
    {
        private static IEnumerable<string> SplitTransforms(string transforms)
        {
            int transformEnd = 0;

            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i] == ')')
                {
                    yield return transforms.Substring(transformEnd, i - transformEnd + 1).Trim();
                    while (i < transforms.Length && !char.IsLetter(transforms[i])) i++;
                    transformEnd = i;
                }
            }
        }
        public override object ConvertFromString(string value, Type targetType, SvgDocument document)
        {
            SvgTransformCollection transformList = new SvgTransformCollection();

            string[] parts;
            string contents;
            string transformName;

            foreach (string transform in SvgTransformConverter.SplitTransforms((string)value))
            {
                if (string.IsNullOrEmpty(transform))
                    continue;

                parts = transform.Split('(', ')');
                transformName = parts[0].Trim();
                contents = parts[1].Trim();

                switch (transformName)
                {
                    case "translate":
                        string[] coords = contents.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (coords.Length == 0 || coords.Length > 2)
                        {
                            throw new FormatException("Translate transforms must be in the format 'translate(x [,y])'");
                        }

                        float x = float.Parse(coords[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
                        if (coords.Length > 1)
                        {
                            float y = float.Parse(coords[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
                            transformList.Add(new SvgTranslate(x, y));
                        }
                        else
                        {
                            transformList.Add(new SvgTranslate(x));
                        }
                        break;
                    case "rotate":
                        string[] args = contents.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (args.Length != 1 && args.Length != 3)
                        {
                            throw new FormatException("Rotate transforms must be in the format 'rotate(angle [cx cy ])'");
                        }

                        float angle = float.Parse(args[0], NumberStyles.Float, CultureInfo.InvariantCulture);

                        if (args.Length == 1)
                        {
                            transformList.Add(new SvgRotate(angle));
                        }
                        else
                        {
                            float cx = float.Parse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture);
                            float cy = float.Parse(args[2], NumberStyles.Float, CultureInfo.InvariantCulture);

                            transformList.Add(new SvgRotate(angle, cx, cy));
                        }
                        break;
                    case "scale":
                        string[] scales = contents.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (scales.Length == 0 || scales.Length > 2)
                        {
                            throw new FormatException("Scale transforms must be in the format 'scale(x [,y])'");
                        }

                        float sx = float.Parse(scales[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);

                        if (scales.Length > 1)
                        {
                            float sy = float.Parse(scales[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
                            transformList.Add(new SvgScale(sx, sy));
                        }
                        else
                        {
                            transformList.Add(new SvgScale(sx));
                        }

                        break;
                    case "matrix":
                        string[] points = contents.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (points.Length != 6)
                        {
                            throw new FormatException("Matrix transforms must be in the format 'matrix(m11, m12, m21, m22, dx, dy)'");
                        }

                        List<float> mPoints = new List<float>();
                        foreach (string point in points)
                        {
                            mPoints.Add(float.Parse(point.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture));
                        }

                        transformList.Add(new SvgMatrix(mPoints));
                        break;
                    case "shear":
                        string[] shears = contents.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (shears.Length == 0 || shears.Length > 2)
                        {
                            throw new FormatException("Shear transforms must be in the format 'shear(x [,y])'");
                        }

                        float hx = float.Parse(shears[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);

                        if (shears.Length > 1)
                        {
                            float hy = float.Parse(shears[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture);
                            transformList.Add(new SvgShear(hx, hy));
                        }
                        else
                        {
                            transformList.Add(new SvgShear(hx));
                        }

                        break;
                    case "skewX":
                        float ax = float.Parse(contents, NumberStyles.Float, CultureInfo.InvariantCulture);
                        transformList.Add(new SvgSkew(ax, 0));
                        break;
                    case "skewY":
                        float ay = float.Parse(contents, NumberStyles.Float, CultureInfo.InvariantCulture);
                        transformList.Add(new SvgSkew(0, ay));
                        break;
                }
            }

            return transformList;
        }

        public override string ConvertToString(object value)
        {
            var transforms = value as SvgTransformCollection;

            if (transforms != null)
            {
                return string.Join(" ", transforms.Select(t => t.WriteToString()).ToArray());
            }
            return null;
        }
    }
}
