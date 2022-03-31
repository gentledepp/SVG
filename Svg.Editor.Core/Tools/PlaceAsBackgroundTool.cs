using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Extensions;
using Svg.Editor.Interfaces;
using Svg.Interfaces;

namespace Svg.Editor.Tools
{
    public class PlaceAsBackgroundTool : UndoableToolBase
    {
        public PlaceAsBackgroundTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Background Image", properties, undoRedoService)
        {
            IconName = "ic_insert_photo.svg";
        }

	    public override bool CanSerialize => false;

	    public override async Task Initialize(ISvgDrawingCanvas ws)
        {
            await base.Initialize(ws);

            Commands = new List<IToolCommand>
            {
                new ToolCommand(this, "Choose background image", async o =>
                {
                    var imgs = SvgEngine.TryResolve<IPickImageService>();
                    if (imgs == null) return;

                    ImagePath = await imgs.PickImagePathAsync(Canvas.ScreenWidth);
                    if (ImagePath == null) return;
                    PlaceImage(ImagePath);
                }, o => ChooseBackgroundEnabled, iconName: "ic_insert_photo.svg"),
                new ToolCommand(this, "Remove background image", o =>
                {
                    var children = Canvas.Document.Children;
                    var background = children.FirstOrDefault(x => x.CustomAttributes.ContainsKey(BackgroundCustomAttributeKey));
                    if (background != null)
                    {
                        children.Remove(background);
                        background.Dispose();

                        Canvas.Constraints = null;

                        ImagePath = null;

                        Canvas.FireInvalidateCanvas();
                        Canvas.FireToolCommandsChanged();
                    }
                }, o => ChooseBackgroundEnabled && Canvas.Document.Children.Any(x => x.CustomAttributes.ContainsKey(BackgroundCustomAttributeKey)), iconName: "ic_delete.svg")
            };

            if (ImagePath != null)
            {
                PlaceImage(ImagePath);
            }
        }

        private void PlaceImage(string path)
        {
            try
            {
                var children = Canvas.Document.Children;

                // create image from path
                if (!path.StartsWith("/")) path = path.Insert(0, "/");
                var image = new SvgImage
                {
                    Href = $"file://{path}"
                };

                // add custom icl attributes
                image.CustomAttributes.Add(BackgroundCustomAttributeKey, "");
                image.AddConstraints(NoSnappingConstraint, NoFillConstraint, NoStrokeConstraint);

                // remove already placed background
                var formerBackground = children.FirstOrDefault(x => x.CustomAttributes.ContainsKey(BackgroundCustomAttributeKey));
                if (formerBackground != null)
                {
                    children.Remove(formerBackground);
                    formerBackground.Dispose();
                }

                // add constraints to the canvas
                var size = image.GetImageSize();

                Canvas.Constraints = RectangleF.Create(0, 0, size.Width, size.Height);

                // insert the background before the first visible element
                var index = children.IndexOf(children.FirstOrDefault(x => x is SvgVisualElement));

                // if there are no visual elements, we want to add it to the end of the list
                if (index == -1) index = children.Count;

                children.Insert(index, image);

                Canvas.FireInvalidateCanvas();
                Canvas.FireToolCommandsChanged();
            }
            catch (IOException)
            {
            }
        }

        public static readonly string ChooseBackgroundEnabledKey = @"choosebackgroundenabled";
        public static readonly string ImagePathKey = @"imagepath";

        public bool ChooseBackgroundEnabled
        {
            get
            {
                if (SvgEngine.TryResolve<IPickImageService>() == null)
                    return false;

                object chooseBackgroundEnabled;
                return Properties.TryGetValue(ChooseBackgroundEnabledKey, out chooseBackgroundEnabled) && Convert.ToBoolean(chooseBackgroundEnabled);
            }
            set { Properties[ChooseBackgroundEnabledKey] = value; }
        }

        public string ImagePath
        {
            get
            {
                object imagePath;
                Properties.TryGetValue(ImagePathKey, out imagePath);
                return imagePath as string;
            }
            set { Properties[ImagePathKey] = value; }
        }
    }
}
