// TODO fill in this information for your driver, then remove this line!
//
// ASCOM Switch hardware class for DashBoardPowerBoxV3
//
// Description:	 <To be completed by driver developer>
//
// Implements:	ASCOM Switch interface version: <To be completed by driver developer>
// Author:		(XXX) Your N. Here <your@email.here>
//

using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.Astrometry.NOVAS;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;

namespace ASCOM.DashBoardPowerBoxV3.Switch
{
    //
    // TODO Replace the not implemented exceptions with code to implement the function or throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM Switch hardware class for DashBoardPowerBoxV3.
    /// </summary>
    [HardwareClass()] // Class attribute flag this as a device hardware class that needs to be disposed by the local server when it exits.
    internal static class SwitchHardware
    {
        // Constants used for Profile persistence
        internal const string comPortProfileName = "COM Port";
        internal const string comPortDefault = "COM1";
        internal const string traceStateProfileName = "Trace Level";
        internal const string traceStateDefault = "true";
        internal static string ConnectionDelayProfileName = "Connection Delay (ms)";
        internal static string ConnectionDelayDefault = "5000";
        internal static string ConnectionDelay;

        internal static string numSwitchProfileName = "Max Switches";
        internal static short numSwitchDefault = 16;
        internal static short numSwitch = 16;

        internal static string SwitchNameDC1Profile = "DC 1 Name";
        internal static string SwitchNameDC2Profile = "DC 2 Name";
        internal static string SwitchNameDC3Profile = "DC 3 Name";
        internal static string SwitchNameDC45Profile = "DC 4-5 Name";
        internal static string SwitchNamePWM1Profile = "PWM A Name";
        internal static string SwitchNamePWM2Profile = "PWM B Name";
        internal static string SwitchNameEXT1Profile = "External 1";
        internal static string SwitchNameEXT2Profile = "External 2";

        internal static string SwitchNameDC1Default = "DC 1";
        internal static string SwitchNameDC2Default = "DC 2";
        internal static string SwitchNameDC3Default = "DC 3";
        internal static string SwitchNameDC45Default = "DC 4-5";
        internal static string SwitchNamePWM1Default = "PWM A";
        internal static string SwitchNamePWM2Default = "PWM B";
        internal static string SwitchNameEXT1Default = "External 1";
        internal static string SwitchNameEXT2Default = "External 2";

        internal static string SwitchNameDC1 = "DC 1";
        internal static string SwitchNameDC2 = "DC 2";
        internal static string SwitchNameDC3 = "DC 3";
        internal static string SwitchNameDC45 = "DC 4-5";
        internal static string SwitchNamePWM1 = "PWM A";
        internal static string SwitchNamePWM2 = "PWM B";
        internal static string SwitchNameTemp = "Temperature sensor";
        internal static string SwitchNameHum = "Humidity sensor";
        internal static string SwitchNameDew = "Dew Point sensor";
        internal static string SwitchNameVoltage = "Voltage";
        internal static string SwitchNameCurrent = "Current";
        internal static string SwitchNamePower = "Power";
        internal static string SwitchNameEnergy = "Energy";
        internal static string SwitchNameA = "Auto PWM";
        internal static string SwitchNameEXT1 = "EXT 1";
        internal static string SwitchNameEXT2 = "EXT 2";
        internal static string SerialString;

        
        internal static string SwitchDescDC1 = "DC Port 1";
        internal static string SwitchDescDC2 = "DC Port 2";
        internal static string SwitchDescDC3 = "DC Port 3";
        internal static string SwitchDescDC45 = "DC Ports 4 and 5";
        internal static string SwitchDescPWM1 = "PWM Port A power in 10% increments";
        internal static string SwitchDescPWM2 = "PWM Port B power in 10% increments";
        internal static string SwitchDescTemp = "Temperature (C)";
        internal static string SwitchDescHum = "Relative Humidity (%)";
        internal static string SwitchDescDew = "Dew Point (C)";
        internal static string SwitchDescVoltage = "Voltage (V)";
        internal static string SwitchDescCurrent = "Current (A)";
        internal static string SwitchDescPower = "Power (W)";
        internal static string SwitchDescEnergy = "Energy (Wh) used since connection";
        internal static string SwitchDescA = "Auto PWM based on Temperature and Humidity";
        internal static string SwitchDescEXT1 = "External 1";
        internal static string SwitchDescEXT2 = "External 2";

        internal static string SwitchStateDC1Profile = "DC 1 State";
        internal static string SwitchStateDC2Profile = "DC 2 State";
        internal static string SwitchStateDC3Profile = "DC 3 State";
        internal static string SwitchStateDC45Profile = "DC 4-5 State";
        internal static string SwitchStatePWM1Profile = "PWM A State";
        internal static string SwitchStatePWM2Profile = "PWM B State";
        internal static string SwitchStateEXT1Profile = "External 1 State";
        internal static string SwitchStateEXT2Profile = "External 2 State";




        internal static string SwitchStateDC1 = "0";
        internal static string SwitchStateDC2 = "0";
        internal static string SwitchStateDC3 = "0";
        internal static string SwitchStateDC45 = "0";
        internal static string SwitchStatePWM1 = "0";
        internal static string SwitchStatePWM2 = "0";
        internal static string SwitchStateTemp = "0.00";
        internal static string SwitchStateHum = "0.00";
        internal static string SwitchStateDew = "0.00";
        internal static string SwitchStateVoltage = "0.00";
        internal static string SwitchStateCurrent = "0.00";
        internal static string SwitchStatePower = "0.00";
        internal static string SwitchStateEnergy = "0.00";
        internal static string SwitchStateA = "0";
        internal static string SwitchStateEXT1 = "0";
        internal static string SwitchStateEXT2 = "0";

