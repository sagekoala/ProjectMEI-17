
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Opens the graveyard scene when the editor starts on SampleScene (editor only)
[InitializeOnLoad]
static class OpenGraveyardOnStartup
{
    const string GameScenePath = "Assets/Scenes/Colonial_Graveyard.unity";
    const string SessionKey = "MEI17.OpenGraveyardOnStartup.Done";

    static OpenGraveyardOnStartup()
    {
        if (SessionState.GetBool(SessionKey, false))
            return;

        EditorApplication.update += TryOpen;
    }

    static void TryOpen()
    {
        // Wait until the editor has finished loading and compiling.
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        EditorApplication.update -= TryOpen;
        SessionState.SetBool(SessionKey, true);

        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        Scene active = SceneManager.GetActiveScene();
        bool onTemplateScene = string.IsNullOrEmpty(active.path) ||
                               active.path.EndsWith("/SampleScene.unity");

        if (!onTemplateScene || active.isDirty)
            return;

        EditorSceneManager.OpenScene(GameScenePath);
    }
}
