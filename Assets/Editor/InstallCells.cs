using UnityEditor;
using UnityEngine;

public class InstallCells : MonoBehaviour
{
    [MenuItem("Tools/Rename with Numbering")]
    private static void RenameSelectedObjects()
    {
        // 選択したオブジェクトを取得
        GameObject[] selectedObjects = Selection.gameObjects;

        // オブジェクトの名前に連番を付与
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            selectedObjects[i].name += i;
        }
    }
}
