
namespace Svg.FilterEffects
{
    public abstract class SvgFilterPrimitive : SvgElement
    {
        private SvgAttributeCollection.Attribute<string> _in;
        private SvgAttributeCollection.Attribute<string> _result;
        public const string SourceGraphic = "SourceGraphic";
        public const string SourceAlpha = "SourceAlpha";
        public const string BackgroundImage = "BackgroundImage";
        public const string BackgroundAlpha = "BackgroundAlpha";
        public const string FillPaint = "FillPaint";
        public const string StrokePaint = "StrokePaint";

        [SvgAttribute("in")]
        public string Input
        {
            get { return (_in ??= this.Attributes.GetAttribute<string>("in")).GetValue(); }
            set { this.Attributes["in"] = value; }
        }

        [SvgAttribute("result")]
        public string Result
        {
            get { return (_result ??= this.Attributes.GetAttribute<string>("result")).GetValue(); }
            set { this.Attributes["result"] = value; }
        }

        protected SvgFilter Owner
        {
            get { return (SvgFilter)this.Parent; }
        }

        public abstract void Process(ImageBuffer buffer);
    }
}