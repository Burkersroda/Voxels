//--------------------------------
//
// Voxels for Unity
//  Version: 1.22.6
//
// © 2014-21 by Ronny Burkersroda
//
//--------------------------------


using UnityEngine;
using System.Collections.Generic;


namespace Voxels
{

    /// <summary>
    /// Specific implementation of the voxel rasterizer depending on preprocessor keywords
    /// </summary>
    [AddComponentMenu("Voxels/Rasterizer")]
    public class Rasterizer : RasterizerBase
    {

        /// <summary>
        /// Create global instance(s) to support various render pipelines
        /// </summary>
        /// <param name="renderSupport">Interface to enhance rasterizing</param>
        /// <param name="processorSupport">Interface to enhance post-processing</param>
        protected override void Initialize(out IRasterizerSupport renderSupport, out IProcessorSupport processorSupport)
        {

#if VOXELS_HDRP

            var supportInstance = new HDRPSupport();
            renderSupport = supportInstance;
            processorSupport = supportInstance;

#elif VOXELS_URP

            var supportInstance = new URPSupport();
            renderSupport = null;
            processorSupport = supportInstance;

#else

            renderSupport = null;
            processorSupport = null;

#endif

        }

#if VOXELS_HDRP

        /// <summary>
        /// Global instance including processing routines for high definition pipeline
        /// </summary>
        protected class HDRPSupport : IRasterizerSupport, IProcessorSupport
        {

            /// <summary>
            /// Camera data instance, which is used by the high definition render pipeline
            /// </summary>
            UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData additionalCameraData;


            /// <summary>
            /// Initialize additional steps before rasterizing starts
            /// </summary>
            /// <param name="projectionCamera">Camera, which is utilized to render scans</param>
            public void PrepareProcessing(Camera projectionCamera)
            {
                // Check if additional camera data instance is not stored
                if (additionalCameraData == null)
                {
                    // Check for high definition render pipeline
                    if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset is UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset)
                    {
                        projectionCamera.gameObject.SetActive(true);

                        // Try to get existing instance from projection camera
                        additionalCameraData = projectionCamera.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
                        if (additionalCameraData == null)
                        {
                            // Create new instance for projection camera
                            additionalCameraData = projectionCamera.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
                        }

                        if (additionalCameraData != null)
                        {
                            // Initialize additional camera data
                            additionalCameraData.clearColorMode = UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.ClearColorMode.Color;
                            additionalCameraData.defaultFrameSettings = UnityEngine.Rendering.HighDefinition.FrameSettingsRenderType.Camera;
                            additionalCameraData.customRenderingSettings = true;

                            // Set flags for settings to override
                            var overrideMask = additionalCameraData.renderingPathCustomFrameSettingsOverrideMask;
                            overrideMask.mask[(uint)UnityEngine.Rendering.HighDefinition.FrameSettingsField.Postprocess] = true;
                            overrideMask.mask[(uint)UnityEngine.Rendering.HighDefinition.FrameSettingsField.DepthOfField] = true;
                            overrideMask.mask[(uint)UnityEngine.Rendering.HighDefinition.FrameSettingsField.MotionBlur] = true;
                            overrideMask.mask[(uint)UnityEngine.Rendering.HighDefinition.FrameSettingsField.Bloom] = true;
                            overrideMask.mask[(uint)UnityEngine.Rendering.HighDefinition.FrameSettingsField.LensDistortion] = true;
                            overrideMask.mask[(uint)UnityEngine.Rendering.HighDefinition.FrameSettingsField.ChromaticAberration] = true;
                            overrideMask.mask[(uint)UnityEngine.Rendering.HighDefinition.FrameSettingsField.Vignette] = true;
                            overrideMask.mask[(uint)UnityEngine.Rendering.HighDefinition.FrameSettingsField.ColorGrading] = true;
                            overrideMask.mask[(uint)UnityEngine.Rendering.HighDefinition.FrameSettingsField.FilmGrain] = true;
                            overrideMask.mask[(uint)UnityEngine.Rendering.HighDefinition.FrameSettingsField.Dithering] = true;
                            overrideMask.mask[(uint)UnityEngine.Rendering.HighDefinition.FrameSettingsField.Antialiasing] = true;
                            overrideMask.mask[(uint)UnityEngine.Rendering.HighDefinition.FrameSettingsField.AtmosphericScattering] = true;
                            overrideMask.mask[(uint)UnityEngine.Rendering.HighDefinition.FrameSettingsField.ExposureControl] = true;
                            additionalCameraData.renderingPathCustomFrameSettingsOverrideMask = overrideMask;

                            // Deactivate post-processing, fog and exposure control
                            var frameSettings = additionalCameraData.renderingPathCustomFrameSettings;
                            frameSettings.SetEnabled(UnityEngine.Rendering.HighDefinition.FrameSettingsField.Postprocess, false);
                            frameSettings.SetEnabled(UnityEngine.Rendering.HighDefinition.FrameSettingsField.DepthOfField, false);
                            frameSettings.SetEnabled(UnityEngine.Rendering.HighDefinition.FrameSettingsField.MotionBlur, false);
                            frameSettings.SetEnabled(UnityEngine.Rendering.HighDefinition.FrameSettingsField.Bloom, false);
                            frameSettings.SetEnabled(UnityEngine.Rendering.HighDefinition.FrameSettingsField.LensDistortion, false);
                            frameSettings.SetEnabled(UnityEngine.Rendering.HighDefinition.FrameSettingsField.ChromaticAberration, false);
                            frameSettings.SetEnabled(UnityEngine.Rendering.HighDefinition.FrameSettingsField.Vignette, false);
                            frameSettings.SetEnabled(UnityEngine.Rendering.HighDefinition.FrameSettingsField.ColorGrading, false);
                            frameSettings.SetEnabled(UnityEngine.Rendering.HighDefinition.FrameSettingsField.FilmGrain, false);
                            frameSettings.SetEnabled(UnityEngine.Rendering.HighDefinition.FrameSettingsField.Dithering, false);
                            frameSettings.SetEnabled(UnityEngine.Rendering.HighDefinition.FrameSettingsField.Antialiasing, false);
                            frameSettings.SetEnabled(UnityEngine.Rendering.HighDefinition.FrameSettingsField.AtmosphericScattering, false);
                            frameSettings.SetEnabled(UnityEngine.Rendering.HighDefinition.FrameSettingsField.ExposureControl, false);
                            additionalCameraData.renderingPathCustomFrameSettings = frameSettings;
                        }
                    }
                }
            }

