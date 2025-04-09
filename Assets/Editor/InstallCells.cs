using UnityEditor;
using UnityEngine;

public class InstallCells : MonoBehaviour
{
    [MenuItem("Tools/Rename with Numbering")]
    private static void RenameSelectedObjects()
    {
        // �I�������I�u�W�F�N�g���擾
        var selectedObjects = Selection.gameObjects;

        // �I�u�W�F�N�g�̖��O�ɘA�Ԃ�t�^
        for (var i = 0; i < selectedObjects.Length; i++) selectedObjects[i].name += i;
    }
}