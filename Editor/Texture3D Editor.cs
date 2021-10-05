//--------------------------------
//
// Voxels for Unity
//  Version: 1.23.7
//
// © 2014-21 by Ronny Burkersroda
//
//--------------------------------


using UnityEditor;
using UnityEngine;


namespace Voxels
{

    // Editor extension for Voxel Texture3D component
    [CustomEditor(typeof(Texture3D))]
    public class VoxelTexture3DEditor : Editor
    {

        // Names and indices of the target file formats
        static string[] fileFormatNames = new string[] {
            "None",

#if UNITY_2020_2_OR_NEWER

            "JPEG",
            "PNG",
            //"TGA",
            "EXR",

#endif

            "Asset File"
        };
        static int[] fileFormatModes = new int[] {
            -1,

#if UNITY_2020_2_OR_NEWER

            (int)Texture3D.FileFormat.JPG,
            (int)Texture3D.FileFormat.PNG,
            //(int)Texture3D.FileFormat.TGA,
            (int)Texture3D.FileFormat.EXR,

#endif

            (int)Texture3D.FileFormat.Asset
        };
        static string[] fileFormatExtensions = new string[] {
            "asset",

#if UNITY_2020_2_OR_NEWER

            "jpg",
            "png",
            "tga",
            "exr",

#endif

        };


        // Show and process inspector
        public override void OnInspectorGUI()
        {
            var voxelTexture = (Texture3D)target;

            // Sampling slider and power of two toggle mode
            int samplingCount = EditorGUILayout.IntSlider("Supersampling Count", voxelTexture.superSamplingCount, 1, 16);
            if (voxelTexture.superSamplingCount != samplingCount)
            {
                Undo.RecordObject(voxelTexture, "Supersampling Count Change");
                voxelTexture.superSamplingCount = samplingCount;
            }

            bool powerOfTwo = EditorGUILayout.Toggle("Power of Two", voxelTexture.powerOfTwo);
            if (voxelTexture.powerOfTwo != powerOfTwo)
            {
                Undo.RecordObject(voxelTexture, "Power-of-Two Flag Change");
                voxelTexture.powerOfTwo = powerOfTwo;
            }

            // Expand edges flag and background color
            bool expandEdges = EditorGUILayout.Toggle("Expand Edges", voxelTexture.expandEdges);
            if (voxelTexture.expandEdges != expandEdges)
            {
                Undo.RecordObject(voxelTexture, "Expand Edges Flag Change");
                voxelTexture.expandEdges = expandEdges;
            }
            if (!expandEdges)
            {
                Color color = EditorGUILayout.ColorField("Background Color", voxelTexture.backgroundColor);
                if (voxelTexture.backgroundColor != color)
                {
                    Undo.RecordObject(voxelTexture, "Background Color Change");
                    voxelTexture.backgroundColor = color;
                }
            }

            // Popup menu for file format
            var fileFormat = (Texture3D.FileFormat)EditorGUILayout.IntPopup("Target File", voxelTexture.fileStoring ? (int)voxelTexture.fileFormat : fileFormatModes[0], fileFormatNames, fileFormatModes);
            var fileStoring = (int)fileFormat != fileFormatModes[0];
            if (fileStoring && voxelTexture.fileFormat != fileFormat)
            {
                if (!string.IsNullOrWhiteSpace(voxelTexture.filePath))
                {
                    // Replace extension after format change
                    var extension = System.IO.Path.GetExtension(voxelTexture.filePath);
                    if (!string.IsNullOrEmpty(voxelTexture.filePath))
                    {
                        if (string.Compare(extension, "." + fileFormatExtensions[System.Math.Min((int)voxelTexture.fileFormat, fileFormatExtensions.Length - 1)], true) == 0)
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
            GUI.enabled = fileStoring;
            string filePath = EditorGUILayout.TextField("File Path", voxelTexture.filePath == null ? "" : voxelTexture.filePath);
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
            GUI.enabled = true;
            if (GUILayout.Button("X", GUILayout.MaxWidth(24)))
            {
                filePath = null;
            }
            else if (filePath != null && filePath.Length == 0)
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