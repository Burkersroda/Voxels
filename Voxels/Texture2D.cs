//--------------------------------
//
// Voxels for Unity
//  Version: 1.22.6
//
// © 2014-21 by Ronny Burkersroda
//
//--------------------------------


using UnityEngine;
using System;
using System.Collections.Generic;


namespace Voxels
{

    // Class to convert incoming voxel data to a texture
    [AddComponentMenu("Voxels/2D Texture Creator"), RequireComponent(typeof(Rasterizer))]
    public class Texture2D : Processor
    {

        /// <summary>
        /// Format of the target file
        /// </summary>
        [System.Serializable]
        public enum FileFormat
        {
            JPG,
            PNG,
            TGA,
            EXR,
            Asset,
        }

        // Class, which is doing the actual work
        public class Process
        {

            // Target UV coordinates for voxel ones
            struct CoordinateAssignment
            {
                public Voxels.Vector source;
                public Vector2 target;
            }

            // Target texture
            protected UnityEngine.Texture2D texture = null;
            public UnityEngine.Texture2D Texture
            {
                get
                {
                    return texture;
                }
            }

            // Flag to create textures with 2^n resolution
            public bool powerOfTwo = false;

            // Current processing position
            int currentIndex = 0;
            float currentProgress = 0;
            public float CurrentProgress
            {
                get
                {
                    return currentProgress;
                }
            }

            // Voxels iterator
            Storage.Iterator iterator;
            bool building = false;

            // Assigned coordinates between source voxels and target texel
            Dictionary<Color, int> colorAssignments;
            CoordinateAssignment[] coordinateAssignments;

