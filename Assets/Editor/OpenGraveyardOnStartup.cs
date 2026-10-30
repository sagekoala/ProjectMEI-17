
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// Editor only: when Unity starts on the template's SampleScene (or an empty
// scene), open the game scene instead. It runs once per editor session, so
// it never fights you if you open another scene on purpose. Builds are not
// affected; they start with the first scene in the build scene list.
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
