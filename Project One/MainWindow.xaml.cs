using System;
using System.IO;
using Project_One.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Project_One.Drawing.Wrappers;

namespace Project_One;

public partial class MainWindow
{
    private const string FilesPath = "D:\\FreeC\\#Coding\\C#\\Project One\\Project One\\Curves\\";
    private readonly string[] _parts = { "First", "Second", "Third" };
    private int _partIndex;

    public MainWindow()
    {
        InitializeComponent();

        _partIndex = 0;
        FirstCanvas.Init(FirstTopPanel);
        FirstTopPanel.Init(this, FirstCanvas, FirstSidePanel);
        FirstSidePanel.Init(FirstCanvas, FirstTopPanel, FilesPath);

        FirstCanvas.StartUpdates();

        SecondCanvas.Init(SecondTopPanel.CarDirectionLabel, SecondTopPanel.CollisionLabel, SecondTopPanel.CpsLabel);
        SecondTopPanel.Init(SecondCanvas, SecondSidePanel);

        SecondSidePanel.Init(SecondCanvas, FilesPath);

        ThirdCanvas.Init();
        //Close();
        ThirdTopPanel.Init(ThirdCanvas, ThirdSidePanel);

        ThirdSidePanel.Init(ThirdCanvas, FilesPath);

        AddHandler(Keyboard.KeyDownEvent, new KeyEventHandler(CloseWindow), true);

        Closed += (_, _) =>
        {
            FirstCanvas.StopUpdates();
            SecondCanvas.StopUpdates();
            ThirdCanvas.StopUpdates();
        };

        ChangePart(PrevPartControl, null!);
    }

    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        if (Keyboard.IsKeyDown(Key.Q) || Keyboard.IsKeyDown(Key.Escape))
        {
            FirstCanvas.StopUpdates();
            SecondCanvas.StopUpdates();
            ThirdCanvas.StopUpdates();
            Close();
        }
    }

    private void ChangePart(object sender, RoutedEventArgs e)
    {
        if ((Button)sender == NextPartControl)
            _partIndex++;
        else if ((Button)sender == PrevPartControl) _partIndex--;

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
        SecondCanvas.StopUpdates();
        ThirdCanvas.StopUpdates();

        switch (_partIndex)
        {
            case 0:
                FirstCanvas.StartUpdates();
                break;
            case 1:
                SecondCanvas.StartUpdates();
                break;
            case 2:
                ThirdCanvas.StartUpdates();
                break;
        }
    }
}