using System;
using Avalonia.Threading;

namespace Svg.Editor.Avalon.Forms.Dialog.Views;

public class ProgressDialogViewModel : ContentDialogViewModelBase
{
    private DispatcherTimer? _closeTimer;
    private double _progress;
    public double Progress
    {
        get => _progress;
        set
        {
            SetField(ref _progress, value);
            if ((int)value == 100)
            {
                _closeTimer = new DispatcherTimer();
                _closeTimer.Interval = TimeSpan.FromSeconds(1);
                _closeTimer.Tick += CloseTimerOnTick;
                _closeTimer.Start();

                void CloseTimerOnTick(object? sender, EventArgs e)
                {
                    _closeTimer.Stop();
                    _closeTimer.Tick -= CloseTimerOnTick;
                    AcceptPrimary();
                }
            }
        }
    }

    private bool _isIndeterminate;
    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        set => SetField(ref _isIndeterminate, value);
    }

    #region Cancel after duration

    private DispatcherTimer? _timer;

    internal void SetupCancelAfterDuration(TimeSpan? duration)
    {
        if (duration.HasValue)
        {
            _timer = new DispatcherTimer
            {
                Interval = duration.Value
            };
            _timer.Tick += OnTimerTick;
            _timer.Start();

            void OnTimerTick(object? sender, EventArgs e)
            {
                _timer.Stop();
                _timer.Tick -= OnTimerTick;
                AcceptPrimary();
            }
        }
    }
    #endregion
}