using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace TimerAssistant
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer _timer;
        private DateTime _targetTime;
        private bool _isUpdatingText = false;

        public MainWindow()
        {
            InitializeComponent();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TimeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingText) return;
            _isUpdatingText = true;

            TextBox tb = sender as TextBox;
            string text = tb.Text;

            if (text.Length > 1 && text.StartsWith("0"))
            {
                tb.Text = text.TrimStart('0');
                if (string.IsNullOrEmpty(tb.Text)) tb.Text = "0";
                tb.CaretIndex = tb.Text.Length;
            }

            CheckStartButtonState();
            _isUpdatingText = false;
        }

        private void Txt_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb.Text == "0") tb.Text = "";
        }

        private void Txt_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (string.IsNullOrWhiteSpace(tb.Text)) tb.Text = "0";
        }

        private void QuickSet_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && int.TryParse(btn.Tag.ToString(), out int mins))
            {
                TxtHours.Text = (mins / 60).ToString();
                TxtMinutes.Text = (mins % 60).ToString();
            }
        }

        private void CheckStartButtonState()
        {
            if (BtnAction == null) return;
            int h = int.TryParse(TxtHours.Text, out int hv) ? hv : 0;
            int m = int.TryParse(TxtMinutes.Text, out int mv) ? mv : 0;
            BtnAction.IsEnabled = (h * 60 + m) > 0;
        }

        private void BtnAction_Click(object sender, RoutedEventArgs e)
        {
            if ((string)BtnAction.Content == "开始")
            {
                StartCountdown();
            }
            else
            {
                StopCountdown();
            }
        }

        private void StartCountdown()
        {
            int h = int.TryParse(TxtHours.Text, out int hv) ? hv : 0;
            int m = int.TryParse(TxtMinutes.Text, out int mv) ? mv : 0;

            _targetTime = DateTime.Now.AddHours(h).AddMinutes(m);

            PanelInput.IsEnabled = false;
            RbSleep.IsEnabled = false;
            RbShutdown.IsEnabled = false;
            BtnAction.Content = "停止";
            BtnAction.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 53, 69));

            UpdateDisplayRemainingTime();
            _timer.Start();
        }

        private void StopCountdown()
        {
            _timer.Stop();

            PanelInput.IsEnabled = true;
            RbSleep.IsEnabled = true;
            RbShutdown.IsEnabled = true;
            BtnAction.Content = "开始";
            BtnAction.Background = (System.Windows.Media.Brush)FindResource("PrimaryColor");
            TxtCountdown.Text = "00:00:00";
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateDisplayRemainingTime();
        }

        private void UpdateDisplayRemainingTime()
        {
            TimeSpan remaining = _targetTime - DateTime.Now;

            if (remaining.TotalSeconds <= 0)
            {
                _timer.Stop();
                TxtCountdown.Text = "00:00:00";
                TriggerEndAction();
            }
            else
            {
                TxtCountdown.Text = string.Format("{0:D2}:{1:D2}:{2:D2}",
                    (int)remaining.TotalHours,
                    remaining.Minutes,
                    remaining.Seconds);
            }
        }

        private void TriggerEndAction()
        {
            bool isSleep = RbSleep.IsChecked == true;
            string actionName = isSleep ? "睡眠" : "关机";

            ConfirmDialog dialog = new ConfirmDialog(actionName);
            dialog.Owner = this;
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                ExecuteSystemAction(isSleep);
            }

            StopCountdown();
        }

        private void ExecuteSystemAction(bool isSleep)
        {
            try
            {
                if (isSleep)
                {
                    ConfirmDialog.SetSuspendState(false, false, false);
                }
                else
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("shutdown.exe", "-s -t 0 -f")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"执行失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}