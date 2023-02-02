using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Project_One;

public class SidePanel
{
    public ObservableCollection<CurveViewModel> CurveViewModels;
    public string FilesPath { get; set; }
    private readonly string _filesType = "crv";
    private readonly ItemsControl _curvesList;
    private readonly WpfCurve _curve;
    private CurveViewModel? _selectedCurve;

    public SidePanel(string filesPath, ItemsControl curvesList, WpfCurve curve)
    {
        FilesPath = filesPath;
        _curvesList = curvesList;
        _curve = curve;
        
        CurveViewModels = new ObservableCollection<CurveViewModel>();
        _curvesList.ItemsSource = CurveViewModels;
        
        Update();
    }

    public void Update()
    {
        CurveViewModels.Clear();
        var files = System.IO.Directory.GetFiles(FilesPath);
        foreach (var file in files)
        {
            if (!file.EndsWith(_filesType)) continue;
            var curve = new WpfCurve(file);
            var curveViewModel = new CurveViewModel
            {
                FileName = file,
                PointCount = curve.Points.Count
            };
            CurveViewModels.Add(curveViewModel);
        }
    }

    public void SaveActiveCurve(object sender, RoutedEventArgs e)
    {
        string curveFileName;
        if (_selectedCurve == null)
        {
            var index = 1;
            while (true)
            {
                curveFileName = FilesPath + "curve" + index.ToString().PadLeft(3, '0') + "." + _filesType;
                if (!File.Exists(curveFileName)) break;
                index++;
            }
            _selectedCurve = new CurveViewModel
            {
                FileName = curveFileName,
                PointCount = _curve.Points.Count
            };
        }
        else
        {
            curveFileName = _selectedCurve.FileName;
        }

        _curve.Save(curveFileName);
        Update();
    }

    public void SelectCurve(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).DataContext is CurveViewModel curveViewModel)
        {
            _curve.Load(curveViewModel.FileName);
            _selectedCurve = curveViewModel;
        }
    }

    public void DeleteCurve(object sender, RoutedEventArgs e)
    {
        var messageBoxResult = MessageBox.Show("Confirm deleting?", caption: "Confirm", button: MessageBoxButton.YesNo);

        switch (messageBoxResult)
        {
            case MessageBoxResult.Yes:
            {
                if (((Button)sender).DataContext is CurveViewModel curveViewModel)
                {
                    File.Delete(curveViewModel.FileName);
                    CurveViewModels.Remove(curveViewModel);
                }

                break;
            }
            case MessageBoxResult.No:
                Console.WriteLine("No");
                break;
        }
    }

    public string FilesType
    {
        get => _filesType;
        set => throw new AccessViolationException("Files type can't be changed");
    }
}