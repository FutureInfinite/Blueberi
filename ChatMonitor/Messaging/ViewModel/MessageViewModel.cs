using MessageManager;
using MessageManager.Interfaces;
using MessagingModule.Model;
using MessagingModule.ViewModel.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MessagingModule.ViewModel
{
    internal class MessageViewModel : IMessageViewModel, INotifyPropertyChanged
    {
        #region Properties&Attributes
        public ObservableCollection<LogEntry> LogEntries { get; private set; } = new ObservableCollection<LogEntry>();
        private int MessageCount { get; set; } = 0;
        public DelegateCommand<object> StartMessagingCommand { get; private set; }
        private IServiceConnectionManager ConnectionManager;

         private bool _CanStartMessagingOperation = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool CanStartMessagingOperation
        {
            get => _CanStartMessagingOperation;
            set
            {
                if (_CanStartMessagingOperation != value)
                {
                    _CanStartMessagingOperation = value;
                    OnPropertyChanged(nameof(CanStartMessagingOperation));                    
                }
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion Properties&Attributes

        #region Lifetime        
        public MessageViewModel(IServiceConnectionManager ConnectionManager)
        {
            this.ConnectionManager = ConnectionManager;
            
            StartMessagingCommand = new DelegateCommand<object>(StartMessaging, CanStartMessaging);

            ConnectionManager.ReceiveMessageEvent += ConnectionManager_ReceiveMessageEvent;
            ConnectionManager.ChannwlStateChangedEvent += ConnectionManager_ChannwlStateChangedEvent;
        }        
        #endregion Lifetime

        #region Operations
        /// <summary>
        /// This is the event that will be called when messages 
        /// are received from the message service
        /// </summary>
        /// <param name="Message"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void ConnectionManager_ReceiveMessageEvent(string Message)
        {
            Application.Current?.Dispatcher?.Invoke(new Action(() =>
            {
                MessageCount += 1;

                if (LogEntries.Count > 49)
                {
                    LogEntries.RemoveAt(0);
                }

                LogEntries.Add(new LogEntry()
                {
                    Message = Message
                });
            }));
        }
        private bool CanStartMessaging(object commandArg) => true;
        private void StartMessaging(object commandArg)
        {
            ConnectionManager.StartCheckGRPCChannel();
        }

        private void ConnectionManager_ChannwlStateChangedEvent(object sender, EventArgs e)
        {
            CanStartMessagingOperation = !ConnectionManager.IsChannelActive;
        }
        #endregion Operations

    }
}
