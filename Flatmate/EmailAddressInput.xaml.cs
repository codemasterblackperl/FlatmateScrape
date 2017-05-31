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
using System.Windows.Shapes;

namespace Flatmate
{
    /// <summary>
    /// Interaction logic for EmailAddressInput.xaml
    /// </summary>
    public partial class EmailAddressInput : Window
    {
        public EmailAddressInput()
        {
            InitializeComponent();

            var dir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)+"\\Flatmate_Tool";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            _userFile = dir + "\\data";

            if (File.Exists(_userFile))
            {
                try
                {
                    var data = File.ReadAllLines(_userFile);
                    TxtEmailAddress.Text = data[0];
                    TxtSpreadsheetUrl.Text = data[1];

                    BtnSave.Content = "Ok";
                }
                catch
                {
                    File.Delete(_userFile);
                    BtnSave.Content = "Save";
                }
            }
        }

        private string _userFile;

        public string EmailAddress { get; set; }
        public string SpreadSheetUrl { get; set; }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var email = TxtEmailAddress.Text.Trim();
            var sheet = TxtSpreadsheetUrl.Text.Trim();

            if (string.IsNullOrEmpty(email)||string.IsNullOrEmpty(sheet))
            {
                MessageBox.Show("Fill all the details");
                return;
            }

            EmailAddress = email;
            SpreadSheetUrl = sheet;

            File.WriteAllText(_userFile, EmailAddress+"\r\n"+SpreadSheetUrl);

            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void TxtEmailAddress_TextChanged(object sender, TextChangedEventArgs e)
        {
            BtnSave.Content = "Save";
        }

        private void TxtSpreadsheetUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            BtnSave.Content = "Save";
        }
    }
}
