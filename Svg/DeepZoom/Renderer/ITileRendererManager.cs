namespace Svg.DeepZoom;

public interface ITileRendererManager
{
    public ITileRenderer GetOrCreateTileRenderer();

    public void DisposeTileRenderer();
}