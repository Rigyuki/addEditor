using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor ;
using UnityEngine;
 

namespace HybridCLR
{
    public class BuildEditor : EditorWindow
    {
        private BuilderController m_Controller;
        private int m_VersionIndex;
        private int m_BuildIndex;
        private int m_HotfixPlatformIndex;

        private int VersionIndex
        {
            get
            {
                return EditorPrefs.GetInt("HybridCLRVersion", 0);
            }
            set
            {
                m_VersionIndex = value;
                EditorPrefs.SetInt("HybridCLRVersion", m_VersionIndex);
            }
        }

        private int BuildIndex
        {
            get
            {
                return EditorPrefs.GetInt("HybridCLRBuildIndex", 0);
            }
            set
            {
                m_BuildIndex = value;
                EditorPrefs.SetInt("HybridCLRBuildIndex", m_BuildIndex);
            }
        }

        private int HotfixPlatformIndex
        {
            get
            {
                return EditorPrefs.GetInt("HybridCLRPlatform", 2);
            }
            set
            {
                m_HotfixPlatformIndex = value;
                EditorPrefs.SetInt("HybridCLRPlatform", m_HotfixPlatformIndex);
            }
        }

        [MenuItem("HybridCLR/HybridCLR Builder", false, 0)]
        private static void Open()
        {
            BuildEditor window = GetWindow<BuildEditor>("HybridCLR Builder", true);
            window.minSize = new Vector2(800f, 580f);          
        }

        private void OnEnable()
        {
            m_Controller = new BuilderController();
            m_VersionIndex = VersionIndex;
            m_BuildIndex = BuildIndex;
            m_HotfixPlatformIndex = HotfixPlatformIndex;
            //�����ļ���
            m_Controller.UnityInstallDirectory = Directory.GetFiles(EditorApplication.applicationPath.ToString(), m_Controller.VersionValues[m_VersionIndex])[0];
            Debug.Log(m_Controller.UnityInstallDirectory);
        }

        private void OnGUI()
        {
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Install HybridCLR��", EditorStyles.boldLabel);
            //EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginVertical("box"); 
            GUISelectUnityDirectory("Unity��װĿ¼��il2cpp", "Select");
            EditorGUILayout.HelpBox(@"·��������il2cppΪ��β������2020.3.33�汾��unity��װ·��Ϊ��C:\Program Files\Unity\Hub\Editor\2020.3.33f1\Editor\Data\il2cpp", MessageType.None);
            int versionIndex = EditorGUILayout.Popup("Unity�汾", m_VersionIndex, m_Controller.VersionNames);
            if (versionIndex != m_VersionIndex)
            {
                VersionIndex = versionIndex;
            }           
            GUIItem("��ʼ��HybridCLR�ֿⲢ��װ��������Ŀ��", "Install", InitHybridCLR);
            EditorGUILayout.HelpBox("���Install��ʼ��װ����ؼ�����н����ȷ�������success ���������������󣬲ű�ʾ��װ�ɹ���", MessageType.Info);
            EditorGUILayout.EndVertical();

            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Build", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("�밴�մ������µ�˳�������Ŀ���", MessageType.Info);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("����ab�������ü����dll���ڱ���hotfix.dlǰ��Ҫbuild����", MessageType.None);
            EditorGUILayout.BeginHorizontal();
            
            int buildIndex = EditorGUILayout.Popup("ѡ��buildƽ̨��", m_HotfixPlatformIndex, m_Controller.PlatformNames);
            if (buildIndex != m_HotfixPlatformIndex)
            {
                HotfixPlatformIndex = buildIndex;
            }
            GUIItem("", "build", build);
            EditorGUILayout.EndHorizontal();
            compilePlatform();
            EditorGUILayout.BeginHorizontal();
            int hotfixPlatformIndex = EditorGUILayout.Popup("��Ҫ��buildһ��", m_HotfixPlatformIndex, m_Controller.PlatformNames);
            if (hotfixPlatformIndex != m_HotfixPlatformIndex)
            {
                HotfixPlatformIndex = hotfixPlatformIndex;                
            }
            GUIItem("", "build", build);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Method Bridge", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("HybridCLR�Ѿ�ɨ���Unity���Ŀ�ͳ����ĵ�������������Ĭ�ϵ��ŽӺ�������");
            EditorGUILayout.LabelField("��ش����ļ�Ϊhybridclr/interpreter/MethodBridge_{abi}.cpp������{abi}Universal32��Universal64��Arm64��");
            EditorGUILayout.LabelField("ʵ����Ŀ���ܻ�����һЩaot�����Ĺ����ŽӺ�������Ĭ���ŽӺ������С�");
            EditorGUILayout.LabelField("����ṩ��Editor���ߣ����ݳ����Զ����������ŽӺ�����");
            GUIItem("���ݳ����Զ����������ŽӺ�����Universal32��", "Generate", MethodBridgeHelper.MethodBridge_Universal32);
            GUIItem("���ݳ����Զ����������ŽӺ�����Universal64��", "Generate", MethodBridgeHelper.MethodBridge_Universal64);
            GUIItem("���ݳ����Զ����������ŽӺ�����Arm64��", "Generate", MethodBridgeHelper.MethodBridge_Arm64);
            GUIItem("���ݳ����Զ����������ŽӺ�����ALL��", "Generate", MethodBridgeHelper.MethodBridge_All);
            EditorGUILayout.EndVertical();

            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Reporter", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");          
            GUIItem("���ɱ���", "Create",ReporterEditor.CreateReporter);           
            EditorGUILayout.EndVertical();