        internal static string[] SerialCommands = new string[16];
        internal static bool workerCanRun = false;

        private static string DriverProgId = ""; // ASCOM DeviceID (COM ProgID) for this driver, the value is set by the driver's class initialiser.
        private static string DriverDescription = ""; // The value is set by the driver's class initialiser.
        internal static string comPort; // COM port name (if required)
        private static bool connectedState; // Local server's connected state
        private static bool runOnce = false; // Flag to enable "one-off" activities only to run once.
        internal static Util utilities; // ASCOM Utilities object for use as required
        internal static AstroUtils astroUtilities; // ASCOM AstroUtilities object for use as required
        internal static TraceLogger tl; // Local server's trace logger object for diagnostic log with information that you specify
        private static ASCOM.Utilities.Serial objSerial;

        /// <summary>
        /// Initializes a new instance of the device Hardware class.
        /// </summary>
        static SwitchHardware()
        {
            try
            {
                // Create the hardware trace logger in the static initialiser.
                // All other initialisation should go in the InitialiseHardware method.
                tl = new TraceLogger("", "DashBoardPowerBoxV3.Hardware");

                // DriverProgId has to be set here because it used by ReadProfile to get the TraceState flag.
                DriverProgId = Switch.DriverProgId; // Get this device's ProgID so that it can be used to read the Profile configuration values

                // ReadProfile has to go here before anything is written to the log because it loads the TraceLogger enable / disable state.
                ReadProfile(); // Read device configuration from the ASCOM Profile store, including the trace state

                LogMessage("SwitchHardware", $"Static initialiser completed.");
            }
            catch (Exception ex)
            {
                try { LogMessage("SwitchHardware", $"Initialisation exception: {ex}"); } catch { }
                MessageBox.Show($"{ex.Message}", "Exception creating ASCOM.DashBoardPowerBoxV3.Switch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        /// <summary>
        /// Place device initialisation code here
        /// </summary>
        /// <remarks>Called every time a new instance of the driver is created.</remarks>
        internal static void InitialiseHardware()
        {
            // This method will be called every time a new ASCOM client loads your driver
            LogMessage("InitialiseHardware", $"Start.");

            // Make sure that "one off" activities are only undertaken once
            if (runOnce == false)
            {
                LogMessage("InitialiseHardware", $"Starting one-off initialisation.");

                DriverDescription = Switch.DriverDescription; // Get this device's Chooser description

                LogMessage("InitialiseHardware", $"ProgID: {DriverProgId}, Description: {DriverDescription}");

                workerCanRun = false;
                connectedState = false; // Initialise connected to false
                utilities = new Util(); //Initialise ASCOM Utilities object
                astroUtilities = new AstroUtils(); // Initialise ASCOM Astronomy Utilities object
                //workerThread = new Thread(new ThreadStart(updateStatus));
                //workerThread.Start();

                LogMessage("InitialiseHardware", "Completed basic initialisation");

                // Add your own "one off" device initialisation here e.g. validating existence of hardware and setting up communications

                LogMessage("InitialiseHardware", $"One-off initialisation complete.");
                runOnce = true; // Set the flag to ensure that this code is not run again
            }
        }

        // PUBLIC COM INTERFACE ISwitchV3 IMPLEMENTATION

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialogue form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public static void SetupDialog()
        {
            // Don't permit the setup dialogue if already connected
            if (IsConnected)
                MessageBox.Show("Already connected, just press OK");

            using (SetupDialogForm F = new SetupDialogForm(tl))
            {
                var result = F.ShowDialog();
                if (result == DialogResult.OK)
                {
                    WriteProfile(); // Persist device configuration values to the ASCOM Profile store
                }
            }
        }

        /// <summary>Returns the list of custom action names supported by this driver.</summary>
        /// <value>An ArrayList of strings (SafeArray collection) containing the names of supported actions.</value>
        public static ArrayList SupportedActions
        {
            get
            {
                LogMessage("SupportedActions Get", "Returning empty ArrayList");
                return new ArrayList();
            }
        }

        /// <summary>Invokes the specified device-specific custom action.</summary>
        /// <param name="ActionName">A well known name agreed by interested parties that represents the action to be carried out.</param>
        /// <param name="ActionParameters">List of required parameters or an <see cref="String.Empty">Empty String</see> if none are required.</param>
        /// <returns>A string response. The meaning of returned strings is set by the driver author.
        /// <para>Suppose filter wheels start to appear with automatic wheel changers; new actions could be <c>QueryWheels</c> and <c>SelectWheel</c>. The former returning a formatted list
        /// of wheel names and the second taking a wheel name and making the change, returning appropriate values to indicate success or failure.</para>
        /// </returns>
        public static string Action(string actionName, string actionParameters)
        {
            LogMessage("Action", $"Action {actionName}, parameters {actionParameters} is not implemented");
            throw new ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and does not wait for a response.
        /// Optionally, protocol framing characters may be added to the string before transmission.
        /// </summary>
        /// <param name="Command">The literal command string to be transmitted.</param>
        /// <param name="Raw">
        /// if set to <c>true</c> the string is transmitted 'as-is'.
        /// If set to <c>false</c> then protocol framing characters may be added prior to transmission.
        /// </param>
        public static void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            // TODO The optional CommandBlind method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandBlind must send the supplied command to the mount and return immediately without waiting for a response

            throw new MethodNotImplementedException($"CommandBlind - Command:{command}, Raw: {raw}.");
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and waits for a boolean response.
        /// Optionally, protocol framing characters may be added to the string before transmission.
        /// </summary>
        /// <param name="Command">The literal command string to be transmitted.</param>
        /// <param name="Raw">
        /// if set to <c>true</c> the string is transmitted 'as-is'.
        /// If set to <c>false</c> then protocol framing characters may be added prior to transmission.
        /// </param>
        /// <returns>
        /// Returns the interpreted boolean response received from the device.
        /// </returns>
        public static bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");
            // TODO The optional CommandBool method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandBool must send the supplied command to the mount, wait for a response and parse this to return a True or False value

            throw new MethodNotImplementedException($"CommandBool - Command:{command}, Raw: {raw}.");
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and waits for a string response.
        /// Optionally, protocol framing characters may be added to the string before transmission.
        /// </summary>
        /// <param name="Command">The literal command string to be transmitted.</param>
        /// <param name="Raw">
        /// if set to <c>true</c> the string is transmitted 'as-is'.
        /// If set to <c>false</c> then protocol framing characters may be added prior to transmission.
        /// </param>
        /// <returns>
        /// Returns the string response received from the device.
        /// </returns>
        public static string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");
            // TODO The optional CommandString method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandString must send the supplied command to the mount and wait for a response before returning this to the client

            throw new MethodNotImplementedException($"CommandString - Command:{command}, Raw: {raw}.");
        }

        /// <summary>
        /// Deterministically release both managed and unmanaged resources that are used by this class.
        /// </summary>
        /// <remarks>
        /// TODO: Release any managed or unmanaged resources that are used in this class.
        /// 
        /// Do not call this method from the Dispose method in your driver class.
        ///
        /// This is because this hardware class is decorated with the <see cref="HardwareClassAttribute"/> attribute and this Dispose() method will be called 
        /// automatically by the  local server executable when it is irretrievably shutting down. This gives you the opportunity to release managed and unmanaged 
        /// resources in a timely fashion and avoid any time delay between local server close down and garbage collection by the .NET runtime.
        ///
        /// For the same reason, do not call the SharedResources.Dispose() method from this method. Any resources used in the static shared resources class
        /// itself should be released in the SharedResources.Dispose() method as usual. The SharedResources.Dispose() method will be called automatically 
        /// by the local server just before it shuts down.
        /// 
        /// </remarks>
        public static void Dispose()
        {
            try { LogMessage("Dispose", $"Disposing of assets and closing down."); } catch { }

            try
            {
                // Clean up the trace logger and utility objects
                tl.Enabled = false;
                tl.Dispose();
                tl = null;
            }
            catch { }

            try
            {
                utilities.Dispose();
                utilities = null;
            }
            catch { }

            try
            {
                astroUtilities.Dispose();
                astroUtilities = null;
            }
            catch { }
        }

        /// <summary>
        /// Set True to connect to the device hardware. Set False to disconnect from the device hardware.
        /// You can also read the property to check whether it is connected. This reports the current hardware state.
        /// </summary>
        /// <value><c>true</c> if connected to the hardware; otherwise, <c>false</c>.</value>
        public static bool Connected
        {

            get
            {
                LogMessage("Connected", $"Get {IsConnected}");
                return IsConnected;
            }
            set
            {
                LogMessage("Connected", $"Set {value}");
                if (value == IsConnected)
                    return;

                if (value)
                {
                    connectedState = true;
                    LogMessage("Connected Set", "Connecting to port " + comPort);
                    // TODO connect to the device
                    objSerial = new ASCOM.Utilities.Serial();
                    string numComPort;
                    numComPort = comPort.Replace("COM", "");
                    objSerial.Port = Convert.ToInt16(numComPort);
                    objSerial.Speed = (SerialSpeed)115200;
                    objSerial.ReceiveTimeout = 5;
                    objSerial.Connected = true;
                    System.Threading.Thread.Sleep(Convert.ToInt16(ConnectionDelay));
                    objSerial.ClearBuffers();
                    objSerial.Transmit(">READEEPROM#"); 
                    objSerial.Transmit(">REFRESHDATA#");
                    SerialString = objSerial.ReceiveTerminated("#").Replace("#", "");
                    workerCanRun = true;

                    // workerThread.Interrupt();
                }
                else
                {
                    objSerial.Transmit(">WRITEEEPROM#");
                    SetSwitchValue(4, 0); ///Turns off PWM1 on disconnect
                    SetSwitchValue(5, 0); ///Turns off PWM2 on disconnect
                    connectedState = false;
                    workerCanRun = false;
                    LogMessage("Connected Set", "Disconnecting from port " + comPort);
                    // TODO disconnect from the device
                    objSerial.Connected = false;
                }
            }
        }

        /// <summary>
        /// Returns a description of the device, such as manufacturer and model number. Any ASCII characters may be used.
        /// </summary>
        /// <value>The description.</value>
        public static string Description
        {
            // TODO customise this device description if required
            get
            {
                string DriverDescription = "ASCOM DashBoard PowerBox V3";
                LogMessage("Description Get", DriverDescription);
                return DriverDescription;
            }
        }

        /// <summary>
        /// Descriptive and version information about this ASCOM driver.
        /// </summary>
        public static string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                // TODO customise this driver description if required
                string driverInfo = $"ASCOM DashBoard PowerBox V3. Version: {version.Major}.{version.Minor}, more information on github ADD SITE";
                LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }

        /// <summary>
        /// A string containing only the major and minor version of the driver formatted as 'm.n'.
        /// </summary>
        public static string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = $"{version.Major}.{version.Minor}";
                LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        /// <summary>
        /// The interface version number that this device supports.
        /// </summary>
        public static short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                LogMessage("InterfaceVersion Get", "3");
                return Convert.ToInt16("3");
            }
        }

