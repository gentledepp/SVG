using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Svg.DeepZoom;
using System.IO.Compression;

namespace Svg.Tests.Benchmarks;

public class Programm
{
    private const string yourFullPath = "C:\\Users\\zepr2\\source\\repos\\SVG\\Svg.Tests.Benchmarks\\Assets\\";

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
            var gen = new TileGenerator();
            var doc = SvgDocument.Open(yourFullPath + "svgPlan2.svg");
            var bitmap = doc.Draw();
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

                await gen.GenerateTilesAsync(Path.Combine( yourFullPath + "imagePlan2.png"),
                    streamProvider);
            }
        }

        [Benchmark]
        public void LoadSvg()
        {
            var doc = SvgDocument.Open(yourFullPath + "svgPlan2.svg");
            var bitmap = doc.Draw();
        }

        [Benchmark]
        public void RenderSvgOften()
        {
            var doc = SvgDocument.Open(yourFullPath + "svgPlan2.svg");
            var bitmap = doc.Draw();
            doc.Draw();
            doc.Draw();
            doc.Draw();
            doc.Draw();
            doc.Draw();
            doc.Draw();
        }

        [Benchmark]
        public void LoadTiles()
        {
            var doc = new SvgDocument();
            var image = doc.AddImageInBackground(yourFullPath + "svgPlan2.svg");
            image.Href = yourFullPath + "TilesStream.zip";
            var bitmap = doc.Draw();
        }

        [Benchmark]
        public void RenderTilesOften()
        {
            var doc = new SvgDocument();
            var image = doc.AddImageInBackground(yourFullPath + "svgPlan2.svg");
            image.Href = yourFullPath + "TilesStream.zip";
            var bitmap = doc.Draw();
            doc.Draw();
            doc.Draw();
            doc.Draw();
            doc.Draw();
            doc.Draw();
            doc.Draw();

        }
    }
}