            // Build voxel object
            public virtual float Build(Storage voxels, Bounds bounds)
            {
                // Check for given array
                if (voxels != null)
                {
                    if (!building)
                    {
                        int existingIndex;
                        int x, y, z;

                        // Get iterator
                        if (iterator == null)
                        {
                            iterator = voxels.GetIterator();
                            currentIndex = 0;
                            currentProgress = 0;
                        }

                        if (colorAssignments == null)
                        {
                            // Create empty list to color assignments to
                            colorAssignments = new Dictionary<Color, int>();
                        }
                        else
                        {
                            // Get current color index from existing hash map
                            currentIndex = colorAssignments.Count;
                        }

                        // Process voxels in steps
                        for (int number = 0; number < 256; ++number)
                        {
                            // Retrieve color and coordinate for current cell
                            Color color = iterator.GetNextColor(out x, out y, out z);

                            // Check for valid voxel
                            if (color.a > 0)
                            {
                                // Add assignment between color and vertex index, if it is not already included
                                if (!colorAssignments.TryGetValue(color, out existingIndex))
                                {
                                    colorAssignments.Add(color, currentIndex++);
                                }
                            }
                            else
                            {
                                iterator = null;
                                break;
                            }
                        }

                        // Return current progress when building has not been finished
                        if (iterator != null)
                        {
                            return currentProgress = (float)iterator.Number / (float)(voxels.Count + 1) * 0.5f;
                        }
                        else
                        {
                            building = true;
                            texture = null;
                        }
                    }

                    if (colorAssignments != null)
                    {
                        CoordinateAssignment assignment;
                        int column = 0, line = 0;

                        // Compute resolution to fit all voxels into a 2D surface
                        int textureWidth = (int)Math.Pow(2, Math.Ceiling(Math.Log(Math.Sqrt(colorAssignments.Count)) / Math.Log(2)));
                        int textureHeight = (int)Math.Ceiling((double)colorAssignments.Count / (double)textureWidth);

                        // Make height 2^n, too, if flag is set
                        if (powerOfTwo)
                        {
                            textureHeight = (int)Math.Pow(2, Math.Ceiling(Math.Log((float)textureHeight) / Math.Log(2)));
                        }

                        //// Change resolution, if current does not match the required resolution
                        //if (texture != null && ((texture.width != textureWidth) || (texture.height != textureHeight)))
                        //{
                        //    try
                        //    {
                        //        texture.Resize(textureWidth, textureHeight, TextureFormat.ARGB32, false);

                        //        // Fill texture and calculate texture coordinates
                        //        foreach (KeyValuePair<Color, int> currentPixel in colorAssignments)
                        //        {
                        //            texture.SetPixel(column = currentPixel.Value % texture.width, line = currentPixel.Value / texture.width, currentPixel.Key);
                        //        }

                        //        iterator = null;
                        //    }
                        //    catch (System.Exception)
                        //    {
                        //        texture = null;
                        //    }
                        //}

                        if (texture == null)
                        {
                            if (textureWidth != 0 && textureHeight != 0)
                            {
                                // Create new texture instance
                                texture = new UnityEngine.Texture2D(textureWidth, textureHeight, voxels.HasHDR() ? TextureFormat.RGBAHalf : TextureFormat.ARGB32, false);
                                if (texture != null)
                                {
                                    texture.filterMode = FilterMode.Point;
                                    texture.wrapMode = TextureWrapMode.Repeat;
                                }
                            }

                            iterator = null;
                        }

                        if (texture != null)
                        {
                            // Check for non-empty array
                            if (voxels.Count > 0)
                            {
                                // Get iterator
                                if (iterator == null)
                                {
                                    iterator = voxels.GetIterator();
                                    currentIndex = 0;
                                    currentProgress = 0;

                                    // Create array to store coordinates to
                                    coordinateAssignments = new CoordinateAssignment[voxels.Count];
                                }

                                // Process voxels in steps
                                for (int number = 0; number < texture.width; ++number)
                                {
                                    // Retrieve color and coordinate for current cell
                                    int index = iterator.Number;
                                    Color color = iterator.GetNextColor(out assignment.source.x, out assignment.source.y, out assignment.source.z);

                                    // Check for valid voxel
                                    if (color.a > 0)
                                    {
                                        // Get index for current color
                                        if (colorAssignments.TryGetValue(color, out currentIndex))
                                        {
                                            // Store color as pixel
                                            texture.SetPixel(column = currentIndex % texture.width, line = currentIndex / texture.width, color);

                                            // Calculate coordinate for center of the current texel
                                            assignment.target.x = ((float)column + 0.5f) / (float)texture.width;
                                            assignment.target.y = ((float)line + 0.5f) / (float)texture.height;

                                            // Store assigned coordinates to array
                                            coordinateAssignments[index] = assignment;
                                        }
                                    }
                                    else
                                    {
                                        iterator = null;
                                        break;
                                    }
                                }

                                // Return current progress when building has not been finished
                                if (iterator != null)
                                {
                                    return currentProgress = (float)iterator.Number / (float)(voxels.Count + 1) * 0.5f + 0.5f;
                                }

                                // Unset remaining texels
                                for (column = colorAssignments.Count % texture.width, line = colorAssignments.Count / texture.width; line < texture.height; ++line)
                                {
                                    for (; column < texture.width; ++column)
                                    {
                                        texture.SetPixel(column, line, Color.clear);
                                    }

                                    column = 0;
                                }
                            }
                        }
                    }
                }

                // Check for texture and color array
                if (texture != null)
                {
                    // Apply color changes on texture
                    texture.Apply();
                }

                // Reset current processing data
                currentIndex = 0;
                iterator = null;
                colorAssignments = null;
                building = false;

                return currentProgress = 1;
            }

            // Return texture coordinate for voxel with given iterator index
            public Vector2 GetTextureCoordinate(Storage voxels, int number)
            {
                if (coordinateAssignments != null && number >= 0 && number < coordinateAssignments.Length)
                {
                    return coordinateAssignments[number].target;
                }

                return new Vector2(float.NaN, float.NaN);
            }

            // Return texture coordinate, column, line and slice for voxel with given iterator index
            public Vector2 GetTextureCoordinate(out Vector voxelCoordinate, Storage voxels, int number)
            {
                if (coordinateAssignments != null && number >= 0 && number < coordinateAssignments.Length)
                {
                    voxelCoordinate = coordinateAssignments[number].source;

                    return coordinateAssignments[number].target;
                }

                voxelCoordinate.x = voxelCoordinate.y = voxelCoordinate.z = 0;

                return new Vector2(float.NaN, float.NaN);
            }

        }