        /// <summary>
        /// The short name of the driver, for display purposes
        /// </summary>
        public static string Name
        {
            // TODO customise this device name as required
            get
            {
                string name = "ASCOM DashBoard PowerBox V3";
                LogMessage("Name Get", name);
                return name;
            }
        }

        #endregion

        #region ISwitchV2 Implementation


        /// <summary>
        /// The number of switches managed by this driver
        /// </summary>
        /// <returns>The number of devices managed by this driver.</returns>
        internal static short MaxSwitch
        {
            get
            {
                LogMessage("MaxSwitch Get", numSwitch.ToString());
                return numSwitch;
            }
        }

        /// <summary>
        /// Return the name of switch device n.
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <returns>The name of the device</returns>
        internal static string GetSwitchName(short id)
        {
            Validate("GetSwitchName", id);
            switch (id)
            {
                case 0: return SwitchNameDC1;
                case 1: return SwitchNameDC2;
                case 2: return SwitchNameDC3;
                case 3: return SwitchNameDC45;
                case 4: return SwitchNamePWM1;
                case 5: return SwitchNamePWM2;
                case 6: return SwitchNameTemp;
                case 7: return SwitchNameHum;
                case 8: return SwitchNameDew;
                case 9: return SwitchNameVoltage;
                case 10: return SwitchNameCurrent;
                case 11: return SwitchNamePower;
                case 12: return SwitchNameEnergy;
                case 13: return SwitchNameA;
                case 14: return SwitchNameEXT1;
                case 15: return SwitchNameEXT2;
                default:
                    LogMessage("GetSwitchName", $"GetSwitchName({id}) - Invalid Switch ID!");
                    throw new InvalidValueException("Switch", numSwitch.ToString(), string.Format("0 to {0}", Convert.ToInt16(numSwitch) - 1));
            }
        }