            string description = "";
            EditorGUILayout.TextArea(description, GUILayout.MaxHeight(75));
            //FindDirectory("2020.3.33" );
            //description = FindDirectory("2020.3.33").ToString();

            //�������Բ����ļ���
            description = Directory.GetFiles(@"c:\", "2020.3.33")[0];
           
        }

        private void compilePlatform()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("�����Ӧƽ̨���б���");
            if (GUILayout.Button("ActiveBuildTarget", GUILayout.Width(150)))
            {
                CompileDllHelper.CompileDllActiveBuildTarget();
            }
            if (GUILayout.Button("win64", GUILayout.Width(100)))
            {
                CompileDllHelper.CompileDllWin64();                
            }
            if (GUILayout.Button("win32", GUILayout.Width(100)))
            {
                CompileDllHelper.CompileDllWin32();
            }
            if (GUILayout.Button("Android", GUILayout.Width(100)))
            {
                CompileDllHelper.CompileDllAndroid();
            }
            if (GUILayout.Button("iOS", GUILayout.Width(100)))
            {
                CompileDllHelper.CompileDllIOS();
            }
            EditorGUILayout.EndHorizontal();
        }
        private void GUIItem(string content, string button, Action onClick)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(content);
            if (GUILayout.Button(button, GUILayout.Width(100)))
            {
                onClick?.Invoke();
                GUIUtility.ExitGUI();
            }
            EditorGUILayout.EndHorizontal();
            
        }

        private void GUISelectUnityDirectory(string content, string selectButton)
        {
            EditorGUILayout.BeginHorizontal();
            

            m_Controller.UnityInstallDirectory = EditorGUILayout.TextField(content, m_Controller.UnityInstallDirectory);
            if (GUILayout.Button(selectButton, GUILayout.Width(100)))
            {
                string temp = EditorUtility.OpenFolderPanel(content, m_Controller.UnityInstallDirectory, string.Empty);
                if (!string.IsNullOrEmpty(temp))
                {
                    m_Controller.UnityInstallDirectory = temp;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void InitHybridCLR()
        {
            m_Controller.InitHybridCLR(m_VersionIndex);
        }

        private void build()
        {
            if (m_HotfixPlatformIndex == 0)
            {
                BuildPlayerHelper.Build_Win32();
            }
            if (m_HotfixPlatformIndex == 1)
            {
                BuildPlayerHelper.Build_Win64();
            }
            if(m_HotfixPlatformIndex == 2)
            {
                BuildPlayerHelper.Build_Android64();
            }
        }



       /* static void Main(string[] args)
        {
            String dirname = Console.ReadLine();
            Console.WriteLine("���ҵ�" + FindDirectory(dirname).ToString() + "���ļ���");
            Console.ReadKey();
        }*/

        public  int FindDirectory(String dirname )
        {
            String[] logicDrivers = Environment.GetLogicalDrives();
            int count = 0;
            for (int i = 0; i < logicDrivers.Length; i++)
            {
                List<String> dirlist = new List<string>();
                getDirs(logicDrivers[i], dirname, dirlist);
                String[] dirs = dirlist.ToArray();
                for (int j = 0; j < dirs.Length; j++)
                {
                    count++;
                    Console.WriteLine(dirs[j]);

                }
            }
            return count;
        }
        static void getDirs(String dirpath, String dirname, List<String> dirlist)
        {
            try
            {
                dirlist.AddRange(Directory.GetDirectories(dirpath, dirname, SearchOption.TopDirectoryOnly));
                String[] dirs = Directory.GetDirectories(dirpath);
                for (int i = 0; i < dirs.Length; i++)
                {
                    getDirs(dirs[i], dirname, dirlist);
                }
            }
            catch
            {
                return;
            }
        }
    }
}
