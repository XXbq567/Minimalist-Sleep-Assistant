using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace TimerAssistant
{
    public partial class ConfirmDialog : Window
    {
        private DispatcherTimer _dialogTimer;
        private int _secondsLeft = 30;
        private string _actionName;

        [DllImport("PowrProf.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

        public ConfirmDialog(string actionName)
        {
            InitializeComponent();
            _actionName = actionName;
            UpdateMessage();

            this.Activate();

            _dialogTimer = new DispatcherTimer();
            _dialogTimer.Interval = TimeSpan.FromSeconds(1);
            _dialogTimer.Tick += DialogTimer_Tick;
            _dialogTimer.Start();
        }

        private void DialogTimer_Tick(object sender, EventArgs e)
        {
            _secondsLeft--;
            if (_secondsLeft <= 0)
            {
                _dialogTimer.Stop();
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                UpdateMessage();
            }
        }

        private void UpdateMessage()
        {
            TxtMessage.Text = $"到时间了，执行{_actionName}吗？";
            TxtCountdown.Text = $"{_secondsLeft} s 自动确认";
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            _dialogTimer.Stop();
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            _dialogTimer.Stop();
            this.DialogResult = false;
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            _dialogTimer?.Stop();
            base.OnClosed(e);
        }
    }
}