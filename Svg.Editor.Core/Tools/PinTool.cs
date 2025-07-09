using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;
using Svg.Transforms;

namespace Svg.Editor.Tools
{
    public interface IPinInputService
	{
        Task<PinTool.PinSize> GetUserInput(
            IEnumerable<string> pinSizeOptions, int oldSizeIndex = 1);
    }

    public class PinTool : UndoableToolBase, ISupportTextColor, ISupportMoving
    {
        #region Private fields

        private ITextInputService _textInputService;
        private IPinInputService _pinInputService;
        private string _text;

        #endregion

        public PinTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Pin", properties, undoRedoService)
        {
            IconName = "ic_pin_tool.svg";
            PinResizeIconName = "ic_pin_resize.svg";
            ToolUsage = ToolUsage.Explicit;
            ToolType = ToolType.Create;
        }

        #region Public properties

        public const string PinSizeAttributeKey = "pinsize";
        public const string PinFillAttributeKey = "pinfill";
        public const string PinSizeNamesKey = "pinsizenames";
        public string PinResizeIconName { get; } 

        public string[] PinSizeNames
        {
            get
            {
                if (!Properties.TryGetValue(PinSizeNamesKey, out var pinSizeNames))
                    pinSizeNames = Enumerable.Empty<string>();
                return (string[])pinSizeNames;
            }
        }

        public PinSize SelectedPinSize
        {
            get
            {
                if (Properties.TryGetValue(PinSizeAttributeKey, out var index))
                {
                    if (Enum.TryParse<PinSize>(index.ToString(), out var s)) return s;
                }
                return PinSize.Medium;
            }
            set { Properties[PinSizeAttributeKey] = value; }
        }

        public string GetDefaultTextColorIndex(string color)
        {
            return color;
        }

        #endregion

        #region Overrides

        public override async Task Initialize(ISvgDrawingCanvas ws)
        {
            await base.Initialize(ws);
            
            _textInputService = SvgEngine.TryResolve<ITextInputService>();
            _pinInputService = SvgEngine.TryResolve<IPinInputService>();
            _text = "";

            // add tool commands
            Commands = new List<IToolCommand>
            {
                new ChangePinSizeCommand(ws, this, "Change size", description:LocalizationService.GetString("Svg.Editor.PinTool.ChangePinSize.Description")),
            };
        }

		protected override async Task OnTap(TapGesture tap)
        {
            await base.OnTap(tap);

            if (!IsActive) return;

            Canvas.SelectedElements.Clear();

            // select elements under pointer
            var pointerRect = Canvas.GetPointerRectangle(tap.Position);
            var selected = Canvas.GetElementsUnder<SvgVisualElement>(pointerRect, SelectionType.Intersect)
                .FirstOrDefault();

            if (selected != null)
            {
                if (selected.CustomAttributes.ContainsKey(PinSizeAttributeKey))
                {
                    Canvas.SelectedElements.Add(selected);
                }
            }

            Canvas.FireInvalidateCanvas();
        }

        protected override async Task OnDoubleTap(DoubleTapGesture doubleTap)
        {
            await base.OnDoubleTap(doubleTap);

            if (!IsActive) return;

            SvgVisualElement selectedElement = null;

            // select elements under pointer
            var pointerRect = Canvas.GetPointerRectangle(doubleTap.Position);
            var selected = Canvas.GetElementsUnder<SvgVisualElement>(pointerRect, SelectionType.Intersect)
                .FirstOrDefault();

            if (selected != null)
            {
                if (selected.CustomAttributes.ContainsKey(PinSizeAttributeKey))
                {
                    selectedElement = selected;
                }
            }

            if (selectedElement != null)
            {
                await GetPinTextFromUserInput();

                selectedElement.CustomAttributes.TryGetValue(PinSizeAttributeKey, out var sizeStr);
                if (!Enum.TryParse<PinSize>(sizeStr, out var size)) return;

                var oldFill = (SvgColourServer)selectedElement.Children[0].Fill;
                var oldStroke = (SvgColourServer)selectedElement.Children[0].Stroke;

                if (_text != "")
                {
                    var pin = HoleyPinToFilled(size, selectedElement);
                    ((SvgText)pin.Children[1]).Text = _text;

                    Canvas.Document.Children.Remove(selectedElement);
                    Canvas.Document.Children.Add(pin);

                    var id = Canvas.Document.Children.Count - 1;
                    Canvas.Document.Children[id].Children[0].Fill = oldFill;
                    Canvas.Document.Children[id].Children[0].Stroke = oldStroke;

                    Canvas.FireInvalidateCanvas();
                }
                else
                {
                    var pin = FilledPinToHoley(size, selectedElement);
                    ((SvgText)pin.Children[1]).Text = "";

                    Canvas.Document.Children.Remove(selectedElement);
                    Canvas.Document.Children.Add(pin);

                    var id = Canvas.Document.Children.Count - 1;
                    Canvas.Document.Children[id].Children[0].Fill = oldFill;
                    Canvas.Document.Children[id].Children[0].Stroke = oldStroke;

                    Canvas.FireInvalidateCanvas();
                }
            }
        }

        protected override async Task OnLongPress(LongPressGesture longPress)
        {
            await base.OnLongPress(longPress);

            if (!IsActive) return;

            System.Diagnostics.Debug.WriteLine(longPress.Position);

            var relativePosition = Canvas.ScreenToCanvas(longPress.Position);
            var pin = CreatePin(SelectedPinSize, relativePosition);

            UndoRedoService.ExecuteCommand(new UndoableActionCommand("Pin", o =>
            {
                Canvas.Document.Children.Add(pin);
                Canvas.FireInvalidateCanvas();
            }, o =>
            {
                Canvas.Document.Children.Remove(pin);
                Canvas.FireInvalidateCanvas();
            }));
        }

        #endregion

        #region Private helpers

        private async Task<PinSize> GetPinSizeFromUserInput(string size = "")
        {
            try
            {
                if (Enum.TryParse<PinSize>(size, out var oldSize))
                {
                    return await _pinInputService.GetUserInput(PinSizeNames, (int)oldSize);
                }
				else
                {
                    return await _pinInputService.GetUserInput(PinSizeNames);
                }
                
            }
            catch (TaskCanceledException)
            {
                return PinSize.Medium;
            }
        }

        private async Task GetPinTextFromUserInput()
        {
            try
            {
                var input = await _textInputService.GetUserInput("Please enter 1 or 2 characters.", maxTextLength: 2);
                _text = input.Text;
            }
            catch (TaskCanceledException)
            {
                _text = "";
            }
        }

        private void ChangePinSize(SvgElement previousGroup, PinSize newSize)
        {
            var oldFill = (SvgColourServer)previousGroup.Children[0].Fill;
            var oldStroke = (SvgColourServer)previousGroup.Children[0].Stroke;
            var oldTextFill = (SvgColourServer)previousGroup.Children[1].Fill;
            var oldTextStroke = (SvgColourServer)previousGroup.Children[1].Stroke;

            if (previousGroup.CustomAttributes[PinFillAttributeKey] == "Holey")
			{
                var newGroup = FilledPinToHoley(newSize, previousGroup);
                newGroup.CustomAttributes[PinSizeAttributeKey] = newSize.ToString();

                Canvas.Document.Children.Remove(previousGroup);
                Canvas.Document.Children.Add(newGroup);

                var id = Canvas.Document.Children.Count - 1;
                Canvas.Document.Children[id].Children[0].Fill = oldFill;
                Canvas.Document.Children[id].Children[0].Stroke = oldStroke;
                Canvas.Document.Children[id].Children[1].Fill = oldTextFill;
                Canvas.Document.Children[id].Children[1].Stroke = oldTextStroke;

                Canvas.FireInvalidateCanvas();
            }
			else if (previousGroup.CustomAttributes[PinFillAttributeKey] == "Filled")
            {
                var newGroup = HoleyPinToFilled(newSize, previousGroup);
                newGroup.CustomAttributes[PinSizeAttributeKey] = newSize.ToString();

                ((SvgText)newGroup.Children[1]).Text = ((SvgText)previousGroup.Children[1]).Text;

                Canvas.Document.Children.Remove(previousGroup);
                Canvas.Document.Children.Add(newGroup);

                var id = Canvas.Document.Children.Count - 1;
                Canvas.Document.Children[id].Children[0].Fill = oldFill;
                Canvas.Document.Children[id].Children[0].Stroke = oldStroke;
                Canvas.Document.Children[id].Children[1].Fill = oldTextFill;
                Canvas.Document.Children[id].Children[1].Stroke = oldTextStroke;

                Canvas.FireInvalidateCanvas();
            }

        }

