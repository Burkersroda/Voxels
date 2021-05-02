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

    // Editor extension for Voxel Texture2D component
    [CustomEditor(typeof(Texture2D))]
    public class VoxelTexture2DEditor : Editor
    {

        // Names and indices of the target file formats
        static string[] fileFormatNames = new string[] { "None", "JPEG", "PNG", "TGA", "EXR", "Asset File" };
        static int[] fileFormatModes = new int[] { -1, (int)Texture2D.FileFormat.JPG, (int)Texture2D.FileFormat.PNG, (int)Texture2D.FileFormat.TGA, (int)Texture2D.FileFormat.EXR, (int)Texture2D.FileFormat.Asset};
        static string[] fileFormatExtensions = new string[] { "jpg", "png", "tga", "exr", "asset" };


        // Show and process inspector
        public override void OnInspectorGUI()
        {
            Texture2D voxelTexture = (Texture2D)target;

            //// Show title at first
            //EditorGUILayout.LabelField(Information.Title, EditorStyles.centeredGreyMiniLabel);

            bool powerOfTwo = EditorGUILayout.Toggle("Power of Two", voxelTexture.powerOfTwo);
            if (voxelTexture.powerOfTwo != powerOfTwo)
            {
                Undo.RecordObject(voxelTexture, "Power-of-Two Flag Change");
                voxelTexture.powerOfTwo = powerOfTwo;
            }

            // Popup menu for file format
            var fileFormat = (Texture2D.FileFormat)EditorGUILayout.IntPopup("Target File", voxelTexture.fileStoring ? (int)voxelTexture.fileFormat : fileFormatModes[0], fileFormatNames, fileFormatModes);
            var fileStoring = (int)fileFormat != fileFormatModes[0];
            if (fileStoring && voxelTexture.fileFormat != fileFormat)
            {
                if (!string.IsNullOrWhiteSpace(voxelTexture.filePath))
                {
                    // Replace extension after format change
                    var extension = System.IO.Path.GetExtension(voxelTexture.filePath);
                    if (!string.IsNullOrEmpty(voxelTexture.filePath))
                    {
                        if (string.Compare(extension, "." + fileFormatExtensions[(int)voxelTexture.fileFormat], true) == 0)
                        {
                            voxelTexture.filePath = voxelTexture.filePath.Substring(0, voxelTexture.filePath.Length - extension.Length + 1) + fileFormatExtensions[(int)fileFormat];
                        }
                    }
                }

                Undo.RecordObject(voxelTexture, "File Format Change");
                voxelTexture.fileFormat = fileFormat;
                voxelTexture.fileStoring = true;
            }
            else if (voxelTexture.fileStoring != fileStoring)
            {
                Undo.RecordObject(voxelTexture, "File Storing Flag Change");
                voxelTexture.fileStoring = fileStoring;
            }

            // Path of the target file
            EditorGUILayout.BeginHorizontal();
            string filePath = EditorGUILayout.TextField("File Path", voxelTexture.filePath == null ? "" : voxelTexture.filePath);
            GUI.enabled = fileStoring;
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
                string temporaryFilePath = EditorUtility.SaveFilePanel("Save texture to file after buildup...", directory, System.IO.Path.GetFileNameWithoutExtension(filePath), fileFormatExtensions[(int)fileFormat]);
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
            else if (filePath.Length == 0)
            {
                filePath = null;
            }
            GUI.enabled = true;
            if (GUILayout.Button("X", GUILayout.MaxWidth(24)))
            {
                filePath = null;
            }
            if (voxelTexture.filePath != filePath)
            {
                Undo.RecordObject(voxelTexture, "File Path Change");
                voxelTexture.filePath = filePath;
            }
            EditorGUILayout.EndHorizontal();

            // Voxel Mesh usage flag
            bool voxelMeshUsage = EditorGUILayout.Toggle("Mesh Creator Usage", voxelTexture.voxelMeshUsage);
            if (voxelTexture.voxelMeshUsage != voxelMeshUsage)
            {
                Undo.RecordObject(voxelTexture, "Voxel Mesh Usage Flag Change");
                voxelTexture.voxelMeshUsage = voxelMeshUsage;
            }
            if (voxelMeshUsage)
            {
                if (voxelTexture.GetComponent<Mesh>() == null)
                {
                    EditorGUILayout.HelpBox("There is no Voxel Mesh component attached!", MessageType.Warning);
                }

                Texture2D[] textures = voxelTexture.GetComponents<Texture2D>();
                foreach(Texture2D texture in textures)
                {
                    if (texture != voxelTexture)
                    {
                        if (texture.voxelMeshUsage)
                        {
                            EditorGUILayout.HelpBox("There are multiple Voxel Textures, which should be used for Voxel Mesh!", MessageType.Error);
                            break;
                        }
                    }
                }
            }
        }

    }

}