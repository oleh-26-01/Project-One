using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Project_One_Objects;

namespace Project_One;

public class TopPanel
{
    private readonly Grid _firstTopPanel;
    private readonly Label _curvePointsLabel;
    private readonly WpfCurve _curve;
    private readonly WpfCurveEraser _curveEraser;

    public readonly RadioButton DrawCurveRadioButton;
    public readonly RadioButton EraseCurveRadioButton;
    private string _onCurveAction = "Draw";

    public readonly Label CurveOptAngleLabel;
    public readonly TextBox CurveOptAngleTextBox;
    public readonly Button DecreaseTextBoxButton;
    public readonly Button IncreaseTextBoxButton;
    public readonly Button ConfirmTextBoxButton;
    
    public readonly double TextBoxMinValue = 1;
    public readonly double TextBoxMaxValue = 30;

    public readonly Button NewCurveButton;
    public readonly Button SaveCurveButton;
    public readonly Button ClearCurveButton;

    public TopPanel(Grid firstTopPanel, WpfCurve curve, WpfCurveEraser curveEraser)
    {
        _firstTopPanel = firstTopPanel;
        _curvePointsLabel = (Label)_firstTopPanel.Children[2];
        _curve = curve;
        _curveEraser = curveEraser;

        CurveOptAngleLabel = firstTopPanel.FindName("CurveOptAngleLabel") as Label;
        CurveOptAngleTextBox = firstTopPanel.FindName("CurveOptAngle") as TextBox;
        DecreaseTextBoxButton = firstTopPanel.FindName("DecreaseTextBox") as Button;
        IncreaseTextBoxButton = firstTopPanel.FindName("IncreaseTextBox") as Button;
        ConfirmTextBoxButton = firstTopPanel.FindName("ConfirmTextBox") as Button;

        NewCurveButton = firstTopPanel.FindName("NewCurve") as Button;
        SaveCurveButton = firstTopPanel.FindName("SaveCurve") as Button;
        ClearCurveButton = firstTopPanel.FindName("ClearCurve") as Button;

        DrawCurveRadioButton = firstTopPanel.FindName("DrawCurve") as RadioButton;
        EraseCurveRadioButton = firstTopPanel.FindName("EraseCurve") as RadioButton;

        DrawCurveRadioButton.Checked += DrawEraseCurve;
        EraseCurveRadioButton.Checked += DrawEraseCurve;

        CurveOptAngleTextBox.Text = Math.Round(_curve.OptAngle * Curve.ToDeg, 1).ToString(CultureInfo.InvariantCulture);
        CurveOptAngleTextBox.TextChanged += CurveOptAngleTextBox_TextChanged;
        DecreaseTextBoxButton.Click += DecreaseTextBoxButton_Click;
        IncreaseTextBoxButton.Click += IncreaseTextBoxButton_Click;
        ConfirmTextBoxButton.Click += SaveCurveOptAngle;

        Update();
    }

    public void Update()
    {
        var curveOptAngle = Math.Round(_curve.OptAngle * Curve.ToDeg, 1).ToString(CultureInfo.InvariantCulture);
        if (CurveOptAngleTextBox.Text != curveOptAngle)
            CurveOptAngleTextBox.Text = curveOptAngle;
        _curvePointsLabel.Content = "Points: " + _curve.Points.Count;
    }

    public void DrawEraseCurve(object sender, RoutedEventArgs e)
    {
        if ((sender is not RadioButton radioButton)) return;
        _curveEraser.SetVisibility(radioButton.Name == "EraseCurve");
        if (_onCurveAction == "ChangingOptAngle") return;
        _onCurveAction = radioButton.Name switch
        {
            "DrawCurve" => "Draw",
            "EraseCurve" => "Erase",
            _ => throw new Exception("Unknown radio button name: " + radioButton.Name)
        };
    }

    public void SaveCurveOptAngle(object sender, RoutedEventArgs e)
    {
        if (DrawCurveRadioButton.IsChecked is not { } drawCurveIsChecked) return;
        if (_onCurveAction == "ChangingOptAngle") 
            _curve.ApplyChanges();
        _onCurveAction = drawCurveIsChecked ? "Draw" : "Erase";
        _curveEraser.SetVisibility(_onCurveAction == "Erase");
        CurveOptAngleLabel.Foreground = System.Windows.Media.Brushes.Black;
        ConfirmTextBoxButton.Foreground = System.Windows.Media.Brushes.Black;
    }

    public void ClearActiveCurve(object sender, RoutedEventArgs e)
    {
        _curve.Clear();
        CurveOptAngleTextBox.Text = "0";
        CurveOptAngleTextBox.Text = "1";
    }

    private void CurveOptAngleTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!double.TryParse(CurveOptAngleTextBox.Text, out var value)) return;
        value = Math.Clamp(value, TextBoxMinValue, TextBoxMaxValue);
        CurveOptAngleTextBox.Text = value.ToString(CultureInfo.InvariantCulture);
        _curve.OptAngle = value * Curve.ToRad;
        
        _onCurveAction = "ChangingOptAngle";
        CurveOptAngleLabel.Foreground = System.Windows.Media.Brushes.Red;
        ConfirmTextBoxButton.Foreground = System.Windows.Media.Brushes.Red;
        _curve.ApplyAngleToCurve(_curve.OptAngle);
        
        Update();
    }

    private void DecreaseTextBoxButton_Click(object sender, RoutedEventArgs e)
    {
        var value = double.Parse(CurveOptAngleTextBox.Text) - 1;
        CurveOptAngleTextBox.Text = value.ToString(CultureInfo.InvariantCulture);
    }

    private void IncreaseTextBoxButton_Click(object sender, RoutedEventArgs e)
    {
        var value = double.Parse(CurveOptAngleTextBox.Text) + 1;
        CurveOptAngleTextBox.Text = value.ToString(CultureInfo.InvariantCulture);
    }

    public string OnCurveAction
    {
        get => _onCurveAction;
        set => _onCurveAction = value;
    }
}