
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatMonitorPackage;
using MediatR;


namespace MessagingServer.MessageHandling
{
    public class SendMessageNotification : INotification
    {
        #region Properties&Attributes
        public string ClientId { get; set; }        
        public InformationMessage Message { get; set; }
        #endregion Properties&Attributes
    }
}
