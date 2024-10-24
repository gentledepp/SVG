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
    public void TryGetValue_501ItemNotExist_ShouldReturnTrue()
    {
        var key = "testKey";
        var bitmap = new SKBitmap();

        for(int i = 0; i< 501; i++)
        {
            _cache.GetOrCreate(key+i, () => bitmap);
        }

        var result = _cache.TryGetValue(key+500, out var cacheItem);
        
        Assert.IsFalse(result);
        Assert.IsNull(cacheItem);
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
}