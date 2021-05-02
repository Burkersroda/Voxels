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

    // Editor extension for Voxel Mesh component
    [CustomEditor(typeof(Mesh))]
    public class VoxelMeshEditor : Editor
    {

        // Show and process inspector
        public override void OnInspectorGUI()
        {
            Mesh voxelMesh = (Mesh)target;

            // Initialize path to store asset files to
            if (voxelMesh.assetPath == null)
            {
                voxelMesh.assetPath = "Voxels/" + voxelMesh.GetInstanceID().ToString("X8");
            }

            //// Show title at first
            //EditorGUILayout.LabelField(Information.Title, EditorStyles.centeredGreyMiniLabel);

            // Object selection for a mesh to use as a voxel
            UnityEngine.Mesh mesh = (UnityEngine.Mesh)EditorGUILayout.ObjectField("Voxel Mesh", voxelMesh.mesh, typeof(UnityEngine.Mesh), true);
            if (voxelMesh.mesh != mesh)
            {
                Undo.RecordObject(voxelMesh, "Voxel Mesh Change");
                voxelMesh.mesh = mesh;
            }

            // Sizing factor for the voxel mesh
            EditorGUILayout.BeginHorizontal();
            float sizeFactor = EditorGUILayout.FloatField("Size Factor", voxelMesh.sizeFactor);
            if (GUILayout.Button("Reset"))
            {
                sizeFactor = 1;
            }
            if (voxelMesh.sizeFactor != sizeFactor)
            {
                Undo.RecordObject(voxelMesh, "Size Factor Change");
                voxelMesh.sizeFactor = sizeFactor;
            }
            EditorGUILayout.EndHorizontal();

            // Flag to make new containers static
            EditorGUILayout.BeginHorizontal();
            bool staticContainers = EditorGUILayout.Toggle("Static Containers", voxelMesh.staticContainers);
            if (voxelMesh.staticContainers != staticContainers)
            {
                Undo.RecordObject(voxelMesh, "Static Containers Change");
                voxelMesh.staticContainers = staticContainers;
            }

            //bool skinnedMesh = EditorGUILayout.ToggleLeft("Skinned Mesh", voxelMesh.skinnedMesh);
            //if (voxelMesh.skinnedMesh != skinnedMesh)
            //{
            //    Undo.RecordObject(voxelMesh, "Skinned Mesh Flag Change");
            //    voxelMesh.skinnedMesh = skinnedMesh;
            //}
            EditorGUILayout.EndHorizontal();

            // Flag to merge meshes with equal materials
            EditorGUILayout.BeginHorizontal();
            bool mergeMeshes = EditorGUILayout.Toggle("Merge Meshes", voxelMesh.mergeMeshes);
            if (voxelMesh.mergeMeshes != mergeMeshes)
            {
                Undo.RecordObject(voxelMesh, "Meshes Merging Change");
                voxelMesh.mergeMeshes = mergeMeshes;
            }

            // Flag to merge only meshes with opaque materials
            EditorGUI.BeginDisabledGroup(!mergeMeshes);
            bool opaqueOnly = EditorGUILayout.ToggleLeft("Opaque Only", voxelMesh.opaqueOnly);
            if (voxelMesh.opaqueOnly != opaqueOnly)
            {
                Undo.RecordObject(voxelMesh, "Only Opaque Mesh Merging Change");
                voxelMesh.opaqueOnly = opaqueOnly;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            // Elements to fill a texture
            EditorGUILayout.LabelField("Textures", GUILayout.MaxWidth(56));
            bool mainTextureTarget = EditorGUILayout.ToggleLeft("Main", voxelMesh.mainTextureTarget, GUILayout.MaxWidth(48));
            if (voxelMesh.mainTextureTarget != mainTextureTarget)
            {
                Undo.RecordObject(voxelMesh, "Main Texture Target Flag Change");
                voxelMesh.mainTextureTarget = mainTextureTarget;
            }
            bool emissiveTextureTarget = EditorGUILayout.ToggleLeft("Emission", voxelMesh.emissiveTextureTarget, GUILayout.MaxWidth(80));
            if (voxelMesh.emissiveTextureTarget != emissiveTextureTarget)
            {
                Undo.RecordObject(voxelMesh, "Emissive Texture Target Flag Change");
                voxelMesh.emissiveTextureTarget = emissiveTextureTarget;
            }

            // Flag to transfer material to vertex color
            EditorGUILayout.LabelField("Colors", GUILayout.MaxWidth(40));
            bool vertexColors = EditorGUILayout.ToggleLeft("Vertex", voxelMesh.vertexColors, GUILayout.MaxWidth(64));
            if (voxelMesh.vertexColors != vertexColors)
            {
                Undo.RecordObject(voxelMesh, "Vertex Color Flag Change");
                voxelMesh.vertexColors = vertexColors;
            }

            EditorGUILayout.EndHorizontal();

            //// Object selection for a voxel texture to use for colors
            //VoxelTexture voxelTexture = (VoxelTexture)EditorGUILayout.ObjectField("Color Texture", voxelMesh.voxelTexture, typeof(VoxelTexture), true);
            //if (voxelMesh.voxelTexture != voxelTexture)
            //{
            //    Undo.RecordObject(voxelMesh, "Color Texture Change");
            //    voxelMesh.voxelTexture = voxelTexture;
            //}

            //if (voxelMesh.voxelTexture != null)
            //{
            //    // Object selection of a material to use for texturing
            //    EditorGUILayout.BeginHorizontal();
            //    EditorGUILayout.LabelField("", GUILayout.MaxWidth(16));
            //    Material templateMaterial = (Material)EditorGUILayout.ObjectField("Material Template", voxelMesh.textureMaterialTemplate, typeof(Material), true);
            //    if (voxelMesh.textureMaterialTemplate != templateMaterial)
            //    {
            //        Undo.RecordObject(voxelMesh, "Texture Template Material Change");
            //        voxelMesh.textureMaterialTemplate = templateMaterial;
            //    }
            //    EditorGUILayout.EndHorizontal();
            //}

            //VoxelTexture[] voxelTextures = voxelMesh.GetComponents<VoxelTexture>();
            //if (voxelTextures.Length >= 1)
            //{
            //    string[] names = new string[voxelTextures.Length + 1];
            //    int[] indices = new int[voxelTextures.Length + 1];
            //    int number;
            //    int index = -1;

            //    names[0] = "(none)";
            //    indices[0] = -1;

            //    for (number = 0; number < voxelTextures.Length; ++number)
            //    {
            //        if (voxelMesh.voxelTexture == voxelTextures[number])
            //        {
            //            index = number;
            //        }

            //        names[number + 1] = voxelTextures[number].name + " (" + number + ")";
            //        indices[number + 1] = index;
            //    }

            //    index = EditorGUILayout.IntPopup("Color Texture", index, names, indices);
            //    //if (voxelConverter.bakingOperationMode != bakingOperationMode)
            //    //{
            //    //    Undo.RecordObject(voxelConverter, "Baking Operation Mode Change");
            //    //    voxelConverter.bakingOperationMode = bakingOperationMode;
            //    //}
            //}

            // Name of the main target container
            string targetName = EditorGUILayout.TextField("Target Name", voxelMesh.targetName);
            if (voxelMesh.targetName != targetName)
            {
                Undo.RecordObject(voxelMesh, "Target Object Name Change");
                voxelMesh.targetName = targetName;
            }

            // Activation flag and path name field to store prefab and asset files to
            EditorGUILayout.BeginHorizontal();
            bool assetFile = EditorGUILayout.ToggleLeft("Asset Files", voxelMesh.assetFile, GUILayout.MaxWidth(116));
            if (voxelMesh.assetFile != assetFile)
            {
                Undo.RecordObject(voxelMesh, "Asset File Flag Change");
                voxelMesh.assetFile = assetFile;
            }
            EditorGUI.BeginDisabledGroup(!assetFile);
            string assetPath = EditorGUILayout.TextField(voxelMesh.assetPath);
            if (GUILayout.Button("...", GUILayout.MaxWidth(24)))
            {
                // Open "save to" dialog
                string temporaryFilePath = EditorUtility.SaveFolderPanel("Save assets to folder after buildup...", assetPath, "");
                if (temporaryFilePath.Length != 0)
                {
                    if (temporaryFilePath == Application.dataPath)
                    {
                        assetPath = "";
                    }
                    else
                    {
                        var dataPath = Application.dataPath + System.IO.Path.AltDirectorySeparatorChar;

                        if (temporaryFilePath.StartsWith(dataPath))
                        {
                            assetPath = temporaryFilePath.Substring(dataPath.Length);
                        }
                        else
                        {
                            assetPath = temporaryFilePath;
                        }
                    }
                }
            }
            if (GUILayout.Button("X", GUILayout.MaxWidth(24)))
            {
                assetPath = null;
            }
            else if (assetPath.Length == 0)
            {
                assetPath = null;
            }
            if (voxelMesh.assetPath != assetPath)
            {
                Undo.RecordObject(voxelMesh, "Asset Path Change");
                voxelMesh.assetPath = assetPath;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

    }

}