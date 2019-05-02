using Microsoft.Win32;
using PerprogSzekvencialis.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace PerprogSzekvencialis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InputButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if(openFileDialog.ShowDialog() == true)
            {
                inputFileText.Text = openFileDialog.FileName;
            }
            
        }

        private void OutputButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if(saveFileDialog.ShowDialog() == true)
            {
                outputFileText.Text = saveFileDialog.FileName;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource = new CancellationTokenSource();
            
        }

        private void StartProcessButton_Click(object sender, RoutedEventArgs e)
        {
            Encryption s;
            if (parallel.IsChecked == true)
            {
                s = new ParallelEncrypt(inputFileText.Text, outputFileText.Text, cancellationTokenSource.Token, "123456777", encrypt.IsChecked == true ? ProcessType.Encrypt : ProcessType.Decrypt, 4);

            }
            else
            {
                s = new SerialEncrypt(inputFileText.Text, outputFileText.Text, cancellationTokenSource.Token, "123456777", encrypt.IsChecked == true ? ProcessType.Encrypt : ProcessType.Decrypt);

            }
            s.PercentageChange += S_PercentageChange;
            s.MessageChange += S_MessageChange;
            Task.Run(() =>s.Start());
        }

        private void S_MessageChange(string message)
        {
            Dispatcher.Invoke(() =>
            {
                messageText.Content = message;
            });
        }

        private void S_PercentageChange(double percent)
        {
            Dispatcher.Invoke(() =>
            {
                progressBar.Maximum = 100;
               
                progressBar.Value = double.IsInfinity(percent) ? 100 :  percent;
            });
        }
    }
}
