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

using System.Data;
using System.Windows.Controls.Primitives;

namespace ANTLRReasoningCounter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Scanner scanner;

        public MainWindow()
        {
            InitializeComponent();
            //SourceCodeInfoInterfaceClass.ResetDB();
            DisplayDBinformation();
        }

        private void OnCheckedDBMode(object sender, RoutedEventArgs e)
        {
            DisplayDBinformation();
        }

        private void OnCheckedFileScanMode(object sender, RoutedEventArgs e)
        {
            ClearDisplay();
        }

        public void DisplayDBinformation()
        {
            try
            {
                dataGridFiles.ItemsSource = SourceCodeInfoInterfaceClass.GetFileData();
            }
            catch (System.Exception ex)
            {
                //Console.Write("\nThere was a problem with the GetFileData returning null.");
                //Console.Write(ex);
            }


            try
            {
                dataGridPackages.ItemsSource = SourceCodeInfoInterfaceClass.GetPackageData();
            }
            catch (System.Exception ex)
            {
                //Console.Write("\nThere was a problem with the GetPackageData returning null.");
                //Console.Write(ex);
            }

            try
            {
                dataGridClasses.ItemsSource = SourceCodeInfoInterfaceClass.GetClassData();
            }
            catch (System.Exception ex)
            {
                //Console.Write("\nThere was a problem with the GetClassData returning null.");
                //Console.Write(ex);
            }

            try
            {
                dataGridMethods.ItemsSource = SourceCodeInfoInterfaceClass.GetMethodData();
            }
            catch (System.Exception ex)
            {
                //Console.Write("\nThere was a problem with the GetMethodData returning null.");
                //Console.Write(ex);
            }
        }

        private void OnClickBrowseButton(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".java";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true){ textBoxFilepath.Text = dlg.FileName;}
        }

        private void OnClickScanButton(object sender, RoutedEventArgs e)
        {
            scanner = new Scanner(textBoxFilepath.Text);
            scanner.Scan();
            DisplayScanner();
        }

        private void OnClickAddToDBButton(object sender, RoutedEventArgs e)
        {
            SourceCodeInfoInterfaceClass.AddScanToDB(scanner);
        }

        private void OnClickClearDBButton(object sender, RoutedEventArgs e)
        {
            SourceCodeInfoInterfaceClass.ResetDB();
            if (displayModeDB.IsChecked == true){ ClearDisplay(); }
        }

        private void ClearDisplay()
        {
            List<SourceCodeFile> defaultGrid1 = new List<SourceCodeFile>();
            dataGridFiles.ItemsSource = defaultGrid1;

            List<PackageInfo> defaultGrid2 = new List<PackageInfo>();
            dataGridPackages.ItemsSource = defaultGrid2;

            List<ClassInfo> defaultGrid3 = new List<ClassInfo>();
            dataGridClasses.ItemsSource = defaultGrid3;

            List<MethodInfo> defaultGrid4 = new List<MethodInfo>();
            dataGridMethods.ItemsSource = defaultGrid4;
        }

        private void DisplayScanner()
        {
            List<SourceCodeFile> fileData = scanner.GetFileGrid();
            dataGridFiles.ItemsSource = fileData;
            foreach(SourceCodeFile scf in fileData)
            {
                DataGridCell dgc = DataGridHelper.GetCell(dataGridFiles, 0, 2); 
            }

            dataGridPackages.ItemsSource = scanner.GetPackageGrid();
            dataGridClasses.ItemsSource = scanner.GetClassGrid();
            dataGridMethods.ItemsSource = scanner.GetMethodGrid();
        }

    }

    static class DataGridHelper
    {
        static public DataGridCell GetCell(DataGrid dg, int row, int column)
        {
            DataGridRow rowContainer = GetRow(dg, row);

            if (rowContainer != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                // try to get the cell but it may possibly be virtualized
                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                if (cell == null)
                {
                    // now try to bring into view and retreive the cell
                    dg.ScrollIntoView(rowContainer, dg.Columns[column]);
                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                }
                return cell;
            }
            return null;
        }

        static public DataGridRow GetRow(DataGrid dg, int index)
        {
            DataGridRow row = (DataGridRow)dg.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                // may be virtualized, bring into view and try again
                dg.ScrollIntoView(dg.Items[index]);
                row = (DataGridRow)dg.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
    }

}