        /// <summary>
        /// Set a switch device name to a specified value.
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <param name="name">The name of the device</param>
        internal static void SetSwitchName(short id, string name)
        {
            Validate("SetSwitchName", id);
            using (var driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Switch";
                switch (id)
                {
                    case 0: LogMessage("SetSwitchName " + id.ToString(), name); SwitchNameDC1 = name; break;
                    case 1: LogMessage("SetSwitchName " + id.ToString(), name); SwitchNameDC2 = name; break;
                    case 2: LogMessage("SetSwitchName " + id.ToString(), name); SwitchNameDC3 = name; break;
                    case 3: LogMessage("SetSwitchName " + id.ToString(), name); SwitchNameDC45 = name; break;
                    case 4: LogMessage("SetSwitchName " + id.ToString(), name); SwitchNamePWM1 = name; break;
                    case 5: LogMessage("SetSwitchName " + id.ToString(), name); SwitchNamePWM2 = name; break;
                    case 6: LogMessage("SetSwitchName " + id.ToString(), name); SwitchNameTemp = name; break;
                    case 7: LogMessage("SetSwitchName " + id.ToString(), name); SwitchNameHum = name; break;
                    case 8: LogMessage("SetSwitchName " + id.ToString(), name); SwitchNameDew = name; break;
                    case 9: LogMessage("SetSwitchName " + id.ToString(), name); SwitchNameVoltage = name; break;
                    case 10: LogMessage("SetSwitchName " + id.ToString(), name); SwitchNameCurrent = name; break;
                    case 11: LogMessage("SetSwitchName " + id.ToString(), name); SwitchNamePower = name; break;
                    case 12: LogMessage("SetSwitchName " + id.ToString(), name); SwitchNameEnergy = name; break;
                    case 13: LogMessage("SetSwitchName " + id.ToString(), name); SwitchNameA = name; break;
                    case 14: LogMessage("SetSwitchName " + id.ToString(), name); SwitchNameEXT1 = name; break;
                    case 15: LogMessage("SetSwitchName " + id.ToString(), name); SwitchNameEXT2 = name; break;
                    default:
                        LogMessage("SetSwitchName", $"SetSwitchName({id}) - Invalid Switch ID!");
                        throw new InvalidValueException("Switch", numSwitch.ToString(), string.Format("0 to {0}", Convert.ToInt16(numSwitch) - 1));
                }
                WriteProfile();
            }
        }

        /// <summary>
        /// Gets the description of the specified switch device. This is to allow a fuller description of
        /// the device to be returned, for example for a tool tip.
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <returns>
        /// String giving the device description.
        /// </returns>
        internal static string GetSwitchDescription(short id)
        {
            Validate("GetSwitchDescription", id);
            switch (id)
            {
                case 0: return SwitchDescDC1;
                case 1: return SwitchDescDC2;
                case 2: return SwitchDescDC3;
                case 3: return SwitchDescDC45;
                case 4: return SwitchDescPWM1;
                case 5: return SwitchDescPWM2;
                case 6: return SwitchDescTemp;
                case 7: return SwitchDescHum;
                case 8: return SwitchDescDew;
                case 9: return SwitchDescVoltage;
                case 10: return SwitchDescCurrent;
                case 11: return SwitchDescPower;
                case 12: return SwitchDescEnergy;
                case 13: return SwitchDescA;
                case 14: return SwitchDescEXT1;
                case 15: return SwitchDescEXT2;
                default:
                    LogMessage("GetSwitchDescription", $"GetSwitchDescription({id}) - MethodNotImplemented");
                    throw new MethodNotImplementedException("GetSwitchDescription");
            }
        }


