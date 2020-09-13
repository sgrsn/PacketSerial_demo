using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PacketSerial_demo;
using InteractiveDataDisplay.WPF;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows;

public class Device
{
    public SerialPortControl serial_control = new SerialPortControl();
    public int[] x = new int[100];
    public int[] y = new int[100];
    public LineGraph linegraph = new LineGraph();
    public SolidColorBrush lg_color = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
    public string description = "";
}


static class DrawSerialGraph
{
    private static List<Device> device = new List<Device>();
    private static MainWindow mainWindow;
    private static ComboBox SerialPortComboBox;
    private static int frame_count;
    public static void Init()
    {
        mainWindow = (MainWindow)App.Current.MainWindow;
        SerialPortComboBox = mainWindow.SerialPortComboBox;

        device.Add(new Device());
        device.Last().description = "first";

        foreach(var d in device)
        {
            mainWindow.lines.Children.Add(d.linegraph);
            d.linegraph.Stroke = d.lg_color;
            d.linegraph.Description = String.Format(d.description);
            d.linegraph.StrokeThickness = 2;
            d.linegraph.Plot(d.x, d.y);
        }
    }

    public static void AddSerialDevice(string port_name)
    {
        SerialReceivedHandle data_received_handler = UpdateChartHandler;
        device.Last().serial_control.SetDatareceivedHandle(data_received_handler);

        COMPortSelector.SetDataReceivedHandle(device.Last().serial_control.aDataReceivedHandler);
        COMPortSelector.ConnectPort(port_name, ref device.Last().serial_control.port);

        device.Add(new Device());   // 次に使うやつ
        mainWindow.lines.Children.Add(device.Last().linegraph);
        device.Last().linegraph.Stroke = device.Last().lg_color;
        device.Last().linegraph.Description = String.Format(device.Last().description);
        device.Last().linegraph.StrokeThickness = 2;
        device.Last().linegraph.Plot(device.Last().x, device.Last().y);
    }
    public static void RemoveSerialDevice(string port_name)
    {
        Device tmp = new Device();
        foreach (var d in device)
        {
            if (d.serial_control.port != null)
                if (d.serial_control.port.PortName == port_name)
                {
                    if (d.serial_control.EnableDisconnect())
                    {
                        COMPortSelector.DisconnectPort(port_name, ref d.serial_control.port);
                        tmp = d;
                    }
                }
        }
        device.Remove(tmp);
        
    }

    public static void SendAllDevice(int data, byte reg)
    {
        foreach(var d in device)
        {
            if(d.serial_control.IsAvailable())
                d.serial_control.WritePieceData(data, reg);
        }
    }

    public static void UpdateChartHandler()
    {
        frame_count++;

        for (int index = 0; index < device.Count-1; index++)
        {
            int size = device[index].x.Length;
            for (int i = 0; i < size - 1; i++)
            {
                device[index].x[i] = device[index].x[i + 1];
                device[index].y[i] = device[index].y[i + 1];
            }
            device[index].x[size - 1] = frame_count;
            //Console.WriteLine(device[index].x[size - 1]);
            device[index].y[size - 1] = device[index].serial_control.Register[0x10];

            if (Application.Current.Dispatcher.CheckAccess())
            {
                device[index].linegraph.Plot(device[index].x, device[index].y);
            }
            else
            {
                int[] tmpx = device[index].x;
                int[] tmpy = device[index].y;
                LineGraph tmp_lg = device[index].linegraph;
                Application.Current.Dispatcher.BeginInvoke(
                  DispatcherPriority.Background,
                  new Action(() => {
                      tmp_lg.Plot(tmpx, tmpy);
                  }));
            }
        }
    }

}
