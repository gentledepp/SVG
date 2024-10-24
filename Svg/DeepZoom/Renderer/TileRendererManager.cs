namespace Svg.DeepZoom;

public class TileRendererManager : ITileRendererManager
{

    private ITileRenderer _tileRendererInstance;
    public ITileRenderer GetOrCreateTileRenderer()
    {
        if (_tileRendererInstance == null)
        {
            _tileRendererInstance = SvgEngine.Resolve<ITileRenderer>();
        }

        return _tileRendererInstance;
    }

    public void DisposeTileRenderer()
    {
        if (_tileRendererInstance != null)
        {
            _tileRendererInstance.Dispose();
            _tileRendererInstance = null;
        }
    }
}