        // Processing instance
        protected Process processor = new Process();

        // File properties
        public string filePath;
        public bool fileStoring = true;
        public FileFormat fileFormat = FileFormat.PNG;

        // Voxel Mesh setting
        public bool voxelMeshUsage = false;

        // Return current progress
        public float CurrentProgress
        {
            get
            {
                return processor.CurrentProgress;
            }
        }

        // Return target texture
        public UnityEngine.Texture2D Texture
        {
            get
            {
                return processor.Texture;
            }
        }

        // Access power-of-two creation flag at the processor
        public bool powerOfTwo = false;


        // Return increased priority to process before VoxelMesh
        public override int GetPriority()
        {
            return 1;
        }

        // Build voxel object
        public override float Build(Storage voxels, Bounds bounds, Informer informer, object parameter)
        {
            processor.powerOfTwo = powerOfTwo;

            // Execute real build-up method
            float progress = processor.Build(voxels, bounds);

            // Check if processing has been finished
            if (progress >= 1)
            {
                // Check if this texture should be used by Voxel Mesh(es)
                if (voxelMeshUsage)
                {
                    // Get all voxel meshes, which are attached to the game object
                    Mesh[] voxelMeshes = gameObject.GetComponents<Mesh>();
                    if (voxelMeshes != null)
                    {
                        // Set processor to all meshes
                        foreach (Mesh mesh in voxelMeshes)
                        {
                            mesh.voxelTexture2D = processor;
                        }
                    }
                }

                // Store file, if it is specified
                if (fileStoring && filePath != null && filePath.Length > 0 && Texture != null)
                {
                    try
                    {
                        byte[] data;
                        string path;

                        // Build target path
                        switch (fileFormat)
                        {
                            case FileFormat.JPG:
                            case FileFormat.PNG:
                            case FileFormat.TGA:
                            case FileFormat.EXR:
                                path = System.IO.Path.Combine(Application.dataPath, filePath);
                                break;

                            default:
                                path = filePath;
                                break;
                        }

                        switch (fileFormat)
                        {
                            case FileFormat.JPG:
                                // Convert texture to JPEG data
                                data = Texture.EncodeToJPG(100);
                                break;

                            case FileFormat.PNG:
                                // Convert texture to PNG data
                                data = Texture.EncodeToPNG();
                                break;

                            case FileFormat.TGA:
                                // Convert texture to TGA data
                                data = Texture.EncodeToTGA();
                                break;

                            case FileFormat.EXR:
                                // Convert texture to EXR data
                                data = Texture.EncodeToEXR(UnityEngine.Texture2D.EXRFlags.CompressZIP);
                                break;

                            default:
                                data = null;
                                break;
                        }

                        // Check for encoded data
                        if (data != null)
                        {
                            // Make sure the target folders exist
                            if (Helper.CreateDirectory(path, true))
                            {
                                // Save data to file
                                System.IO.File.WriteAllBytes(path, data);
                            }
                        }
                        else
                        {
                            // Save texture as asset file
                            Helper.StoreAsset(processor.Texture, path, null);
                        }
                    }
                    catch (System.Exception exception)
                    {
                        Debug.Log(exception.Message);
                    }
                }

#if UNITY_EDITOR

                // Add object creation undo operation
                if (!Application.isPlaying)
                {
                    UnityEditor.Undo.RegisterCreatedObjectUndo(processor.Texture, "\"VoxelTexture2D\" Creation");
                }

#endif

                // Execute informer callback
                informer?.Invoke(new UnityEngine.Object[] { processor.Texture }, parameter);
            }

            return progress;
        }

        // Return texture coordinates for voxel with given iterator index
        public Vector2 GetTextureCoordinate(Storage voxels, int number)
        {
            return processor.GetTextureCoordinate(voxels, number);
        }

        // Return texture coordinate, column, line and slice for voxel with given iterator index
        public Vector2 GetTextureCoordinate(out Vector voxelCoordinate, Storage voxels, int number)
        {
            return processor.GetTextureCoordinate(out voxelCoordinate, voxels, number);
        }

    }

}