        /// <summary>
        /// Reports if the specified switch device can be written to, default true.
        /// This is false if the device cannot be written to, for example a limit switch or a sensor.
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <returns>
        /// <c>true</c> if the device can be written to, otherwise <c>false</c>.
        /// </returns>
        internal static bool CanWrite(short id)
        {
            Validate("CanWrite", id);
            switch (id)
            {
                case 0: LogMessage("CanWrite", $"CanWrite({id}): true"); return true; 
                case 1: LogMessage("CanWrite", $"CanWrite({id}): true"); return true; 
                case 2: LogMessage("CanWrite", $"CanWrite({id}): true"); return true; 
                case 3: LogMessage("CanWrite", $"CanWrite({id}): true"); return true; 
                case 4: LogMessage("CanWrite", $"CanWrite({id}): true"); return true; 
                case 5: LogMessage("CanWrite", $"CanWrite({id}): true"); return true; 
                case 6: LogMessage("CanWrite", $"CanWrite({id}): false"); return false; 
                case 7: LogMessage("CanWrite", $"CanWrite({id}): false"); return false; 
                case 8: LogMessage("CanWrite", $"CanWrite({id}): false"); return false; 
                case 9: LogMessage("CanWrite", $"CanWrite({id}): false"); return false; 
                case 10: LogMessage("CanWrite", $"CanWrite({id}): false"); return false; 
                case 11: LogMessage("CanWrite", $"CanWrite({id}): false"); return false; 
                case 12: LogMessage("CanWrite", $"CanWrite({id}): false"); return false; 
                case 13: LogMessage("CanWrite", $"CanWrite({id}): true"); return true; 
                case 14: LogMessage("CanWrite", $"CanWrite({id}): true"); return true; 
                case 15: LogMessage("CanWrite", $"CanWrite({id}): true"); return true; 
                default:
                    LogMessage("CanWrite", $"CanWrite({id}): MethodNotImplemented");
                    throw new MethodNotImplementedException("CanWrite");
            }
        }

        #region Boolean switch members

        /// <summary>
        /// Return the state of switch device id as a boolean
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <returns>True or false</returns>
        internal static bool GetSwitch(short id)
        {
            Validate("GetSwitch", id);
            using (var driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Switch";
                if ((id == 0) || (id == 1) || (id == 2) || (id == 3) || (id == 13) || (id == 14) || (id == 15))
                {
                    string Message = ">GET" + id + "#";
                    objSerial.Transmit(Message);
                    string Received = objSerial.ReceiveTerminated("#").Replace("#", "");
                    LogMessage(Message, Received);
                    bool rtrn = NumberToBool(Convert.ToDouble(Received));
                    switch (id)
                    {
                        case 0: SwitchStateDC1 = Received; break;
                        //Convert.ToDouble(SerialCommands[0]); LogMessage("GetSwitch " + id.ToString(), Convert.ToString(NumberToBool(DC1))); return NumberToBool(DC1); 
                        case 1: SwitchStateDC2 = Received; break;
                        //Convert.ToDouble(SerialCommands[1]); LogMessage("GetSwitch " + id.ToString(), Convert.ToString(NumberToBool(DC2))); return NumberToBool(DC2);
                        case 2: SwitchStateDC3 = Received; break;
                        //Convert.ToDouble(SerialCommands[2]); LogMessage("GetSwitch " + id.ToString(), Convert.ToString(NumberToBool(DC3))); return NumberToBool(DC3); 
                        case 3: SwitchStateDC45 = Received; break;
                        //Convert.ToDouble(SerialCommands[3]); LogMessage("GetSwitch " + id.ToString(), Convert.ToString(NumberToBool(DC45))); return NumberToBool(DC45);
                        case 13: SwitchStateA = Received; break;
                        //Convert.ToDouble(SerialCommands[13]); LogMessage("GetSwitch " + id.ToString(), Convert.ToString(NumberToBool(AUTOPWM))); return NumberToBool(AUTOPWM);
                        case 14: SwitchStateEXT1 = Received; break;
                        //Convert.ToDouble(SerialCommands[14]); LogMessage("GetSwitch " + id.ToString(), Convert.ToString(NumberToBool(EXT1))); return NumberToBool(EXT1);
                        case 15: SwitchStateEXT2 = Received; break;
                        //Convert.ToDouble(SerialCommands[15]); LogMessage("GetSwitch " + id.ToString(), Convert.ToString(NumberToBool(EXT2))); return NumberToBool(EXT2);
                        default:
                            break;
                    }
                    return rtrn;
                    
                }
                else 
                {
                    LogMessage("GetSwitch", $"GetSwitch({id}) - not implemented");
                    throw new MethodNotImplementedException("GetSwitch");
                }
            }
        }