        private SvgElement CreatePin(PinSize size, PointF position)
        {
            SvgDocument pinSvg;
            SvgElement group;

            float w, h;
            switch (SelectedPinSize)
            {
                case PinSize.Small:
                    pinSvg = SvgDocument.FromSvg<SvgDocument>(
                        "<svg><g><path d=\"M10.82 0.03L11.22 0.07L11.62 0.13L12.02 0.2L12.4 0.29L12.79 0.39L13.16 0.5L13.53 0.63L13.89 0.77L14.25 0.93L14.6 1.1L14.94 1.28L15.27 1.48L15.59 1.68L15.91 1.9L16.21 2.13L16.51 2.37L16.79 2.62L17.07 2.89L17.34 3.16L17.59 3.44L17.84 3.73L18.07 4.03L18.29 4.34L18.5 4.66L18.7 4.99L18.88 5.32L19.06 5.67L19.21 6.02L19.36 6.37L19.49 6.74L19.61 7.11L19.71 7.48L19.8 7.87L19.87 8.25L19.93 8.65L19.97 9.04L19.99 9.45L20 9.85L19.99 10.26L19.97 10.66L19.93 11.06L19.87 11.46L19.8 11.85L19.71 12.25L19.61 12.64L19.49 13.03L19.36 13.42L19.21 13.82L19.06 14.21L18.88 14.6L18.7 15L18.5 15.4L18.29 15.8L18.07 16.2L17.84 16.61L17.59 17.02L17.34 17.43L17.07 17.86L16.79 18.28L16.51 18.71L16.21 19.15L15.91 19.59L15.59 20.05L15.27 20.51L14.94 20.97L14.6 21.45L14.25 21.93L13.89 22.43L13.53 22.93L13.16 23.45L12.79 23.98L12.4 24.51L12.02 25.06L11.62 25.62L11.22 26.2L10.82 26.78L10.41 27.38L10 28L9.59 27.38L9.18 26.78L8.78 26.2L8.38 25.62L7.98 25.06L7.6 24.51L7.21 23.98L6.84 23.45L6.47 22.93L6.11 22.43L5.75 21.93L5.4 21.45L5.06 20.97L4.73 20.51L4.41 20.05L4.09 19.59L3.79 19.15L3.49 18.71L3.21 18.28L2.93 17.86L2.66 17.43L2.41 17.02L2.16 16.61L1.93 16.2L1.71 15.8L1.5 15.4L1.3 15L1.12 14.6L0.94 14.21L0.79 13.82L0.64 13.42L0.51 13.03L0.39 12.64L0.29 12.25L0.2 11.85L0.13 11.46L0.07 11.06L0.03 10.66L0.01 10.26L0 9.85L0.01 9.45L0.03 9.04L0.07 8.65L0.13 8.25L0.2 7.87L0.29 7.48L0.39 7.11L0.51 6.74L0.64 6.37L0.79 6.02L0.94 5.67L1.12 5.32L1.3 4.99L1.5 4.66L1.71 4.34L1.93 4.03L2.16 3.73L2.41 3.44L2.66 3.16L2.93 2.89L3.21 2.62L3.49 2.37L3.79 2.13L4.09 1.9L4.41 1.68L4.73 1.48L5.06 1.28L5.4 1.1L5.75 0.93L6.11 0.77L6.47 0.63L6.84 0.5L7.21 0.39L7.6 0.29L7.98 0.2L8.38 0.13L8.78 0.07L9.18 0.03L9.59 0.01L10 0L10.41 0.01L10.82 0.03ZM9.61 4.68L9.42 4.7L9.23 4.73L9.05 4.76L8.86 4.8L8.68 4.85L8.5 4.9L8.33 4.97L8.16 5.03L7.99 5.11L7.82 5.19L7.66 5.27L7.5 5.37L7.35 5.46L7.2 5.57L7.06 5.68L6.92 5.79L6.78 5.91L6.65 6.03L6.52 6.16L6.4 6.3L6.29 6.43L6.18 6.58L6.07 6.72L5.97 6.88L5.88 7.03L5.79 7.19L5.71 7.35L5.64 7.52L5.57 7.69L5.5 7.86L5.45 8.03L5.4 8.21L5.36 8.39L5.33 8.58L5.3 8.76L5.28 8.95L5.27 9.14L5.26 9.33L5.27 9.53L5.28 9.72L5.3 9.9L5.33 10.09L5.36 10.27L5.4 10.45L5.45 10.63L5.5 10.81L5.57 10.98L5.64 11.15L5.71 11.32L5.79 11.48L5.88 11.64L5.97 11.79L6.07 11.94L6.18 12.09L6.29 12.23L6.4 12.37L6.52 12.5L6.65 12.63L6.78 12.76L6.92 12.88L7.06 12.99L7.2 13.1L7.35 13.2L7.5 13.3L7.66 13.39L7.82 13.48L7.99 13.56L8.16 13.63L8.33 13.7L8.5 13.76L8.68 13.82L8.86 13.86L9.05 13.91L9.23 13.94L9.42 13.97L9.61 13.98L9.8 14L10 14L10.2 14L10.39 13.98L10.58 13.97L10.77 13.94L10.95 13.91L11.14 13.86L11.32 13.82L11.5 13.76L11.67 13.7L11.84 13.63L12.01 13.56L12.18 13.48L12.34 13.39L12.5 13.3L12.65 13.2L12.8 13.1L12.94 12.99L13.08 12.88L13.22 12.76L13.35 12.63L13.48 12.5L13.6 12.37L13.71 12.23L13.82 12.09L13.93 11.94L14.03 11.79L14.12 11.64L14.21 11.48L14.29 11.32L14.36 11.15L14.43 10.98L14.5 10.81L14.55 10.63L14.6 10.45L14.64 10.27L14.67 10.09L14.7 9.9L14.72 9.72L14.73 9.53L14.74 9.33L14.73 9.14L14.72 8.95L14.7 8.76L14.67 8.58L14.64 8.39L14.6 8.21L14.55 8.03L14.5 7.86L14.43 7.69L14.36 7.52L14.29 7.35L14.21 7.19L14.12 7.03L14.03 6.88L13.93 6.72L13.82 6.58L13.71 6.43L13.6 6.3L13.48 6.16L13.35 6.03L13.22 5.91L13.08 5.79L12.94 5.68L12.8 5.57L12.65 5.46L12.5 5.37L12.34 5.27L12.18 5.19L12.01 5.11L11.84 5.03L11.67 4.97L11.5 4.9L11.32 4.85L11.14 4.8L10.95 4.76L10.77 4.73L10.58 4.7L10.39 4.68L10.2 4.67L10 4.67L9.8 4.67L9.61 4.68Z\"></path>" +
                        "<text text-anchor=\"middle\"></text></g></svg>");
                    group = pinSvg.Children[0];
                    ((SvgText)group.Children[1]).FontSize = 10;
                    group.CustomAttributes.Add(PinSizeAttributeKey, "Small");
                    w = 10;
                    h = 28;
                    break;
                case PinSize.Medium:
                default:
                    pinSvg = SvgDocument.FromSvg<SvgDocument>(
                        "<svg><g><path d=\"M21.64 0.07L22.45 0.15L23.24 0.26L24.03 0.4L24.81 0.57L25.57 0.77L26.32 1L27.06 1.26L27.78 1.55L28.5 1.86L29.19 2.2L29.87 2.56L30.54 2.95L31.18 3.37L31.81 3.8L32.42 4.26L33.02 4.74L33.59 5.25L34.14 5.77L34.67 6.32L35.19 6.88L35.67 7.46L36.14 8.07L36.58 8.69L37 9.32L37.4 9.98L37.77 10.65L38.11 11.33L38.43 12.03L38.72 12.75L38.98 13.48L39.21 14.22L39.42 14.97L39.59 15.73L39.74 16.51L39.85 17.29L39.93 18.09L39.98 18.89L40 19.7L39.98 20.52L39.93 21.32L39.85 22.12L39.74 22.92L39.59 23.71L39.42 24.49L39.21 25.28L38.98 26.06L38.72 26.85L38.43 27.63L38.11 28.42L37.77 29.21L37.4 30L37 30.79L36.58 31.6L36.14 32.4L35.67 33.22L35.19 34.04L34.67 34.87L34.14 35.71L33.59 36.56L33.02 37.42L32.42 38.3L31.81 39.19L31.18 40.09L30.54 41.01L29.87 41.95L29.19 42.9L28.5 43.87L27.78 44.86L27.06 45.87L26.32 46.9L25.57 47.95L24.81 49.02L24.03 50.12L23.24 51.25L22.45 52.39L21.64 53.57L20.82 54.77L20 56L19.18 54.77L18.36 53.57L17.55 52.39L16.76 51.25L15.97 50.12L15.19 49.02L14.43 47.95L13.68 46.9L12.94 45.87L12.22 44.86L11.5 43.87L10.81 42.9L10.13 41.95L9.46 41.01L8.82 40.09L8.19 39.19L7.58 38.3L6.98 37.42L6.41 36.56L5.86 35.71L5.33 34.87L4.81 34.04L4.33 33.22L3.86 32.4L3.42 31.6L3 30.79L2.6 30L2.23 29.21L1.89 28.42L1.57 27.63L1.28 26.85L1.02 26.06L0.79 25.28L0.58 24.49L0.41 23.71L0.26 22.92L0.15 22.12L0.07 21.32L0.02 20.52L0 19.7L0.02 18.89L0.07 18.09L0.15 17.29L0.26 16.51L0.41 15.73L0.58 14.97L0.79 14.22L1.02 13.48L1.28 12.75L1.57 12.03L1.89 11.33L2.23 10.65L2.6 9.98L3 9.32L3.42 8.69L3.86 8.07L4.33 7.46L4.81 6.88L5.33 6.32L5.86 5.77L6.41 5.25L6.98 4.74L7.58 4.26L8.19 3.8L8.82 3.37L9.46 2.95L10.13 2.56L10.81 2.2L11.5 1.86L12.22 1.55L12.94 1.26L13.68 1L14.43 0.77L15.19 0.57L15.97 0.4L16.76 0.26L17.55 0.15L18.36 0.07L19.18 0.02L20 0L20.82 0.02L21.64 0.07ZM19.22 9.36L18.84 9.4L18.46 9.46L18.09 9.52L17.72 9.6L17.36 9.7L17.01 9.81L16.66 9.93L16.31 10.07L15.98 10.21L15.65 10.38L15.32 10.55L15.01 10.73L14.7 10.93L14.4 11.13L14.12 11.35L13.83 11.58L13.56 11.82L13.3 12.07L13.05 12.33L12.81 12.59L12.58 12.87L12.35 13.15L12.14 13.45L11.95 13.75L11.76 14.06L11.58 14.38L11.42 14.7L11.27 15.03L11.13 15.37L11.01 15.72L10.9 16.07L10.8 16.42L10.72 16.79L10.65 17.15L10.6 17.52L10.56 17.9L10.53 18.28L10.53 18.67L10.53 19.05L10.56 19.43L10.6 19.81L10.65 20.18L10.72 20.55L10.8 20.91L10.9 21.27L11.01 21.62L11.13 21.96L11.27 22.3L11.42 22.63L11.58 22.96L11.76 23.27L11.95 23.58L12.14 23.89L12.35 24.18L12.58 24.46L12.81 24.74L13.05 25.01L13.3 25.27L13.56 25.51L13.83 25.75L14.12 25.98L14.4 26.2L14.7 26.41L15.01 26.6L15.32 26.79L15.65 26.96L15.98 27.12L16.31 27.27L16.66 27.4L17.01 27.52L17.36 27.63L17.72 27.73L18.09 27.81L18.46 27.88L18.84 27.93L19.22 27.97L19.61 27.99L20 28L20.39 27.99L20.78 27.97L21.16 27.93L21.54 27.88L21.91 27.81L22.28 27.73L22.64 27.63L22.99 27.52L23.34 27.4L23.69 27.27L24.02 27.12L24.35 26.96L24.68 26.79L24.99 26.6L25.3 26.41L25.6 26.2L25.88 25.98L26.17 25.75L26.44 25.51L26.7 25.27L26.95 25.01L27.19 24.74L27.42 24.46L27.65 24.18L27.86 23.89L28.05 23.58L28.24 23.27L28.42 22.96L28.58 22.63L28.73 22.3L28.87 21.96L28.99 21.62L29.1 21.27L29.2 20.91L29.28 20.55L29.35 20.18L29.4 19.81L29.44 19.43L29.47 19.05L29.47 18.67L29.47 18.28L29.44 17.9L29.4 17.52L29.35 17.15L29.28 16.79L29.2 16.42L29.1 16.07L28.99 15.72L28.87 15.37L28.73 15.03L28.58 14.7L28.42 14.38L28.24 14.06L28.05 13.75L27.86 13.45L27.65 13.15L27.42 12.87L27.19 12.59L26.95 12.33L26.7 12.07L26.44 11.82L26.17 11.58L25.88 11.35L25.6 11.13L25.3 10.93L24.99 10.73L24.68 10.55L24.35 10.38L24.02 10.21L23.69 10.07L23.34 9.93L22.99 9.81L22.64 9.7L22.28 9.6L21.91 9.52L21.54 9.46L21.16 9.4L20.78 9.36L20.39 9.34L20 9.33L19.61 9.34L19.22 9.36Z\"></path>" +
                        "<text text-anchor=\"middle\"></text></g></svg>");
                    group = pinSvg.Children[0];
                    ((SvgText)group.Children[1]).FontSize = 20;
                    group.CustomAttributes.Add(PinSizeAttributeKey, "Medium");
                    w = 20;
                    h = 56;
                    break;
                case PinSize.Large:
                    pinSvg = SvgDocument.FromSvg<SvgDocument>(
                        "<svg><g><path d=\"M32.46 0.1L33.67 0.22L34.87 0.39L36.05 0.6L37.21 0.86L38.36 1.16L39.48 1.51L40.59 1.89L41.68 2.32L42.74 2.79L43.79 3.3L44.81 3.84L45.8 4.43L46.77 5.05L47.72 5.7L48.63 6.39L49.52 7.11L50.38 7.87L51.21 8.66L52.01 9.47L52.78 10.32L53.51 11.2L54.21 12.1L54.88 13.03L55.51 13.99L56.1 14.97L56.65 15.97L57.17 17L57.64 18.05L58.08 19.12L58.47 20.21L58.82 21.32L59.13 22.45L59.39 23.6L59.61 24.76L59.78 25.94L59.9 27.13L59.97 28.34L60 29.56L59.97 30.77L59.9 31.98L59.78 33.18L59.61 34.37L59.39 35.56L59.13 36.74L58.82 37.92L58.47 39.1L58.08 40.27L57.64 41.45L57.17 42.63L56.65 43.81L56.1 45L55.51 46.19L54.88 47.39L54.21 48.6L53.51 49.82L52.78 51.06L52.01 52.3L51.21 53.57L50.38 54.84L49.52 56.14L48.63 57.45L47.72 58.78L46.77 60.14L45.8 61.52L44.81 62.92L43.79 64.35L42.74 65.8L41.68 67.29L40.59 68.8L39.48 70.35L38.36 71.93L37.21 73.54L36.05 75.18L34.87 76.87L33.67 78.59L32.46 80.35L31.24 82.15L30 84L28.76 82.15L27.54 80.35L26.33 78.59L25.13 76.87L23.95 75.18L22.79 73.54L21.64 71.93L20.52 70.35L19.41 68.8L18.32 67.29L17.26 65.8L16.21 64.35L15.19 62.92L14.2 61.52L13.23 60.14L12.28 58.78L11.37 57.45L10.48 56.14L9.62 54.84L8.79 53.57L7.99 52.3L7.22 51.06L6.49 49.82L5.79 48.6L5.12 47.39L4.49 46.19L3.9 45L3.35 43.81L2.83 42.63L2.36 41.45L1.92 40.27L1.53 39.1L1.18 37.92L0.87 36.74L0.61 35.56L0.39 34.37L0.22 33.18L0.1 31.98L0.03 30.77L0 29.56L0.03 28.34L0.1 27.13L0.22 25.94L0.39 24.76L0.61 23.6L0.87 22.45L1.18 21.32L1.53 20.21L1.92 19.12L2.36 18.05L2.83 17L3.35 15.97L3.9 14.97L4.49 13.99L5.12 13.03L5.79 12.1L6.49 11.2L7.22 10.32L7.99 9.47L8.79 8.66L9.62 7.87L10.48 7.11L11.37 6.39L12.28 5.7L13.23 5.05L14.2 4.43L15.19 3.84L16.21 3.3L17.26 2.79L18.32 2.32L19.41 1.89L20.52 1.51L21.64 1.16L22.79 0.86L23.95 0.6L25.13 0.39L26.33 0.22L27.54 0.1L28.76 0.02L30 0L31.24 0.02L32.46 0.1ZM28.83 14.05L28.26 14.1L27.69 14.18L27.14 14.28L26.59 14.41L26.04 14.55L25.51 14.71L24.98 14.9L24.47 15.1L23.96 15.32L23.47 15.56L22.99 15.82L22.51 16.1L22.05 16.39L21.61 16.7L21.17 17.03L20.75 17.37L20.34 17.73L19.95 18.1L19.57 18.49L19.21 18.89L18.86 19.3L18.53 19.73L18.22 20.17L17.92 20.63L17.64 21.09L17.38 21.57L17.13 22.05L16.91 22.55L16.7 23.06L16.51 23.57L16.35 24.1L16.2 24.64L16.08 25.18L15.98 25.73L15.89 26.29L15.84 26.85L15.8 27.42L15.79 28L15.8 28.58L15.84 29.15L15.89 29.71L15.98 30.27L16.08 30.82L16.2 31.36L16.35 31.9L16.51 32.43L16.7 32.94L16.91 33.45L17.13 33.95L17.38 34.43L17.64 34.91L17.92 35.37L18.22 35.83L18.53 36.27L18.86 36.7L19.21 37.11L19.57 37.51L19.95 37.9L20.34 38.27L20.75 38.63L21.17 38.97L21.61 39.3L22.05 39.61L22.51 39.9L22.99 40.18L23.47 40.44L23.96 40.68L24.47 40.9L24.98 41.1L25.51 41.29L26.04 41.45L26.59 41.59L27.14 41.72L27.69 41.82L28.26 41.9L28.83 41.95L29.41 41.99L30 42L30.59 41.99L31.17 41.95L31.74 41.9L32.31 41.82L32.86 41.72L33.41 41.59L33.96 41.45L34.49 41.29L35.02 41.1L35.53 40.9L36.04 40.68L36.53 40.44L37.01 40.18L37.49 39.9L37.95 39.61L38.39 39.3L38.83 38.97L39.25 38.63L39.66 38.27L40.05 37.9L40.43 37.51L40.79 37.11L41.14 36.7L41.47 36.27L41.78 35.83L42.08 35.37L42.36 34.91L42.62 34.43L42.87 33.95L43.09 33.45L43.3 32.94L43.49 32.43L43.65 31.9L43.8 31.36L43.92 30.82L44.02 30.27L44.11 29.71L44.16 29.15L44.2 28.58L44.21 28L44.2 27.42L44.16 26.85L44.11 26.29L44.02 25.73L43.92 25.18L43.8 24.64L43.65 24.1L43.49 23.57L43.3 23.06L43.09 22.55L42.87 22.05L42.62 21.57L42.36 21.09L42.08 20.63L41.78 20.17L41.47 19.73L41.14 19.3L40.79 18.89L40.43 18.49L40.05 18.1L39.66 17.73L39.25 17.37L38.83 17.03L38.39 16.7L37.95 16.39L37.49 16.1L37.01 15.82L36.53 15.56L36.04 15.32L35.53 15.1L35.02 14.9L34.49 14.71L33.96 14.55L33.41 14.41L32.86 14.28L32.31 14.18L31.74 14.1L31.17 14.05L30.59 14.01L30 14L29.41 14.01L28.83 14.05Z\"></path>" +
                        "<text text-anchor=\"middle\"></text></g></svg>");
                    group = pinSvg.Children[0];
                    ((SvgText)group.Children[1]).FontSize = 30;
                    group.CustomAttributes.Add(PinSizeAttributeKey, "Large");
                    w = 30;
                    h = 84;
                    break;
                case PinSize.ExtraLarge:
                    pinSvg = SvgDocument.FromSvg<SvgDocument>(
                        "<svg><g><path d=\"M43.28 0.13L44.89 0.29L46.49 0.52L48.06 0.8L49.61 1.15L51.14 1.55L52.64 2.01L54.12 2.53L55.57 3.1L56.99 3.72L58.38 4.4L59.74 5.13L61.07 5.9L62.36 6.73L63.62 7.6L64.85 8.52L66.03 9.49L67.18 10.49L68.28 11.54L69.35 12.63L70.37 13.76L71.35 14.93L72.28 16.13L73.17 17.37L74.01 18.65L74.8 19.96L75.54 21.3L76.22 22.67L76.86 24.07L77.44 25.5L77.96 26.95L78.43 28.43L78.84 29.94L79.19 31.47L79.48 33.02L79.7 34.59L79.87 36.18L79.97 37.78L80 39.41L79.97 41.03L79.87 42.64L79.7 44.24L79.48 45.83L79.19 47.41L78.84 48.99L78.43 50.56L77.96 52.13L77.44 53.7L76.86 55.27L76.22 56.84L75.54 58.41L74.8 60L74.01 61.59L73.17 63.19L72.28 64.8L71.35 66.43L70.37 68.08L69.35 69.74L68.28 71.42L67.18 73.12L66.03 74.85L64.85 76.6L63.62 78.38L62.36 80.19L61.07 82.02L59.74 83.89L58.38 85.8L56.99 87.74L55.57 89.72L54.12 91.74L52.64 93.8L51.14 95.9L49.61 98.05L48.06 100.25L46.49 102.49L44.89 104.79L43.28 107.14L41.65 109.54L40 112L38.35 109.54L36.72 107.14L35.11 104.79L33.51 102.49L31.94 100.25L30.39 98.05L28.86 95.9L27.36 93.8L25.88 91.74L24.43 89.72L23.01 87.74L21.62 85.8L20.26 83.89L18.93 82.02L17.64 80.19L16.38 78.38L15.15 76.6L13.97 74.85L12.82 73.12L11.72 71.42L10.65 69.74L9.63 68.08L8.65 66.43L7.72 64.8L6.83 63.19L5.99 61.59L5.2 60L4.46 58.41L3.78 56.84L3.14 55.27L2.56 53.7L2.04 52.13L1.57 50.56L1.16 48.99L0.81 47.41L0.52 45.83L0.3 44.24L0.13 42.64L0.03 41.03L0 39.41L0.03 37.78L0.13 36.18L0.3 34.59L0.52 33.02L0.81 31.47L1.16 29.94L1.57 28.43L2.04 26.95L2.56 25.5L3.14 24.07L3.78 22.67L4.46 21.3L5.2 19.96L5.99 18.65L6.83 17.37L7.72 16.13L8.65 14.93L9.63 13.76L10.65 12.63L11.72 11.54L12.82 10.49L13.97 9.49L15.15 8.52L16.38 7.6L17.64 6.73L18.93 5.9L20.26 5.13L21.62 4.4L23.01 3.72L24.43 3.1L25.88 2.53L27.36 2.01L28.86 1.55L30.39 1.15L31.94 0.8L33.51 0.52L35.11 0.29L36.72 0.13L38.35 0.03L40 0L41.65 0.03L43.28 0.13ZM38.45 18.73L37.68 18.8L36.93 18.91L36.18 19.05L35.45 19.21L34.72 19.4L34.01 19.62L33.31 19.86L32.62 20.13L31.95 20.43L31.29 20.75L30.65 21.1L30.02 21.46L29.41 21.85L28.81 22.27L28.23 22.7L27.67 23.16L27.13 23.64L26.6 24.13L26.1 24.65L25.61 25.19L25.15 25.74L24.71 26.31L24.29 26.9L23.89 27.5L23.52 28.12L23.17 28.75L22.84 29.4L22.54 30.07L22.27 30.74L22.02 31.43L21.8 32.13L21.6 32.85L21.44 33.57L21.3 34.31L21.19 35.05L21.12 35.8L21.07 36.56L21.05 37.33L21.07 38.1L21.12 38.86L21.19 39.62L21.3 40.36L21.44 41.1L21.6 41.82L21.8 42.53L22.02 43.23L22.27 43.92L22.54 44.6L22.84 45.26L23.17 45.91L23.52 46.55L23.89 47.17L24.29 47.77L24.71 48.36L25.15 48.93L25.61 49.48L26.1 50.02L26.6 50.53L27.13 51.03L27.67 51.51L28.23 51.96L28.81 52.4L29.41 52.81L30.02 53.2L30.65 53.57L31.29 53.92L31.95 54.24L32.62 54.53L33.31 54.8L34.01 55.05L34.72 55.27L35.45 55.46L36.18 55.62L36.93 55.76L37.68 55.86L38.45 55.94L39.22 55.98L40 56L40.78 55.98L41.55 55.94L42.32 55.86L43.07 55.76L43.82 55.62L44.55 55.46L45.28 55.27L45.99 55.05L46.69 54.8L47.38 54.53L48.05 54.24L48.71 53.92L49.35 53.57L49.98 53.2L50.59 52.81L51.19 52.4L51.77 51.96L52.33 51.51L52.87 51.03L53.4 50.53L53.9 50.02L54.39 49.48L54.85 48.93L55.29 48.36L55.71 47.77L56.11 47.17L56.48 46.55L56.83 45.91L57.16 45.26L57.46 44.6L57.73 43.92L57.98 43.23L58.2 42.53L58.4 41.82L58.56 41.1L58.7 40.36L58.81 39.62L58.88 38.86L58.93 38.1L58.95 37.33L58.93 36.56L58.88 35.8L58.81 35.05L58.7 34.31L58.56 33.57L58.4 32.85L58.2 32.13L57.98 31.43L57.73 30.74L57.46 30.07L57.16 29.4L56.83 28.75L56.48 28.12L56.11 27.5L55.71 26.9L55.29 26.31L54.85 25.74L54.39 25.19L53.9 24.65L53.4 24.13L52.87 23.64L52.33 23.16L51.77 22.7L51.19 22.27L50.59 21.85L49.98 21.46L49.35 21.1L48.71 20.75L48.05 20.43L47.38 20.13L46.69 19.86L45.99 19.62L45.28 19.4L44.55 19.21L43.82 19.05L43.07 18.91L42.32 18.8L41.55 18.73L40.78 18.68L40 18.67L39.22 18.68L38.45 18.73Z\"></path>" +
                        "<text text-anchor=\"middle\"></text></g></svg>");
                    group = pinSvg.Children[0];
                    ((SvgText)group.Children[1]).FontSize = 40;
                    group.CustomAttributes.Add(PinSizeAttributeKey, "ExtraLarge");
                    w = 40;
                    h = 112;
                    break;
            }
            group.Fill = SvgColourServer.ContextFill;
            group.Stroke = SvgColourServer.ContextStroke;
            group.CustomAttributes.Add(PinFillAttributeKey, "Holey");

            group.Transforms.Add(new SvgTranslate(position.X, position.Y));
            group.Children[0].Transforms.Add(new SvgTranslate(-w, -h));

            return group;
        }

