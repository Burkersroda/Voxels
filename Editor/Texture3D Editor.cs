//--------------------------------
//
// Voxels for Unity
//  Version: 1.22.6
//
// © 2014-21 by Ronny Burkersroda
//
//--------------------------------


using UnityEngine;
using UnityEditor;


namespace Voxels
{

    // Editor extension for Voxel Texture3D component
    [CustomEditor(typeof(Texture3D))]
    public class VoxelTexture3DEditor : Editor
    {

        // Show and process inspector
        public override void OnInspectorGUI()
        {
            var voxelTexture = (Texture3D)target;

            bool powerOfTwo = EditorGUILayout.Toggle("Power of Two", voxelTexture.powerOfTwo);
            if (voxelTexture.powerOfTwo != powerOfTwo)
            {
                Undo.RecordObject(voxelTexture, "Power-of-Two Flag Change");
                voxelTexture.powerOfTwo = powerOfTwo;
            }

            // Path of the target file
            EditorGUILayout.BeginHorizontal();
            var fileStoring = EditorGUILayout.ToggleLeft("Asset File", voxelTexture.fileStoring, GUILayout.MaxWidth(116));
            if (voxelTexture.fileStoring != fileStoring)
            {
                Undo.RecordObject(voxelTexture, "File Storing Flag Change");
                voxelTexture.fileStoring = fileStoring;
            }
            EditorGUI.BeginDisabledGroup(!fileStoring);
            string filePath = EditorGUILayout.TextField(voxelTexture.filePath == null ? "" : voxelTexture.filePath);
            if (GUILayout.Button("...", GUILayout.MaxWidth(24)))
            {
                string directory;

                // get directory from path name
                if (filePath.Length != 0)
                {
                    directory = System.IO.Path.GetDirectoryName(filePath);
                    if (directory == null)
                    {
                        directory = "";
                    }
                }
                else
                {
                    directory = "";
                }

                // Open "save to" dialog
                string temporaryFilePath = EditorUtility.SaveFilePanel("Save texture to file after buildup...", directory, System.IO.Path.GetFileNameWithoutExtension(filePath), "asset");
                if (temporaryFilePath.Length != 0)
                {
                    var dataPath = Application.dataPath + System.IO.Path.AltDirectorySeparatorChar;

                    if (temporaryFilePath.StartsWith(dataPath))
                    {
                        filePath = temporaryFilePath.Substring(dataPath.Length);
                    }
                    else
                    {
                        filePath = temporaryFilePath;
                    }
                }
            }
            if (filePath.Length == 0)
            {
                filePath = null;
            }
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("X", GUILayout.MaxWidth(24)))
            {
                filePath = null;
            }
            else if (filePath.Length == 0)
            {
                filePath = null;
            }
            if (voxelTexture.filePath != filePath)
            {
                Undo.RecordObject(voxelTexture, "File Path Change");
                voxelTexture.filePath = filePath;
            }
            EditorGUILayout.EndHorizontal();
        }

    }

}