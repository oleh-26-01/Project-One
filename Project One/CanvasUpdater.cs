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

internal class CanvasUpdater
{
    private Canvas[] _canvasArray;
    private Camera[] _cameraArray;
    private WpfCurve _curve;
    private int _canvasIndex = 0;

    private int _tickRate = 90;
    private int _lastUpdate;
    public int TargetRefreshTime => 1000 / _tickRate;

    private Thread _thread;
    private bool _threadRunning = true;
    private Dispatcher _dispatcher;

    /// <summary>
    /// Creates a new CanvasUpdater. Runs in a separate thread.
    /// </summary>
    /// <param name="canvasArrayArray"></param>
    /// <param name="cameraArray"></param>
    /// <param name="curve"></param>
    public CanvasUpdater(Dispatcher dispatcher, Canvas[] canvasArrayArray, Camera[] cameraArray, WpfCurve curve)
    {
        _dispatcher = dispatcher;

        _canvasArray = canvasArrayArray;
        _cameraArray = cameraArray;
        _curve = curve;
        _curve.Draw(_canvasArray[_canvasIndex], _cameraArray[_canvasIndex]);

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
                        _curve.Update(_cameraArray[_canvasIndex]);
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