using ASCOM.Utilities;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ASCOM.DashBoardPowerBoxV3.Switch
{
    [ComVisible(false)] // Form not registered for COM!
    public partial class SetupDialogForm : Form
    {
        const string NO_PORTS_MESSAGE = "No COM ports found";
        TraceLogger tl; // Holder for a reference to the driver's trace logger

        public SetupDialogForm(TraceLogger tlDriver)
        {
            InitializeComponent();

            // Save the provided trace logger for use within the setup dialogue
            tl = tlDriver;

            // Initialise current values of user settings from the ASCOM Profile
            InitUI();
        }

        private void CmdOK_Click(object sender, EventArgs e) // OK button event handler
        {
            // Place any validation constraint checks here and update the state variables with results from the dialogue

            tl.Enabled = chkTrace.Checked;

            // Update the COM port variable if one has been selected
            if (comboBoxComPort.SelectedItem is null) // No COM port selected
            {
                tl.LogMessage("Setup OK", $"New configuration values - COM Port: Not selected");
            }
            else if (comboBoxComPort.SelectedItem.ToString() == NO_PORTS_MESSAGE)
            {
                tl.LogMessage("Setup OK", $"New configuration values - NO COM ports detected on this PC.");
            }
            else // A valid COM port has been selected
            {
                SwitchHardware.comPort = (string)comboBoxComPort.SelectedItem;
                tl.LogMessage("Setup OK", $"New configuration values - COM Port: {comboBoxComPort.SelectedItem} , DC1: {DC1Text.Text}, DC2: {DC2Text.Text}, DC3: {DC3Text.Text}, DC4-5: {DC45Text.Text}, PWM A: {PWMAText.Text}, PWM B: {PWMBText.Text}, EXT 1: {EXT1Text.Text}, EXT2: {EXT2Text.Text}");
                SwitchHardware.SwitchNameDC1 = (string)DC1Text.Text;
                SwitchHardware.SwitchNameDC2 = (string)DC2Text.Text;
                SwitchHardware.SwitchNameDC3 = (string)DC3Text.Text;
                SwitchHardware.SwitchNameDC45 = (string)DC45Text.Text;
                SwitchHardware.SwitchNamePWM1 = (string)PWMAText.Text;
                SwitchHardware.SwitchNamePWM2 = (string)PWMBText.Text;
                SwitchHardware.SwitchNameEXT1 = (string)EXT1Text.Text;
                SwitchHardware.SwitchNameEXT2 = (string)EXT2Text.Text;
                SwitchHardware.KeepPWMOnOnDisconnect = BoolToString(keepPWMon.Checked);
                SwitchHardware.KeepPortsOnOnDisconnect = BoolToString(KeepPortsOn.Checked);
                SwitchHardware.KeepEXTOnOnDisconnect = BoolToString(keepEXTOn.Checked);
            }
        }

        private void CmdCancel_Click(object sender, EventArgs e) // Cancel button event handler
        {
            Close();
        }

        private void BrowseToAscom(object sender, EventArgs e) // Click on ASCOM logo event handler
        {
            try
            {
                System.Diagnostics.Process.Start("https://ascom-standards.org/");
            }
            catch (Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void InitUI()
        {

            // Set the trace checkbox
            chkTrace.Checked = tl.Enabled;

            // set the list of COM ports to those that are currently available
            comboBoxComPort.Items.Clear(); // Clear any existing entries
            using (Serial serial = new Serial()) // User the Se5rial component to get an extended list of COM ports
            {
                comboBoxComPort.Items.AddRange(serial.AvailableCOMPorts);
            }

            // If no ports are found include a message to this effect
            if (comboBoxComPort.Items.Count == 0)
            {
                comboBoxComPort.Items.Add(NO_PORTS_MESSAGE);
                comboBoxComPort.SelectedItem = NO_PORTS_MESSAGE;
            }

            // select the current port if possible
            if (comboBoxComPort.Items.Contains(SwitchHardware.comPort))
            {
                comboBoxComPort.SelectedItem = SwitchHardware.comPort;
            }
            DC1Text.Text = SwitchHardware.SwitchNameDC1;
            DC2Text.Text = SwitchHardware.SwitchNameDC2;
            DC3Text.Text = SwitchHardware.SwitchNameDC3;
            DC45Text.Text = SwitchHardware.SwitchNameDC45;
            PWMAText.Text = SwitchHardware.SwitchNamePWM1;
            PWMBText.Text = SwitchHardware.SwitchNamePWM2;
            EXT1Text.Text = SwitchHardware.SwitchNameEXT1;
            EXT2Text.Text = SwitchHardware.SwitchNameEXT2;
            KeepPortsOn.Checked = StringToBool(SwitchHardware.KeepPortsOnOnDisconnect);
            keepEXTOn.Checked = StringToBool(SwitchHardware.KeepEXTOnOnDisconnect);
            keepPWMon.Checked = StringToBool(SwitchHardware.KeepPWMOnOnDisconnect);

            tl.LogMessage("InitUI", $"Set UI controls to Trace: {chkTrace.Checked}, COM Port: {comboBoxComPort.SelectedItem}");
        }

        private void SetupDialogForm_Load(object sender, EventArgs e)
        {
            // Bring the setup dialogue to the front of the screen
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
            else
            {
                TopMost = true;
                Focus();
                BringToFront();
                TopMost = false;
            }
        }
        private static bool StringToBool(string str)
        {
            if (str == "false") return false;
            else return true;
        }
        private static string BoolToString (bool boolin)
        {
            if (boolin) return "true";
            else return "false";
        }


    }
}