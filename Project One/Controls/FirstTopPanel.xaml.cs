using System;
using System.Collections.Generic;
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
using Project_One_Objects;

namespace Project_One;

public partial class FirstTopPanel : UserControl
{
    private WpfCurve _curve;
    private WpfCurveEraser _curveEraser;
    public readonly double TextBoxMinValue = 1;
    public readonly double TextBoxMaxValue = 30;

    public FirstTopPanel()
    {
        InitializeComponent();
    }

    public void Init(WpfCurve curve, WpfCurveEraser curveEraser)
    {
        _curve = curve;
        _curveEraser = curveEraser;

        DrawCurve.Checked += DrawEraseCurve;
        EraseCurve.Checked += DrawEraseCurve;

        CurveOptAngle.Text = Math.Round(_curve.OptAngle * Curve.ToDeg, 1).ToString();
        CurveOptAngle.TextChanged += CurveOptAngleTextBox_TextChanged;
        DecreaseTextBox.Click += DecreaseTextBoxButton_Click;
        IncreaseTextBox.Click += IncreaseTextBoxButton_Click;
        ConfirmTextBox.Click += SaveCurveOptAngle;
    }

    public void Update()
    {
        var curveOptAngle = Math.Round(_curve.OptAngle * Curve.ToDeg, 1).ToString(CultureInfo.InvariantCulture);
        if (CurveOptAngle.Text != curveOptAngle)
            CurveOptAngle.Text = curveOptAngle;
        CurvePointsLabel.Content = "Points: " + _curve.Points.Count;
    }

    public void DrawEraseCurve(object sender, RoutedEventArgs e)
    {
        if ((sender is not RadioButton radioButton)) return;
        _curveEraser.SetVisibility(radioButton.Name == "EraseCurve");
        if (OnCurveAction == "ChangingOptAngle") return;
        OnCurveAction = radioButton.Name switch
        {
            "DrawCurve" => "Draw",
            "EraseCurve" => "Erase",
            _ => throw new Exception("Unknown radio button name: " + radioButton.Name)
        };
    }

    private void CurveOptAngleTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!double.TryParse(CurveOptAngle.Text, out var value)) return;
        value = Math.Clamp(value, TextBoxMinValue, TextBoxMaxValue);
        CurveOptAngle.Text = value.ToString(CultureInfo.InvariantCulture);
        _curve.OptAngle = value * Curve.ToRad;
        
        OnCurveAction = "ChangingOptAngle";
        CurveOptAngleLabel.Foreground = System.Windows.Media.Brushes.Red;
        ConfirmTextBox.Foreground = System.Windows.Media.Brushes.Red;
        _curve.ApplyAngleToCurve(_curve.OptAngle);
        
        Update();
    }

    private void DecreaseTextBoxButton_Click(object sender, RoutedEventArgs e)
    {
        var value = double.Parse(CurveOptAngle.Text) - 1;
        CurveOptAngle.Text = value.ToString(CultureInfo.InvariantCulture);
    }

    private void IncreaseTextBoxButton_Click(object sender, RoutedEventArgs e)
    {
        var value = double.Parse(CurveOptAngle.Text) + 1;
        CurveOptAngle.Text = value.ToString(CultureInfo.InvariantCulture);
    }

    public void SaveCurveOptAngle(object sender, RoutedEventArgs e)
    {
        if (DrawCurve.IsChecked is not { } drawCurveIsChecked) return;
        if (OnCurveAction == "ChangingOptAngle") 
            _curve.ApplyChanges();
        Keyboard.ClearFocus();
        OnCurveAction = drawCurveIsChecked ? "Draw" : "Erase";
        _curveEraser.SetVisibility(OnCurveAction == "Erase");
        CurveOptAngleLabel.Foreground = System.Windows.Media.Brushes.Black;
        ConfirmTextBox.Foreground = System.Windows.Media.Brushes.Black;
    }

    public void ClearActiveCurve(object sender, RoutedEventArgs e)
    {
        _curve.Clear();
        CurveOptAngle.Text = "0";
        CurveOptAngle.Text = "1";
    }

    /// <summary>Represents the current action on the curve.</summary>
    public string OnCurveAction { get; set; } = "Draw";
}