        /// <summary>
        /// Sets a switch controller device to the specified state, true or false.
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <param name="state">The required control state</param>
        internal static void SetSwitch(short id, bool state)
        {
            Validate("SetSwitch", id);
            string stringState = BoolToNumber(state).ToString();
            if (!CanWrite(id))
            {
                var str = $"SetSwitch({id}) - Cannot Write";
                LogMessage("SetSwitch", str);
                throw new MethodNotImplementedException(str);
            }
            else
            {
                using (var driverProfile = new Profile())
                {
                    driverProfile.DeviceType = "Switch";
                    switch (id)
                    {
                        case 0: SwitchStateDC1 = stringState; LogMessage("SetSwitch " + id.ToString(), stringState); objSerial.Transmit(">SETDC1_" + stringState + "#"); break;
                        case 1: SwitchStateDC2 = stringState; LogMessage("SetSwitch " + id.ToString(), stringState); objSerial.Transmit(">SETDC2_" + stringState + "#"); break;
                        case 2: SwitchStateDC3 = stringState; LogMessage("SetSwitch " + id.ToString(), stringState); objSerial.Transmit(">SETDC3_" + stringState + "#"); break;
                        case 3: SwitchStateDC45 = stringState; LogMessage("SetSwitch " + id.ToString(), stringState); objSerial.Transmit(">SETDC4_" + stringState + "#"); break;
                        case 13: SwitchStateA = stringState; LogMessage("SetSwitch " + id.ToString(), stringState); objSerial.Transmit(">SETAUTOPWM_" + stringState + "#"); break;
                        case 14: SwitchStateEXT1 = stringState; LogMessage("SetSwitch " + id.ToString(), stringState); objSerial.Transmit(">SETEXT1_" + stringState + "#"); break;
                        case 15: SwitchStateEXT2 = stringState; LogMessage("SetSwitch " + id.ToString(), stringState); objSerial.Transmit(">SETEXT2_" + stringState + "#"); break;
                        default:
                            LogMessage("SetSwitch", $"SetSwitch({id}) - not implemented");
                            throw new MethodNotImplementedException("SetSwitch");
                    }
                    //WriteProfile();
                }
            }
        }

        #endregion

        #region Analogue members

