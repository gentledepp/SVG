using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Plugins.File;
using Svg.Editor.Interfaces;
using Svg.Editor.Sample.Core.Tools;
using Svg.Editor.Tools;
using Svg.Interfaces;

namespace Svg.Editor.Sample.Core.ViewModels
{
	public class EditorViewModel
		: MvxViewModel
	{
		public SvgDrawingCanvas Canvas { get; set; }

		public IMvxCommand SaveToolPropertiesCommand { get; }

		public EditorViewModel()
		{
			SaveToolPropertiesCommand = new MvxCommand(SaveToolProperties);
		}

		private const string Path = "toolProperties.txt";

		private void SaveToolProperties()
		{
			var toolProperties = Canvas?.GetToolPropertiesJson();
			var content = Encoding.UTF8.GetBytes(toolProperties);
			var fileSystem = Mvx.Resolve<IMvxFileStore>();
			if (fileSystem.Exists(Path))
				fileSystem.DeleteFile(Path);
			using (var stream = fileSystem.OpenWrite(Path))
				stream.Write(content, 0, content.Length);
		}

		public void Init()
		{
			var toolProperties = ReadToolProperties();

			SvgEngine.Register<IPickImageService>(() => new MvxPickImageService());

			#region Register tools

			// this part should be in the designer, when the iCL is created
			var gridToolProperties = new Dictionary<string, object>
			{
				{GridTool.AlphaAngleKey, 30.0f},
				{GridTool.StepSizeYKey, 20.0f},
				{GridTool.IsSnappingEnabledKey, true}
			};

			var colorToolProperties = new Dictionary<string, object>
			{
				{
					ColorTool.SelectableColorsKey,
					new[] {"#000000", "#FF0000", "#00FF00", "#0000FF", "#FFFF00", "#FF00FF", "#00FFFF", "#FFFFFF"}
				},
				{ColorTool.SelectableColorNamesKey, new[] {"Black", "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White"}}
			};

			var strokeStyleToolProperties = new Dictionary<string, object>
			{
				{StrokeStyleTool.SelectedStrokeDashIndexKey, 0},
				{StrokeStyleTool.StrokeDashesKey, new[] {"none", "3 3", "17 17", "34 34"}},
				{StrokeStyleTool.StrokeDashNamesKey, new[] {"----------", "- - - - - -", "--  --  --", "---   ---"}},
				{StrokeStyleTool.SelectedStrokeWidthIndexKey, 1},
				{StrokeStyleTool.StrokeWidthsKey, new[] {2, 6, 12, 24}},
				{StrokeStyleTool.StrokeWidthNamesKey, new[] {"thin", "normal", "thick", "ultra-thick"}}
			};

			var lineToolProperties = new Dictionary<string, object>
			{
				{LineTool.MarkerStartIdsKey, new[] {"none", "arrowStart", "circle"}},
				{LineTool.MarkerStartNamesKey, new[] {"---", "<--", "O--"}},
				{LineTool.MarkerEndIdsKey, new[] {"none", "arrowEnd", "circle"}},
				{LineTool.MarkerEndNamesKey, new[] {"---", "-->", "--O"}},
				{LineTool.SelectedMarkerEndIndexKey, 1},
				{LineTool.DefaultStrokeWidthKey, 2}
			};

			var freeDrawToolProperties = new Dictionary<string, object>
			{
				{FreeDrawingTool.DefaultStrokeWidthKey, 12}
			};

			var textToolProperties = new Dictionary<string, object>
			{
				{TextTool.FontSizesKey, new[] {12f, 16f, 20f, 24f, 36f, 48f}},
				{TextTool.SelectedFontSizeIndexKey, 1},
				{TextTool.FontSizeNamesKey, new[] {"12px", "16px", "20px", "24px", "36px", "48px"}}
			};

			//var zoomToolProperties = JsonConvert.SerializeObject(new Dictionary<string, object>
			//{
			//    { "minscale", 1.0f },
			//    { "maxscale", 5.0f }
			//}, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All });

			var zoomToolProperties = new Dictionary<string, object>();

			var panToolProperties = new Dictionary<string, object>();

			//var fs = SvgEngine.Resolve<IFileSystem>();
			//var path = fs.PathCombine(fs.GetDefaultStoragePath(), "background.png");

			var placeAsBackgroundToolProperties = new Dictionary<string, object>
			{
				//{ PlaceAsBackgroundTool.ImagePathKey, path },
				{PlaceAsBackgroundTool.ChooseBackgroundEnabledKey, true}
			};

			var rotationToolProperties = new Dictionary<string, object>
			{
				{RotationTool.RotationStepKey, 45.0f},
				{RotationTool.FilterKey, (Func<SvgVisualElement, bool>) (e => e is SvgTextBase)}
			};

			var undoRedoService = SvgEngine.Resolve<IUndoRedoService>();

			//SvgEngine.Register(() => new ToolFactoryProvider(new Func<ITool>[]
			//{
			//	() => new GridTool(gridToolProperties, undoRedoService),
			//	() => new MoveTool(undoRedoService),
			//	() => new PanTool(panToolProperties),
			//	() => new RotationTool(rotationToolProperties, undoRedoService),
			//	() => new ZoomTool(zoomToolProperties),
			//	() => new SelectionTool(undoRedoService),
			//	() => new TextTool(textToolProperties, undoRedoService),
			//	() => new LineTool(lineToolProperties, undoRedoService),
			//	() => new EllipseTool(null, undoRedoService),
			//	() => new FreeDrawingTool(freeDrawToolProperties, undoRedoService),
			//	() => new ColorTool(colorToolProperties, undoRedoService),
			//	() => new StrokeStyleTool(strokeStyleToolProperties, undoRedoService),
			//	() => new UndoRedoTool(undoRedoService),
			//	() => new ArrangeTool(undoRedoService),
			//	() => new AuxiliaryLineTool(),
			//	() => new SaveTool(false),
			//	() => new PlaceAsBackgroundTool(placeAsBackgroundToolProperties, undoRedoService),
			//	() => new AddRandomItemTool() {SourceProvider = GetSource}
			//}));

			#endregion

			Canvas = new SvgDrawingCanvas {IsDebugEnabled = true};

			Canvas.LoadTools(toolProperties, () => new GridTool(gridToolProperties, undoRedoService), () => new MoveTool(undoRedoService),
				() => new PanTool(panToolProperties), () => new RotationTool(rotationToolProperties, undoRedoService),
				() => new ZoomTool(zoomToolProperties), () => new SelectionTool(undoRedoService),
				() => new TextTool(textToolProperties, undoRedoService), () => new LineTool(lineToolProperties, undoRedoService),
				() => new EllipseTool(null, undoRedoService), () => new FreeDrawingTool(freeDrawToolProperties, undoRedoService),
				() => new ColorTool(colorToolProperties, undoRedoService),
				() => new StrokeStyleTool(strokeStyleToolProperties, undoRedoService), () => new UndoRedoTool(undoRedoService),
				() => new ArrangeTool(undoRedoService), () => new AuxiliaryLineTool(), () => new SaveTool(false),
				() => new PlaceAsBackgroundTool(placeAsBackgroundToolProperties, undoRedoService),
				() => new AddRandomItemTool {SourceProvider = GetSource});

			//Canvas.Document = SvgDocument.Open<SvgDocument>(GetSource("svg/svg_80ae394472b24f3daaaca4d067656058_78c.svg"));
			//Canvas.Document = SvgDocument.Open<SvgDocument>(GetSource("svg/ground_floor_plan.svg"));
		}

		private static string ReadToolProperties()
		{
			var fileSystem = Mvx.Resolve<IMvxFileStore>();
			if (fileSystem.Exists(Path))
				using (var stream = fileSystem.OpenRead(Path))
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					return reader.ReadToEnd();
				}
			return null;
		}

		private ISvgSource GetSource(string source)
		{
			return SvgEngine.Resolve<ISvgSourceFactory>().Create(source);
		}
	}
}