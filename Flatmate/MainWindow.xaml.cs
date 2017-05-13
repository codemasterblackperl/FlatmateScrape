using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Flatmate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private FastCollection<ExcelDisplay> _fcSuburbs;

        private bool _isProcessing;
        private string _excelFile;

        
        private void BtnLoadExcel_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = "*.xlsx";
            dlg.Multiselect = false;
            var dlgRes = dlg.ShowDialog();
            if (dlgRes == false)
                return;
            _excelFile = dlg.FileName;
            LoadExcelSheet();
        }

        private void BtnStartWork_Click(object sender, RoutedEventArgs e)
        {
            if (_fcSuburbs.Count == 0)
            {
                MessageBox.Show("Please load excel sheet before starting process", "Info");
                return;
            }

            _isProcessing = true;
            BtnStopWork.IsEnabled = true;

            
        }

        private void BtnStopWork_Click(object sender, RoutedEventArgs e)
        {
            _isProcessing = false;
            BtnStopWork.IsEnabled = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        async private void Init()
        {
            _fcSuburbs = new FastCollection<ExcelDisplay>();
            DgExcelSheet.IsReadOnly = true;
            DgExcelSheet.ItemsSource = _fcSuburbs;
            DgExcelSheet.MinColumnWidth = 50;
            _isProcessing = false;
            BtnStopWork.IsEnabled = _isProcessing;
        }

        private void UpdateLog(string text)
        {
            var message = "[ " + DateTime.Now.ToString() + " ]:  " + text+"\r\n";
            TxtLog.AppendText(message);
            if (TxtLog.Text.Length > 100000)
            {
                TxtLog.Text.Remove(0, 20000);
            }
            TxtLog.ScrollToEnd();
        }

        async private void LoadExcelSheet()
        {
            try
            {
                using (ExcelPackage ep = new ExcelPackage(new FileInfo(_excelFile)))
                {
                    var suburbsSheet = ep.Workbook.Worksheets["SuburbList"];
                    var rowCount = suburbsSheet.Dimension.End.Row;

                    for(int i = 0; i < rowCount; i++)
                    {
                        var excelDisp = new ExcelDisplay
                        {
                            SubUrb = suburbsSheet.Cells[i, 0].Value.ToString(),
                            State=suburbsSheet.Cells[i,1].Value.ToString()
                        };
                        _fcSuburbs.Add(excelDisp);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

    }
}
