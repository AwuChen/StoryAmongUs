using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

public class PostProcessBuildScript
{
    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        Debug.Log("Called from OnPostprocessBuild: " + pathToBuiltProject);
    }
}