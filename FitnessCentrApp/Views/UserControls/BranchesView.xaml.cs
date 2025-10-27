using DbFirst.Models;
using DbFirst.Services;
using FitnessCentrApp.ViewModels;
using System;
using System.Collections.Generic;
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

namespace FitnessCentrApp.Views.UserControls;

/// <summary>
/// Логика взаимодействия для BranchesView.xaml
/// </summary>
public partial class BranchesView : UserControl
{
    private BranchesViewModel viewModel = new();
    //private Repository<Branch> repository = new Repository<Branch>();
    public BranchesView()
    {
        InitializeComponent();

        DataContext = viewModel;
        //repository.Add(new Branch { BranchName = "Test", Address = "Addr", Phone = "123", Email = "a@a.com" });
        //_repo.Add(new Branch { BranchName = "Test", Address = "Addr", Phone = "123", Email = "a@a.com" });
    }
}
