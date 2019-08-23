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
using MahApps.Metro.Controls;
using Microsoft.Win32;
using iText;
using iText.IO;
using com.itextpdf;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace NESPTI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
           
        }
     
        public void Button_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                InitialDirectory = @"c:\"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filename = openFileDialog.FileName;
                //MessageBox.Show(filename);
                PdfDocument pdf = new PdfDocument(new PdfReader(filename), new PdfWriter(@"c:\test.pdf")); // needs administrator permissions
                var pageCount = pdf.GetNumberOfPages();
                PdfPage page = pdf.GetPage(1);
                //PdfPage page = new PdfPage(pdf);
                string theText = PdfTextExtractor.GetTextFromPage(page);
                pdf.Close();
                nesTextBox.Text = theText;
                //MessageBox.Show(theText);
            }

        }
    }
}
