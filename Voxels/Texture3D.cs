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

    // Class to convert incoming voxel data to a volume texture
    [AddComponentMenu("Voxels/3D Texture Creator"), RequireComponent(typeof(Rasterizer))]
    public class Texture3D : Processor
    {

        // Class, which is doing the actual work
        public class Process
        {

            // Target texture
            protected UnityEngine.Texture3D texture = null;
            public UnityEngine.Texture3D Texture
            {
                get
                {
                    return texture;
                }
            }

            // Flag to create textures with 2^n resolution
            public bool powerOfTwo = false;

            // Current processing position
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

            // Array to colors for all texels
            Color[] texels;

            // Build voxel object
            public virtual float Build(Storage voxels, Bounds bounds)
            {
                // Check for given array
                if (voxels != null)
                {
                    //if (colorAssignments != null)
                    {
                        // Check for non-empty array
                        if (voxels.Count > 0)
                        {
                            // Get iterator
                            if (iterator == null)
                            {
                                iterator = voxels.GetIterator();
                                currentProgress = 0;
                            }

                            if (texture == null)
                            {
                                int textureWidth = voxels.Width;
                                int textureHeight = voxels.Height;
                                int textureDepth = voxels.Depth;

                                // Make resolution 2^n, if flag is set
                                if (powerOfTwo)
                                {
                                    textureWidth = (int)Math.Pow(2, Math.Ceiling(Math.Log((float)textureWidth) / Math.Log(2)));
                                    textureHeight = (int)Math.Pow(2, Math.Ceiling(Math.Log((float)textureHeight) / Math.Log(2)));
                                    textureDepth = (int)Math.Pow(2, Math.Ceiling(Math.Log((float)textureDepth) / Math.Log(2)));
                                }

                                if (textureWidth != 0 && textureHeight != 0 && textureDepth != 0)
                                {
                                    texels = new Color[textureWidth * textureHeight * textureDepth];

                                    // Create new texture instance
                                    texture = new UnityEngine.Texture3D(voxels.Width, voxels.Height, voxels.Depth, voxels.HasHDR() ? TextureFormat.RGBAHalf : TextureFormat.ARGB32, false);
                                    if (texture != null)
                                    {
                                        //texture.filterMode = FilterMode.Point;
                                        texture.wrapMode = TextureWrapMode.Clamp;
                                    }
                                }
                            }

                            if (texture != null)
                            {
                                // Process voxels in steps
                                for (int number = 0; number < 10; ++number)
                                {
                                    // Retrieve color and coordinate for current cell
                                    int x, y, z;
                                    Color color = iterator.GetNextColor(out x, out y, out z);

                                    // Check for valid voxel
                                    if (color.a > 0)
                                    {
                                        // Store color to texels array
                                        texels[x + (y + z * texture.height) * texture.width] = color;
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
                                    return currentProgress = (float)iterator.Number / (float)(voxels.Count + 1);
                                }
                                else
                                {
                                    // Transfer all texel colors to the texture
                                    texture.SetPixels(texels);
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
                iterator = null;

                return currentProgress = 1;
            }

        }

        // Processing instance
        protected Process processor = new Process();

        // File properties
        public string filePath;
        public bool fileStoring = true;

        // Return current progress
        public float CurrentProgress
        {
            get
            {
                return processor.CurrentProgress;
            }
        }

        // Return target texture
        public UnityEngine.Texture3D Texture
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
                // Store file, if it is specified
                if (fileStoring && filePath != null && filePath.Length > 0 && Texture != null)
                {
                    // Save texture as asset file
                    Helper.StoreAsset(processor.Texture, filePath, null);
                }

#if UNITY_EDITOR

                // Add object creation undo operation
                if (!Application.isPlaying)
                {
                    UnityEditor.Undo.RegisterCreatedObjectUndo(processor.Texture, "\"VoxelTexture3D\" Creation");
                }

#endif

                // Execute informer callback
                informer?.Invoke(new UnityEngine.Object[] { processor.Texture }, parameter);
            }

            return progress;
        }

    }

}