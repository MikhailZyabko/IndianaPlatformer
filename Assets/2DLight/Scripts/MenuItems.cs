using UnityEditor;
using UnityEngine;

public class MenuItems : MonoBehaviour
{
    [MenuItem("GameObject/Light 2D/Point Light", false, 2)]
    static void CreatePointLigt(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = new GameObject("Point Light 2D");
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        go.AddComponent<PointLight2D>();
        go.GetComponent<PointLight2D>().color = Color.white;
        go.GetComponent<PointLight2D>().radius = 5;
        Selection.activeObject = go;
    }
}
