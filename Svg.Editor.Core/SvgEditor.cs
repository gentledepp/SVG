using System.Reactive.Concurrency;
using System.Threading;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;
using Svg.Editor.UndoRedo;
using Svg.Interfaces;

namespace Svg.Editor
{
    public static class SvgEditor
    {
        private static bool _initialized;
        private static readonly SemaphoreSlim _lock = new SemaphoreSlim(1);

        public static void Init()
        {
            if (_initialized)
                return;
            try
            {
                _lock.Wait();

                if (_initialized)
                    return;

                SvgPlatform.Init();

                var context = SynchronizationContext.Current ?? new SynchronizationContext();
                if (context != null)
                {
                    var mainScheduler = new SynchronizationContextScheduler(context);
                    var schedulerProvider = new SchedulerProvider(mainScheduler, NewThreadScheduler.Default);
                    SvgEngine.Register<ISchedulerProvider>(() => schedulerProvider);
                    SvgEngine.RegisterSingleton<IGestureRecognizer>(() => new ReactiveGestureRecognizer(schedulerProvider));
                }

                SvgEngine.RegisterSingleton<IUndoRedoService>(() => new UndoRedoService());
                SvgEngine.RegisterSingleton<IEmbeddedResourceRegistry>(() => new EmbeddedResourceRegistry());
                SvgEngine.Register<ISvgSourceFactory>(() => new EmbeddedResourceSvgSourceFactory(SvgEngine.Resolve<IEmbeddedResourceRegistry>()));
                SvgEngine.RegisterSingleton<IImageSourceProvider>(() => new DefaultImageSourceProvider());


                _initialized = true;
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
