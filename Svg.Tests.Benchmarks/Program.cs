using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Svg.DeepZoom;
using Svg.Transforms;
using System.IO.Compression;

namespace Svg.Tests.Benchmarks;

public class Programm
{
    ///the path is hardcoded, you should change it to your own path where you have the svg and where you want to save the png and zip files.
    /// copy assets folder from Svg.Tests.Benchmarks project to your desktop and change the path accordingly
    private const string yourFullPath = "C:\\Users\\zepr2\\Desktop\\Assets\\";

    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run(typeof(BenchmarkSvg).Assembly);
    }


    [RPlotExporter, MarkdownExporter, AsciiDocExporter, HtmlExporter]
    public class BenchmarkSvg
    {

        [GlobalSetup]
        public async Task SetUp()
        {
            SvgPlatform.Init();
        }

        [Benchmark]
        public void LoadSvg()
        {
            SvgPlatform.Init();
            var doc = SvgDocument.Open(yourFullPath + "svgPlan2.svg");
            var bitmap = doc.Draw();
        }

        [Benchmark]
        public void RenderSvgOften()
        {
            SvgPlatform.Init();
            var doc = SvgDocument.Open(yourFullPath + "svgPlan2.svg");
            for (int i = 0; i <= 15; i++)
                doc.Draw();
        }

        [Benchmark]
        public async Task LoadTiles()
        {
            SvgPlatform.Init();
            var gen = new TileGenerator();
            var svgDoc = SvgDocument.Open(yourFullPath + "svgPlan2.svg");

            int targetWidth = 7680 / 2;
            int targetHeight = (int)(targetWidth * (svgDoc.Height / svgDoc.Width)); // keep aspect ratio

            var docWidth = svgDoc.Width;
            var docHeight = svgDoc.Height;
            var scale = Math.Max(targetWidth / docWidth, targetHeight / docHeight);

            svgDoc.Width = targetWidth;
            svgDoc.Height = targetHeight;
            svgDoc.ViewBox = null;
            svgDoc.Transforms.Add(new SvgScale(scale));
            var bitmap = svgDoc.Draw();
            var file = File.OpenWrite(yourFullPath + "imagePlan2.png");
            bitmap.SavePng(file);
            file.Close();

            var outPutZiFile = yourFullPath + "TilesStream.zip";
            if (File.Exists(outPutZiFile))
                File.Delete(outPutZiFile);
            using (var zipFileStream = File.OpenWrite(outPutZiFile))
            {
                using var archive = new ZipArchive(zipFileStream, ZipArchiveMode.Create);

                var streamProvider = (string folderName, string fileName) =>
                {
                    var entry = archive.CreateEntry(Path.Combine(folderName, fileName));
                    return Task.FromResult<Stream>(entry.Open());
                };

                await gen.GenerateTilesAsync(Path.Combine(yourFullPath + "imagePlan2.png"),
                    streamProvider);
            }
        }

        [Benchmark]
        public void RenderTilesOften()
        {
            SvgPlatform.Init();
            var doc = new SvgDocument();
            var image = doc.AddImageInBackground(yourFullPath + "svgPlan2.svg");
            image.Href = yourFullPath + "TilesStream.zip";
            var bitmap = doc.Draw();
            for(int i = 0; i <= 15; i++)
                doc.Draw();
        }

        [Benchmark]
        public void RenderTilesInSvgOften()
        {
            SvgPlatform.Init();
            var gen = new TileGenerator();
            var doc = new SvgDocument();
            var image = doc.AddImageInBackground(yourFullPath + "svgPlan2.svg");
            image.Href = yourFullPath + "TilesStream.zip";
            var bitmap = doc.Draw();
            for(int i = 0; i <= 15; i++)
                doc.Draw();
        }
    }
}