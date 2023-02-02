using System;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_One_Objects;

namespace Project_One;

internal class EventHandler
{
    private readonly Grid _windowGrid;
    private readonly Grid _canvasGrid;
    private readonly Grid _firstTopPanelGrid;
    private readonly CanvasUpdater _canvasUpdater;
    private readonly SidePanel _sidePanel;
    private readonly Camera[] _cameraArray;
    private readonly WpfCurve _curve;
    public int _canvasIndex = 0;

    private bool _isCanvasLeftMouseDown;
    private bool _isCanvasRightMouseDown;
    private bool _isArrowUpPressed;
    private bool _isArrowDownPressed;
    private bool _isArrowLeftPressed;
    private bool _isArrowRightPressed;

    public Vector2 MousePosition = new(-1, -1);
    private Vector2 _cameraMoveDirection = new();

    public EventHandler(
        Grid windowGrid, Grid canvasGrid, Grid firstFirstTopPanelGrid,
        CanvasUpdater canvasUpdater, SidePanel sidePanel, 
        Camera[] cameraArray, WpfCurve curve)
    {
        _windowGrid = windowGrid;
        _canvasGrid = canvasGrid;
        _firstTopPanelGrid = firstFirstTopPanelGrid;

        _canvasUpdater = canvasUpdater;
        _sidePanel = sidePanel;

        _cameraArray = cameraArray;
        _curve = curve;

        _windowGrid.MouseMove += MouseMove;
        _windowGrid.MouseWheel += MouseWheel;
        
        _canvasGrid.MouseDown += CanvasMouseClick;
        _canvasGrid.MouseUp += CanvasMouseClick;
        _canvasGrid.SizeChanged += CanvasSizeChanged;

        _windowGrid.PreviewKeyDown += WindowGridKeyDown;
        _windowGrid.PreviewKeyUp += WindowGridKeyUp;

        if (_firstTopPanelGrid.FindName("SaveCurve") is Button saveCurveButton)
        {
            saveCurveButton.Click += _sidePanel.SaveActiveCurve;
        }
    }

    public void WindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        _canvasUpdater.StopThread();
    }

    private void WindowGridKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Up)
        {
            _isArrowUpPressed = true;
        }
        else if (e.Key == Key.Down)
        {
            _isArrowDownPressed = true;
        }
        else if (e.Key == Key.Left)
        {
            _isArrowLeftPressed = true;
        }
        else if (e.Key == Key.Right)
        {
            _isArrowRightPressed = true;
        }
        else if (e.Key == Key.Add)
        {
            _cameraArray[_canvasIndex].ZoomIn(0.2f);
        }
        else if (e.Key == Key.Subtract)
        {
            _cameraArray[_canvasIndex].ZoomOut(0.2f);
        }
        UpdateCamera();
    }

    private void WindowGridKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Up)
        {
            _isArrowUpPressed = false;
        }
        else if (e.Key == Key.Down)
        {
            _isArrowDownPressed = false;
        }
        else if (e.Key == Key.Left)
        {
            _isArrowLeftPressed = false;
        }
        else if (e.Key == Key.Right)
        {
            _isArrowRightPressed = false;
        }
        //UpdateCamera();
    }

    private void UpdateCamera()
    {
        _cameraMoveDirection = new Vector2(0, 0);
        _cameraMoveDirection.X += _isArrowRightPressed ? 1 : 0;
        _cameraMoveDirection.X -= _isArrowLeftPressed ? 1 : 0;
        _cameraMoveDirection.Y += _isArrowDownPressed ? 1 : 0;
        _cameraMoveDirection.Y -= _isArrowUpPressed ? 1 : 0;
        if (_cameraMoveDirection.Length() > 0)
        {
            _cameraArray[_canvasIndex].MoveSync(
                Vector2.Normalize(_cameraMoveDirection),
                _canvasUpdater.TargetRefreshTime,
                1000);
        }
    }

    private void MouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(_canvasGrid);
        var newMousePosition = new Vector2((float)position.X, (float)position.Y);
        if (newMousePosition == MousePosition) return;

        if (_isCanvasLeftMouseDown && _canvasIndex == 0)
        {
            _curve.AddPoint(_cameraArray[0].ConvertIn(newMousePosition));
        }
        else if (_isCanvasRightMouseDown)
        {
            _cameraArray[_canvasIndex].Move((MousePosition - newMousePosition) / _cameraArray[_canvasIndex].Zoom);
        }
        MousePosition = newMousePosition;
    }

    private void MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta > 0)
        {
            _cameraArray[_canvasIndex].ZoomIn(e.Delta / 1000f);
        }
        else if (e.Delta < 0)
        {
            _cameraArray[_canvasIndex].ZoomOut(-e.Delta / 1000f);
        }
    }

    private void CanvasMouseClick(object sender, MouseButtonEventArgs e)
    {
        Mouse.Capture(_canvasGrid);
        switch (e.ChangedButton)
        {
            case MouseButton.Left:
            {
                _isCanvasLeftMouseDown = e.ButtonState == MouseButtonState.Pressed;
                if (!_isCanvasLeftMouseDown && _canvasIndex == 0)
                {
                    _curve.ApplyAngleToCurve(_curve.OptAngle);
                    _curve.ApplyChanges();
                }

                break;
            }
            case MouseButton.Right:
                _isCanvasRightMouseDown = e.ButtonState == MouseButtonState.Pressed;
                break;
        }
        if (e.ButtonState == MouseButtonState.Released)
        {
            Mouse.Capture(null);
        }
    }

    private void CanvasSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var canvasSize = new Vector2((float)e.NewSize.Width, (float)e.NewSize.Height);
        var newCenter = canvasSize / 2;
        foreach (var camera in _cameraArray)
        {
            camera.Move(camera.Center - newCenter);
            camera.Center = newCenter;
        }
    }

    public int CanvasIndex
    {
        get => _canvasIndex;
        set
        {
            if (value >= _cameraArray.Length || value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "CanvasIndex must be less than the number of canvases.");
            }

            _canvasIndex = value;
        }
    }

}