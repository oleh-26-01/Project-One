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
    private string _lastCurveFileName = "";

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
        var files = Directory.GetFiles(FilesPath);
        foreach (var file in files)
        {
            if (!file.EndsWith(_filesType)) continue;
            var curve = new WpfCurve(file);
            var curveViewModel = new CurveViewModel
            {
                FileName = file,
                PointCount = curve.Points.Count,
                IsSelected = false
            };

            if (_selectedCurve != null)
            {
                if (_selectedCurve.FileName == file)
                    curveViewModel = _selectedCurve;
            }
            
            CurveViewModels.Add(curveViewModel);
        }
    }

    public void SaveCurve(object sender, RoutedEventArgs e, string type)
    {
        var curveFileName = FindFreeCurveName();
        if (_selectedCurve != null && type == "Active")
        {
            _selectedCurve.IsSelected = false;
            curveFileName = _selectedCurve.FileName;
        }

        _selectedCurve = new CurveViewModel
        {
            FileName = curveFileName,
            PointCount = _curve.Points.Count,
            IsSelected = true
        };

        _curve.Save(curveFileName);
        Update();
    }

    public string FindFreeCurveName()
    {
        var index = 1;
        var curveFileName = "";
        while (true)
        {
            curveFileName = FilesPath + "curve" + index.ToString().PadLeft(3, '0') + "." + _filesType;
            if (!File.Exists(curveFileName)) break;
            index++;
        }

        return curveFileName;
    }

    public void SelectCurve(object sender, RoutedEventArgs e)
    {
        if (((Button)sender).DataContext is not CurveViewModel curveViewModel) return;
        
        _curve.Load(curveViewModel.FileName);
        if (_selectedCurve != null)
        {
            _selectedCurve.IsSelected = false;
        }
        _selectedCurve = curveViewModel;
        curveViewModel.IsSelected = true;
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
                break;
        }
    }

    public string FilesType
    {
        get => _filesType;
        set => throw new AccessViolationException("Files type can't be changed");
    }
}