            /// <summary>
            /// Set properties for rendering a scan to a white background
            /// </summary>
            /// <param name="backgroundColor">[In] Original background color, [Out] Modified background color</param>
            public void PrepareWhiteScan(ref Color backgroundColor)
            {
                if (additionalCameraData != null)
                {
                    // Change background color to maximum for more precise reconstruction of the transparency
                    additionalCameraData.backgroundColorHDR = backgroundColor = new Color(32767, 32767, 32767, 0);
                }
            }

            /// <summary>
            /// Set properties for rendering a scan to a black background
            /// </summary>
            /// <param name="backgroundColor">Black background color</param>
            public void PrepareBlackScan(Color backgroundColor)
            {
                if (additionalCameraData != null)
                {
                    // Set given background color to data element of the render pipeline
                    additionalCameraData.backgroundColorHDR = backgroundColor;
                }
            }


            /// <summary>
            /// Create material to as template for opaque or transparent voxels, if user has not specified own ones
            /// </summary>
            /// <param name="transparent">Flag to indicate creation of a material for transparent results, opaque ones otherwise</param>
            public Material CreateTemplateMaterial(bool transparent = false)
            {
                var shader = Shader.Find("HDRP/Unlit");
                if (shader)
                {
                    var material = new Material(shader);
                    if (material)
                    {
                        if (transparent)
                        {
                            // Set properties for transparent rendering
                            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                            material.SetOverrideTag("RenderType", "Transparent");
                            material.SetFloat("_SurfaceType", 1f);
                            material.SetFloat("_BlendMode", 0f);
                            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            material.SetInt("_ZWrite", 0);
                            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                            material.name = "Transparent";
                        }
                        else
                        {
                            material.name = "Opaque";
                        }

                        // Set white as base color
                        material.SetColor("_UnlitColor", Color.white);

                        //material.EnableKeyword("_EMISSIVE_COLOR_MAP");
                        //material.SetFloat("_UseEmissiveIntensity", 1);
                        //material.SetColor("_EmissiveColorLDR", Color.white);
                        //material.SetColor("_EmissiveColor", Color.white);
                        //material.SetFloat("_EmissiveIntensity", 500f);
                        //material.SetFloat("_EmissiveExposureWeight", 0.5f);

                        return material;
                    }
                }

                return null;
            }

        }

#elif VOXELS_URP

