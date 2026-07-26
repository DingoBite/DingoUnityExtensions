using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;

namespace DingoUnityExtensions.Editor
{
    [InitializeOnLoad]
    public class OnLoad : UnityEditor.Editor
    {
        /// Walking every type of every loaded assembly costs seconds, and [InitializeOnLoad] runs it
        /// on every domain reload. The answer can only change when the set of loaded assemblies
        /// changes, so the scan is skipped while that set stays the same within an editor session.
        private const string AssembliesKey = "DingoUnityExtensions.OnLoad.ScannedAssemblies";

        private static readonly (string Namespace, string Define)[] NamespaceDefines =
        {
            ("Cysharp.Threading.Tasks", "UNITASK_EXISTS"),
            ("Newtonsoft.Json.Serialization", "NEWTONSOFT_EXISTS"),
            ("NaughtyAttributes", "NAUGHTYATTRIBUTES_EXISTS"),
            ("NaughtyAttributes.ColorKeyProperties", "NAUGHTYATTRIBUTES_CK_EXISTS"),
            ("Bind", "BIND_EXISTS"),
            ("UnityEngine.ResourceManagement.AsyncOperations", "ADDRESSABLES_EXISTS"),
            ("VInspector", "VINSPECTOR_EXISTS"),
            ("UnityEngine.UI.ProceduralImage", "PROCEDURAL_IMAGE_EXISTS"),
            ("DTT.UI.ProceduralUI", "PROCEDURAL_UI_EXISTS"),
            ("MoreMountains.Feedbacks", "MMFEEL_EXISTS"),
        };

        static OnLoad()
        {
            ProcessDefines(false);
        }

        [MenuItem("Edit/Manual Process Defines")]
        private static void ManualProcessDefines()
        {
            ProcessDefines(true);
        }

        private static void ProcessDefines(bool force)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var loadedAssemblies = GetAssemblySignature(assemblies);
            if (!force && SessionState.GetString(AssembliesKey, string.Empty) == loadedAssemblies)
                return;

#if UNITY_6000_0_OR_NEWER
            var buildTargetGroup = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var definesString = PlayerSettings.GetScriptingDefineSymbols(buildTargetGroup);
#else
            var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
#endif
            var allDefines = definesString
                .Split(';')
                .Where(define => !string.IsNullOrWhiteSpace(define))
                .ToList();
            allDefines.AddRange(GetSymbols(assemblies).Except(allDefines));

            SessionState.SetString(AssembliesKey, loadedAssemblies);

            // Writing the same value back still dirties the settings and can cost a recompile.
            var updatedDefines = string.Join(";", allDefines);
            if (updatedDefines == definesString)
                return;

#if UNITY_6000_0_OR_NEWER
            PlayerSettings.SetScriptingDefineSymbols(buildTargetGroup, updatedDefines);
#else
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                updatedDefines);
#endif
        }

        private static string GetAssemblySignature(Assembly[] assemblies)
        {
            var names = assemblies.Select(assembly => assembly.GetName().Name).ToArray();
            Array.Sort(names, StringComparer.Ordinal);
            return string.Join(";", names);
        }

        private static IEnumerable<string> GetSymbols(Assembly[] assemblies)
        {
            var namespaces = new HashSet<string>(StringComparer.Ordinal);
            foreach (var assembly in assemblies)
            {
                foreach (var type in GetTypes(assembly))
                {
                    if (type.Namespace != null)
                        namespaces.Add(type.Namespace);
                }
            }

            return NamespaceDefines
                .Where(pair => namespaces.Contains(pair.Namespace))
                .Select(pair => pair.Define);
        }

        private static IEnumerable<Type> GetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(type => type != null);
            }
        }
    }
}
