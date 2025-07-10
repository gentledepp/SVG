using System.Reactive.Concurrency;
using System.Threading;
using Svg;
using Svg.Editor.Interfaces;
using Svg.Editor.Services;

[assembly: SvgPlatform(typeof(SvgEditorPlatformSetup))]
namespace Svg
{
    public class SvgEditorPlatformSetup
    {
        public void Initialize()
        {
            var context = SynchronizationContext.Current;
            if (context != null)
            {
                var mainScheduler = new SynchronizationContextScheduler(context);
                var schedulerProvider = new SchedulerProvider(mainScheduler, NewThreadScheduler.Default);
                Engine.Register<ISchedulerProvider, SchedulerProvider>(() => schedulerProvider);
                Engine.RegisterSingleton<IGestureRecognizer, GestureRecognizer>(() => new GestureRecognizer(schedulerProvider));
            }
        }
    }
}