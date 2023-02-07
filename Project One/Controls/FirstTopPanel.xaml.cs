using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Project_One.Helpers;
using Project_One_Objects;

namespace Project_One;

public partial class FirstTopPanel : UserControl
{
    private FirstSidePanel _firstSidePanel;
    private WpfCurve _curve;
    private WpfCurveEraser _curveEraser;
    private UIElement _window;
    public readonly double TextBoxMinValue = 1;
    public readonly double TextBoxMaxValue = 30;

    public FirstTopPanel()
    {
        InitializeComponent();
    }

    public void Init(FirstSidePanel firstSidePanelWpfCurve, WpfCurve curve, WpfCurveEraser curveEraser, UIElement window)
    {
        _firstSidePanel = firstSidePanelWpfCurve;
        _curve = curve;
        _curveEraser = curveEraser;
        _window = window;

        Update();

        DrawCurve.Checked += DrawCurve_OnChecked;
        EraseCurve.Checked += EraseCurve_OnChecked;

        CurveOptAngle.TextChanged += CurveOptAngle_OnTextChanged;
    }

    public void Update()
    {
        var curveOptAngle = Math.Round(_curve.OptAngle.ToDeg(), 1).ToString(CultureInfo.InvariantCulture);
        if (CurveOptAngle.Text != curveOptAngle)
            CurveOptAngle.Text = curveOptAngle;
        CurvePointsLabel.Content = "Points: " + _curve.Points.Count;
    }

    public void DrawCurve_OnChecked(object sender, RoutedEventArgs e)
    {
        _curveEraser.SetVisibility(false);
        OnCurveAction = Strings.DrawAction;
    }

    public void EraseCurve_OnChecked(object sender, RoutedEventArgs e)
    {
        _curveEraser.SetVisibility(true);
        OnCurveAction = Strings.EraseAction;
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

        if (OnCurveAction == Strings.ChangingOptAngleAction) 
            _curve.ApplyChanges();
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
        _firstSidePanel.SaveCurve(sender, e, Strings.NewFileAction);
        Update();
    }

    private void SaveCurve_OnClick(object sender, RoutedEventArgs e)
    {
        ConfirmOptAngle_OnClick(sender, e);
        _firstSidePanel.SaveCurve(sender, e, Strings.UpdateFileAction);
    }

    private void ClearCurve_OnClick(object sender, RoutedEventArgs e)
    {
        _curve.Clear();
        _curve.OptAngle = Curve.DefaultOptAngle;
        ConfirmOptAngle_OnClick(sender, e);
    }

    /// <summary>Represents the current action on the curve.</summary>
    public string OnCurveAction { get; set; } = Strings.DrawAction;
}