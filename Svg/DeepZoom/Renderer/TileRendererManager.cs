using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using Svg.Interfaces;

namespace Svg.DeepZoom;

public class TileRendererManager : ITileRendererManager
{

    private ITileRenderer _tileRendererInstance;

    // Open zip archives + prebuilt tile lookups, keyed by zip path, so the archive is opened
    // and its entry list enumerated once per plan instead of on every render.
    private readonly ConcurrentDictionary<string, Lazy<ZipTileArchive>> _zipArchives = new();

    public ITileRenderer GetOrCreateTileRenderer()
    {
        if (_tileRendererInstance == null)
        {
            _tileRendererInstance = SvgEngine.Resolve<ITileRenderer>();
        }

        return _tileRendererInstance;
    }

    /// <summary>
    /// Returns a tile provider backed by a cached, already-open zip archive for
    /// <paramref name="zipPath"/>. The archive is opened (and its entries enumerated) only on
    /// the first request for a given path; subsequent renders reuse it. Disposed in
    /// <see cref="DisposeTileRenderer"/>.
    /// </summary>
    public Func<string, string, Stream> GetOrCreateZipTileProvider(string zipPath)
    {
        if (zipPath == null) throw new ArgumentNullException(nameof(zipPath));

        var archive = _zipArchives.GetOrAdd(zipPath,
            p => new Lazy<ZipTileArchive>(() => new ZipTileArchive(p))).Value;

        return archive.GetEntryStream;
    }

    public void DisposeTileRenderer()
    {
        if (_tileRendererInstance != null)
        {
            _tileRendererInstance.Dispose();
            _tileRendererInstance = null;
        }

        foreach (var key in _zipArchives.Keys)
        {
            if (_zipArchives.TryRemove(key, out var lazy) && lazy.IsValueCreated)
                lazy.Value.Dispose();
        }
    }

    /// <summary>
    /// Holds an open zip archive and its prebuilt entry lookup. Entry reads are serialised
    /// because <see cref="ZipArchive"/> does not support concurrent access.
    /// </summary>
    private sealed class ZipTileArchive : IDisposable
    {
        private readonly Stream _stream;
        private readonly ZipArchive _archive;
        private readonly Func<string, string, Stream> _provider;
        private readonly object _gate = new();

        public ZipTileArchive(string zipPath)
        {
            var fileSystem = SvgEngine.Resolve<IFileSystem>();
            _stream = fileSystem.OpenRead(zipPath);
            _archive = new ZipArchive(_stream, ZipArchiveMode.Read);
            _provider = SvgImage.CreateZipTileProvider(_archive, (a, b) => fileSystem.PathCombine(a, b));
        }

        public Stream GetEntryStream(string folderName, string fileName)
        {
            lock (_gate)
                return _provider(folderName, fileName);
        }

        public void Dispose()
        {
            _archive.Dispose();
            _stream.Dispose();
        }
    }
}
