using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Reflection;
using System.Threading.Tasks;
namespace ASCOM.DashBoardPowerBoxV3App
{
    
    public partial class Form1 : Form
    {
        internal static int hours = 0;
        internal static int minutes = 0;
        internal static string minutesString = "00";
        internal static int seconds = 0;
        internal static string secondsString = "00";
        internal static int previous_pwm_a = 0;
        internal static int previous_pwm_b = 0;
        System.Threading.Thread t;
        System.Threading.Thread clk;
        
        private ASCOM.DriverAccess.Switch driver;

        public Form1()
        {
            InitializeComponent();
            SetUIState();
            Off_PWMA.Enabled = false;
            Off_PWMB.Enabled = false;
            Max_PWMA.Enabled = false;
            Max_PWMB.Enabled = false;
            Mid_PWMA.Enabled = false;
            Mid_PWMB.Enabled = false;
            autoPWMA.Enabled = false;
            autoPWMB.Enabled = false;
            levelA.Enabled = false;
            levelB.Enabled = false;
            addPWMB.BackColor = Color.DimGray;
            removePWMB.BackColor = Color.DimGray;
            addPWMA.BackColor = Color.DimGray;
            removePWMA.BackColor = Color.DimGray;
            connectionStatus.ForeColor = Color.Red;
            connectionStatus.Text = "NOT CONNECTED";
            buttonConnect.Text = "Connect";
            buttonChoose.Enabled = true;
            temp.Text = "";
            humidity.Text = "";
            dewpoint.Text = "";
            runtime.Text = "";
            voltage.Text = "";
            current.Text = "";
            power.Text = "";
            energy.Text = "";
            nameDC1.Text = "";
            nameDC1.Enabled = false;
            nameDC2.Text = "";
            nameDC2.Enabled = false;
            nameDC3.Text = "";
            nameDC3.Enabled = false;
            nameDC45.Text = "";
            nameDC45.Enabled = false;
            namePWMA.Text = "";
            namePWMA.Enabled = false;
            namePWMB.Text = "";
            namePWMB.Enabled = false;
            nameEXT1.Text = "";
            nameEXT1.Enabled = false;
            nameEXT2.Text = "";
            nameEXT2.Enabled = false;
            buttonDC1.Checked = false;
            buttonDC1.Enabled = false;
            buttonDC2.Checked = false;
            buttonDC2.Enabled = false;
            buttonDC3.Checked = false;
            buttonDC3.Enabled = false;
            buttonDC45.Checked = false;
            buttonDC45.Enabled = false;
            buttonEXT1.Checked = false;
            buttonEXT1.Enabled = false;
            buttonEXT2.Checked = false;
            buttonEXT2.Enabled = false;
            autoPWMB.Checked = false;
            autoPWMB.Enabled = false;
            addPWMA.Enabled = false;
            addPWMB.Enabled = false;
            removePWMA.Enabled = false;
            removePWMB.Enabled = false;
            setPWMA.Enabled = false;
            setPWMB.Enabled = false;
            valuePWMA.Text = "0";
            valuePWMB.Text = "0";
            currentPWMA.Text = "0";
            currentPWMB.Text = "0";
        }
        public async void Stopwatch() // clock
        {
            
            while (driver.Connected)
            {
                if (seconds < 60)
                {
                    seconds = seconds + 1;
                }
                else
                {
                    seconds = 0;
                    if (minutes < 60)
                    {
                        minutes = minutes + 1;
                    }
                    else
                    {
                        minutes = 0;
                        hours = hours + 1;
                    }
                }
                if (minutes < 10)
                {
                    minutesString = "0" + minutes.ToString();
                }
                else
                {
                    minutesString = minutes.ToString();
                }
                if (seconds < 10)
                {
                    secondsString = "0" + seconds.ToString();
                }
                else
                {
                    secondsString = seconds.ToString();
                }
                await Task.Delay(1000);
                string stopwatch = hours.ToString() + ":" + minutesString + ":" + secondsString;
                MethodInvoker clock = delegate () { runtime.Text = stopwatch.ToString(); };
                this.Invoke(clock);
            }

        }
        public async void RefreshData() //refresh data every 5 seconds
        {
            while (driver.Connected)
            {
                await Task.Delay(5000);
                MethodInvoker refresh = delegate ()
                {
                    buttonDC1.Checked = driver.GetSwitch(0);
                    buttonDC2.Checked = driver.GetSwitch(1);
                    buttonDC3.Checked = driver.GetSwitch(2);
                    buttonDC45.Checked = driver.GetSwitch(3);
                    currentPWMA.Text = driver.GetSwitchValue(4).ToString();
                    currentPWMB.Text = driver.GetSwitchValue(5).ToString();
                    buttonEXT1.Checked = driver.GetSwitch(14);
                    buttonEXT2.Checked = driver.GetSwitch(15);
                    temp.Text = driver.GetSwitchValue(6).ToString();
                    humidity.Text = driver.GetSwitchValue(7).ToString();
                    dewpoint.Text = driver.GetSwitchValue(8).ToString();
                    voltage.Text = driver.GetSwitchValue(9).ToString();
                    currentPWMA.Text = driver.GetSwitchValue(4).ToString();
                    //valuePWMA.Text = currentPWMA.Text;
                    currentPWMB.Text = driver.GetSwitchValue(5).ToString();
                    //valuePWMB.Text = currentPWMB.Text;
                    double delta_t_d = Convert.ToDouble(temp.Text) - Convert.ToDouble(dewpoint.Text);
                    int delta_t = Convert.ToInt16(Math.Round(delta_t_d));
                    driver.SetSwitch(13,false); //Turn Arduino-level AUTO PWM off, since dedicated software uses 3 separate levels;
                    current.Text = driver.GetSwitchValue(10).ToString();
                    power.Text = driver.GetSwitchValue(11).ToString();
                    energy.Text = driver.GetSwitchValue(12).ToString();
                    if (autoPWMA.Checked)
                    {
                        previous_pwm_a = Int16.Parse(valuePWMA.Text);
                        if (levelA.Text == "Low")
                        {
                            if (delta_t <= 10)
                            {
                                int pwma_low = (10 - delta_t) * 10;
                                if (pwma_low >= 100) { pwma_low = 100; };
                                valuePWMA.Text = pwma_low.ToString();
                            }
                            else { valuePWMA.Text = "0"; }
                        }
                        else if (levelA.Text == "Mid")
                        {
                            if (delta_t <= 10)
                            {
                                int pwma_mid = (10 - delta_t) * 15;
                                if (pwma_mid >= 100) { pwma_mid = 100; };
                                valuePWMA.Text = pwma_mid.ToString();
                            }
                            else { valuePWMA.Text = "0"; }
                        }
                        else if (levelA.Text == "High")
                        {
                            if (delta_t <= 10)
                            {
                                int pwma_high = (10 - delta_t) * 20;
                                if (pwma_high >= 100) { pwma_high = 100; };
                                valuePWMA.Text = pwma_high.ToString();
                            }
                            else { valuePWMA.Text = "0"; }
                        }
                        if (previous_pwm_a != Int16.Parse(valuePWMA.Text)) { driver.SetSwitchValue(4, Convert.ToDouble(valuePWMA.Text)); currentPWMA.Text = driver.GetSwitchValue(4).ToString(); }
                    }
                    if (autoPWMB.Checked)
                    {
                        previous_pwm_b = Int16.Parse(valuePWMB.Text);
                        if (levelB.Text == "Low")
                        {
                            if (delta_t <= 10)
                            {
                                int pwmb_low = (10 - delta_t) * 10;
                                if (pwmb_low >= 100) { pwmb_low = 100; };
                                valuePWMB.Text = pwmb_low.ToString();
                            }
                            else { valuePWMB.Text = "0"; }
                        }
                        else if (levelB.Text == "Mid")
                        {
                            if (delta_t <= 10)
                            {
                                int pwmb_mid = (10 - delta_t) * 15;
                                if (pwmb_mid >= 100) { pwmb_mid = 100; };
                                valuePWMB.Text = pwmb_mid.ToString();
                            }
                            else { valuePWMB.Text = "0"; }
                        }
                        else if (levelB.Text == "High")
                        {
                            if (delta_t <= 10)
                            {
                                int pwmb_high = (10 - delta_t) * 20;
                                if (pwmb_high >= 100) { pwmb_high = 100; };
                                valuePWMB.Text = pwmb_high.ToString();
                            }
                            else { valuePWMB.Text = "0"; }
                        }
                        if (previous_pwm_b != Int16.Parse(valuePWMB.Text)) { driver.SetSwitchValue(5, Convert.ToDouble(valuePWMB.Text)); currentPWMB.Text = driver.GetSwitchValue(5).ToString(); }
                    }

                };
                this.Invoke(refresh);
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsConnected)
                driver.Connected = false;

            Properties.Settings.Default.Save();
        }

