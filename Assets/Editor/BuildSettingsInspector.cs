using UnityEditor;
using UnityEngine;
using System;
using System.Text;
using System.Reflection;

public static class BuildSettingsInspector
{
    public static void PrintAllSettings()
    {
        Debug.Log("[BuildSettingsInspector] Dumping Unity Build Settings...");

        DumpStaticProperties("PlayerSettings", typeof(PlayerSettings));
        DumpStaticProperties("PlayerSettings.WebGL", typeof(PlayerSettings.WebGL));
        DumpStaticProperties("EditorUserBuildSettings", typeof(EditorUserBuildSettings));
        PrintBatchingSettings();
    }

    public static void PrintBatchingSettings()
    {
        Debug.Log("[BuildSettingsInspector] Batching settings: skipped (API not available in this Unity version)");
    }

    private static void DumpStaticProperties(string title, Type type)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"\n[BuildSettingsInspector] {title} ===================");

        var props = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var prop in props)
        {
            if (!prop.CanRead) continue;

            try
            {
                object value = prop.GetValue(null, null);
                if (value is byte[] byteArray)
                {
                    sb.AppendLine($"- {type.Name}.{prop.Name} = byte[{byteArray.Length}]");
                }
                else
                {
                    sb.AppendLine($"- {type.Name}.{prop.Name} = {value}");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"- {type.Name}.{prop.Name} = <exception: {ex.Message}>");
            }
        }

        Debug.Log(sb.ToString());
    }
}
