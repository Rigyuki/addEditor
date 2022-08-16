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
            window.minSize = new Vector2(800f, 500f);
        }

        private void OnEnable()
        {
            m_Controller = new BuilderController();
            m_VersionIndex = VersionIndex;
            m_BuildIndex = BuildIndex;
            m_HotfixPlatformIndex = HotfixPlatformIndex;
        }

        private void OnGUI()
        {
            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Install HybridCLR：", EditorStyles.boldLabel);
            //EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginVertical("box"); 
            GUISelectUnityDirectory("Unity安装目录的il2cpp", "Select");
            int versionIndex = EditorGUILayout.Popup("Unity版本", m_VersionIndex, m_Controller.VersionNames);
            if (versionIndex != m_VersionIndex)
            {
                VersionIndex = versionIndex;
            }
            GUIItem("初始化HybridCLR仓库并安装到到本项目。", "Install", InitHybridCLR);
            EditorGUILayout.HelpBox("安装HybridCLR需要git和网络。点击Install开始安装，务必检查运行结果，确保输出了success ，而不是其他错误，才表示安装成功。", MessageType.Info);
            EditorGUILayout.EndVertical();

            GUILayout.Space(5f);
            EditorGUILayout.LabelField("Build", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("请按照从上往下的顺序，完成项目打包", MessageType.Info);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("由于ab包依赖裁剪后的dll，在编译hotfix.dl前需要build工程", MessageType.None);
            EditorGUILayout.BeginHorizontal();
            
            int buildIndex = EditorGUILayout.Popup("选择build平台。", m_HotfixPlatformIndex, m_Controller.PlatformNames);
            if (buildIndex != m_HotfixPlatformIndex)
            {
                HotfixPlatformIndex = buildIndex;
            }
            GUIItem("", "build", build);
            EditorGUILayout.EndHorizontal();
            compilePlatform();
            EditorGUILayout.BeginHorizontal();
            int hotfixPlatformIndex = EditorGUILayout.Popup("需要再build一次", m_HotfixPlatformIndex, m_Controller.PlatformNames);
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
            EditorGUILayout.LabelField("HybridCLR已经扫描过Unity核心库和常见的第三方库生成了默认的桥接函数集。");
            EditorGUILayout.LabelField("相关代码文件为hybridclr/interpreter/MethodBridge_{abi}.cpp，其中{abi}Universal32、Universal64、Arm64。");
            EditorGUILayout.LabelField("实践项目中总会遇到一些aot函数的共享桥接函数不在默认桥接函数集中。");
            EditorGUILayout.LabelField("因此提供了Editor工具，根据程序集自动生成所有桥接函数。");
            GUIItem("根据程序集自动生成所有桥接函数（Universal32）", "Generate", MethodBridgeHelper.MethodBridge_Universal32);
            GUIItem("根据程序集自动生成所有桥接函数（Universal64）", "Generate", MethodBridgeHelper.MethodBridge_Universal64);
            GUIItem("根据程序集自动生成所有桥接函数（Arm64）", "Generate", MethodBridgeHelper.MethodBridge_Arm64);
            GUIItem("根据程序集自动生成所有桥接函数（ALL）", "Generate", MethodBridgeHelper.MethodBridge_All);
            EditorGUILayout.EndVertical();
        }

        private void compilePlatform()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("点击对应平台进行编译");
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
    }
}
