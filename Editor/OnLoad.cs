using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Packages.DingoUnityExtensions.Editor
{
    [InitializeOnLoad]
    public class OnLoad : UnityEditor.Editor
    {
        static OnLoad()
        {
            ManualProcessDefines();
        }

        [MenuItem("Edit/Manual Process Defines")]
        private static void ManualProcessDefines()
        {
            var definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var allDefines = definesString.Split(';').ToList();
            allDefines.AddRange(GetSymbols().Except(allDefines));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray()));
        }

        private static IEnumerable<string> GetSymbols()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = (from assembly in assemblies
                from type in assembly.GetTypes()
                select type).ToArray();
            
            var unitaskNamespaceFound = (from type in types
                where type.Namespace == "Cysharp.Threading.Tasks"
                select type).Any();
            if (unitaskNamespaceFound)
                yield return "UNITASK_EXISTS";
            
            var newtonsoftNamespaceFound = (from type in types
                where type.Namespace == "Newtonsoft.Json.Serialization"
                select type).Any();
            if (newtonsoftNamespaceFound)
                yield return "NEWTONSOFT_EXISTS";
            
            var naughtyAttributesNamespaceFound = (from type in types
                where type.Namespace == "NaughtyAttributes"
                select type).Any();
            if (naughtyAttributesNamespaceFound)
                yield return "NAUGHTYATTRIBUTES_EXISTS";
            
            var naughtyAttributesColorKeysNamespaceFound = (from type in types
                where type.Namespace == "NaughtyAttributes.ColorKeyProperties"
                select type).Any();
            if (naughtyAttributesColorKeysNamespaceFound)
                yield return "NAUGHTYATTRIBUTES_CK_EXISTS";
            
            var bindNamespaceFound = (from type in types
                where type.Namespace == "Bind"
                select type).Any();
            if (bindNamespaceFound)
                yield return "BIND_EXISTS";

        }
    }
}