        /// <summary>
        /// Global instance including processing routines for universal pipeline
        /// </summary>
        protected class URPSupport : IProcessorSupport
        {

            /// <summary>
            /// Create material to as template for opaque or transparent voxels, if user has not specified own ones
            /// </summary>
            /// <param name="transparent">Flag to indicate creation of a material for transparent results, opaque ones otherwise</param>
            public Material CreateTemplateMaterial(bool transparent = false)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader)
                {
                    var material = new Material(shader);
                    if (material)
                    {
                        if (transparent)
                        {
                            // Set properties for transparent rendering
                            material.SetOverrideTag("RenderType", "Transparent");
                            material.SetFloat("_Surface", 1f);
                            material.SetFloat("_Blend", 0f);
                            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                            material.SetInt("_ZWrite", 0);
                            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                            material.name = "Transparent";
                        }
                        else
                        {
                            material.name = "Opaque";
                        }

                        // Set white as base color
                        material.SetColor("_BaseColor", Color.white);

                        return material;
                    }
                }

                return null;
            }

        }

#endif

#if UNITY_EDITOR

        /// <summary>
        /// Class to update symbols definition
        /// </summary>
        [UnityEditor.InitializeOnLoad]
        public static class UpdateRenderPipelineDefineSymbols
        {

            /// <summary>
            /// Preprocessor definition keyword for high definition render pipeline support
            /// </summary>
            const string definitionHDRP = "VOXELS_HDRP";

            /// <summary>
            /// Preprocessor definition keyword for universal render pipeline support
            /// </summary>
            const string definitionURP = "VOXELS_URP";

            /// <summary>
            /// Constructor is executed, when project has been loaded or scripts are being compiled
            /// </summary>
            static UpdateRenderPipelineDefineSymbols()
            {
                // Determine, if costum pipeline is active
                var renderPipelineType = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset ? UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset.GetType() : null;
                var pipelineActive = renderPipelineType != null && renderPipelineType.Name != null;
                
                // Change pre-processor definition depending on current render pipeline
                SetDefineSymbol(definitionHDRP, pipelineActive && renderPipelineType.Name.Contains("HDRenderPipelineAsset"));
                SetDefineSymbol(definitionURP, pipelineActive && renderPipelineType.Name.Contains("UniversalRenderPipelineAsset"));
            }
        }

        /// <summary>
        /// Add or remove given preprocessor keyword from list of symbols
        /// </summary>
        /// <param name="symbol">Preprocessor definition</param>
        /// <param name="active">Flag to set or unset definition</param>
        static protected void SetDefineSymbol(string symbol, bool active = true)
        {
            var buildTargetGroup = UnityEditor.EditorUserBuildSettings.selectedBuildTargetGroup;
            if (buildTargetGroup != UnityEditor.BuildTargetGroup.Unknown)
            {
                // Get pre-processor definitions for current build target
                var defines = new List<string>(UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Split(';'));
                var update = false;

                // Check for HDRP
                if (active)
                {
                    // Check if definition is missing
                    if (!defines.Contains(symbol))
                    {
                        // Add definition
                        defines.Add(symbol);
                        update = true;
                    }
                }
                else
                {
                    // Check if definition is set
                    if (defines.Contains(symbol))
                    {
                        // Remove definition
                        defines.Remove(symbol);
                        update = true;
                    }
                }

                if (update)
                {
                    // Store modified symbols
                    UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", defines.ToArray()));
                }
            }
        }

