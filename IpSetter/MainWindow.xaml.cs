using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Input;

namespace IpSetter
{
    public partial class MainWindow : Window
    {
        NetController _netController;
        Dictionary<string, IpConfig> _configs = new Dictionary<string, IpConfig>();

        bool _startup = true;

        System.Windows.Threading.DispatcherTimer _timer;

        public MainWindow()
        {
            if (Debugger.IsAttached)
                Properties.Settings.Default.Reset();

            InitializeComponent();
            _startup = false;
            _netController = new NetController();
            
            foreach(string name in _netController.NiNames)
            {
                cbInterfaces.Items.Add(name);
            }

            try
            {
                RestoreSettings();
            }
            catch
            {
                MessageBox.Show("Unable to restore settings. Reseting.");
                Properties.Settings.Default.Reset();
            }
            
            UpdateFrontend();
            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Tick += Timer_Tick;
            _timer.Interval = new TimeSpan(0, 0, 1);
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            lbCurrentIps.Items.Clear();
            var ips = _netController.GetIps();
            foreach (KeyValuePair<string, string> pair in ips)
            {                
                lbCurrentIps.Items.Add(pair.Key + " - " + pair.Value);
            }
        }

        private void UpdateFrontend()
        {
            lbConfigs.Items.Clear();
            foreach (KeyValuePair<string, IpConfig> entry in _configs)
            {
                var str = entry.Key + ": " + entry.Value.Ip;
                lbConfigs.Items.Add(str);
            }
        }


        private string ExtractName()
        {
            string selStr = (string)lbConfigs.SelectedValue;

            if (!string.IsNullOrEmpty(selStr))
            {
                int index = selStr.IndexOf(":");
                return selStr.Substring(0, index);
            }
            else
                return null;     
        }

        #region Entered value checks
        private bool CheckValuesFromUi()
        {
            var ipOk = CheckIp();
            var sbOk = CheckSb();
            var nameOk = !string.IsNullOrEmpty(tbName.Text);

            if (!ipOk || !sbOk || !nameOk)
                return false;
            else
                return true;
        }
        private bool CheckIp()
        {
            var notTooLong = (tbIp1.Text.Length <= 3) && (tbIp2.Text.Length <= 3) && (tbIp3.Text.Length <= 3) && (tbIp4.Text.Length <= 3);
            var notTooShort = (tbIp1.Text.Length > 0) && (tbIp2.Text.Length > 0) && (tbIp3.Text.Length > 0) && (tbIp4.Text.Length > 0);

            int[] ip = new int[4];
            var parseOk = int.TryParse(tbIp1.Text, out ip[0]) && int.TryParse(tbIp2.Text, out ip[1]) && int.TryParse(tbIp3.Text, out ip[2]) && int.TryParse(tbIp4.Text, out ip[3]);
            var inRange = AddressInRange(ip);
            return notTooLong && notTooShort && parseOk && inRange;
        }
        private bool CheckSb()
        {
            var notTooLong = (tbSb1.Text.Length <= 3) && (tbSb2.Text.Length <= 3) && (tbSb3.Text.Length <= 3) && (tbSb4.Text.Length <= 3);
            var notTooShort = (tbSb1.Text.Length > 0) && (tbSb2.Text.Length > 0) && (tbSb3.Text.Length > 0) && (tbSb4.Text.Length > 0);

            int[] Sb = new int[4];
            var parseOk = int.TryParse(tbSb1.Text, out Sb[0]) && int.TryParse(tbSb2.Text, out Sb[1]) && int.TryParse(tbSb3.Text, out Sb[2]) && int.TryParse(tbSb4.Text, out Sb[3]);
            var inRange = AddressInRange(Sb);
            return notTooLong && notTooShort && parseOk && inRange;
        }
        private bool AddressInRange(int[] address)
        {
            var notTooSmall = (address[0] >= 0) && (address[1] >= 0) && (address[2] >= 0) && (address[3] >= 0);
            var notTooBig = (address[0] < 256) && (address[1] < 256) && (address[2] < 256) && (address[3] < 256);

            return notTooSmall && notTooBig;
        }
        #endregion

        #region Clicks
        private void btnAddNewItem_Click(object sender, RoutedEventArgs e)
        {
            var valuesOk = CheckValuesFromUi();

            if (!valuesOk)
            {
                MessageBox.Show("Wrong values entered!");
                return;
            }

            var ip = int.Parse(tbIp1.Text) + "." + int.Parse(tbIp2.Text) + "." + int.Parse(tbIp3.Text) + "." + int.Parse(tbIp4.Text);
            var subnet = int.Parse(tbSb1.Text) + "." + int.Parse(tbSb2.Text) + "." + int.Parse(tbSb3.Text) + "." + int.Parse(tbSb4.Text);
            var name = tbName.Text;

            if (_configs.ContainsKey(name))
            {
                MessageBox.Show("Please enter a different name!");
                return;
            }
            if (cbInterfaces.SelectedValue == null)
            {
                MessageBox.Show("Please select an interface!");
                return;
            }

            IpConfig ipConfig = new IpConfig(ip, subnet, cbInterfaces.SelectedValue.ToString(), name);
            _configs.Add(name, ipConfig);
            UpdateFrontend();
        }
        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            var tempName = ExtractName();
            if (tempName == null)
                return;

