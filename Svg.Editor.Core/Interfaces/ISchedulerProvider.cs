using System.Reactive.Concurrency;

namespace Svg.Editor.Interfaces
{
    public interface ISchedulerProvider
    {
        IScheduler MainScheduer { get; }
        IScheduler BackgroundScheduler { get; }
    }
}