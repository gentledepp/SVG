# Tile System Performance — Follow-ups

Follow-up tracker for items deferred from [perf.md](perf.md) after the 2026-04-17 implementation pass.

The concrete "Performance Issues" section of perf.md has been implemented. This document tracks the two architectural items from perf.md's "Potential Improvements" section that were too large to bundle into that pass.

## 1. Efficient tile creation from SVG

### Problem
`TileGenerator.GenerateTilesAsync` rasterises the entire SVG to a single large `SKBitmap` up front. A 3840px-wide drawing can consume 50–100+ MB of pixel data even when most of the content is never visited at full zoom.

### Proposed approach
1. Render only the lowest zoom level (`z_max`) directly from the SVG, **per tile**, using a viewport / clip rectangle. No full-image bitmap is ever allocated.
2. Build each lower zoom level by drawing the four child tiles from the level immediately above into a new 256×256 tile. This is a 2×2 downsample of already-rendered tiles — fast, O(tiles) total work, bounded memory.
3. Keep existing output contract (zip of `z{n}/y{y}_x{x}.png`) so the renderer is unchanged.

### Acceptance criteria
- Peak working set during `GenerateTilesAsync` stays roughly constant regardless of source SVG dimensions (measure with a 3840×2160 SVG vs. a 15360×8640 SVG — memory delta should be < 50 MB).
- Output tiles are visually equivalent to the current implementation for the same source SVG (pixel-diff within anti-aliasing tolerance at z0, exact at interior pixels).
- Generation wall-clock time improves for large SVGs (because the giant rasterise is gone).

### Test approach
- Unit: feed in an SVG whose nominal size is 20000×20000 and verify no `SKBitmap` larger than a single tile is constructed. Use a memory-pressure harness or an `SKBitmap` allocation counter.
- Integration: render same SVG via current and new pipeline, compare per-tile checksums with a small SSIM / L1 tolerance.

### Risk / unknowns
- SkiaSharp SVG renderer currently expects to render into a full-sized canvas. Need to confirm a viewport-clip approach produces pixel-stable output at tile boundaries (no seams from hinting / AA carrying across tiles).
- Some SVG features (filters, `mask`, `clipPath`) may extend content outside their tile's bounds; the z_max tile-by-tile renderer must include a small overdraw margin, then crop to 256×256.

---

## 2. Efficient tile creation from raster images via `SKCodec`

### Problem
Raster sources (PNG, JPEG, WebP) are currently decoded into a full `SKBitmap` before tiling. A 24 000 × 12 000 JPEG is ~1 GB uncompressed — wastes memory and blocks the GC for seconds.

### Proposed approach
Use `SKCodec` to decode **subsets** of the source image on demand:

1. Open the source file once with `SKCodec.Create(stream)` and keep it alive across all tile creations.
2. For each z_max tile, compute the source rectangle, then:
   - Call `codec.GetValidSubset(ref rect)`. If it succeeds, use `SKCodecOptions.SubsetRect` + `codec.GetPixels` to decode only that region.
   - If it returns false, fall back to scanline decoding (`codec.StartScanlineDecode`).
3. For scanline fallback, **process all tiles in the same row band in a single scanline pass**. `StartScanlineDecode` restarts from row 0, so doing it per-tile would be O(n²).
4. Lower zoom levels follow the same 2×2 downsample strategy as in (1) above — reuse the already-written tiles from the level immediately above.

### Format matrix
| Format | `SubsetRect` supported? | Strategy |
|--------|-------------------------|----------|
| JPEG   | Yes (snapped to MCU, usually 8×8 or 16×16) | `SubsetRect` per tile |
| WebP   | Mostly yes              | `SubsetRect` per tile, verify each |
| PNG    | No                      | Scanline fallback, batched by row band |

### Acceptance criteria
- Peak memory usage for a 24 000 × 12 000 JPEG source is < 100 MB during tiling.
- Generation wall-clock time for large JPEGs is comparable or better than current implementation (SubsetRect saves decode work; scanline batching keeps PNG competitive).
- Correctness: tiles match pixel-for-pixel (JPEG) or within 1 LSB (scanline) the current implementation.

### Test approach
- Unit: mock `SKCodec` and verify `SubsetRect` is used when supported, scanline path otherwise.
- Integration: tile a 12 000 × 6 000 JPEG and a 12 000 × 6 000 PNG; compare resulting tiles to current output.
- Memory: assert peak `GC.GetTotalMemory` during generation stays bounded.

### Risk / unknowns
- `SKCodec.GetValidSubset` behaviour on edge-of-image rectangles (last column/row) may clamp in surprising ways — handle with a padding-and-crop pass.
- Single `SKCodec` instance is not thread-safe; if tile generation stays parallel, each worker needs its own codec or access must be serialised.

---
