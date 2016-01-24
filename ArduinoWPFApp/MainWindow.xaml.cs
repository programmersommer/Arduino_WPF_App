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


using System.Timers;
using System.IO.Ports;

namespace ArduinoWPFApp
{
    public partial class MainWindow : Window
    {

        System.Timers.Timer aTimer;
        SerialPort currentPort;
        private delegate void updateDelegate(string txt);


        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOne_Click(object sender, RoutedEventArgs e)
        {
            if (!currentPort.IsOpen) return;
            currentPort.Write("1");
        }

        private void btnZero_Click(object sender, RoutedEventArgs e)
        {
            if (!currentPort.IsOpen) return;
            currentPort.Write("0");
        }


        private bool ArduinoDetected()
        {
            try
            {
                currentPort.Open();
                System.Threading.Thread.Sleep(1000); // just wait a lot

                string returnMessage = currentPort.ReadLine();
                currentPort.Close();

                // in arduino sketch should be Serial.println("Info from Arduino") inside  void loop()
                if (returnMessage.Contains("Info from Arduino"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool ArduinoPortFound = false;

            try
            {
                string[] ports = SerialPort.GetPortNames();
                foreach (string port in ports)
                {
                    currentPort = new SerialPort(port, 9600);
                    if (ArduinoDetected())
                    {
                        ArduinoPortFound = true;
                        break;
                    }
                    else
                    {
                        ArduinoPortFound = false;
                    }
                }
            }
            catch { }

            if (ArduinoPortFound == false) return;

            System.Threading.Thread.Sleep(500); // wait a lot after closing

            currentPort.BaudRate = 9600;
            currentPort.DtrEnable = true;
            currentPort.ReadTimeout= 1000;
            try
            {
                currentPort.Open();
            }
            catch { }

            aTimer = new System.Timers.Timer(1000);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if (!currentPort.IsOpen) return;
            try
            {
                currentPort.DiscardInBuffer();  // remove old information from buffer
                string POT = currentPort.ReadLine();  // read last value
                lblPortData.Dispatcher.BeginInvoke(new updateDelegate(updateTextBox), POT);
            }
            catch (Exception ex)
            {
                lblPortData.Dispatcher.BeginInvoke(new updateDelegate(updateTextBox), ex.Message);
            }
}

        private void updateTextBox(string txt)
        {
            lblPortData.Content = txt;
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            aTimer.Enabled = false;
            currentPort.Close();
        }
    }
}
