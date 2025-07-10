using System.Reactive.Concurrency;
using Svg.Editor.Interfaces;

namespace Svg.Editor.Services
{
    public class SchedulerProvider : ISchedulerProvider
    {
        public IScheduler MainScheduer { get; }
        public IScheduler BackgroundScheduler { get; }

        public SchedulerProvider(IScheduler mainScheduler, IScheduler backgroundScheduler)
        {
            MainScheduer = mainScheduler;
            BackgroundScheduler = backgroundScheduler;
        }
    }
}