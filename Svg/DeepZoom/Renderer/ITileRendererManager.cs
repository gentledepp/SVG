using System;
using System.IO;

namespace Svg.DeepZoom;

public interface ITileRendererManager
{
    public ITileRenderer GetOrCreateTileRenderer();

    /// <summary>
    /// Returns a tile provider backed by a cached, already-open zip archive for the given path,
    /// so the archive is opened and enumerated once per plan rather than on every render.
    /// </summary>
    Func<string, string, Stream> GetOrCreateZipTileProvider(string zipPath);

    public void DisposeTileRenderer();
}