        private SvgElement HoleyPinToFilled(PinSize size, SvgElement previousPin)
        {
            SvgDocument pinSvg;
            SvgElement pin;

            float w, h;
            switch (size)
            {
                case PinSize.Small:
                    pinSvg = SvgDocument.FromSvg<SvgDocument>(
                        "<svg><path d=\"M20 9.85C20 4.41 15.52 0 10 0C4.48 0 0 4.41 0 9.85C0 15.29 4.48 19.7 10 28C15.52 19.7 20 15.29 20 9.85Z\"></path></svg>");
                    pin = pinSvg.Children[0];
                    ((SvgText)previousPin.Children[1]).FontSize = 10;
                    w = 10;
                    h = 28;
                    break;
                case PinSize.Medium:
                default:
                    pinSvg = SvgDocument.FromSvg<SvgDocument>(
                        "<svg><path d=\"M40 19.7C40 8.82 31.05 0 20 0C8.95 0 0 8.82 0 19.7C0 30.59 8.95 39.41 20 56C31.05 39.41 40 30.59 40 19.7Z\"></path></svg>");
                    pin = pinSvg.Children[0];
                    ((SvgText)previousPin.Children[1]).FontSize = 20;
                    w = 20;
                    h = 56;
                    break;
                case PinSize.Large:
                    pinSvg = SvgDocument.FromSvg<SvgDocument>(
                        "<svg><path d=\"M60 29.56C60 13.23 46.57 0 30 0C13.43 0 0 13.23 0 29.56C0 45.88 13.43 59.11 30 84C46.57 59.11 60 45.88 60 29.56Z\"></path></svg>");
                    pin = pinSvg.Children[0];
                    ((SvgText)previousPin.Children[1]).FontSize = 30;
                    w = 30;
                    h = 84;
                    break;
                case PinSize.ExtraLarge:
                    pinSvg = SvgDocument.FromSvg<SvgDocument>(
                        "<svg><path d=\"M80 39.41C80 17.64 62.09 0 40 0C17.91 0 0 17.64 0 39.41C0 61.17 17.91 78.81 40 112C62.09 78.81 80 61.17 80 39.41Z\"></path></svg>");
                    pin = pinSvg.Children[0];
                    ((SvgText)previousPin.Children[1]).FontSize = 40;
                    w = 40;
                    h = 112;
                    break;
            }

            previousPin.Children[0] = pin;

            previousPin.Children[0].Transforms.Add(new SvgTranslate(-w, -h));
            previousPin.Children[1].Transforms.RemoveAll(t => t.GetType() == typeof(SvgTranslate));
            previousPin.Children[1].Transforms.Add(new SvgTranslate(0, (-h / 2)));

            previousPin.CustomAttributes[PinFillAttributeKey] = "Filled";

            return previousPin;
        }

