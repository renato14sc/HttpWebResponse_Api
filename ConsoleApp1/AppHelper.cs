using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections;


namespace ConsoleApp1
{ 
    public static class ServiceRecoveryOptionHelper
    {
        //Action Enum
        public enum RecoverAction
        {
            None = 0, Restart = 1, Reboot = 2, RunCommand = 3
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]

        public struct ServiceFailureActions
        {
            public int dwResetPeriod;
            [MarshalAs(UnmanagedType.LPWStr)]

            public string lpRebootMsg;
            [MarshalAs(UnmanagedType.LPWStr)]

            public string lpCommand;
            public int cActions;
            public IntPtr lpsaActions;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class ScAction
        {
            public int type;
            public uint dwDelay;
        }

        // Win32 function to open the service control manager
        [DllImport("advapi32.dll")]
        public static extern IntPtr OpenSCManager(string lpMachineName, string lpDatabaseName, int dwDesiredAccess);

        // Win32 function to open a service instance
        [DllImport("advapi32.dll")]
        public static extern IntPtr OpenService(IntPtr hScManager, string lpServiceName, int dwDesiredAccess);

        // Win32 function to change the service config for the failure actions.
        [DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig2")]

        public static extern bool ChangeServiceFailureActions(IntPtr hService, int dwInfoLevel,
            [MarshalAs(UnmanagedType.Struct)]
            ref ServiceFailureActions lpInfo);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "QueryServiceConfig2W")]
        public static extern Boolean QueryServiceConfig2(IntPtr hService, UInt32 dwInfoLevel, IntPtr buffer, UInt32 cbBufSize, out UInt32 pcbBytesNeeded);

        [DllImport("kernel32.dll")]
        public static extern int GetLastError();
    }
    public class FailureAction
    {
        // Default constructor
        public FailureAction() { }

        // Constructor
        public FailureAction(ServiceRecoveryOptionHelper.RecoverAction actionType, int actionDelay)
        {
            Type = actionType;
            Delay = actionDelay;
        }

        // Property to set recover action type
        public ServiceRecoveryOptionHelper.RecoverAction Type { get; set; } = ServiceRecoveryOptionHelper.RecoverAction.None;

        // Property to set recover action delay
        public int Delay { get; set; }
    }

    public static class AppHelpers
    {

        public static long ConvertToUnixTime(DateTime datetime)
        {
            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (long)(datetime - sTime).TotalMilliseconds;
        }

        public static DateTime ConvertToDateTime(long unixTime)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixTime).ToLocalTime();
            return dtDateTime;
        }

        //public static byte[] ConvertToByte(Bitmap image)
        //{
        //    var stream = new MemoryStream();
        //    image.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
        //    return stream.ToArray();
        //}

        //public static Bitmap ConvertToBitmap(string fileName)
        //{
        //    Bitmap bitmap;
        //    using (Stream bmpStream = System.IO.File.Open(fileName, System.IO.FileMode.Open))
        //    {
        //        Image image = Image.FromStream(bmpStream);
        //        bitmap = new Bitmap(image);
        //    }
        //    return bitmap;
        //}

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static T Get<T>(string key)
        {
            var appSetting = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(appSetting)) throw new AppSettingNotFoundException(key);

            var converter = TypeDescriptor.GetConverter(typeof(T));
            return (T)(converter.ConvertFromInvariantString(appSetting));
        }

        public static string[] GetArray(string key)
        {
            var appSetting = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(appSetting)) throw new AppSettingNotFoundException(key);

            string[] str_array = appSetting.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            return str_array;
        }

        public static void Set(string key, string value)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = config.AppSettings.Settings;
                if (settings[key] == null)

                {
                    settings.Add(key, value);

                }
                else
                {
                    settings[key].Value = value;
                }

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException err)
            {
               // Logger.WriteToFile("Erro 1013:Falha ao gravar configuração: {0}", err.ToString());
            }
        }
    }

    public class LocalServiceHelper
    {
        //Change service recovery option settings
        private const int ServiceAllAccess = 0xF01FF;
        private const int ScManagerAllAccess = 0xF003F;
        private const int ServiceConfigFailureActions = 0x2;
        private const int ErrorAccessDenied = 5;

        public static void ChangeRevoveryOption(string serviceName, ServiceRecoveryOptionHelper.RecoverAction firstFailureAction,
            ServiceRecoveryOptionHelper.RecoverAction secondFailureAction, ServiceRecoveryOptionHelper.RecoverAction thirdFailureAction)
        {
            try
            {
                // Open the service control manager
                var scmHndl = ServiceRecoveryOptionHelper.OpenSCManager(null, null, ScManagerAllAccess);
                if (scmHndl.ToInt32() <= 0)
                    return;

                // Open the service
                var svcHndl = ServiceRecoveryOptionHelper.OpenService(scmHndl, serviceName, ServiceAllAccess);

                if (svcHndl.ToInt32() <= 0)
                    return;

                var failureActions = new ArrayList
                {
                    // First Failure Actions and Delay (msec)
                    new FailureAction(firstFailureAction, 0),
                    // Second Failure Actions and Delay (msec)
                    new FailureAction(secondFailureAction, 0),
                    // Subsequent Failures Actions and Delay (msec)
                    new FailureAction(thirdFailureAction, 0)
                };

                var numActions = failureActions.Count;
                var myActions = new int[numActions * 2];
                var currInd = 0;

                foreach (FailureAction fa in failureActions)
                {
                    myActions[currInd] = (int)fa.Type;
                    myActions[++currInd] = fa.Delay;
                    currInd++;
                }

                // Need to pack 8 bytes per struct
                var tmpBuf = Marshal.AllocHGlobal(numActions * 8);

                // Move array into marshallable pointer
                Marshal.Copy(myActions, 0, tmpBuf, numActions * 2);

                // Set the SERVICE_FAILURE_ACTIONS struct
                var config =
                    new ServiceRecoveryOptionHelper.ServiceFailureActions
                    {
                        cActions = 3,
                        dwResetPeriod = 0,
                        lpCommand = null,
                        lpRebootMsg = null,
                        lpsaActions = new IntPtr(tmpBuf.ToInt32())
                    };

                // Call the ChangeServiceFailureActions() abstraction of ChangeServiceConfig2()
                var result =
                    ServiceRecoveryOptionHelper.ChangeServiceFailureActions(svcHndl, ServiceConfigFailureActions,
                        ref config);

                //Check the return
                if (!result)
                {
                    var err = ServiceRecoveryOptionHelper.GetLastError();
                    if (err == ErrorAccessDenied)
                    {
                        throw new Exception("Access Denied while setting Failure Actions");

                    }

                    // Free the memory
                    Marshal.FreeHGlobal(tmpBuf);
                }
            }
            catch (Exception)
            {
                throw new Exception("Unable to set service recovery options");
            }
        }
    }


    [Serializable]
    internal class AppSettingNotFoundException : Exception
    {
        public AppSettingNotFoundException()
        {
        }

        public AppSettingNotFoundException(string message) : base(message)
        {
        }

        public AppSettingNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AppSettingNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
