using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Svg.Editor.Interfaces;
using System.Diagnostics;
using Svg.Editor.Gestures;
using System;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Svg.Editor.Avalon.Forms.ToolBar;

public class LongPressTipGestureRecognizer : GestureRecognizer
{
    private readonly IToolTipInfoService _toolTipInfoService;
    private string _text;
    private DispatcherTimer _holdingTimer;
    private readonly TimeSpan _holdingThreshold = TimeSpan.FromMilliseconds(500);
    private bool _isHoldingTriggered;
    private Control _owner;

    // when tooltip service never initialized, we disable gesture recognizer
    private readonly bool _disabled;
    private bool _isRightclick;
    private bool _tipShown;

    public LongPressTipGestureRecognizer(Control owner, string text)
    {
        _text = text;
        _owner = owner;
        _holdingTimer = new DispatcherTimer()
        {
            Interval = _holdingThreshold
        };
        _holdingTimer.Tick += OnHoldingTimerTick;

        _toolTipInfoService = SvgEngine.TryResolve<IToolTipInfoService>();
        if (_toolTipInfoService == null)
        {
            _disabled = true;
        }
    }

    private void OnHoldingTimerTick(object? sender, EventArgs e)
    {
        // Trigger the holding event
        _holdingTimer.Stop();
        _isHoldingTriggered = true;
    }

    protected override async void PointerPressed(PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(_owner);
        _isRightclick = point.Properties.IsRightButtonPressed;

        if (!_disabled)
        {
            if (_isRightclick)
            {
                if (_tipShown)
                {
                    _toolTipInfoService.CloseToolTip();
                    _tipShown = false;
                }

                await _toolTipInfoService.ShowToolTip(_text);
                _tipShown = true;
            }
            else
            {
                _isHoldingTriggered = false;
                _holdingTimer.Start();
            }
        }
    }

    protected override async void PointerReleased(PointerReleasedEventArgs e)
    {
        if (!_disabled && !_isRightclick)
        {
            _holdingTimer.Stop();
            _toolTipInfoService.CloseToolTip();
            _isHoldingTriggered = false;
        }
    }

    protected override async void PointerMoved(PointerEventArgs e)
    {
        if (!_disabled && !_isRightclick)
        {
            if (_isHoldingTriggered)
            {
                _isHoldingTriggered = false;
                _holdingTimer.Stop();
                await _toolTipInfoService.ShowToolTip(_text);
            }
        }
    }

    protected override void PointerCaptureLost(IPointer pointer)
    {
        if (!_disabled && !_isRightclick)
        {
            _holdingTimer.Stop();
            _toolTipInfoService.CloseToolTip();
            _isHoldingTriggered = false;
        }
    }
}