        private void buttonChoose_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DriverId = ASCOM.DriverAccess.Switch.Choose(Properties.Settings.Default.DriverId);
            SetUIState();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                t.Abort();
                clk.Abort();
                
                seconds = 0;
                minutes = 0;
                hours = 0;
                
                driver.Connected = false;
                connectionStatus.ForeColor = Color.Red;
                connectionStatus.Text = "NOT CONNECTED";
                buttonConnect.Text = "Connect";
                buttonChoose.Enabled = true;
                
                temp.Text = "";
                humidity.Text = "";
                dewpoint.Text = "";
                runtime.Text = "";
                voltage.Text = "";
                current.Text = "";
                power.Text = "";
                energy.Text = "";
                
                nameDC1.Text = "";
                nameDC1.Enabled = false;
                nameDC2.Text = "";
                nameDC2.Enabled = false;
                nameDC3.Text = "";
                nameDC3.Enabled = false;
                nameDC45.Text = "";
                nameDC45.Enabled = false;
                
                namePWMA.Text = "";
                namePWMA.Enabled = false;
                namePWMB.Text = "";
                namePWMB.Enabled = false;
                
                nameEXT1.Text = "";
                nameEXT1.Enabled = false;
                nameEXT2.Text = "";
                nameEXT2.Enabled = false;
                
