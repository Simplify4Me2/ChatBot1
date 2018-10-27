using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Azure.Devices;

namespace BotApplication1.Helpers
{
    public class IoTHelper
    {
        private static ServiceClient _serviceClient;
        private static readonly string connectionString = "HostName=TechoRamaIoT.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=c+/WB7J/5hf2G0fPbbYhGQ1RruqDmu5smji4h8x2Dto=";

        public static async Task SendAsync(string command)
        {
            _serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            var commandMessage = new Message(Encoding.ASCII.GetBytes(command));
            await _serviceClient.SendAsync("IoTHubCar", commandMessage);
        }

    }
}