#endif

    }

    /// <summary>
    /// Class with helper functions
    /// </summary>
    public sealed class Helper
    {

        /// <summary>
        /// Save given object into an asset file
        /// </summary>
        /// <param name="assetObject">Source instance</param>
        /// <param name="pathName">Target path name</param>
        /// <param name="fileName">Target file name</param>
        /// <returns>If operation was successful.</returns>
        static public bool StoreAsset(UnityEngine.Object assetObject, string pathName, string fileName = null)
        {

#if UNITY_EDITOR

            // Check if result has to be stored into an asset file
            if (assetObject != null && pathName != null)
            {
                // Check if object is already stored to the database
                if (!UnityEditor.AssetDatabase.Contains(assetObject))
                {
                    // Split path into directory and file path, if file name is not given
                    if (fileName == null)
                    {
                        fileName = System.IO.Path.GetFileName(pathName);
                        pathName = pathName.Substring(0, pathName.Length - fileName.Length);
                    }

                    // Combine asset folder with path name
                    pathName = System.IO.Path.Combine("Assets", pathName);

                    // Get file extension or set standard one, if none is given
                    var extension = System.IO.Path.GetExtension(fileName);
                    if (string.IsNullOrEmpty(extension))
                    {
                        extension = ".asset";
                    }
                    else
                    {
                        fileName = fileName.Substring(0, fileName.Length - extension.Length);
                    }

                    // Make sure that a folders are existing or create them
                    if (CreateDirectory(pathName, false, true))
                    {
                        try
                        {
                            // Save given object into file
                            UnityEditor.AssetDatabase.CreateAsset(assetObject, System.IO.Path.Combine(pathName, fileName) + extension);
                            UnityEditor.AssetDatabase.SaveAssets();

                            return true;
                        }
                        catch (System.Exception exception)
                        {
                            Debug.LogWarning(exception.Message);
                        }
                    }
                }
                else
                {
                    return true;
                }
            }

#endif

            return false;
        }

        /// <summary>
        /// Create directories for given path, if they do not exist
        /// </summary>
        /// <param name="pathName">Path containing directories</param>
        /// <returns>If folder exists.</returns>
        static public bool CreateDirectory(string pathName, bool cutFileName = false, bool assetFolder = false)
        {

#if UNITY_EDITOR

            // Normalize separators
            pathName = pathName.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);

            // Check if directory does not exist
            if ((assetFolder && !UnityEditor.AssetDatabase.IsValidFolder(pathName)) || (!assetFolder && !System.IO.Directory.Exists(cutFileName ? System.IO.Path.GetDirectoryName(pathName) : pathName)))

#else

            // Add data path
            if (assetFolder)
            {
                pathName = System.IO.Path.Combine(Application.dataPath, pathName);
            }

            // Normalize separators
            pathName = pathName.Replace(System.IO.Path.AltDirectorySeparatorChar, System.IO.Path.DirectorySeparatorChar);

            // Check if directory does not exist
            if (!System.IO.Directory.Exists(cutFileName ? System.IO.Path.GetDirectoryName(pathName) : pathName))

#endif

            {
                string parentPathName = null;
                int pathNameSeparator;

                // Repeat until all parts of path have been processed
                do
                {
                    string directory;

                    // Find separator to get next directory
                    pathNameSeparator = pathName.IndexOf(System.IO.Path.DirectorySeparatorChar);
                    if (pathNameSeparator >= 0)
                    {
                        // Copy directory and trim path to remaining part
                        directory = pathName.Substring(0, pathNameSeparator);
                        pathName = pathName.Substring(pathNameSeparator + 1);
                    }
                    else if (cutFileName)
                    {
                        // Unset directory, if last part is a file name
                        directory = null;
                    }
                    else
                    {
                        // Use remaining path as directory
                        directory = pathName;
                    }

                    // Check for valid directory
                    if (directory != null && directory.Length > 0)
                    {
                        // Build path name including current directory
                        string currentPathName;
                        if (parentPathName != null)
                        {
                            currentPathName = parentPathName + System.IO.Path.DirectorySeparatorChar + directory;
                        }
                        else
                        {
                            currentPathName = directory;
                        }

#if UNITY_EDITOR

                        // Check if current directory does not already exist
                        if ((assetFolder && !UnityEditor.AssetDatabase.IsValidFolder(currentPathName)) || (!assetFolder && !System.IO.Directory.Exists(currentPathName)))

#else

                        // Check if current directory does not already exist
                        if (!System.IO.Directory.Exists(currentPathName))

#endif

                        {
                            try
                            {

#if UNITY_EDITOR

                                if (assetFolder)
                                {
                                    // Create new directory
                                    UnityEditor.AssetDatabase.CreateFolder(parentPathName, directory);
                                }
                                else

#endif

                                {
                                    // Create new directory
                                    System.IO.Directory.CreateDirectory(currentPathName);
                                }
                            }
                            catch (System.Exception exception)
                            {
                                Debug.Log(exception.Message);

                                return false;
                            }
                        }

                        // Store current path for next iteration
                        parentPathName = currentPathName;
                    }
                }
                while (pathNameSeparator >= 0);
            }

            return true;
        }

    }

}

