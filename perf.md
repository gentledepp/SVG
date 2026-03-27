# Tile System Performance & Efficiency Analysis

Analysis of the deep-zoom tile generation and rendering pipeline in `Svg.DeepZoom`.

## Architecture Overview

The active tile pipeline works as follows:

1. Source image (raster or pre-rendered SVG bitmap) is decoded into an `SKBitmap`
2. `TileGenerator.GenerateTilesAsync` creates a pyramid of 256x256 PNG tiles across zoom levels and writes them into a zip archive via a stream provider
3. A single `SvgImage` element references the `.zip` file
4. At render time, `SvgImage.Render` delegates to `TileRenderer`, which loads only the visible tiles at the appropriate zoom level from the zip/cache

## Performance Issues

### TileGenerator

**Full-resolution rasterization up front**
When tiling an SVG, the entire document must be rendered to a single large bitmap before tiling begins. A 3840px-wide drawing could consume 50-100+ MB of pixel data. There is no viewport-aware SVG rendering -- the entire SVG is rasterized regardless of what is visible.

**No `SKFilterQuality` on downsampling**
When drawing scaled-down regions via `canvas.DrawBitmap()` in `CreateTileAsync`, no `SKPaint` with filter quality is specified. SkiaSharp may use nearest-neighbor at some scales, producing aliased tiles at higher zoom levels. Passing a paint with `SKFilterQuality.Medium` or `High` would improve tile quality at lower zoom levels.

**Thread safety with unlimited parallelism**
`GenerateTilesAsync` with `maxParallelTasks: -1` (unlimited) can trigger `SEHException` in SkiaSharp's native layer when many threads concurrently read from the same source `SKBitmap`. A reasonable limit (e.g., `Environment.ProcessorCount`) should be used as a default instead of `int.MaxValue`.

**PNG encoding at quality 100**
Tiles are encoded as PNG at quality 100. For photographic content (phone images), JPEG at quality 85-90 would produce significantly smaller files with minimal visual difference.
But the tiles are generated for plans only - so vector graphics essentially. Isn't then png the best option?

### TileRenderer

**Zip archive reopened per tile**
In `SvgImage.Render`, the zip-based tile provider (`tileProvider` lambda at line 206) reopens the zip file and creates a new `ZipArchive` for every single tile. This is extremely expensive for I/O. The zip should be opened once and reused across all tile loads within a render pass.

**Tiles loaded before visibility check**
In `RenderBitmap` (sync path), `LoadTile()` is called at line 118, then `IntersectsWith()` is checked at line 128. Tiles outside the visible area are loaded and decoded unnecessarily. The bounds check should happen before the tile load.

**MemoryStream copy for every tile load**
`LoadTileStream` and `LoadTileStreamAsync` copy the entire file into a `MemoryStream` before decoding. For tiles already on disk, `SKBitmap.Decode(FileStream)` would avoid the intermediate copy.

### TileCache

**No LRU eviction** (capacity issue)
When the cache reaches `MaximalTiles`, new items are still fully created (decoded from disk) but simply not stored. The work is wasted. A proper LRU eviction policy would evict the oldest entry to make room, avoiding redundant decode work.

**Cleanup bug (fixed)**
The `CleanUp` timer callback was disposing expired items but not removing them from the `ConcurrentDictionary`. This caused the cache count to never decrease, eventually filling with dead entries and permanently rejecting all new items. Fixed by adding `TryRemove` before `Dispose` in the cleanup loop.

## Potential Improvements (not yet implemented)

### Efficient tile creation from svg
SVG rendering is expensive. Instead, render the lowest tile level first using SVG. 
Then use the existing tiles to create the next zoom level (zoomed out) and so on.

### Efficient tile creation from image (png, jpeg, ...)
Do not load the whole image into memory. Instead use SKCodec to only load the parts that you need to create the tiles of the lowest level (max zoom as in SVG)
SKCodec decodes on demand from the underlying stream instead of materialising the whole bitmap. 
You feed it a sub-rectangle via SKCodecOptions.SubsetRect and it only pulls the bytes required for that region.
The important caveat is format-dependent: JPEG supports arbitrary subsets (snapped to MCU boundaries), WebP mostly does, and PNG rejects subsets entirely. 
Always ask the codec to validate the rectangle first with GetValidSubset; for codecs that refuse, fall back to scanline decoding, which still beats loading the whole image.
For JPEG sources, keep a single SKCodec alive and call GetPixels once per tile — reopening the file per tile re-parses the JFIF headers unnecessarily. 
For the PNG fallback, each StartScanlineDecode is a fresh pass from row 0, so process all tiles of a given row band in one scanline pass before moving on

### Shared zip handle per render pass
Open the zip archive once at the start of a render pass and pass it to the tile provider, rather than reopening per tile. This alone would significantly reduce I/O overhead during rendering.

### Proper LRU cache eviction
Replace the current reject-when-full strategy with LRU eviction. When the cache is full and a new tile is needed, evict the least-recently-used entry rather than discarding the newly decoded tile.


## Verification
Verify your implementation by adding tests.
Verify your tests by removing the implementation that it covers so it fails (red).
When it fails, add the implementation back to verify it succeeds (green)

---

See [perf_2.md](perf_2.md) for deferred follow-ups (SVG pyramid, SKCodec subset decoding).