        /// <summary>
        /// Returns the maximum value for this switch device, this must be greater than <see cref="MinSwitchValue"/>.
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <returns>The maximum value to which this device can be set or which a read only sensor will return.</returns>
        internal static double MaxSwitchValue(short id)
        {
            Validate("MaxSwitchValue", id);
            switch (id)
            {
                case 0: LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}): 1"); return 1;
                case 1: LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}): 1"); return 1;
                case 2: LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}): 1"); return 1;
                case 3: LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}): 1"); return 1;
                case 4: LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}): 1"); return 100;
                case 5: LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}): 1"); return 100;
                case 6: LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}): 100"); return 100;
                case 7: LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}): 100"); return 100;
                case 8: LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}): 100"); return 100;
                case 9: LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}): 30"); return 30;
                case 10: LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}): 32"); return 32;
                case 11: LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}): 300"); return 300;
                case 12: LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}): 1000000"); return 1000000;
                case 13: LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}): 1"); return 1;
                case 14: LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}): 1"); return 1;
                case 15: LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}): 1"); return 1;
                default:
                    LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}): MethodNotImplemented");
                    throw new MethodNotImplementedException("MaxSwitchValue");
            }
        }

        /// <summary>
        /// Returns the minimum value for this switch device, this must be less than <see cref="MaxSwitchValue"/>
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <returns>The minimum value to which this device can be set or which a read only sensor will return.</returns>
        internal static double MinSwitchValue(short id)
        {
            Validate("MinSwitchValue", id);
            switch (id)
            {
                case 0: LogMessage("MinSwitchValue", $"MinSwitchValue({id}): 0"); return 0;
                case 1: LogMessage("MinSwitchValue", $"MinSwitchValue({id}): 0"); return 0;
                case 2: LogMessage("MinSwitchValue", $"MinSwitchValue({id}): 0"); return 0;
                case 3: LogMessage("MinSwitchValue", $"MinSwitchValue({id}): 0"); return 0;
                case 4: LogMessage("MinSwitchValue", $"MinSwitchValue({id}): 0"); return 0;
                case 5: LogMessage("MinSwitchValue", $"MinSwitchValue({id}): 0"); return 0;
                case 6: LogMessage("MinSwitchValue", $"MinSwitchValue({id}): -100"); return -100;
                case 7: LogMessage("MinSwitchValue", $"MinSwitchValue({id}): 0"); return 0;
                case 8: LogMessage("MinSwitchValue", $"MinSwitchValue({id}): -100"); return -100;
                case 9: LogMessage("MinSwitchValue", $"MinSwitchValue({id}): 0"); return 0;
                case 10: LogMessage("MinSwitchValue", $"MinSwitchValue({id}): 0"); return 0;
                case 11: LogMessage("MinSwitchValue", $"MinSwitchValue({id}): 0"); return 0;
                case 12: LogMessage("MinSwitchValue", $"MinSwitchValue({id}): 0"); return 0;
                case 13: LogMessage("MinSwitchValue", $"MinSwitchValue({id}): 0"); return 0;
                case 14: LogMessage("MinSwitchValue", $"MinSwitchValue({id}): 0"); return 0;
                case 15: LogMessage("MinSwitchValue", $"MinSwitchValue({id}): 0"); return 0;
                default:
                    LogMessage("MinSwitchValue", $"MinSwitchValue({id}) - not implemented");
                    throw new MethodNotImplementedException("MinSwitchValue");
            }   
        }

        /// <summary>
        /// Returns the step size that this device supports (the difference between successive values of the device).
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <returns>The step size for this device.</returns>
        internal static double SwitchStep(short id)
        {
            Validate("SwitchStep", id);
            switch (id)
            {
                case 0: LogMessage("SwitchStep", $"SwitchStep({id}): 1"); return 1;
                case 1: LogMessage("SwitchStep", $"SwitchStep({id}): 1"); return 1;
                case 2: LogMessage("SwitchStep", $"SwitchStep({id}): 1"); return 1;
                case 3: LogMessage("SwitchStep", $"SwitchStep({id}): 1"); return 1;
                case 4: LogMessage("SwitchStep", $"SwitchStep({id}): 5"); return 5;
                case 5: LogMessage("SwitchStep", $"SwitchStep({id}): 5"); return 5;
                case 6: LogMessage("SwitchStep", $"SwitchStep({id}): 0.01"); return 0.01;
                case 7: LogMessage("SwitchStep", $"SwitchStep({id}): 0.01"); return 0.01;
                case 8: LogMessage("SwitchStep", $"SwitchStep({id}): 0.01"); return 0.01;
                case 9: LogMessage("SwitchStep", $"SwitchStep({id}): 0.01"); return 0.01;
                case 10: LogMessage("SwitchStep", $"SwitchStep({id}): 0.01"); return 0.01;
                case 11: LogMessage("SwitchStep", $"SwitchStep({id}): 0.01"); return 0.01;
                case 12: LogMessage("SwitchStep", $"SwitchStep({id}): 0.01"); return 0.01;
                case 13: LogMessage("SwitchStep", $"SwitchStep({id}): 1"); return 1;
                case 14: LogMessage("SwitchStep", $"SwitchStep({id}): 1"); return 1;
                case 15: LogMessage("SwitchStep", $"SwitchStep({id}): 1"); return 1;
                default:
                    LogMessage("SwitchStep", $"SwitchStep({id}) - not implemented");
                    throw new MethodNotImplementedException("SwitchStep");
            }
        }

        /// <summary>
        /// Returns the value for switch device id as a double
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <returns>The value for this switch, this is expected to be between <see cref="MinSwitchValue"/> and
        /// <see cref="MaxSwitchValue"/>.</returns>
        internal static double GetSwitchValue(short id)
        {
            Validate("GetSwitchValue", id);
            string Message = ">GET" + id + "#";
            objSerial.Transmit(Message);
            string Received = objSerial.ReceiveTerminated("#").Replace("#", "");
            LogMessage(Message, Received);
            if ((id == 4) || (id == 5) || (id == 6) || (id == 7) || (id == 8) || (id == 9) || (id == 10) || (id == 11) || (id == 12))
            {
                return Convert.ToDouble(Received);
            }   
            else
            {      LogMessage("GetSwitchValue", $"GetSwitchValue({id}) - not implemented");
                    throw new MethodNotImplementedException("GetSwitchValue");
            }
        }

        /// <summary>
        /// Set the value for this device as a double.
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <param name="value">The value to be set, between <see cref="MinSwitchValue"/> and <see cref="MaxSwitchValue"/></param>
        internal static void SetSwitchValue(short id, double value)
        {
            Validate("SetSwitchValue", id, value);
            string SwitchValue;
            if (!CanWrite(id))
            {
                LogMessage("SetSwitchValue", $"SetSwitchValue({id}) - Cannot write");
                throw new ASCOM.MethodNotImplementedException($"SetSwitchValue({id}) - Cannot write");
            }
            else
            {
                using (var driverProfile = new Profile())
                {
                    driverProfile.DeviceType = "Switch";
                    if (id == 4)
                    {
                        LogMessage("SetSwitchValue ", id.ToString(), value);
                        SwitchValue = ">SETPWM1_" + value.ToString() + "#";
                        objSerial.Transmit(SwitchValue);
                    }
                    else if (id == 5)
                    {
                        LogMessage("SetSwitchValue ", id.ToString(), value);
                        SwitchValue = ">SETPWM2_" + value.ToString() + "#";
                        objSerial.Transmit(SwitchValue);
                    }
                    else
                    {
                        LogMessage("SetSwitchValue", $"SetSwitchValue({id}) = {value} - not implemented");
                        throw new MethodNotImplementedException("SetSwitchValue");
                    }
                    WriteProfile();
                }
            }
        }

        #endregion

        #endregion

        #region Private methods

        /// <summary>
        /// Checks that the switch id is in range and throws an InvalidValueException if it isn't
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="id">The id.</param>
        private static void Validate(string message, short id)
        {
            if (id < 0 || id >= numSwitch)
            {
                LogMessage(message, string.Format("Switch {0} not available, range is 0 to {1}", id, numSwitch - 1));
                throw new InvalidValueException(message, id.ToString(), string.Format("0 to {0}", numSwitch - 1));
            }
        }

        /// <summary>
        /// Checks that the switch id and value are in range and throws an
        /// InvalidValueException if they are not.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="id">The id.</param>
        /// <param name="value">The value.</param>
        private static void Validate(string message, short id, double value)
        {
            Validate(message, id);
            var min = MinSwitchValue(id);
            var max = MaxSwitchValue(id);
            if (value < min || value > max)
            {
                LogMessage(message, string.Format("Value {1} for Switch {0} is out of the allowed range {2} to {3}", id, value, min, max));
                throw new InvalidValueException(message, value.ToString(), string.Format("Switch({0}) range {1} to {2}", id, min, max));
            }
        }

        #endregion

        #region Private properties and methods
        // Useful methods that can be used as required to help with driver development

        private static bool NumberToBool(double number)
        {
            if (number == 0)
            {
                return false;
            }
            else if (number == 1)
            { 
            return true;
            }
            else
            {
                return false;
            }
        }
        private static short BoolToNumber(bool boolean)
        {
            if (boolean == true)
            {
                return 1;
            }
            else if (boolean == false)
            {
                return 0;
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        private static bool IsConnected
        {
            get
            {
                // TODO check that the driver hardware connection exists and is connected to the hardware
                return connectedState;
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private static void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new NotConnectedException(message);
            }
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        internal static void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Switch";
                tl.Enabled = Convert.ToBoolean(driverProfile.GetValue(DriverProgId, traceStateProfileName, string.Empty, traceStateDefault));
                comPort = driverProfile.GetValue(DriverProgId, comPortProfileName, string.Empty, comPortDefault);
                SwitchNameDC1 = driverProfile.GetValue(DriverProgId, SwitchNameDC1Profile, string.Empty, SwitchNameDC1Default);
                SwitchNameDC2 = driverProfile.GetValue(DriverProgId, SwitchNameDC2Profile, string.Empty, SwitchNameDC2Default);
                SwitchNameDC3 = driverProfile.GetValue(DriverProgId, SwitchNameDC3Profile, string.Empty, SwitchNameDC3Default);
                SwitchNameDC45 = driverProfile.GetValue(DriverProgId, SwitchNameDC45Profile, string.Empty, SwitchNameDC45Default);
                SwitchNamePWM1 = driverProfile.GetValue(DriverProgId, SwitchNamePWM1Profile, string.Empty, SwitchNamePWM1Default);
                SwitchNamePWM2 = driverProfile.GetValue(DriverProgId, SwitchNamePWM2Profile, string.Empty, SwitchNamePWM2Default);
                SwitchNameEXT1 = driverProfile.GetValue(DriverProgId, SwitchNameEXT1Profile, string.Empty, SwitchNameEXT1Default);
                SwitchNameEXT2 = driverProfile.GetValue(DriverProgId, SwitchNameEXT2Profile, string.Empty, SwitchNameEXT2Default);
                SwitchStateDC1 = driverProfile.GetValue(DriverProgId, SwitchStateDC1Profile);
                SwitchStateDC2 = driverProfile.GetValue(DriverProgId, SwitchStateDC2Profile);
                SwitchStateDC3 = driverProfile.GetValue(DriverProgId, SwitchStateDC3Profile);
                SwitchStateDC45 = driverProfile.GetValue(DriverProgId, SwitchStateDC45Profile);
                SwitchStatePWM1 = driverProfile.GetValue(DriverProgId, SwitchStatePWM1Profile );
                SwitchStatePWM2 = driverProfile.GetValue(DriverProgId, SwitchStatePWM2Profile);
                SwitchStateEXT1 = driverProfile.GetValue(DriverProgId, SwitchStateEXT1Profile);
                SwitchStateEXT2 = driverProfile.GetValue(DriverProgId, SwitchStateEXT2Profile);
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        internal static void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Switch";
                driverProfile.WriteValue(DriverProgId, traceStateProfileName, tl.Enabled.ToString());
                driverProfile.WriteValue(DriverProgId, comPortProfileName, comPort.ToString());
                driverProfile.WriteValue(DriverProgId, SwitchNameDC1Profile, SwitchNameDC1);
                driverProfile.WriteValue(DriverProgId, SwitchNameDC2Profile, SwitchNameDC2);
                driverProfile.WriteValue(DriverProgId, SwitchNameDC3Profile, SwitchNameDC3);
                driverProfile.WriteValue(DriverProgId, SwitchNameDC45Profile, SwitchNameDC45);
                driverProfile.WriteValue(DriverProgId, SwitchNamePWM1Profile, SwitchNamePWM1);
                driverProfile.WriteValue(DriverProgId, SwitchNamePWM2Profile, SwitchNamePWM2);
                driverProfile.WriteValue(DriverProgId, SwitchNameEXT1Profile, SwitchNameEXT1);
                driverProfile.WriteValue(DriverProgId, SwitchNameEXT2Profile, SwitchNameEXT2);
                driverProfile.WriteValue(DriverProgId, SwitchStateDC1Profile, SwitchStateDC1);
                driverProfile.WriteValue(DriverProgId, SwitchStateDC2Profile, SwitchStateDC2);
                driverProfile.WriteValue(DriverProgId, SwitchStateDC3Profile, SwitchStateDC3);
                driverProfile.WriteValue(DriverProgId, SwitchStateDC45Profile, SwitchStateDC45);
                driverProfile.WriteValue(DriverProgId, SwitchStatePWM1Profile, SwitchStatePWM1);
                driverProfile.WriteValue(DriverProgId, SwitchStatePWM2Profile, SwitchStatePWM2);
                driverProfile.WriteValue(DriverProgId, SwitchStateEXT1Profile, SwitchStateEXT1);
                driverProfile.WriteValue(DriverProgId, SwitchStateEXT2Profile, SwitchStateEXT2);

            }
        }

        /// <summary>
        /// Log helper function that takes identifier and message strings
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        internal static void LogMessage(string identifier, string message)
        {
            tl.LogMessageCrLf(identifier, message);
        }

        /// <summary>
        /// Log helper function that takes formatted strings and arguments
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        internal static void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            LogMessage(identifier, msg);
        }
        #endregion
    }
}

