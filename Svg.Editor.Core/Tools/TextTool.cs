using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Svg.Editor.Extensions;
using Svg.Editor.Gestures;
using Svg.Editor.Interfaces;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;
using Svg.Transforms;

namespace Svg.Editor.Tools
{
    public class TextTool : UndoableToolBase
	{
		#region Private fields

		private bool _dialogShown;
		private ITextInputService TextInputService { get; set; }

		#endregion

		#region Public properties

		public const string FontSizesKey = "fontsizes";
		public const string FontSizeNamesKey = "fontsizenames";
		public const string SelectedFontSizeIndexKey = "selectedfontsizeindex";

		public override int InputOrder => 300;

		public float[] FontSizes
		{
			get
			{
				object fontSizes;
				if (!Properties.TryGetValue(FontSizesKey, out fontSizes))
					fontSizes = Enumerable.Empty<float>();
				return (float[]) fontSizes;
			}
		}

		public int SelectedFontSizeIndex
		{
			get
			{
				object selectedFontSizeIndex;
				return Properties.TryGetValue(SelectedFontSizeIndexKey, out selectedFontSizeIndex)
					? Convert.ToInt32(selectedFontSizeIndex)
					: 0;
			}
			set { Properties[SelectedFontSizeIndexKey] = value; }
		}

		public string[] FontSizeNames
		{
			get
			{
				object fontSizeNames;
				if (!Properties.TryGetValue(FontSizeNamesKey, out fontSizeNames))
					fontSizeNames = Enumerable.Empty<string>();
				return (string[]) fontSizeNames;
			}
		}

		#endregion

		public TextTool(IDictionary<string, object> properties, IUndoRedoService undoRedoService) : base("Text", properties,
			undoRedoService)
		{
			IconName = "ic_text_fields.svg";
			ToolUsage = ToolUsage.Explicit;
			ToolType = ToolType.Create;
		}

		#region Overrides

		public override async Task Initialize(ISvgDrawingCanvas ws)
		{
			await base.Initialize(ws);

			IsActive = false;

			Canvas.DefaultEditors.Add(async element =>
			{
				var svgText = element as SvgTextBase ?? element.Descendants().OfType<SvgTextBase>().FirstOrDefault();

				if (svgText == null
				    || svgText.HasConstraints(ImmutableTextConstraint)
				    || svgText.Descendants().OfType<SvgTextBase>().Any(x => x.HasConstraints(ImmutableTextConstraint)))
					return;

				await ChangeText(svgText);
			});

			TextInputService = SvgEngine.TryResolve<ITextInputService>();

			if (TextInputService == null)
				throw new InvalidOperationException("Text input service needs to be registered before initializing this tool.");
		}

		protected override async Task OnTap(TapGesture tap)
		{
			await base.OnTap(tap);

			if (!IsActive) return;

			// if there is text below the pointer, edit it
			var svgText = Canvas.GetElementsUnderPointer<SvgTextBase>(tap.Position, SelectionType.IntersectBoundingBoxes).FirstOrDefault();

			if (svgText != null)
			{
				if (svgText.HasConstraints(ImmutableTextConstraint)
				    || svgText.Descendants().OfType<SvgTextBase>().Any(x => x.HasConstraints(ImmutableTextConstraint)))
					return;

				await ChangeText(svgText);
			}
			// else add new text   
			else
			{
				var txtProperties =
					await GetTextPropertiesFromUserInput("Add text", null, SelectedFontSizeIndex);
				if (txtProperties == null) return;

				var txt = txtProperties.Text;
				var fontSize = FontSizes[txtProperties.FontSizeIndex];
				var lineHeight = txtProperties.LineHeight;

				CreateSvgText(txt, fontSize, lineHeight, tap.Position);
			}
		}

		protected override async Task OnDoubleTap(DoubleTapGesture doubleTap)
		{
			await base.OnDoubleTap(doubleTap);

			if (Canvas.ActiveTool.ToolType != ToolType.Select) return;

			// determine if pointer was put down on a text
			var svgText = Canvas.GetElementsUnderPointer<SvgTextBase>(doubleTap.Position, SelectionType.IntersectBoundingBoxes).FirstOrDefault();

			if (svgText == null
			    || svgText.HasConstraints(ImmutableTextConstraint)
			    || svgText.Descendants().OfType<SvgTextBase>().Any(x => x.HasConstraints(ImmutableTextConstraint)))
				return;

			await ChangeText(svgText);
		}

		#endregion

		#region Private helpers

		private async Task<TextProperties> GetTextPropertiesFromUserInput(string title, string text, int index)
		{
			if (_dialogShown) return null;

			TextProperties txtProperties;
			try
			{
                _dialogShown = true;
				txtProperties = await TextInputService.GetUserInput(title, text, FontSizeNames, index);
			}
			catch (TaskCanceledException)
			{
				txtProperties = null;
			}
            finally
            {
                _dialogShown = false;
            }

			if (txtProperties == null)
				return null;

			SelectedFontSizeIndex = txtProperties.FontSizeIndex;

			return txtProperties;
		}

