using System;
using System.ComponentModel;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_One.Helpers;
using Project_One_Objects;

namespace Project_One;

public class EventsHandler
{
    private readonly Camera[] _cameraArray;
    private readonly Grid _canvasGrid;
    private readonly CanvasUpdater _canvasUpdater;
    private readonly WpfCurve _curve;
    private readonly WpfCurveEraser _curveEraser;
    private readonly FirstSidePanel _firstSidePanel;
    private readonly FirstTopPanel _firstTopPanel;
    private readonly Grid _firstTopPanelGrid;
    private readonly Grid _windowGrid;
    private Vector2 _cameraMoveDirection;
    public int _canvasIndex;
    private bool _isArrowDownPressed;
    private bool _isArrowLeftPressed;
    private bool _isArrowRightPressed;
    private bool _isArrowUpPressed;

    private bool _isCanvasLeftMouseDown;
    private bool _isCanvasRightMouseDown;
    public Vector2 CameraRelativeMousePosition = new(-1, -1);

    public Vector2 MousePosition = new(-1, -1);

    public EventsHandler(
        UIElement window,
        Grid windowGrid, Grid canvasGrid,
        CanvasUpdater canvasUpdater, FirstTopPanel firstTopPanel, FirstSidePanel firstSidePanel,
        Camera[] cameraArray, WpfCurve curve, WpfCurveEraser curveEraser)
    {
        _windowGrid = windowGrid;
        _canvasGrid = canvasGrid;

        _canvasUpdater = canvasUpdater;
        _firstTopPanel = firstTopPanel;
        _firstSidePanel = firstSidePanel;

        _cameraArray = cameraArray;
        _curve = curve;
        _curveEraser = curveEraser;

        _canvasGrid.MouseDown += CanvasMouseClick;
        _canvasGrid.MouseUp += CanvasMouseClick;
        _canvasGrid.SizeChanged += CanvasSizeChanged;

        window.AddHandler(Mouse.MouseMoveEvent, new MouseEventHandler(MouseMove), true);
        window.AddHandler(Mouse.MouseWheelEvent, new MouseWheelEventHandler(MouseWheel), true);
        window.AddHandler(Keyboard.KeyDownEvent, new KeyEventHandler(WindowGridKeyDown), true);
        window.AddHandler(Keyboard.KeyUpEvent, new KeyEventHandler(WindowGridKeyUp), true);
    }

    public int CanvasIndex
    {
        get => _canvasIndex;
        set
        {
            if (value >= _cameraArray.Length || value < 0)
                throw new ArgumentOutOfRangeException(nameof(value),
                    "CanvasIndex must be less than the number of canvases.");

            _canvasIndex = value;
        }
    }

    public void WindowClosing(object? sender, CancelEventArgs e)
    {
        _canvasUpdater.StopThread();
    }

    private void Exit()
    {
        _canvasUpdater.StopThread();
        Application.Current.Shutdown();
    }

    private void WindowGridKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Up)
            _isArrowUpPressed = true;
        else if (e.Key == Key.Down)
            _isArrowDownPressed = true;
        else if (e.Key == Key.Left)
            _isArrowLeftPressed = true;
        else if (e.Key == Key.Right)
            _isArrowRightPressed = true;
        else if (e.Key == Key.Add)
            _cameraArray[_canvasIndex].ZoomIn(0.2f);
        else if (e.Key == Key.Subtract)
            _cameraArray[_canvasIndex].ZoomOut(0.2f);
        else if (e.Key is Key.Q or Key.Escape) Exit();
        UpdateCamera();
    }

    private void WindowGridKeyUp(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Up)
            _isArrowUpPressed = false;
        else if (e.Key == Key.Down)
            _isArrowDownPressed = false;
        else if (e.Key == Key.Left)
            _isArrowLeftPressed = false;
        else if (e.Key == Key.Right) _isArrowRightPressed = false;
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
            _cameraArray[_canvasIndex].MoveSync(
                Vector2.Normalize(_cameraMoveDirection),
                _canvasUpdater.TargetRefreshTime,
                1000);
    }

    private void MouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(_canvasGrid);
        var newMousePosition = new Vector2((float)position.X, (float)position.Y);
        if (newMousePosition == MousePosition) return;

        CameraRelativeMousePosition = _cameraArray[_canvasIndex].ConvertIn(newMousePosition);

        switch (_canvasIndex)
        {
            case 0:
                _curveEraser.MoveToNearPoint(_curve.Points, CameraRelativeMousePosition);
                if (_isCanvasLeftMouseDown)
                {
                    switch (_firstTopPanel.OnCurveAction)
                    {
                        case Strings.DrawAction:
                            _curve.AddPoint(CameraRelativeMousePosition);
                            break;
                        case Strings.EraseAction:
                            _curveEraser.ErasePoints(_curve.Points, CameraRelativeMousePosition, _cameraArray[0].Zoom);
                            break;
                    }

                    _firstTopPanel.Update();
                }
                else if (_isCanvasRightMouseDown)
                {
                    _cameraArray[_canvasIndex].Move((MousePosition - newMousePosition) / _cameraArray[0].Zoom);
                }

                break;

            case 1:
                break;
            case 2:
                break;
        }

        MousePosition = newMousePosition;
    }

    private void MouseWheel(object sender, MouseWheelEventArgs e)
    {
        // check if mouse is over canvas
        var position = e.GetPosition(_canvasGrid);
        if (position.X < 0 || position.X > _canvasGrid.ActualWidth ||
            position.Y < 0 || position.Y > _canvasGrid.ActualHeight)
            return;
        switch (e.Delta)
        {
            case > 0:
                _cameraArray[_canvasIndex].ZoomIn(e.Delta / 1000f);
                break;
            case < 0:
                _cameraArray[_canvasIndex].ZoomOut(-e.Delta / 1000f);
                break;
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
                if (!_isCanvasLeftMouseDown && _canvasIndex == 0 && _firstTopPanel.OnCurveAction == Strings.EraseAction)
                {
                    _curveEraser.EraseNearestPoint(_curve.Points);
                    _curveEraser.MoveToNearPoint(_curve.Points, CameraRelativeMousePosition);
                    _firstTopPanel.Update();
                }

                break;
            }
            case MouseButton.Right:
                _isCanvasRightMouseDown = e.ButtonState == MouseButtonState.Pressed;
                break;
        }

        if (e.ButtonState == MouseButtonState.Released) Mouse.Capture(null);
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
}