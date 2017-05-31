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
        private FastCollection<Flat> _fcFlats;

        private bool _isProcessing;
        private string _excelFile;

        
        async private void BtnLoadExcel_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = "*.xlsx";
            dlg.Multiselect = false;
            var dlgRes = dlg.ShowDialog();
            if (dlgRes == false)
                return;
            _excelFile = dlg.FileName;
            _fcSuburbs.Clear();
            await LoadExcelSheet();
        }

         async private void BtnStartWork_Click(object sender, RoutedEventArgs e)
        {
            if (_fcSuburbs.Count == 0)
            {
                MessageBox.Show("Please load excel sheet before starting process", "Info");
                return;
            }

            UpdateUi(true);

            await StartProcess();

            UpdateUi(false);

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

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            if(CmbSuburbs.SelectedIndex==0)
                DgSuburbs.ItemsSource = _fcFlats;
            else
            {
                var list = _fcFlats.Where(x => x.SubUrb == CmbSuburbs.Text).ToList();
                DgSuburbs.ItemsSource = list;
            }
            DgSuburbs.Items.Refresh();
        }

        async private void Init()
        {
            _fcSuburbs = new FastCollection<ExcelDisplay>();
            
            DgExcelSheet.IsReadOnly = true;
            DgExcelSheet.ItemsSource = _fcSuburbs;
            DgExcelSheet.MinColumnWidth = 100;

            _fcFlats = new FastCollection<Flat>();

            DgSuburbs.AutoGenerateColumns = false;
            var tc1 = new DataGridTextColumn
            {
                Header="Name",
                Width=300,
                Binding=new Binding("Name")
            };
            var tc2 = new DataGridTextColumn
            {
                Header = "Price",
                Binding = new Binding("Price")
            };

            DgSuburbs.Columns.Add(tc1);
            DgSuburbs.Columns.Add(tc2);

            DgSuburbs.IsReadOnly = true;

            DgSuburbs.ItemsSource = _fcFlats;
            
            _isProcessing = false;
            BtnStopWork.IsEnabled = _isProcessing;
        }

        private void UpdateUi(bool processing)
        {
            _isProcessing = processing;
            BtnStartWork.IsEnabled = !processing;
            BtnStopWork.IsEnabled = processing;
            BtnFilter.IsEnabled = !processing;
            BtnLoadExcel.IsEnabled = !processing;
            CmbSuburbs.IsEnabled = !processing;
            Cursor =processing==true? Cursors.Wait:Cursors.Arrow;
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

        async private Task LoadExcelSheet()
        {
            try
            {
                CmbSuburbs.Items.Clear();
                CmbSuburbs.Items.Add("All");

                using (ExcelPackage ep = new ExcelPackage(new FileInfo(_excelFile)))
                {
                    var suburbsSheet = ep.Workbook.Worksheets["SuburbList"];
                    var rowCount = suburbsSheet.Dimension.End.Row;

                    for (int i = 0; i < rowCount-1; i++)
                    {
                        if (suburbsSheet.Cells[i+2, 1].Value == null)
                            break;
                        var suburb = suburbsSheet.Cells[i+2, 1].Value.ToString();
                        if (suburbsSheet.Cells[i+2, 2].Value == null)
                            break;
                        var state = suburbsSheet.Cells[i+2, 2].Value.ToString();

                        var excelDisp = new ExcelDisplay
                        {
                            SubUrb = suburb,
                            State = state
                        };
                        _fcSuburbs.Add(excelDisp);
                        CmbSuburbs.Items.Add(suburb);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }

            UpdateLog("Total "+_fcSuburbs.Count.ToString()+" suburbs are loaded");

            _fcFlats.Clear();
            CmbSuburbs.SelectedIndex = 0;
        }


        async private Task StartProcess()
        {
            Search search = new Search();
            //UpdateLog("Search started");
            int subCount = 0;
            
            foreach(var sub in _fcSuburbs)
            {
                
                UpdateLog("Searching " + sub.SubUrb);
                try
                {
                    if (subCount == 0)
                    {
                        await search.InitFlatmateSearch();
                        await Task.Delay(5000);
                    }

                    UpdateLog("Getting webpage");
                    var html =await search.SearchFlat(sub.SubUrb, sub.State);
                    UpdateLog("Webpage successfully received");
                    UpdateLog("Getting flat details from the webpage");
                    var list = search.GetFlatDetails(html, sub.SubUrb);
                    UpdateLog(list.Count + " flat details found");
                    _fcFlats.AddRange(list);
                }
                catch(Exception ex)
                {
                    UpdateLog("Error: " + ex.Message);
                }
                

                if (_isProcessing == false)
                    break;

                await Task.Delay(45000);

                subCount++;
                if (subCount == 10)
                    subCount = 0;
            }
            //await search.DownloadWebPage("");
            //var html = File.ReadAllText(@"c:\temp\f.htm");
            //var token = search.GetToken(html);
            UpdateLog("Search complete");
        }



        
    }
}