                buttonDC1.Checked = false;
                buttonDC1.Enabled = false;
                buttonDC2.Checked = false;
                buttonDC2.Enabled = false;
                buttonDC3.Checked = false;
                buttonDC3.Enabled = false;
                buttonDC45.Checked = false;
                buttonDC45.Enabled = false;
                
                buttonEXT1.Checked = false;
                buttonEXT1.Enabled = false;
                buttonEXT2.Checked = false;
                buttonEXT2.Enabled = false;

                
                addPWMA.BackColor = Color.DimGray;
                removePWMA.BackColor = Color.DimGray;
                addPWMA.Enabled = false;
                addPWMB.Enabled = false;

                addPWMB.BackColor = Color.DimGray;
                removePWMB.BackColor = Color.DimGray;
                removePWMA.Enabled = false;
                removePWMB.Enabled = false;

                setPWMA.Enabled = false;
                setPWMB.Enabled = false;

                levelA.Enabled = false;
                levelB.Enabled = false;
                autoPWMB.Checked = false;
                autoPWMB.Enabled = false;
                autoPWMA.Checked = false;
                autoPWMA.Enabled = false;
                Off_PWMA.Enabled = false;
                Off_PWMB.Enabled = false;
                Max_PWMA.Enabled = false;
                Max_PWMB.Enabled = false;
                autoPWMA.Enabled = false;
                autoPWMB.Enabled = false;
                Mid_PWMA.Enabled = false;
                Mid_PWMB.Enabled = false;

