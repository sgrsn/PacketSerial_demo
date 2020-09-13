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

/*
 * COMPortSelector
 * 存在するCOMポートと、接続について管理
 * 
 * SerialPortController
 * 接続されているか返す
 * 
*/

namespace PacketSerial_demo
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        //private SerialPortControl mySerial = new SerialPortControl();
        
        //private List<SerialPortControl> mySerial = new List<SerialPortControl>();

        private static System.Timers.Timer data_incre_timer;
        //private int frame_count = 0;

        //private int[][] x = new int[2][];
        //private int[][] y = new int[2][];

        //private LineGraph[] linegraph = { new LineGraph(), new LineGraph(), new LineGraph()};

        private static SolidColorBrush lg_color1 = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
        //private static SolidColorBrush lg_color2 = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
        //private SolidColorBrush[] lg_color = { lg_color1, lg_color2 };

        public MainWindow()
        {
            InitializeComponent();
            COMPortSelector.Init();
            //mySerial.Add(new SerialPortControl());

            this.Closing += new CancelEventHandler(CloseSerialPort);
            this.Closing += new CancelEventHandler(StopTimer);

            DrawSerialGraph.Init();
            /*x[0] = new int[100];
            x[1] = new int[100];
            y[0] = new int[100];
            y[1] = new int[100];

            for (int i = 0; i < x.Length; i++)
            {
                lines.Children.Add(linegraph[i]);
                linegraph[i].Stroke = lg_color[i];
                linegraph[i].Description = String.Format("Data {0}", i + 1);
                linegraph[i].StrokeThickness = 2;
                linegraph[i].Plot(x[i], y[i]);
            }*/
        }
        /*private void UpdateChartHandler()
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
                y[0][y[0].Length - 1] = mySerial[0].Register[0x10];
                y[1][y[1].Length - 1] = mySerial[1].Register[0x09];
                this.Dispatcher.Invoke((Action)(() =>
                {
                    linegraph[index].Plot(x[index], y[index]);
                }));
            }
        }*/

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
            if(!COMPortSelector.IsComboBoxItemConnected())
            {
                /*SerialReceivedHandle data_received_handler = this.UpdateChartHandler;
                mySerial.Last().SetDatareceivedHandle(data_received_handler);

                COMPortSelector.SetDataReceivedHandle(mySerial.Last().aDataReceivedHandler);
                COMPortSelector.ConnectPort(SerialPortComboBox.Text, ref mySerial.Last().port);
                mySerial.Add(new SerialPortControl());*/
                DrawSerialGraph.AddSerialDevice(SerialPortComboBox.Text);
                DemoStart();
            }

            else
            {
                DrawSerialGraph.RemoveSerialDevice(SerialPortComboBox.Text);
                // 切断するmySerialを探す
                /*SerialPortControl tmp = new SerialPortControl();
                foreach (SerialPortControl ser in mySerial)
                {
                    if(ser.port != null)
                    if (ser.port.PortName == SerialPortComboBox.Text)
                    {
                        if(ser.EnableDisconnect())
                        {
                            COMPortSelector.DisconnectPort(SerialPortComboBox.Text, ref ser.port);
                            tmp = ser;
                        }
                    }
                }
                mySerial.Remove(tmp);*/
            }
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
            DrawSerialGraph.SendAllDevice(incre_data, 0x05);
            /*foreach(SerialPortControl ser in mySerial)
            {
                if (ser.IsAvailable())
                {
                    ser.WritePieceData((int)incre_data, 0x05);
                }
            }*/
        }
    }
}