        private SvgElement FilledPinToHoley(PinSize size, SvgElement previousPin)
        {
            SvgDocument pinSvg;
            SvgElement pin;

            float w, h;
            switch (size)
            {
                case PinSize.Small:
                    pinSvg = SvgDocument.FromSvg<SvgDocument>(
                        "<svg><path d=\"M10.82 0.03L11.22 0.07L11.62 0.13L12.02 0.2L12.4 0.29L12.79 0.39L13.16 0.5L13.53 0.63L13.89 0.77L14.25 0.93L14.6 1.1L14.94 1.28L15.27 1.48L15.59 1.68L15.91 1.9L16.21 2.13L16.51 2.37L16.79 2.62L17.07 2.89L17.34 3.16L17.59 3.44L17.84 3.73L18.07 4.03L18.29 4.34L18.5 4.66L18.7 4.99L18.88 5.32L19.06 5.67L19.21 6.02L19.36 6.37L19.49 6.74L19.61 7.11L19.71 7.48L19.8 7.87L19.87 8.25L19.93 8.65L19.97 9.04L19.99 9.45L20 9.85L19.99 10.26L19.97 10.66L19.93 11.06L19.87 11.46L19.8 11.85L19.71 12.25L19.61 12.64L19.49 13.03L19.36 13.42L19.21 13.82L19.06 14.21L18.88 14.6L18.7 15L18.5 15.4L18.29 15.8L18.07 16.2L17.84 16.61L17.59 17.02L17.34 17.43L17.07 17.86L16.79 18.28L16.51 18.71L16.21 19.15L15.91 19.59L15.59 20.05L15.27 20.51L14.94 20.97L14.6 21.45L14.25 21.93L13.89 22.43L13.53 22.93L13.16 23.45L12.79 23.98L12.4 24.51L12.02 25.06L11.62 25.62L11.22 26.2L10.82 26.78L10.41 27.38L10 28L9.59 27.38L9.18 26.78L8.78 26.2L8.38 25.62L7.98 25.06L7.6 24.51L7.21 23.98L6.84 23.45L6.47 22.93L6.11 22.43L5.75 21.93L5.4 21.45L5.06 20.97L4.73 20.51L4.41 20.05L4.09 19.59L3.79 19.15L3.49 18.71L3.21 18.28L2.93 17.86L2.66 17.43L2.41 17.02L2.16 16.61L1.93 16.2L1.71 15.8L1.5 15.4L1.3 15L1.12 14.6L0.94 14.21L0.79 13.82L0.64 13.42L0.51 13.03L0.39 12.64L0.29 12.25L0.2 11.85L0.13 11.46L0.07 11.06L0.03 10.66L0.01 10.26L0 9.85L0.01 9.45L0.03 9.04L0.07 8.65L0.13 8.25L0.2 7.87L0.29 7.48L0.39 7.11L0.51 6.74L0.64 6.37L0.79 6.02L0.94 5.67L1.12 5.32L1.3 4.99L1.5 4.66L1.71 4.34L1.93 4.03L2.16 3.73L2.41 3.44L2.66 3.16L2.93 2.89L3.21 2.62L3.49 2.37L3.79 2.13L4.09 1.9L4.41 1.68L4.73 1.48L5.06 1.28L5.4 1.1L5.75 0.93L6.11 0.77L6.47 0.63L6.84 0.5L7.21 0.39L7.6 0.29L7.98 0.2L8.38 0.13L8.78 0.07L9.18 0.03L9.59 0.01L10 0L10.41 0.01L10.82 0.03ZM9.61 4.68L9.42 4.7L9.23 4.73L9.05 4.76L8.86 4.8L8.68 4.85L8.5 4.9L8.33 4.97L8.16 5.03L7.99 5.11L7.82 5.19L7.66 5.27L7.5 5.37L7.35 5.46L7.2 5.57L7.06 5.68L6.92 5.79L6.78 5.91L6.65 6.03L6.52 6.16L6.4 6.3L6.29 6.43L6.18 6.58L6.07 6.72L5.97 6.88L5.88 7.03L5.79 7.19L5.71 7.35L5.64 7.52L5.57 7.69L5.5 7.86L5.45 8.03L5.4 8.21L5.36 8.39L5.33 8.58L5.3 8.76L5.28 8.95L5.27 9.14L5.26 9.33L5.27 9.53L5.28 9.72L5.3 9.9L5.33 10.09L5.36 10.27L5.4 10.45L5.45 10.63L5.5 10.81L5.57 10.98L5.64 11.15L5.71 11.32L5.79 11.48L5.88 11.64L5.97 11.79L6.07 11.94L6.18 12.09L6.29 12.23L6.4 12.37L6.52 12.5L6.65 12.63L6.78 12.76L6.92 12.88L7.06 12.99L7.2 13.1L7.35 13.2L7.5 13.3L7.66 13.39L7.82 13.48L7.99 13.56L8.16 13.63L8.33 13.7L8.5 13.76L8.68 13.82L8.86 13.86L9.05 13.91L9.23 13.94L9.42 13.97L9.61 13.98L9.8 14L10 14L10.2 14L10.39 13.98L10.58 13.97L10.77 13.94L10.95 13.91L11.14 13.86L11.32 13.82L11.5 13.76L11.67 13.7L11.84 13.63L12.01 13.56L12.18 13.48L12.34 13.39L12.5 13.3L12.65 13.2L12.8 13.1L12.94 12.99L13.08 12.88L13.22 12.76L13.35 12.63L13.48 12.5L13.6 12.37L13.71 12.23L13.82 12.09L13.93 11.94L14.03 11.79L14.12 11.64L14.21 11.48L14.29 11.32L14.36 11.15L14.43 10.98L14.5 10.81L14.55 10.63L14.6 10.45L14.64 10.27L14.67 10.09L14.7 9.9L14.72 9.72L14.73 9.53L14.74 9.33L14.73 9.14L14.72 8.95L14.7 8.76L14.67 8.58L14.64 8.39L14.6 8.21L14.55 8.03L14.5 7.86L14.43 7.69L14.36 7.52L14.29 7.35L14.21 7.19L14.12 7.03L14.03 6.88L13.93 6.72L13.82 6.58L13.71 6.43L13.6 6.3L13.48 6.16L13.35 6.03L13.22 5.91L13.08 5.79L12.94 5.68L12.8 5.57L12.65 5.46L12.5 5.37L12.34 5.27L12.18 5.19L12.01 5.11L11.84 5.03L11.67 4.97L11.5 4.9L11.32 4.85L11.14 4.8L10.95 4.76L10.77 4.73L10.58 4.7L10.39 4.68L10.2 4.67L10 4.67L9.8 4.67L9.61 4.68Z\"></path></svg>");
                    pin = pinSvg.Children[0];
                    ((SvgText)previousPin.Children[1]).FontSize = 20;
                    w = 20;
                    h = 56;
                    break;
                case PinSize.Medium:
                default:
                    pinSvg = SvgDocument.FromSvg<SvgDocument>(
                "<svg><path d=\"M21.64 0.07L22.45 0.15L23.24 0.26L24.03 0.4L24.81 0.57L25.57 0.77L26.32 1L27.06 1.26L27.78 1.55L28.5 1.86L29.19 2.2L29.87 2.56L30.54 2.95L31.18 3.37L31.81 3.8L32.42 4.26L33.02 4.74L33.59 5.25L34.14 5.77L34.67 6.32L35.19 6.88L35.67 7.46L36.14 8.07L36.58 8.69L37 9.32L37.4 9.98L37.77 10.65L38.11 11.33L38.43 12.03L38.72 12.75L38.98 13.48L39.21 14.22L39.42 14.97L39.59 15.73L39.74 16.51L39.85 17.29L39.93 18.09L39.98 18.89L40 19.7L39.98 20.52L39.93 21.32L39.85 22.12L39.74 22.92L39.59 23.71L39.42 24.49L39.21 25.28L38.98 26.06L38.72 26.85L38.43 27.63L38.11 28.42L37.77 29.21L37.4 30L37 30.79L36.58 31.6L36.14 32.4L35.67 33.22L35.19 34.04L34.67 34.87L34.14 35.71L33.59 36.56L33.02 37.42L32.42 38.3L31.81 39.19L31.18 40.09L30.54 41.01L29.87 41.95L29.19 42.9L28.5 43.87L27.78 44.86L27.06 45.87L26.32 46.9L25.57 47.95L24.81 49.02L24.03 50.12L23.24 51.25L22.45 52.39L21.64 53.57L20.82 54.77L20 56L19.18 54.77L18.36 53.57L17.55 52.39L16.76 51.25L15.97 50.12L15.19 49.02L14.43 47.95L13.68 46.9L12.94 45.87L12.22 44.86L11.5 43.87L10.81 42.9L10.13 41.95L9.46 41.01L8.82 40.09L8.19 39.19L7.58 38.3L6.98 37.42L6.41 36.56L5.86 35.71L5.33 34.87L4.81 34.04L4.33 33.22L3.86 32.4L3.42 31.6L3 30.79L2.6 30L2.23 29.21L1.89 28.42L1.57 27.63L1.28 26.85L1.02 26.06L0.79 25.28L0.58 24.49L0.41 23.71L0.26 22.92L0.15 22.12L0.07 21.32L0.02 20.52L0 19.7L0.02 18.89L0.07 18.09L0.15 17.29L0.26 16.51L0.41 15.73L0.58 14.97L0.79 14.22L1.02 13.48L1.28 12.75L1.57 12.03L1.89 11.33L2.23 10.65L2.6 9.98L3 9.32L3.42 8.69L3.86 8.07L4.33 7.46L4.81 6.88L5.33 6.32L5.86 5.77L6.41 5.25L6.98 4.74L7.58 4.26L8.19 3.8L8.82 3.37L9.46 2.95L10.13 2.56L10.81 2.2L11.5 1.86L12.22 1.55L12.94 1.26L13.68 1L14.43 0.77L15.19 0.57L15.97 0.4L16.76 0.26L17.55 0.15L18.36 0.07L19.18 0.02L20 0L20.82 0.02L21.64 0.07ZM19.22 9.36L18.84 9.4L18.46 9.46L18.09 9.52L17.72 9.6L17.36 9.7L17.01 9.81L16.66 9.93L16.31 10.07L15.98 10.21L15.65 10.38L15.32 10.55L15.01 10.73L14.7 10.93L14.4 11.13L14.12 11.35L13.83 11.58L13.56 11.82L13.3 12.07L13.05 12.33L12.81 12.59L12.58 12.87L12.35 13.15L12.14 13.45L11.95 13.75L11.76 14.06L11.58 14.38L11.42 14.7L11.27 15.03L11.13 15.37L11.01 15.72L10.9 16.07L10.8 16.42L10.72 16.79L10.65 17.15L10.6 17.52L10.56 17.9L10.53 18.28L10.53 18.67L10.53 19.05L10.56 19.43L10.6 19.81L10.65 20.18L10.72 20.55L10.8 20.91L10.9 21.27L11.01 21.62L11.13 21.96L11.27 22.3L11.42 22.63L11.58 22.96L11.76 23.27L11.95 23.58L12.14 23.89L12.35 24.18L12.58 24.46L12.81 24.74L13.05 25.01L13.3 25.27L13.56 25.51L13.83 25.75L14.12 25.98L14.4 26.2L14.7 26.41L15.01 26.6L15.32 26.79L15.65 26.96L15.98 27.12L16.31 27.27L16.66 27.4L17.01 27.52L17.36 27.63L17.72 27.73L18.09 27.81L18.46 27.88L18.84 27.93L19.22 27.97L19.61 27.99L20 28L20.39 27.99L20.78 27.97L21.16 27.93L21.54 27.88L21.91 27.81L22.28 27.73L22.64 27.63L22.99 27.52L23.34 27.4L23.69 27.27L24.02 27.12L24.35 26.96L24.68 26.79L24.99 26.6L25.3 26.41L25.6 26.2L25.88 25.98L26.17 25.75L26.44 25.51L26.7 25.27L26.95 25.01L27.19 24.74L27.42 24.46L27.65 24.18L27.86 23.89L28.05 23.58L28.24 23.27L28.42 22.96L28.58 22.63L28.73 22.3L28.87 21.96L28.99 21.62L29.1 21.27L29.2 20.91L29.28 20.55L29.35 20.18L29.4 19.81L29.44 19.43L29.47 19.05L29.47 18.67L29.47 18.28L29.44 17.9L29.4 17.52L29.35 17.15L29.28 16.79L29.2 16.42L29.1 16.07L28.99 15.72L28.87 15.37L28.73 15.03L28.58 14.7L28.42 14.38L28.24 14.06L28.05 13.75L27.86 13.45L27.65 13.15L27.42 12.87L27.19 12.59L26.95 12.33L26.7 12.07L26.44 11.82L26.17 11.58L25.88 11.35L25.6 11.13L25.3 10.93L24.99 10.73L24.68 10.55L24.35 10.38L24.02 10.21L23.69 10.07L23.34 9.93L22.99 9.81L22.64 9.7L22.28 9.6L21.91 9.52L21.54 9.46L21.16 9.4L20.78 9.36L20.39 9.34L20 9.33L19.61 9.34L19.22 9.36Z\"></path></svg>");
                    pin = pinSvg.Children[0];
                    ((SvgText)previousPin.Children[1]).FontSize = 20;
                    w = 20;
                    h = 56;
                    break;
                case PinSize.Large:
                    pinSvg = SvgDocument.FromSvg<SvgDocument>(
                "<svg><path d=\"M32.46 0.1L33.67 0.22L34.87 0.39L36.05 0.6L37.21 0.86L38.36 1.16L39.48 1.51L40.59 1.89L41.68 2.32L42.74 2.79L43.79 3.3L44.81 3.84L45.8 4.43L46.77 5.05L47.72 5.7L48.63 6.39L49.52 7.11L50.38 7.87L51.21 8.66L52.01 9.47L52.78 10.32L53.51 11.2L54.21 12.1L54.88 13.03L55.51 13.99L56.1 14.97L56.65 15.97L57.17 17L57.64 18.05L58.08 19.12L58.47 20.21L58.82 21.32L59.13 22.45L59.39 23.6L59.61 24.76L59.78 25.94L59.9 27.13L59.97 28.34L60 29.56L59.97 30.77L59.9 31.98L59.78 33.18L59.61 34.37L59.39 35.56L59.13 36.74L58.82 37.92L58.47 39.1L58.08 40.27L57.64 41.45L57.17 42.63L56.65 43.81L56.1 45L55.51 46.19L54.88 47.39L54.21 48.6L53.51 49.82L52.78 51.06L52.01 52.3L51.21 53.57L50.38 54.84L49.52 56.14L48.63 57.45L47.72 58.78L46.77 60.14L45.8 61.52L44.81 62.92L43.79 64.35L42.74 65.8L41.68 67.29L40.59 68.8L39.48 70.35L38.36 71.93L37.21 73.54L36.05 75.18L34.87 76.87L33.67 78.59L32.46 80.35L31.24 82.15L30 84L28.76 82.15L27.54 80.35L26.33 78.59L25.13 76.87L23.95 75.18L22.79 73.54L21.64 71.93L20.52 70.35L19.41 68.8L18.32 67.29L17.26 65.8L16.21 64.35L15.19 62.92L14.2 61.52L13.23 60.14L12.28 58.78L11.37 57.45L10.48 56.14L9.62 54.84L8.79 53.57L7.99 52.3L7.22 51.06L6.49 49.82L5.79 48.6L5.12 47.39L4.49 46.19L3.9 45L3.35 43.81L2.83 42.63L2.36 41.45L1.92 40.27L1.53 39.1L1.18 37.92L0.87 36.74L0.61 35.56L0.39 34.37L0.22 33.18L0.1 31.98L0.03 30.77L0 29.56L0.03 28.34L0.1 27.13L0.22 25.94L0.39 24.76L0.61 23.6L0.87 22.45L1.18 21.32L1.53 20.21L1.92 19.12L2.36 18.05L2.83 17L3.35 15.97L3.9 14.97L4.49 13.99L5.12 13.03L5.79 12.1L6.49 11.2L7.22 10.32L7.99 9.47L8.79 8.66L9.62 7.87L10.48 7.11L11.37 6.39L12.28 5.7L13.23 5.05L14.2 4.43L15.19 3.84L16.21 3.3L17.26 2.79L18.32 2.32L19.41 1.89L20.52 1.51L21.64 1.16L22.79 0.86L23.95 0.6L25.13 0.39L26.33 0.22L27.54 0.1L28.76 0.02L30 0L31.24 0.02L32.46 0.1ZM28.83 14.05L28.26 14.1L27.69 14.18L27.14 14.28L26.59 14.41L26.04 14.55L25.51 14.71L24.98 14.9L24.47 15.1L23.96 15.32L23.47 15.56L22.99 15.82L22.51 16.1L22.05 16.39L21.61 16.7L21.17 17.03L20.75 17.37L20.34 17.73L19.95 18.1L19.57 18.49L19.21 18.89L18.86 19.3L18.53 19.73L18.22 20.17L17.92 20.63L17.64 21.09L17.38 21.57L17.13 22.05L16.91 22.55L16.7 23.06L16.51 23.57L16.35 24.1L16.2 24.64L16.08 25.18L15.98 25.73L15.89 26.29L15.84 26.85L15.8 27.42L15.79 28L15.8 28.58L15.84 29.15L15.89 29.71L15.98 30.27L16.08 30.82L16.2 31.36L16.35 31.9L16.51 32.43L16.7 32.94L16.91 33.45L17.13 33.95L17.38 34.43L17.64 34.91L17.92 35.37L18.22 35.83L18.53 36.27L18.86 36.7L19.21 37.11L19.57 37.51L19.95 37.9L20.34 38.27L20.75 38.63L21.17 38.97L21.61 39.3L22.05 39.61L22.51 39.9L22.99 40.18L23.47 40.44L23.96 40.68L24.47 40.9L24.98 41.1L25.51 41.29L26.04 41.45L26.59 41.59L27.14 41.72L27.69 41.82L28.26 41.9L28.83 41.95L29.41 41.99L30 42L30.59 41.99L31.17 41.95L31.74 41.9L32.31 41.82L32.86 41.72L33.41 41.59L33.96 41.45L34.49 41.29L35.02 41.1L35.53 40.9L36.04 40.68L36.53 40.44L37.01 40.18L37.49 39.9L37.95 39.61L38.39 39.3L38.83 38.97L39.25 38.63L39.66 38.27L40.05 37.9L40.43 37.51L40.79 37.11L41.14 36.7L41.47 36.27L41.78 35.83L42.08 35.37L42.36 34.91L42.62 34.43L42.87 33.95L43.09 33.45L43.3 32.94L43.49 32.43L43.65 31.9L43.8 31.36L43.92 30.82L44.02 30.27L44.11 29.71L44.16 29.15L44.2 28.58L44.21 28L44.2 27.42L44.16 26.85L44.11 26.29L44.02 25.73L43.92 25.18L43.8 24.64L43.65 24.1L43.49 23.57L43.3 23.06L43.09 22.55L42.87 22.05L42.62 21.57L42.36 21.09L42.08 20.63L41.78 20.17L41.47 19.73L41.14 19.3L40.79 18.89L40.43 18.49L40.05 18.1L39.66 17.73L39.25 17.37L38.83 17.03L38.39 16.7L37.95 16.39L37.49 16.1L37.01 15.82L36.53 15.56L36.04 15.32L35.53 15.1L35.02 14.9L34.49 14.71L33.96 14.55L33.41 14.41L32.86 14.28L32.31 14.18L31.74 14.1L31.17 14.05L30.59 14.01L30 14L29.41 14.01L28.83 14.05Z\"></path></svg>");
                    pin = pinSvg.Children[0];
                    ((SvgText)previousPin.Children[1]).FontSize = 30;
                    w = 30;
                    h = 84;
                    break;
                case PinSize.ExtraLarge:
                    pinSvg = SvgDocument.FromSvg<SvgDocument>(
                "<svg><path d=\"M43.28 0.13L44.89 0.29L46.49 0.52L48.06 0.8L49.61 1.15L51.14 1.55L52.64 2.01L54.12 2.53L55.57 3.1L56.99 3.72L58.38 4.4L59.74 5.13L61.07 5.9L62.36 6.73L63.62 7.6L64.85 8.52L66.03 9.49L67.18 10.49L68.28 11.54L69.35 12.63L70.37 13.76L71.35 14.93L72.28 16.13L73.17 17.37L74.01 18.65L74.8 19.96L75.54 21.3L76.22 22.67L76.86 24.07L77.44 25.5L77.96 26.95L78.43 28.43L78.84 29.94L79.19 31.47L79.48 33.02L79.7 34.59L79.87 36.18L79.97 37.78L80 39.41L79.97 41.03L79.87 42.64L79.7 44.24L79.48 45.83L79.19 47.41L78.84 48.99L78.43 50.56L77.96 52.13L77.44 53.7L76.86 55.27L76.22 56.84L75.54 58.41L74.8 60L74.01 61.59L73.17 63.19L72.28 64.8L71.35 66.43L70.37 68.08L69.35 69.74L68.28 71.42L67.18 73.12L66.03 74.85L64.85 76.6L63.62 78.38L62.36 80.19L61.07 82.02L59.74 83.89L58.38 85.8L56.99 87.74L55.57 89.72L54.12 91.74L52.64 93.8L51.14 95.9L49.61 98.05L48.06 100.25L46.49 102.49L44.89 104.79L43.28 107.14L41.65 109.54L40 112L38.35 109.54L36.72 107.14L35.11 104.79L33.51 102.49L31.94 100.25L30.39 98.05L28.86 95.9L27.36 93.8L25.88 91.74L24.43 89.72L23.01 87.74L21.62 85.8L20.26 83.89L18.93 82.02L17.64 80.19L16.38 78.38L15.15 76.6L13.97 74.85L12.82 73.12L11.72 71.42L10.65 69.74L9.63 68.08L8.65 66.43L7.72 64.8L6.83 63.19L5.99 61.59L5.2 60L4.46 58.41L3.78 56.84L3.14 55.27L2.56 53.7L2.04 52.13L1.57 50.56L1.16 48.99L0.81 47.41L0.52 45.83L0.3 44.24L0.13 42.64L0.03 41.03L0 39.41L0.03 37.78L0.13 36.18L0.3 34.59L0.52 33.02L0.81 31.47L1.16 29.94L1.57 28.43L2.04 26.95L2.56 25.5L3.14 24.07L3.78 22.67L4.46 21.3L5.2 19.96L5.99 18.65L6.83 17.37L7.72 16.13L8.65 14.93L9.63 13.76L10.65 12.63L11.72 11.54L12.82 10.49L13.97 9.49L15.15 8.52L16.38 7.6L17.64 6.73L18.93 5.9L20.26 5.13L21.62 4.4L23.01 3.72L24.43 3.1L25.88 2.53L27.36 2.01L28.86 1.55L30.39 1.15L31.94 0.8L33.51 0.52L35.11 0.29L36.72 0.13L38.35 0.03L40 0L41.65 0.03L43.28 0.13ZM38.45 18.73L37.68 18.8L36.93 18.91L36.18 19.05L35.45 19.21L34.72 19.4L34.01 19.62L33.31 19.86L32.62 20.13L31.95 20.43L31.29 20.75L30.65 21.1L30.02 21.46L29.41 21.85L28.81 22.27L28.23 22.7L27.67 23.16L27.13 23.64L26.6 24.13L26.1 24.65L25.61 25.19L25.15 25.74L24.71 26.31L24.29 26.9L23.89 27.5L23.52 28.12L23.17 28.75L22.84 29.4L22.54 30.07L22.27 30.74L22.02 31.43L21.8 32.13L21.6 32.85L21.44 33.57L21.3 34.31L21.19 35.05L21.12 35.8L21.07 36.56L21.05 37.33L21.07 38.1L21.12 38.86L21.19 39.62L21.3 40.36L21.44 41.1L21.6 41.82L21.8 42.53L22.02 43.23L22.27 43.92L22.54 44.6L22.84 45.26L23.17 45.91L23.52 46.55L23.89 47.17L24.29 47.77L24.71 48.36L25.15 48.93L25.61 49.48L26.1 50.02L26.6 50.53L27.13 51.03L27.67 51.51L28.23 51.96L28.81 52.4L29.41 52.81L30.02 53.2L30.65 53.57L31.29 53.92L31.95 54.24L32.62 54.53L33.31 54.8L34.01 55.05L34.72 55.27L35.45 55.46L36.18 55.62L36.93 55.76L37.68 55.86L38.45 55.94L39.22 55.98L40 56L40.78 55.98L41.55 55.94L42.32 55.86L43.07 55.76L43.82 55.62L44.55 55.46L45.28 55.27L45.99 55.05L46.69 54.8L47.38 54.53L48.05 54.24L48.71 53.92L49.35 53.57L49.98 53.2L50.59 52.81L51.19 52.4L51.77 51.96L52.33 51.51L52.87 51.03L53.4 50.53L53.9 50.02L54.39 49.48L54.85 48.93L55.29 48.36L55.71 47.77L56.11 47.17L56.48 46.55L56.83 45.91L57.16 45.26L57.46 44.6L57.73 43.92L57.98 43.23L58.2 42.53L58.4 41.82L58.56 41.1L58.7 40.36L58.81 39.62L58.88 38.86L58.93 38.1L58.95 37.33L58.93 36.56L58.88 35.8L58.81 35.05L58.7 34.31L58.56 33.57L58.4 32.85L58.2 32.13L57.98 31.43L57.73 30.74L57.46 30.07L57.16 29.4L56.83 28.75L56.48 28.12L56.11 27.5L55.71 26.9L55.29 26.31L54.85 25.74L54.39 25.19L53.9 24.65L53.4 24.13L52.87 23.64L52.33 23.16L51.77 22.7L51.19 22.27L50.59 21.85L49.98 21.46L49.35 21.1L48.71 20.75L48.05 20.43L47.38 20.13L46.69 19.86L45.99 19.62L45.28 19.4L44.55 19.21L43.82 19.05L43.07 18.91L42.32 18.8L41.55 18.73L40.78 18.68L40 18.67L39.22 18.68L38.45 18.73Z\"></path></svg>");
                    pin = pinSvg.Children[0];
                    ((SvgText)previousPin.Children[1]).FontSize = 40;
                    w = 40;
                    h = 112;
                    break;
            }

            previousPin.Children[0] = pin;

            previousPin.Children[0].Transforms.Add(new SvgTranslate(-w, -h));

            previousPin.CustomAttributes[PinFillAttributeKey] = "Holey";

            return previousPin;
        }

