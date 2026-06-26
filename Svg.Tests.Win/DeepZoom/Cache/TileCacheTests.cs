using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using NUnit.Framework;
using SkiaSharp;
using Svg.DeepZoom;

namespace Svg.Tests.Win.Cache;

public class TileCacheTests
{
    private ITileCache _cache;
    private TileCacheOptions _options;

    [SetUp]
    public void SetUp()
    {
        _options = new TileCacheOptions
        {
            CleanupInterval = TimeSpan.FromSeconds(1),
            MaximalTiles = 500
        };

        _cache = new TileCache(_options);
    }

    [TearDown]
    public void TearDown()
    {
        _cache.Dispose();
        _cache = null;
    }

    [Test]
    public void GetOrCreate_WithNullKey_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _cache.GetOrCreate(null, () => new SKBitmap()));
    }

    [Test]
    public void GetOrCreate_WithNullItemProvider_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _cache.GetOrCreate("key", null));
    }

    [Test]
    public void GetOrCreate_ItemNotInCache_ShouldAddAndReturnNewItem()
    {
        var key = "testKey";
        var bitmap = new SKBitmap();

        var result = _cache.GetOrCreate(key, () => bitmap);

        Assert.IsNotNull(result);
        Assert.AreEqual(bitmap, result.Tile);
    }

    [Test]
    public void GetOrCreate_ItemAlreadyInCache_ShouldReturnExistingItem()
    {
        var key = "testKey";
        var bitmap = new SKBitmap();

        var firstResult = _cache.GetOrCreate(key, () => bitmap);
        var secondResult = _cache.GetOrCreate(key, () => throw new Exception("Should not be called"));

        Assert.AreEqual(firstResult, secondResult);
    }

    [Test]
    public void GetOrCreate_ItemExpired_ShouldReplaceWithNewItem()
    {
        var key = "testKey";
        var bitmap1 = new SKBitmap();
        var bitmap2 = new SKBitmap();

        var options = new TileCacheOptions { CleanupInterval = TimeSpan.FromMilliseconds(500) };
        using var cache = new TileCache(options);

        var firstResult = cache.GetOrCreate(key, () => bitmap1);

        Thread.Sleep(1000);

        var secondResult = cache.GetOrCreate(key, () => bitmap2);

        Assert.AreNotEqual(firstResult, secondResult);
        Assert.AreEqual(bitmap2, secondResult.Tile);
    }

    [Test]
    public async Task GetOrCreateAsync_WithNullKey_ShouldThrowArgumentNullException()
    {
        await Task.Yield();
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await _cache.GetOrCreateAsync(null, async () => new SKBitmap()));
    }

    [Test]
    public async Task GetOrCreateAsync_WithNullItemProvider_ShouldThrowArgumentNullException()
    {
        await Task.Yield();
        Assert.ThrowsAsync<ArgumentNullException>(async () => await _cache.GetOrCreateAsync("key", null));
    }

    [Test]
    public async Task GetOrCreateAsync_ItemNotInCache_ShouldAddAndReturnNewItem()
    {
        var key = "testKey";
        var bitmap = new SKBitmap();

        var result = await _cache.GetOrCreateAsync(key, async () =>
        {
            await Task.Delay(100);
            return bitmap;
        });

        Assert.IsNotNull(result);
        Assert.AreEqual(bitmap, result.Tile);
    }

    [Test]
    public async Task GetOrCreateAsync_ItemAlreadyInCache_ShouldReturnExistingItem()
    {
        var key = "testKey";
        var bitmap = new SKBitmap();

        var firstResult = await _cache.GetOrCreateAsync(key, async () => bitmap);
        var secondResult = await _cache.GetOrCreateAsync(key, async () => throw new Exception("Should not be called"));

        Assert.AreEqual(firstResult, secondResult);
    }

    [Test]
    public async Task GetOrCreateAsync_ItemExpired_ShouldReplaceWithNewItem()
    {
        var key = "testKey";
        var bitmap1 = new SKBitmap();
        var bitmap2 = new SKBitmap();

        var options = new TileCacheOptions { CleanupInterval = TimeSpan.FromMilliseconds(500) };
        using var cache = new TileCache(options);

        var firstResult = await cache.GetOrCreateAsync(key, async () =>
        {
            await Task.Delay(50);
            return bitmap1;
        });

        await Task.Delay(1000);

        var secondResult = await cache.GetOrCreateAsync(key, async () =>
        {
            await Task.Delay(50);
            return bitmap2;
        });

        Assert.AreNotEqual(firstResult, secondResult);
        Assert.AreEqual(bitmap2, secondResult.Tile);
    }

    [Test]
    public void TryGetValue_WithNullKey_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _cache.TryGetValue(null, out _));
    }

    [Test]
    public void TryGetValue_ItemExistsAndNotExpired_ShouldReturnTrue()
    {
        var key = "testKey";
        var bitmap = new SKBitmap();

        _cache.GetOrCreate(key, () => bitmap);

        var result = _cache.TryGetValue(key, out var cacheItem);

        Assert.IsTrue(result);
        Assert.AreEqual(bitmap, cacheItem.Tile);
    }

    [Test]
    public void TryGetValue_OverCapacity_LruEntryEvictedAndNewItemCached()
    {
        // Fill to capacity (500), then insert one more → LRU entry evicted, new item stored.
        var key = "testKey";

        for (int i = 0; i < 500; i++)
            _cache.GetOrCreate(key + i, () => new SKBitmap(1, 1));

        // key0 is the oldest (LRU candidate). Insert one beyond capacity.
        var newest = new SKBitmap(1, 1);
        _cache.GetOrCreate("newest", () => newest);

        // New item must be cached.
        Assert.IsTrue(_cache.TryGetValue("newest", out var newItem));
        Assert.AreEqual(newest, newItem.Tile);

        // The oldest entry (key0) must have been evicted to make room.
        Assert.IsFalse(_cache.TryGetValue(key + 0, out _), "LRU entry (key0) should be evicted.");
    }

    [Test]
    public void GetOrCreate_WhenFull_EvictsLeastRecentlyUsed()
    {
        var opts = new TileCacheOptions { CleanupInterval = TimeSpan.FromHours(1), MaximalTiles = 3 };
        using var cache = new TileCache(opts);

        var bmp0 = new SKBitmap(1, 1);
        var bmp1 = new SKBitmap(1, 1);
        var bmp2 = new SKBitmap(1, 1);

        cache.GetOrCreate("k0", () => bmp0);
        cache.GetOrCreate("k1", () => bmp1);
        cache.GetOrCreate("k2", () => bmp2);

        // Touch k1 so its LastAccess is more recent than k0.
        cache.TryGetValue("k1", out _);

        // Insert beyond capacity → k0 (true LRU) should be evicted.
        var bmpNew = new SKBitmap(1, 1);
        cache.GetOrCreate("kNew", () => bmpNew);

        Assert.IsFalse(cache.TryGetValue("k0", out _), "k0 (LRU) should have been evicted.");
        Assert.IsTrue(cache.TryGetValue("k1", out _), "k1 (recently accessed) should still be cached.");
        Assert.IsTrue(cache.TryGetValue("kNew", out _), "new item should be stored after LRU eviction.");
    }

    [Test]
    public void TryGetValue_ItemDoesNotExist_ShouldReturnFalse()
    {
        var result = _cache.TryGetValue("nonExistentKey", out var cacheItem);

        Assert.IsFalse(result);
        Assert.IsNull(cacheItem);
    }

    [Test]
    public void TryGetValue_ItemExpired_ShouldReturnFalseAndRemoveItem()
    {
        var key = "testKey";
        var bitmap = new SKBitmap();

        var options = new TileCacheOptions { CleanupInterval = TimeSpan.FromMilliseconds(500) };
        using var cache = new TileCache(options);

        cache.GetOrCreate(key, () => bitmap);

        Thread.Sleep(1000);

        var result = cache.TryGetValue(key, out var cacheItem);

        Assert.IsFalse(result);
        Assert.IsNull(cacheItem);
    }

    [Test]
    public void Remove_WithNullKey_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _cache.Remove(null));
    }

    [Test]
    public void Remove_ExistingItem_ShouldRemoveItem()
    {
        var key = "testKey";
        var bitmap = new SKBitmap();

        _cache.GetOrCreate(key, () => bitmap);
        _cache.Remove(key);

        var result = _cache.TryGetValue(key, out var cacheItem);

        Assert.IsFalse(result);
        Assert.IsNull(cacheItem);
    }

    [Test]
    public void Remove_NonExistingItem_ShouldNotThrowException()
    {
        Assert.DoesNotThrow(() => _cache.Remove("nonExistentKey"));
    }

    [Test]
    public void CleanUp_ShouldRemoveExpiredItems()
    {
        var key = "testKey";
        var bitmap = new SKBitmap();

        _cache.GetOrCreate(key, () => bitmap);

        Thread.Sleep(2000);

        var result = _cache.TryGetValue(key, out var cacheItem);

        Assert.IsFalse(result);
        Assert.IsNull(cacheItem);
    }

    [Test]
    public void CleanUp_ShouldNotRemoveNonExpiredItems()
    {
        var key = "testKey";
        var bitmap = new SKBitmap();

        _cache.GetOrCreate(key, () => bitmap);

        var result = _cache.TryGetValue(key, out var cacheItem);

        Assert.IsTrue(result);
        Assert.AreEqual(bitmap, cacheItem.Tile);
    }

    [Test]
    public void DrainPendingDisposals_AfterEviction_DisposesEvictedBitmap()
    {
        var opts = new TileCacheOptions { CleanupInterval = TimeSpan.FromHours(1), MaximalTiles = 2 };
        using var cache = new TileCache(opts);

        var evicted = new SKBitmap(1, 1);
        cache.GetOrCreate("k0", () => evicted);
        cache.GetOrCreate("k1", () => new SKBitmap(1, 1));

        // Exceed capacity → k0 (LRU) is evicted but disposal is deferred.
        cache.GetOrCreate("k2", () => new SKBitmap(1, 1));
        Assert.AreNotEqual(IntPtr.Zero, evicted.Handle, "evicted bitmap must not be disposed before drain");

        cache.DrainPendingDisposals();
        Assert.AreEqual(IntPtr.Zero, evicted.Handle, "evicted bitmap must be disposed after drain");
    }

    [Test]
    public void DrainPendingDisposals_AfterRemove_DisposesRemovedBitmap()
    {
        var removed = new SKBitmap(1, 1);
        _cache.GetOrCreate("k", () => removed);

        _cache.Remove("k");
        Assert.AreNotEqual(IntPtr.Zero, removed.Handle, "removed bitmap must not be disposed before drain");

        _cache.DrainPendingDisposals();
        Assert.AreEqual(IntPtr.Zero, removed.Handle, "removed bitmap must be disposed after drain");
    }

    [Test]
    public void DrainPendingDisposals_DoesNotDisposeLiveItems()
    {
        var live = new SKBitmap(1, 1);
        _cache.GetOrCreate("live", () => live);

        _cache.DrainPendingDisposals();

        Assert.AreNotEqual(IntPtr.Zero, live.Handle, "a cached (live) bitmap must not be disposed");
        Assert.IsTrue(_cache.TryGetValue("live", out var item));
        Assert.AreEqual(live, item.Tile);
    }

    [Test]
    public void DrainPendingDisposals_AfterExpiryCleanup_DisposesExpiredBitmap()
    {
        var expired = new SKBitmap(1, 1);
        var options = new TileCacheOptions { CleanupInterval = TimeSpan.FromMilliseconds(300) };
        using var cache = new TileCache(options);

        cache.GetOrCreate("k", () => expired);

        // Wait for the cleanup timer to remove the expired entry (still not disposed yet).
        Thread.Sleep(800);
        Assert.AreNotEqual(IntPtr.Zero, expired.Handle, "expired bitmap must not be disposed before drain");

        cache.DrainPendingDisposals();
        Assert.AreEqual(IntPtr.Zero, expired.Handle, "expired bitmap must be disposed after drain");
    }

    [Test]
    public void CleanUp_ShouldFreeCapacityForNewItems()
    {
        // Fill the cache to capacity with short-lived items
        var options = new TileCacheOptions
        {
            CleanupInterval = TimeSpan.FromMilliseconds(300),
            MaximalTiles = 3
        };
        using var cache = new TileCache(options);

        for (int i = 0; i < 3; i++)
            cache.GetOrCreate($"old_{i}", () => new SKBitmap(1, 1));

        // Cache is full — LRU eviction means the new item IS stored (oldest evicted).
        var overflowBitmap = new SKBitmap(1, 1);
        cache.GetOrCreate("overflow", () => overflowBitmap);
        Assert.IsTrue(cache.TryGetValue("overflow", out var overflowItem),
            "Item added at capacity should be stored after LRU eviction.");
        Assert.AreEqual(overflowBitmap, overflowItem.Tile);

        // Wait for items to expire and cleanup to run
        Thread.Sleep(800);

        // After cleanup removed expired items, new items should still be cacheable.
        var newBitmap = new SKBitmap(1, 1);
        cache.GetOrCreate("after_cleanup", () => newBitmap);

        Assert.IsTrue(cache.TryGetValue("after_cleanup", out var cached));
        Assert.AreEqual(newBitmap, cached.Tile);
    }
}