                valuePWMA.Text = "0";
                valuePWMB.Text = "0";
                currentPWMA.Text = "0";
                currentPWMB.Text = "0";

            }
            else
            {
                
                driver = new ASCOM.DriverAccess.Switch(Properties.Settings.Default.DriverId);
                driver.Connected = true;
                runtime.Text = "0:00:00";
                levelA.Enabled = true;
                levelB.Enabled = true;
                addPWMB.BackColor = Color.Salmon;
                removePWMB.BackColor = Color.Salmon;
                addPWMA.BackColor = Color.Salmon;
                removePWMA.BackColor = Color.Salmon;
                connectionStatus.ForeColor = Color.LightGreen;
                connectionStatus.Text = "CONNECTED";
                buttonChoose.Enabled = false;
                temp.Text = driver.GetSwitchValue(6).ToString();
                humidity.Text = driver.GetSwitchValue(7).ToString();
                dewpoint.Text = driver.GetSwitchValue(8).ToString();
                voltage.Text = driver.GetSwitchValue(9).ToString();
                valuePWMA.Text = driver.GetSwitchValue(4).ToString();
                valuePWMB.Text = driver.GetSwitchValue(5).ToString();
                current.Text = driver.GetSwitchValue(10).ToString();
                power.Text = driver.GetSwitchValue(11).ToString();
                energy.Text = driver.GetSwitchValue(12).ToString();
                //autoPWM.Checked = driver.GetSwitch(13);
                autoPWMB.Enabled = true;
                addPWMA.Enabled = true;
                addPWMB.Enabled = true;
                removePWMA.Enabled = true;
                removePWMB.Enabled = true;
                setPWMA.Enabled = true;
                setPWMB.Enabled = true;
                nameDC1.Text = driver.GetSwitchName(0);
                nameDC1.Enabled = true;
                nameDC2.Text = driver.GetSwitchName(1);
                nameDC2.Enabled = true;
                nameDC3.Text = driver.GetSwitchName(2);
                nameDC3.Enabled = true;
                nameDC45.Text= driver.GetSwitchName(3);
                nameDC45.Enabled = true;
                namePWMA.Text= driver.GetSwitchName(4);
                namePWMA.Enabled = true;
                namePWMB.Text= driver.GetSwitchName(5);
                namePWMB.Enabled = true;
                nameEXT1.Text= driver.GetSwitchName(14);
                nameEXT1.Enabled = true;
                nameEXT2.Text= driver.GetSwitchName(15);
                nameEXT2.Enabled = true;
                buttonDC1.Checked = driver.GetSwitch(0);
                buttonDC1.Enabled = true;
                buttonDC2.Checked = driver.GetSwitch(1);
                buttonDC2.Enabled = true;
                buttonDC3.Checked = driver.GetSwitch(2);
                buttonDC3.Enabled = true;
                buttonDC45.Checked = driver.GetSwitch(3);
                buttonDC45.Enabled= true;
                Off_PWMA.Enabled = true;
                Off_PWMB.Enabled = true;
                Max_PWMA.Enabled = true;
                Max_PWMB.Enabled = true;
                Mid_PWMA.Enabled = true;
                Mid_PWMB.Enabled = true;
                autoPWMA.Enabled = true;
                autoPWMB.Enabled = true;
                buttonEXT1.Checked = driver.GetSwitch(14);
                buttonEXT1.Enabled = true;
                buttonEXT2.Checked = driver.GetSwitch(15);
                buttonEXT2.Enabled = true;
                currentPWMA.Text = driver.GetSwitchValue(4).ToString();
                currentPWMB.Text = driver.GetSwitchValue(5).ToString();
                t = new System.Threading.Thread(RefreshData);
                t.Start();
                clk = new System.Threading.Thread(Stopwatch);
                clk.Start();
            }
            SetUIState();
        }

        private void SetUIState()
        {
            buttonConnect.Enabled = !string.IsNullOrEmpty(Properties.Settings.Default.DriverId);
            buttonChoose.Enabled = !IsConnected;
            buttonConnect.Text = IsConnected ? "Disconnect" : "Connect";
        }

        private bool IsConnected
        {
            get
            {
                return ((this.driver != null) && (driver.Connected == true));
            }
        }

        private void renameDC1_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.SetSwitchName(0, nameDC1.Text.ToString());
                nameDC1.Text = driver.GetSwitchName(0);
            }
            else
            {
                MessageBox.Show("Connect to the powerbox first!");
                nameDC1.Text = "";
            }
        }

        private void renameDC2_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.SetSwitchName(1, nameDC2.Text.ToString());
                nameDC2.Text= driver.GetSwitchName(1);
            }
            else
            {
                MessageBox.Show("Connect to the powerbox first!");
                nameDC2.Text = string.Empty;
            }
        }

        private void renameDC3_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.SetSwitchName(2, nameDC3.Text.ToString());
                nameDC3.Text= driver.GetSwitchName(2);
            }
            else
            {
                MessageBox.Show("Connect to the powerbox first!");
                nameDC3.Text = string.Empty;
            }
        }

        private void renameDC45_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.SetSwitchName(3, nameDC45.Text.ToString());
                nameDC45.Text = driver.GetSwitchName(3);
            }
            else
            {
                MessageBox.Show("Connect to the powerbox first!");
                nameDC45.Text = string.Empty;
            }
        }

        private void renameEXT1_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.SetSwitchName(14, nameEXT1.Text.ToString());
                nameEXT1.Text= driver.GetSwitchName(14);
            }
            else
            {
                MessageBox.Show("Connect to the powerbox first!");
                nameEXT1.Text = string.Empty;
            }
        }

        private void renameEXT2_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.SetSwitchName(15, nameEXT2.Text.ToString());
                nameEXT2.Text= driver.GetSwitchName(15);
            }
            else
            {
                MessageBox.Show("Connect to the powerbox first!");
                nameEXT2.Text = string.Empty;
            }
        }

        private void renamePWMB_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.SetSwitchName(5, namePWMB.Text.ToString());
                namePWMB.Text = driver.GetSwitchName(5);
            }
            else
            {
                MessageBox.Show("Connect to the powerbox first!");
                namePWMB.Text = string.Empty;
            }
        }

        private void renamePWMA_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.SetSwitchName(4, namePWMA.Text.ToString());
                namePWMA.Text = driver.GetSwitchName(4);
            }
            else
            {
                MessageBox.Show("Connect to the powerbox first!");
                namePWMA.Text = string.Empty;
            }
        }


        private void buttonDC1_CheckedChanged(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.SetSwitch(0, buttonDC1.Checked);
                Thread.Sleep(200);
                //buttonDC1.Checked = driver.GetSwitch(0);
            }
        }

        private void buttonDC2_CheckedChanged(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.SetSwitch(1, buttonDC2.Checked);
                Thread.Sleep(200);
                //buttonDC2.Checked = driver.GetSwitch(1);
            }
        }

        private void buttonDC3_CheckedChanged(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.SetSwitch(2, buttonDC3.Checked);
                Thread.Sleep(200);
                //buttonDC3.Checked = driver.GetSwitch(2);
            }
        }

        private void buttonDC45_CheckedChanged(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.SetSwitch(3, buttonDC45.Checked);
                Thread.Sleep(200);
                //buttonDC45.Checked = driver.GetSwitch(3);
             
            }
        }

        private void buttonEXT1_CheckedChanged(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.SetSwitch(14, buttonEXT1.Checked);
                Thread.Sleep(200);
                //buttonEXT1.Checked = driver.GetSwitch(14);
            }
        }

        private void buttonEXT2_CheckedChanged(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.SetSwitch(15, buttonEXT2.Checked);
                Thread.Sleep(200);
                //buttonEXT2.Checked = driver.GetSwitch(15);
            }
        }

        private void addPWMA_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                if (Convert.ToInt16(valuePWMA.Text) < 100)
                {
                    valuePWMA.Text = (Convert.ToInt16(valuePWMA.Text) + 5).ToString();
                }
            }
            else
            {
                MessageBox.Show("Connect to the powerbox first!");
            }
        }

        private void removePWMA_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                if (Convert.ToInt16(valuePWMA.Text) > 0)
                {
                    valuePWMA.Text = (Convert.ToInt16(valuePWMA.Text) - 5).ToString();
                }
            }
            else
            {
                MessageBox.Show("Connect to the powerbox first!");
            }
        }

        private void addPWMB_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                if (Convert.ToInt16(valuePWMB.Text) < 100)
                {
                    valuePWMB.Text = (Convert.ToInt16(valuePWMB.Text) + 5).ToString();
                }
            }
            else
            {
                MessageBox.Show("Connect to the powerbox first!");
            }
        }

        private void removePWMB_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                if (Convert.ToInt16(valuePWMB.Text) > 0)
                {
                    valuePWMB.Text = (Convert.ToInt16(valuePWMB.Text) - 5).ToString();
                }
            }
            else
            {
                MessageBox.Show("Connect to the powerbox first!");
            }
        }

        private void setPWMA_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.SetSwitchValue(4, Convert.ToDouble(valuePWMA.Text));
                //currentPWMA.Text = driver.GetSwitchValue(4).ToString();
            }
            else
            {
                MessageBox.Show("Connect to the powerbox first!");
            }
        }

        private void setPWMB_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.SetSwitchValue(5, Convert.ToDouble(valuePWMB.Text));
                //currentPWMB.Text = driver.GetSwitchValue(5).ToString();
            }
            else
            {
                MessageBox.Show("Connect to the powerbox first!");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                
                seconds = 0;
                minutes = 0;
                hours = 0;
                driver.Connected = false;
                addPWMB.BackColor = Color.DimGray;
                removePWMB.BackColor = Color.DimGray;
                addPWMA.BackColor = Color.DimGray;
                removePWMA.BackColor = Color.DimGray;
                connectionStatus.ForeColor = Color.Red;
                connectionStatus.Text = "NOT CONNECTED";
                buttonConnect.Text = "Connect";
                buttonChoose.Enabled = true;
                temp.Text = "";
                humidity.Text = "";
                dewpoint.Text = "";
                runtime.Text = "";
                voltage.Text = "";
                current.Text = "";
                power.Text = "";
                energy.Text = "";
                nameDC1.Text = "";
                nameDC1.Enabled = false;
                nameDC2.Text = "";
                nameDC2.Enabled = false;
                nameDC3.Text = "";
                nameDC3.Enabled = false;
                nameDC45.Text = "";
                nameDC45.Enabled = false;
                namePWMA.Text = "";
                namePWMA.Enabled = false;
                namePWMB.Text = "";
                namePWMB.Enabled = false;
                nameEXT1.Text = "";
                nameEXT1.Enabled = false;
                nameEXT2.Text = "";
                nameEXT2.Enabled = false;
                buttonDC1.Checked = false;
                buttonDC1.Enabled = false;
                buttonDC2.Checked = false;
                buttonDC2.Enabled = false;
                buttonDC3.Checked = false;
                buttonDC3.Enabled = false;
                buttonDC45.Checked = false;
                buttonDC45.Enabled = false;
                buttonEXT1.Checked = false;
                buttonEXT1.Enabled = false;
                buttonEXT2.Checked = false;
                buttonEXT2.Enabled = false;
                autoPWMB.Checked = false;
                autoPWMB.Enabled = false;
                addPWMA.Enabled = false;
                addPWMB.Enabled = false;
                removePWMA.Enabled = false;
                removePWMB.Enabled = false;
                setPWMA.Enabled = false;
                setPWMB.Enabled = false;
                valuePWMA.Text = "0";
                valuePWMB.Text = "0";
                currentPWMA.Text = "0";
                currentPWMB.Text = "0";
                t.Abort();
                clk.Abort();
                Dispose();
            }
            Thread.Sleep(1000);
            this.Close();
        }

        private void Off_PWMA_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                valuePWMA.Text = "0";
                driver.SetSwitchValue(4, 0);
                currentPWMA.Text = "0";
            }
        }

        private void Max_PWMA_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                valuePWMA.Text = "100";
                driver.SetSwitchValue(4, 100);
                currentPWMA.Text = "100";
            }
        }

        private void Off_PWMB_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                valuePWMB.Text = "0";
                driver.SetSwitchValue(5, 0);
                currentPWMB.Text = "0";
            }
        }

        private void Max_PWMB_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                valuePWMB.Text = "100";
                driver.SetSwitchValue(5, 100);
                currentPWMB.Text = "100";
            }
        }

        private void autoPWMA_CheckedChanged(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                if (!autoPWMA.Checked)
                {
                    addPWMA.Enabled = true;
                    removePWMA.Enabled = true;
                    addPWMA.BackColor = Color.Salmon;
                    removePWMA.BackColor = Color.Salmon;
                    setPWMA.Enabled = true;
                    Off_PWMA.Enabled = true;
                    Max_PWMA.Enabled = true;
                    Mid_PWMA.Enabled = true;
                }
                else
                {
                    addPWMA.Enabled = false;
                    removePWMA.Enabled = false;
                    addPWMA.BackColor = Color.DimGray;
                    removePWMA.BackColor = Color.DimGray;
                    setPWMA.Enabled = false;
                    Off_PWMA.Enabled = false;
                    Max_PWMA.Enabled = false;
                    Mid_PWMA.Enabled = false;
                }
            }
        }
        private void autoPWMB_CheckedChanged(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                if (!autoPWMB.Checked)
                {
                    setPWMB.Enabled = true;
                    addPWMB.Enabled = true;
                    addPWMB.BackColor = Color.Salmon;
                    removePWMB.BackColor = Color.Salmon;
                    removePWMB.Enabled = true;
                    Off_PWMB.Enabled = true;
                    Max_PWMB.Enabled = true;
                    Mid_PWMB.Enabled = true;

                }
                else
                {
                    setPWMB.Enabled = false;
                    addPWMB.BackColor = Color.DimGray;
                    removePWMB.BackColor = Color.DimGray;
                    addPWMB.Enabled = false;
                    removePWMB.Enabled = false;
                    Off_PWMB.Enabled = false;
                    Max_PWMB.Enabled = false;
                    Mid_PWMB.Enabled = false;
                }
            }
        }

        private void Mid_PWMA_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                valuePWMA.Text = "50";
                driver.SetSwitchValue(4, 50);
                currentPWMA.Text = "50";
            }
        }

        private void Mid_PWMB_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                valuePWMB.Text = "50";
                driver.SetSwitchValue(5, 50);
                currentPWMB.Text = "50";
            }
        }
    }
}
