using System;
using System.IO.Ports;

public class SerialPortManager
{
    private SerialPort _serialPort;
    public event EventHandler<string> ConnectionStatusChanged;

    public void Connect(string portName, int baudRate = 9600)
    {
        try
        {
            _serialPort = new SerialPort(portName, baudRate);
            _serialPort.Open();
            ConnectionStatusChanged?.Invoke(this, $"Подключено к {portName}");
        }
        catch (Exception ex)
        {
            ConnectionStatusChanged?.Invoke(this, $"Ошибка подключения: {ex.Message}");
        }
    }

    public void Disconnect()
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Close();
            ConnectionStatusChanged?.Invoke(this, "Отключено");
        }
    }

    public void SendCommand(string command)
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.WriteLine(command);
        }
    }

    public string ReceiveData()
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            try
            {
                return _serialPort.ReadLine();
            }
            catch
            {
                return null;
            }
        }
        return null;
    }

    public bool IsConnected => _serialPort?.IsOpen ?? false;
}
