using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.IO.Ports;

namespace Yak130Controller
{
    public partial class MainWindow : Window
    {
        private SerialPortManager serialPortManager;
        private ControlSurfaceManager controlSurfaceManager;
        private Yak130Aerodynamics aerodynamics;
        private ObservableCollection<string> comPorts;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            serialPortManager = new SerialPortManager();
            controlSurfaceManager = new ControlSurfaceManager();
            aerodynamics = new Yak130Aerodynamics();
            comPorts = new ObservableCollection<string>();
            RefreshComPorts();
            SetupEventHandlers();
        }

        private void RefreshComPorts()
        {
            comPorts.Clear();
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                comPorts.Add(port);
            }
            ComPortComboBox.ItemsSource = comPorts;
            if (ports.Length > 0)
                ComPortComboBox.SelectedIndex = 0;
        }

        private void SetupEventHandlers()
        {
            serialPortManager.ConnectionStatusChanged += (s, msg) => 
            {
                StatusLabel.Content = $"Статус: {msg}";
            };

            AileronSlider.ValueChanged += (s, e) => 
                AileronLabel.Content = $"{AileronSlider.Value:F1}°";
            
            FlapSlider.ValueChanged += (s, e) => 
                FlapLabel.Content = $"{FlapSlider.Value:F1}°";
            
            RudderYawSlider.ValueChanged += (s, e) => 
                RudderYawLabel.Content = $"{RudderYawSlider.Value:F1}°";
            
            RudderPitchSlider.ValueChanged += (s, e) => 
                RudderPitchLabel.Content = $"{RudderPitchSlider.Value:F1}°";
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshComPorts();
            StatusLabel.Content = "Статус: Список портов обновлён";
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (ComPortComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите COM-порт", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string port = ComPortComboBox.SelectedItem.ToString();
            try
            {
                serialPortManager.Connect(port, 9600);
                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
                ComPortComboBox.IsEnabled = false;
                RefreshButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!double.TryParse(SpeedTextBox.Text, out double speed))
                    speed = 0;
                if (!double.TryParse(AltitudeTextBox.Text, out double altitude))
                    altitude = 0;
                if (!double.TryParse(AngleTextBox.Text, out double angle))
                    angle = 0;

                var (lift, drag, thrust, calcAilerons, calcFlaps, calcRudderYaw, calcRudderPitch) = 
                    aerodynamics.CalculateAircraftCharacteristics(speed, altitude, angle);

                // Use slider values if connected, otherwise use calculated values
                float ailerons = serialPortManager.IsConnected ? (float)AileronSlider.Value : (float)calcAilerons;
                float flaps = serialPortManager.IsConnected ? (float)FlapSlider.Value : (float)calcFlaps;
                float rudderYaw = serialPortManager.IsConnected ? (float)RudderYawSlider.Value : (float)calcRudderYaw;
                float rudderPitch = serialPortManager.IsConnected ? (float)RudderPitchSlider.Value : (float)calcRudderPitch;

                controlSurfaceManager.SetAileronDeflection(ailerons);
                controlSurfaceManager.SetFlapDeflection(flaps);
                controlSurfaceManager.SetRudderYawDeflection(rudderYaw);
                controlSurfaceManager.SetRudderPitchDeflection(rudderPitch);

                string command = $"A:{ailerons:F2};F:{flaps:F2};Y:{rudderYaw:F2};P:{rudderPitch:F2}\n";
                
                if (serialPortManager.IsConnected)
                {
                    serialPortManager.SendCommand(command);
                    StatusLabel.Content = $"Статус: Команда отправлена - {command.Trim()}";
                }
                else
                {
                    StatusLabel.Content = $"Статус: Порт не подключен (режим расчета)";
                }

                LiftLabel.Content = $"{lift:F2} N";
                DragLabel.Content = $"{drag:F2} N";
                ThrustLabel.Content = $"{thrust:F2} N";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчете: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            serialPortManager.Disconnect();
            ConnectButton.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
            ComPortComboBox.IsEnabled = true;
            RefreshButton.IsEnabled = true;
            StatusLabel.Content = "Статус: Отключено";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            serialPortManager.Disconnect();
        }
    }
}