		private void CreateSvgText(string txt, float fontSize, float lineHeight, PointF position)
		{
			// only add if user really entered text.
			if (string.IsNullOrWhiteSpace(txt)) return;

			var svgText = new SvgText
			{
				FontSize = new SvgUnit(SvgUnitType.Pixel, fontSize),
				Stroke = new SvgColourServer(SvgEngine.Factory.CreateColorFromArgb(255, 0, 0, 0)),
				Fill = new SvgColourServer(SvgEngine.Factory.CreateColorFromArgb(255, 0, 0, 0))
			};

			var lines = txt.Split('\n', '\r');
			if (lines.Length > 1)
			{
				var spans = lines.Select(
					(t, i) =>
						new SvgTextSpan
						{
							Nodes = {new SvgContentNode {Content = t}},
							X = new SvgUnitCollection {0},
							Y = new SvgUnitCollection {fontSize * lineHeight * i}
						});
				foreach (var span in spans)
				{
					svgText.Children.Add(span);
				}
			}
			else
			{
				svgText.Text = lines.First();
			}

			var relativePosition = Canvas.ScreenToCanvas(position);
			var childBounds = svgText.Bounds;
			var halfRelChildWidth = childBounds.Width / 2;
			var halfRelChildHeight = childBounds.Height / 2;

			var x = relativePosition.X - halfRelChildWidth;
			var y = relativePosition.Y - halfRelChildHeight;
			svgText.Transforms.Add(new SvgTranslate(x, y));

			UndoRedoService.ExecuteCommand(new UndoableActionCommand("Add text", o =>
			{
				Canvas.Document.Children.Add(svgText);
				Canvas.FireInvalidateCanvas();
			}, o =>
			{
				Canvas.Document.Children.Remove(svgText);
				Canvas.FireInvalidateCanvas();
			}));
		}

		private async Task ChangeText(SvgTextBase svgText)
		{
			// joining the spans as newlines
			var text = !string.IsNullOrWhiteSpace(svgText.Text)
				? svgText.Text
				: string.Join(Environment.NewLine, svgText.Children.OfType<SvgTextSpan>().Select(x => x.Text));

			var txtProperties =
				await
					GetTextPropertiesFromUserInput("Edit text", text,
						Array.IndexOf(FontSizes, (int) Math.Round(svgText.FontSize, 0)));
			if (txtProperties == null) return;

			var txt = txtProperties.Text;
			var fontSize = FontSizes[txtProperties.FontSizeIndex];
			var lineHeight = txtProperties.LineHeight;

			EditSvgText(svgText, txt, fontSize, lineHeight);
		}

		private void EditSvgText(SvgTextBase svgText, string text, float fontSize, float lineHeight)
		{
			// make sure there is at least empty text in it so we actually still have a bounding box!!
			if (string.IsNullOrEmpty(text?.Trim()))
				text = "  ";

			// if text was removed, and parent was document, remove element
			// if parent was not the document, then this would be a text within another group and should not be removed
			if (string.IsNullOrWhiteSpace(text) && svgText.Parent is SvgDocument)
			{
				var parent = svgText.Parent;
				UndoRedoService.ExecuteCommand(new UndoableActionCommand("Remove text", o =>
				{
					parent.Children.Remove(svgText);
					Canvas.FireInvalidateCanvas();
				}, o =>
				{
					parent.Children.Add(svgText);
					Canvas.FireInvalidateCanvas();
				}));

				return;
			}

			if ((text == svgText.Text || text == svgText.Children.OfType<SvgTextSpan>().FirstOrDefault()?.Text) &&
			    Math.Abs(svgText.FontSize.Value - fontSize) < 0.1f) return;

			var formerText = svgText.Text;
			var formerChildren = svgText.Children.OfType<SvgTextSpan>()
				.Select(span =>
					new SvgTextSpan
					{
						Nodes = {new SvgContentNode {Content = span.Text}},
						X = span.X,
						Y = span.Y,
						TextAnchor = span.TextAnchor,
						SpaceHandling = span.SpaceHandling
					})
				.ToArray();
			var formerFontSize = svgText.FontSize;
			UndoRedoService.ExecuteCommand(new UndoableActionCommand("Edit text", o =>
			{
				var lines = text.Split('\n', '\r');
				// if we have more lines, we need to put each in a different span
				if (lines.Length > 1)
				{
					svgText.Text = null;
					var origin = svgText.Children.OfType<SvgTextSpan>().FirstOrDefault() ?? svgText;
					var spans = lines.Select((t, i) =>
						new SvgTextSpan
						{
							Nodes = {new SvgContentNode {Content = t}},
							X = origin.X,
							Y =
								new SvgUnitCollection
								{
									origin.Y.FirstOrDefault() + fontSize * lineHeight * i
								},
							TextAnchor = origin.TextAnchor,
							SpaceHandling = origin.SpaceHandling
						});

					// add spans as children
					svgText.Children.Clear();
					foreach (var span in spans)
					{
						svgText.Children.Add(span);
					}
				}
				// else we can just set the text accordingly
				else
				{
					var span = svgText.Children.OfType<SvgTextSpan>().FirstOrDefault();
					if (span != null)
					{
						span.Text = lines.First();
						span.FontSize = new SvgUnit(SvgUnitType.Pixel, fontSize);
						svgText.Children.Clear();
						svgText.Children.Add(span);
					}
					else
					{
						svgText.Text = lines.First();
					}
				}
				svgText.FontSize = new SvgUnit(SvgUnitType.Pixel, fontSize);
				Canvas.FireInvalidateCanvas();
			}, o =>
			{
				svgText.Text = formerText;
				svgText.Children.Clear();
				foreach (var child in formerChildren)
				{
					svgText.Children.Add(child);
				}
				svgText.FontSize = formerFontSize;
				Canvas.FireInvalidateCanvas();
			}));
		}

		#endregion

		#region Inner types

		public class TextProperties
		{
			public string Text { get; set; }
			public int FontSizeIndex { get; set; }
			public float LineHeight { get; set; } = 1.25f;
		}

		#endregion
	}
}