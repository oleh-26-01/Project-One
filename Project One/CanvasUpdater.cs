using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Project_One_Objects;

namespace Project_One;

public class CanvasUpdater
{
    private readonly Canvas[] _canvasArray;
    private readonly Camera[] _cameraArray;
    private readonly WpfCurve _curve;
    private readonly WpfCurveEraser _curveEraser;
    private readonly WpfCrosshair _crosshair;
    private int _canvasIndex = 0;

    private int _tickRate = 90;
    private int _lastUpdate;
    public int TargetRefreshTime => 1000 / _tickRate;

    private Thread _thread;
    private bool _threadRunning = true;
    private readonly Dispatcher _dispatcher;

    /// <summary>Creates a new CanvasUpdater. Runs in a separate thread.</summary>
    public CanvasUpdater(
        Dispatcher dispatcher, Canvas[] canvasArrayArray, Camera[] cameraArray, 
        WpfCurve curve, WpfCurveEraser curveEraser)
    {
        _dispatcher = dispatcher;

        _canvasArray = canvasArrayArray;
        _cameraArray = cameraArray;
        _curve = curve;
        _curve.DrawOn(_canvasArray[_canvasIndex]);
        _curveEraser = curveEraser;
        _curveEraser.DrawOn(_canvasArray[_canvasIndex]);

        _crosshair = new WpfCrosshair(new Vector2(0, 0));
        _crosshair.DrawOn(_canvasArray[_canvasIndex]);

        _lastUpdate = Environment.TickCount;
        _thread = new Thread(Parallel);
        _thread.Start();
    }

    private void Parallel()
    {
        while (_threadRunning)
        {
            _dispatcher.Invoke(() =>
            {
                switch (_canvasIndex)
                {
                    case 0:
                        _curve.Update(_cameraArray[0]);
                        _crosshair.Update(_cameraArray[0]);
                        _curveEraser.Update(_cameraArray[0]);
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                }
            });

            var sleepTime = TargetRefreshTime - (Environment.TickCount - _lastUpdate);
            if (sleepTime > 0)
            {
                Thread.Sleep(sleepTime);
            }
            _lastUpdate = Environment.TickCount;
        }
    }

    public void StopThread()
    {
        _threadRunning = false;
    }

    public int CanvasIndex
    {
        get => _canvasIndex;
        set
        {
            if (value >= _canvasArray.Length || value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "CanvasIndex must be less than the number of canvases.");
            }

            _canvasIndex = value;
        }
    }

    /// <summary>The number of times per second the canvas will be updated.</summary>
    /// <exception cref="ArgumentOutOfRangeException">TickRate must be greater than 0.</exception>
    /// <remarks>TickRate is not guaranteed to be accurate.</remarks>
    public int TickRate
    {
        get => _tickRate;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "TickRate must be greater than 0.");
            }

            _tickRate = value;
        }
    }

    public int LastUpdate => _lastUpdate;
}