using System;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_One_Objects;

namespace Project_One;

public class EventHandler
{
    private readonly Grid _windowGrid;
    private readonly Grid _canvasGrid;
    private readonly Grid _firstTopPanelGrid;
    private readonly CanvasUpdater _canvasUpdater;
    private readonly FirstTopPanel _firstTopPanel;
    private readonly SidePanel _sidePanel;
    private readonly Camera[] _cameraArray;
    private readonly WpfCurve _curve;
    private readonly WpfCurveEraser _curveEraser;
    public int _canvasIndex = 0;

    private bool _isCanvasLeftMouseDown;
    private bool _isCanvasRightMouseDown;
    private bool _isArrowUpPressed;
    private bool _isArrowDownPressed;
    private bool _isArrowLeftPressed;
    private bool _isArrowRightPressed;

    public Vector2 MousePosition = new(-1, -1);
    public Vector2 CameraRelativeMousePosition = new(-1, -1);
    private Vector2 _cameraMoveDirection;

    public EventHandler(
        UIElement window,
        Grid windowGrid, Grid canvasGrid, 
        CanvasUpdater canvasUpdater, FirstTopPanel firstTopPanel, SidePanel sidePanel,
        Camera[] cameraArray, WpfCurve curve, WpfCurveEraser curveEraser)
    {
        _windowGrid = windowGrid;
        _canvasGrid = canvasGrid;

        _canvasUpdater = canvasUpdater;
        _firstTopPanel = firstTopPanel;
        _sidePanel = sidePanel;

        _cameraArray = cameraArray;
        _curve = curve;
        _curveEraser = curveEraser;

        _windowGrid.MouseMove += MouseMove;
        _windowGrid.MouseWheel += MouseWheel;
        
        _canvasGrid.MouseDown += CanvasMouseClick;
        _canvasGrid.MouseUp += CanvasMouseClick;
        _canvasGrid.SizeChanged += CanvasSizeChanged;

        window.AddHandler(Keyboard.KeyDownEvent, new KeyEventHandler(WindowGridKeyDown), true);
        window.AddHandler(Keyboard.KeyUpEvent, new KeyEventHandler(WindowGridKeyUp), true);

        //_windowGrid.PreviewKeyDown += WindowGridKeyDown;
        //_windowGrid.PreviewKeyUp += WindowGridKeyUp;

        _firstTopPanel.NewCurve.Click += (object sender, RoutedEventArgs e) =>
        {
            _firstTopPanel.ClearActiveCurve(sender, e);
            _firstTopPanel.SaveCurveOptAngle(sender, e);
            _sidePanel.SaveCurve(sender, e, "new");
        };
        _firstTopPanel.SaveCurve.Click += (object sender, RoutedEventArgs e) =>
        {
            _firstTopPanel.SaveCurveOptAngle(sender, e);
            _sidePanel.SaveCurve(sender, e, "active");
        };
        _firstTopPanel.ClearCurve.Click += (object sender, RoutedEventArgs e) =>
        {
            _firstTopPanel.ClearActiveCurve(sender, e);
            _firstTopPanel.SaveCurveOptAngle(sender, e);
        };
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
        else if (e.Key is Key.Q or Key.Escape)
        {
            Exit();
        }
        UpdateCamera();
    }

    private void Exit()
    {
        _canvasUpdater.StopThread();
        Application.Current.Shutdown();
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

        CameraRelativeMousePosition = _cameraArray[_canvasIndex].ConvertIn(newMousePosition);

        switch (_canvasIndex)
        {
            case 0:
                _curveEraser.MoveToNearPoint(_curve.Points, CameraRelativeMousePosition);
                if (_isCanvasLeftMouseDown)
                {
                    switch (_firstTopPanel.OnCurveAction)
                    {
                        case "Draw":
                            _curve.AddPoint(CameraRelativeMousePosition);
                            break;
                        case "Erase":
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
                        //_curve.ApplyAngleToCurve(_curve.OptAngle);
                        //_curve.ApplyChanges();
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

    public void DeleteCurve_OnClick(object sender, RoutedEventArgs routedEventArgs)
    {
        _sidePanel.DeleteCurve(sender, routedEventArgs);
    }

    public void SelectCurve_OnClick(object sender, RoutedEventArgs routedEventArgs)
    {
        _sidePanel.SelectCurve(sender, routedEventArgs);
        _sidePanel.Update();
        _firstTopPanel.Update();
        _firstTopPanel.SaveCurveOptAngle(sender, routedEventArgs);
    }
}