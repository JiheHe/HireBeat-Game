using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FrostweepGames.VoicePro
{
    [InitializeOnLoad]
    public class WelcomeDialog : EditorWindow
    {
        private static bool _Inited;

        static WelcomeDialog()
        {
            EditorApplication.update += Startup;
        }

        private static void Startup()
        {
            EditorApplication.update -= Startup;

            if (GeneralConfig.Config.showWelcomeDialogAtStartup)
            {
                Init();
            }
        }

        [MenuItem("Window/Frostweep Games/Voice Pro")]
        private static void Init()
        {
            if (_Inited)
                return;

            WelcomeDialog window = (WelcomeDialog)GetWindow(typeof(WelcomeDialog), false, "Voice Pro", true);
            window.minSize = new Vector2(500, 400);
            window.maxSize = new Vector2(500, 400);
            window.Show();

            _Inited = true;
        }

		private void OnDestroy()
		{
            _Inited = false;
        }

		private void OnGUI()
        {
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Welcome to Frostweep Games - Voice Pro!", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            if (GUILayout.Button("Asset Store Page"))
            {
                Application.OpenURL("https://assetstore.unity.com/publishers/14839");
            }
            if (GUILayout.Button("Frostweep Games Website"))
            {
                Application.OpenURL("https://frostweepgames.com");
            }
            if (GUILayout.Button("Frostweep Games Store Page"))
            {
                Application.OpenURL("https://store.frostweepgames.com");
            }
            if (GUILayout.Button("Official Discord Server"))
            {
                Application.OpenURL("https://discord.gg/TZdhnWy");
            }
            if (GUILayout.Button("Contact Us"))
            {
                Application.OpenURL("mailto: assets@frostweepgames.com");
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Tools");

            if (GUILayout.Button("Locate Voice Pro Settings"))
            {
                Selection.objects = new UnityEngine.Object[] { GeneralConfig.Config };
                EditorGUIUtility.PingObject(GeneralConfig.Config);
            }
            if (GUILayout.Button("Add Voice Pro Prefab to Scene"))
            {
                Undo.RegisterCreatedObjectUndo(PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("VoicePro")), "VoicePro");
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }
            if (GUILayout.Button("Open Documentation"))
            {
                System.Diagnostics.Process.Start(Application.dataPath + "/FrostweepGames/VoicePro/Documentation.pdf");
            }
            if (GUILayout.Button("Open README"))
            {
                System.Diagnostics.Process.Start(Application.dataPath + "/FrostweepGames/VoicePro/README.txt");
            }

            EditorGUILayout.Space();
            bool showOnStartup = GUILayout.Toggle(GeneralConfig.Config.showWelcomeDialogAtStartup, "Show on startup");

            if (showOnStartup != GeneralConfig.Config.showWelcomeDialogAtStartup)
            {
                GeneralConfig.Config.showWelcomeDialogAtStartup = showOnStartup;
                EditorUtility.SetDirty(GeneralConfig.Config);
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            var selectedNetworkProvider = (GeneralConfig.NetworkProvider)EditorGUILayout.EnumPopup("Current Network Provider:", GeneralConfig.Config.networkProvider);

            if (GeneralConfig.Config.networkProvider != selectedNetworkProvider)
            {
                GeneralConfig.Config.networkProvider = selectedNetworkProvider;

                Plugins.DefineProcessing.AddOrRemoveDefines(false, true, System.Enum.GetNames(typeof(GeneralConfig.NetworkProvider)));

                if (GeneralConfig.Config.networkProvider != GeneralConfig.NetworkProvider.Unknown)
                    Plugins.DefineProcessing.AddOrRemoveDefines(true, true, GeneralConfig.Config.networkProvider.ToString());

                EditorUtility.SetDirty(GeneralConfig.Config);
                SaveProject();
            }
        }

        private void SaveProject()
        {
            EditorApplication.ExecuteMenuItem("File/Save Project");
        }
    }
}