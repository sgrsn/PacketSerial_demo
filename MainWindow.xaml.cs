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

using System.IO.Ports;
using System.Timers;
using System.Windows.Threading;
using System.ComponentModel;

using static COMPortSelector;
using InteractiveDataDisplay.WPF;

namespace PacketSerial_demo
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private SerialPortControl mySerial = new SerialPortControl();
        private SerialPortControl mySerial2 = new SerialPortControl();
        private static System.Timers.Timer data_incre_timer;
        private int frame_count = 0;

        private int[][] x = new int[2][];
        private int[][] y = new int[2][];

        private LineGraph[] linegraph = { new LineGraph(), new LineGraph(), new LineGraph()};

        public MainWindow()
        {
            InitializeComponent();
            COMPortSelector.Init();
            SerialReceivedHandle data_received_handler = this.UpdateChartHandler;
            mySerial.SetDatareceivedHandle(data_received_handler);
            this.Closing += new CancelEventHandler(CloseSerialPort);
            this.Closing += new CancelEventHandler(StopTimer);

            x[0] = new int[100];
            x[1] = new int[100];
            y[0] = new int[100];
            y[1] = new int[100];

            for (int i = 0; i < linegraph.Length; i++)
            {
                lines.Children.Add(linegraph[i]);
                linegraph[i].Description = String.Format("Data {0}", i+1);
            }
        }
        private void UpdateChartHandler()
        {
            frame_count++;

            for(int index = 0; index < x.Length; index++)
            {
                for (int i = 0; i < x[index].Length - 1; i++)
                {
                    x[index][i] = x[index][i + 1];
                    y[index][i] = y[index][i + 1];
                }
                x[index][x[0].Length - 1] = frame_count;
                y[0][y[0].Length - 1] = mySerial.Register[0x10];
                y[1][y[1].Length - 1] = mySerial.Register[0x09];
                this.Dispatcher.Invoke((Action)(() =>
                {
                    linegraph[index].Plot(x[index], y[index]);
                }));
            }
        }

        private void StopTimer(object sender, CancelEventArgs e)
        {
            data_incre_timer.Stop();
            data_incre_timer.Dispose();
        }

        private void CloseSerialPort(object sender, CancelEventArgs e)
        {
            //mySerial.ClosePort();
            //COMPortSelector.CloseAll();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if(mySerial.EnableDisconnect())
            {
                COMPortSelector.SetDataReceivedHandle(mySerial.aDataReceivedHandler);
                COMPortSelector.PushConnectButton(SerialPortComboBox.Text, ref mySerial.port);
            }

            DemoStart();
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private int incre_data = 0;
        private void DemoStart()
        {
            data_incre_timer = new System.Timers.Timer(10);
            // Hook up the Elapsed event for the timer. 
            data_incre_timer.Elapsed += Incre;
            data_incre_timer.AutoReset = true;
            data_incre_timer.Enabled = true;
        }
        private void Incre(Object source, ElapsedEventArgs e)
        {
            incre_data++;
            if(COMPortSelector.IsConnected(mySerial.port.PortName))
            {
                mySerial.WritePieceData((int)incre_data, 0x05);
            }
        }
    }
}
