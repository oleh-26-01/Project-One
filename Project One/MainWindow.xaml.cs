using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Project_One;

public partial class MainWindow
{
    private readonly string _filesPath = "C:\\Coding\\C#\\Project One\\Project One\\Curves\\";
    private readonly string[] _parts = { "First", "Second", "Third" };
    private int _partIndex;
    public MainWindow()
    {
        InitializeComponent();

        _partIndex = 0;
        FirstCanvas.Init(FirstTopPanel);
        FirstTopPanel.Init(this, FirstCanvas, FirstSidePanel);
        FirstSidePanel.Init(FirstCanvas, FirstTopPanel, _filesPath);

        FirstCanvas.StartUpdates();

        AddHandler(Keyboard.KeyDownEvent, new KeyEventHandler(CloseWindow), true);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
    }

    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        FirstCanvas.StopUpdates();
        if (Keyboard.IsKeyDown(Key.Q) || Keyboard.IsKeyDown(Key.Escape))
            Close();
    }

    private void ChangePart(object sender, RoutedEventArgs e)
    {
        if ((Button)sender == NextPartControl)
            _partIndex++;
        else if ((Button)sender == PrevPartControl) 
            _partIndex--;

        _partIndex += _parts.Length;
        _partIndex %= _parts.Length;

        PartTitle.Text = $"{_parts[_partIndex]} Part";

        FirstTopPanel.Visibility = _partIndex == 0 ? Visibility.Visible : Visibility.Hidden;
        FirstSidePanel.Visibility = _partIndex == 0 ? Visibility.Visible : Visibility.Hidden;
        FirstCanvas.Visibility = _partIndex == 0 ? Visibility.Visible : Visibility.Hidden;

        SecondTopPanel.Visibility = _partIndex == 1 ? Visibility.Visible : Visibility.Hidden;
        SecondSidePanel.Visibility = _partIndex == 1 ? Visibility.Visible : Visibility.Hidden;
        SecondCanvas.Visibility = _partIndex == 1 ? Visibility.Visible : Visibility.Hidden;

        ThirdTopPanel.Visibility = _partIndex == 2 ? Visibility.Visible : Visibility.Hidden;
        ThirdSidePanel.Visibility = _partIndex == 2 ? Visibility.Visible : Visibility.Hidden;
        ThirdCanvas.Visibility = _partIndex == 2 ? Visibility.Visible : Visibility.Hidden;

        FirstCanvas.StopUpdates();

        switch (_partIndex)
        {
            case 0:
                FirstCanvas.StartUpdates();
                break;
        }
    }
}