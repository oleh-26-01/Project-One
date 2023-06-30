using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Project_One.Drawing.WpfOnly;
using Project_One.Drawing.Wrappers;
using Project_One.Helpers;
using Project_One_Objects.Environment;
using Project_One_Objects.Helpers;

namespace Project_One.Controls;

public partial class FirstTopPanel
{
    public readonly double TextBoxMaxValue = 30;
    public readonly double TextBoxMinValue = 1;
    private CurveWPF _curve;
    private WpfCurveEraser _curveEraser;
    private FirstSidePanel _firstSidePanel;
    private UIElement _window;

    public FirstTopPanel()
    {
        InitializeComponent();
    }

    /// <summary>Represents the current action on the curve.</summary>
    public string OnCurveAction { get; set; } = Strings.DrawAction;

    public void Init(UIElement window, FirstCanvas firstCanvas, FirstSidePanel firstSidePanel)
    {
        _firstSidePanel = firstSidePanel;
        _curve = firstCanvas.Curve;
        _curveEraser = firstCanvas.CurveEraser;
        _window = window;

        Update();

        DrawCurve.Checked += DrawCurve_OnChecked;
        EraseCurve.Checked += EraseCurve_OnChecked;

        CurveOptAngle.TextChanged += CurveOptAngle_OnTextChanged;
    }

    public void Update()
    {
        var curveOptAngle = Math.Round(_curve.OptAngle.ToDeg(), 1).ToString(CultureInfo.InvariantCulture);
        if (CurveOptAngle.Text != curveOptAngle) CurveOptAngle.Text = curveOptAngle;

        CurvePointsLabel.Content = "Points: " + _curve.Points.Count;
    }

    public void DrawCurve_OnChecked(object sender, RoutedEventArgs e)
    {
        _curveEraser.SetVisibility(false);
        if (OnCurveAction != Strings.ChangingOptAngleAction) OnCurveAction = Strings.DrawAction;
    }

    public void EraseCurve_OnChecked(object sender, RoutedEventArgs e)
    {
        _curveEraser.SetVisibility(true);
        if (OnCurveAction != Strings.ChangingOptAngleAction) OnCurveAction = Strings.EraseAction;
    }

    private void CurveOptAngle_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        CurveOptAngleLabel.Foreground = Brushes.Red;
        ConfirmOptAngle.Foreground = Brushes.Red;
        OnCurveAction = Strings.ChangingOptAngleAction;

        if (!double.TryParse(CurveOptAngle.Text, out var value)) return;

        value = Math.Clamp(value, TextBoxMinValue, TextBoxMaxValue);
        _curve.OptAngle = value.ToRad();
        _curve.ApplyAngleToCurve(_curve.OptAngle);

        Update();
    }

    private void DecreaseOptAngle_OnClick(object sender, RoutedEventArgs e)
    {
        var value = Math.Clamp(_curve.OptAngle.ToDeg() - 1, TextBoxMinValue, TextBoxMaxValue);
        _curve.OptAngle = value.ToRad();
        CurveOptAngle.Text = value.ToString(CultureInfo.InvariantCulture);
    }

    private void IncreaseOptAngle_OnClick(object sender, RoutedEventArgs e)
    {
        var value = Math.Clamp(_curve.OptAngle.ToDeg() + 1, TextBoxMinValue, TextBoxMaxValue);
        _curve.OptAngle = value.ToRad();
        CurveOptAngle.Text = value.ToString(CultureInfo.InvariantCulture);
    }

    public void ConfirmOptAngle_OnClick(object sender, RoutedEventArgs e)
    {
        if (DrawCurve.IsChecked is not { } drawCurveIsChecked) return;

        if (!double.TryParse(CurveOptAngle.Text, out _)) return;

        if (OnCurveAction == Strings.ChangingOptAngleAction) _curve.ApplyChanges();

        FocusManager.SetFocusedElement(FocusManager.GetFocusScope(CurveOptAngle), _window);
        OnCurveAction = drawCurveIsChecked ? Strings.DrawAction : Strings.EraseAction;
        _curveEraser.SetVisibility(OnCurveAction == Strings.EraseAction);
        CurveOptAngleLabel.Foreground = Brushes.Black;
        ConfirmOptAngle.Foreground = Brushes.Black;
    }

    public void NewCurve_OnClick(object sender, RoutedEventArgs e)
    {
        _curve.Clear();
        _curve.OptAngle = Curve.DefaultOptAngle;
        ConfirmOptAngle_OnClick(sender, e);
        _firstSidePanel.SaveCurve(sender, e, true);
        Update();
    }

    private void SaveCurve_OnClick(object sender, RoutedEventArgs e)
    {
        ConfirmOptAngle_OnClick(sender, e);
        _firstSidePanel.SaveCurve(sender, e, false);
    }

    private void ClearCurve_OnClick(object sender, RoutedEventArgs e)
    {
        _curve.Clear();
        _curve.OptAngle = Curve.DefaultOptAngle;
        Update();
        ConfirmOptAngle_OnClick(sender, e);

        OnCurveAction = Strings.DrawAction;
        DrawCurve.IsChecked = true;
        _curveEraser.SetVisibility(false);
    }
}