            _configs.Remove(tempName);
            UpdateFrontend();
        }
        private void btnSetIp_Click(object sender, RoutedEventArgs e)
        {
            string tempName = ExtractName();
            _netController.ApplyConfig(_configs[tempName]);
        }
        private void btnDhcp_Click(object sender, RoutedEventArgs e)
        {
            string tempName = ExtractName();
            _netController.EnableDHCP(_configs[tempName].IfaceName);

        }
        #endregion

        #region EventsForSelectingTexts
        private void tbIp1_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbIp1.SelectAll();
        }
        private void tbIp1_GotMouseCapture(object sender, MouseEventArgs e)
        {
            //tbIp1.SelectAll();
        }
        private void tbIp2_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbIp2.SelectAll();
        }
        private void tbIp2_GotMouseCapture(object sender, MouseEventArgs e)
        {
            tbIp2.SelectAll();
        }
        private void tbIp3_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbIp3.SelectAll();
        }
        private void tbIp3_GotMouseCapture(object sender, MouseEventArgs e)
        {
            tbIp3.SelectAll();
        }
        private void tbIp4_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbIp4.SelectAll();
        }
        private void tbIp4_GotMouseCapture(object sender, MouseEventArgs e)
        {
            tbIp4.SelectAll();
        }    
        private void tbSb1_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbSb1.SelectAll();
        }
        private void tbSb1_GotMouseCapture(object sender, MouseEventArgs e)
        {
            tbSb1.SelectAll();

        }
        private void tbSb2_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbSb2.SelectAll();
        }
        private void tbSb2_GotMouseCapture(object sender, MouseEventArgs e)
        {
            tbSb2.SelectAll();
        }
        private void tbSb3_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbSb3.SelectAll();
        }
        private void tbSb3_GotMouseCapture(object sender, MouseEventArgs e)
        {
            tbSb3.SelectAll();
        }
        private void tbSb4_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            tbSb4.SelectAll();
        }
        private void tbSb4_GotMouseCapture(object sender, MouseEventArgs e)
        {
            tbSb4.SelectAll();
        }
        #endregion

        #region Settings save/load
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }
        private void SaveSettings()
        {
            if (_configs.Count == 0)
            {
                Properties.Settings.Default.ConfigExists = false;
                Properties.Settings.Default.Save();
                return;
            }

            MemoryStream ms = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            try
            {
                formatter.Serialize(ms, _configs);
                ms.Position = 0;
                byte[] buffer = new byte[(int)ms.Length];
                ms.Read(buffer, 0, buffer.Length);
                Properties.Settings.Default.IpSettings = Convert.ToBase64String(buffer);
                Properties.Settings.Default.ConfigExists = true;
                Properties.Settings.Default.Save();
            }
            catch (SerializationException ex)
            {
                MessageBox.Show("Failed to serialize. Reason: " + ex.Message);
            }
            finally
            {
                ms.Close();
            }
        }
        private void RestoreSettings()
        {
            if (!Properties.Settings.Default.ConfigExists)
                return;

            Dictionary<string, IpConfig> _restoredConfigs = null;

            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(Properties.Settings.Default.IpSettings)))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                _restoredConfigs = (Dictionary<string, IpConfig>)formatter.Deserialize(ms);
            }

            _configs = _restoredConfigs;

        }

        #endregion

        private void tbIp1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (tbIp1.Text.Length == 3 &! _startup)
            {
                tbIp2.Focus();
            }
        }

        private void tbIp2_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (tbIp2.Text.Length == 3 & !_startup)
            {
                tbIp3.Focus();
            }
        }

        private void tbIp3_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (tbIp3.Text.Length == 3 & !_startup)
            {
                tbIp4.Focus();
            }
        }

        private void tbIp4_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (tbIp4.Text.Length == 3 & !_startup)
            {
                tbSb1.Focus();
            }
        }

        private void tbSb1_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (tbSb1.Text.Length == 3 & !_startup)
            {
                tbSb2.Focus();
            }
        }

        private void tbSb2_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (tbSb2.Text.Length == 3 & !_startup)
            {
                tbSb3.Focus();
            }
        }

        private void tbSb3_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (tbSb3.Text.Length == 3 & !_startup)
            {
                tbSb4.Focus();
            }
        }

        private void tbSb4_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (tbSb4.Text.Length > 3 & !_startup)
            {
                tbSb4.Text = tbSb4.Text.Substring(0, 3);
            }
        }
    }
}