		#endregion

		#region Inner types
		public enum PinSize
        {
            Small,
            Medium,
            Large,
            ExtraLarge
        };

        public enum PinFill
        {
            Holey,
            Filled
        };

        private class ChangePinSizeCommand : ToolCommand
        {
            private readonly ISvgDrawingCanvas _canvas;

            public ChangePinSizeCommand(ISvgDrawingCanvas canvas, PinTool tool, string name, string description = null)
                : base(tool, name, o => { }, iconName: tool.PinResizeIconName, sortFunc: tc => 500, description: description)
            {
                _canvas = canvas;
            }

            private new PinTool Tool => (PinTool)base.Tool;

            public override async void Execute(object parameter)
            {
                var t = Tool;
                PinSize selectedSize;

                if (_canvas.SelectedElements.Any())
                {
                    var selectedElement = _canvas.SelectedElements[0];
                    selectedSize = await t.GetPinSizeFromUserInput(selectedElement.CustomAttributes[PinSizeAttributeKey]);

                    t.UndoRedoService.ExecuteCommand(new UndoableActionCommand("Change size of selected elements", o => { }));

                    t.ChangePinSize(selectedElement, selectedSize);
                    // don't change the global color when items are selected
                    return;
                }

                selectedSize = await t.GetPinSizeFromUserInput();

                var formerSize = t.SelectedPinSize;
                t.UndoRedoService.ExecuteCommand(new UndoableActionCommand(Name, o =>
                {
                    t.SelectedPinSize = selectedSize;
                    t.Canvas.FireToolCommandsChanged();
                }, o =>
                {
                    t.SelectedPinSize = formerSize;
                    t.Canvas.FireToolCommandsChanged();
                }));
            }

            public override bool CanExecute(object parameter)
            {
                return Tool.IsActive;
            }
        }

        #endregion
    }
}
