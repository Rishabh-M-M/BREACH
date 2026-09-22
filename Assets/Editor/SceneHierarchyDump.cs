#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Text;

public static class SceneHierarchyDump
{
    [MenuItem("Tools/Dump Scene Hierarchy")]
    public static void DumpHierarchy()
    {
        GameObject[] roots =
            UnityEngine.SceneManagement.SceneManager
            .GetActiveScene()
            .GetRootGameObjects();

        StringBuilder output = new StringBuilder();

        output.AppendLine("========== SCENE HIERARCHY ==========");

        foreach (GameObject root in roots)
        {
            DumpGameObject(root.transform, output, 0);
        }

        output.AppendLine("========== END HIERARCHY ==========");

        Debug.Log(output.ToString());

        EditorGUIUtility.systemCopyBuffer =
            output.ToString();

        Debug.Log("Hierarchy copied to clipboard.");
    }

    private static void DumpGameObject(
        Transform current,
        StringBuilder output,
        int depth)
    {
        string indent = new string(' ', depth * 4);

        output.AppendLine(
            indent + "- " + current.name
        );

        Component[] components =
            current.GetComponents<Component>();

        foreach (Component component in components)
        {
            if (component != null)
            {
                output.AppendLine(
                    indent +
                    "    [" +
                    component.GetType().Name +
                    "]"
                );
            }
        }

        for (int i = 0; i < current.childCount; i++)
        {
            DumpGameObject(
                current.GetChild(i),
                output,
                depth + 1
            );
        }
    }
}

#endif