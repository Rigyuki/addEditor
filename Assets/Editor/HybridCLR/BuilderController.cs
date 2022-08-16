using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Diagnostics;

using Debug = UnityEngine.Debug;

namespace HybridCLR
{
    public partial class BuilderController
    {
        private readonly string m_InitBatTemplate;
        private readonly string m_InitBatTemp;
        private readonly string m_InitShTemplate;
        private string m_UnityInstallDirectory;

        public string UnityInstallDirectory
        {
            get
            {
                return m_UnityInstallDirectory;
            }
            set
            {
                m_UnityInstallDirectory = value;
                if (!string.IsNullOrEmpty(m_UnityInstallDirectory))
                {
                    EditorPrefs.SetString("UnityInstallDirectory", m_UnityInstallDirectory);
                }
            }
        }

        public string[] VersionNames
        {
            get;
        }

        public string[] VersionValues
        {
            get;
        }

        public string[] PlatformNames
        {
            get;
        }

        public BuilderController()
        {
            m_InitBatTemplate = Application.dataPath + "/../HybridCLRData/init_local_il2cpp_data.bat";
            m_InitBatTemp = Application.dataPath + "/../HybridCLRData/init_local_il2cpp_data_temp.bat";

            m_InitShTemplate = Application.dataPath + "/../HybridCLRData/init_local_il2cpp_data.sh";

            m_UnityInstallDirectory = EditorPrefs.GetString("UnityInstallDirectory");

            VersionNames = new[]
            {
                "2020.3.x",
                "2021.3.x"
            };

            VersionValues = new[]
            {
                "2020.3.33",
                "2021.3.1"
            };

            PlatformNames = Enum.GetNames(typeof(Platform));
        }

        public enum Platform : int
        {
            Windows32 = 1 << 0,
            Windows64 = 1 << 1,      
            Android = 1 << 2,

           
        }

        public void InitHybridCLR(int versionIndex)
        {
            if (!File.Exists(m_InitBatTemplate))
            {
                Debug.LogErrorFormat("File not Exit : {0}", m_InitBatTemplate);
                return;
            }
            if (!UnityInstallDirectory.Contains(VersionValues[versionIndex]))
            {
                Debug.LogErrorFormat("Please select {0} install unity path :", VersionValues[versionIndex]);
                return;
            }

            string  command ;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                command = File.ReadAllText(m_InitBatTemplate);
                command = command.Replace("__VERSION__", VersionValues[versionIndex]);
                command = command.Replace("__PATH__", UnityInstallDirectory);
                File.WriteAllText(m_InitBatTemp, command);
                RunProcess(m_InitBatTemp);
            }
            else
            {
                command = File.ReadAllText(m_InitShTemplate);
                command = command.Replace("__VERSION__", VersionValues[versionIndex]);
                command = command.Replace("__PATH__", UnityInstallDirectory);
                File.WriteAllText(m_InitShTemplate, command);
                RunProcess(m_InitShTemplate);
            }
        }

        private void RunProcess(string fileName)
        {
            using (Process p = new Process())
            {
                p.StartInfo.WorkingDirectory = Application.dataPath + "/../HybridCLRData";
                p.StartInfo.FileName = fileName;
                p.StartInfo.UseShellExecute = true;
                p.Start();
                p.WaitForExit();
            }
        }
    }
}
