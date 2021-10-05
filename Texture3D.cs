//--------------------------------
//
// Voxels for Unity
//  Version: 1.23.7
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

        /// <summary>
        /// Format of the target file
        /// </summary>
        [System.Serializable]
        public enum FileFormat
        {
            Asset,

#if UNITY_2020_2_OR_NEWER

            JPG,
            PNG,
            TGA,
            EXR,

#endif

        }

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

            // Number of samples per axis to merge into one voxel
            public int superSamplingCount = 1;

            // Flag to create textures with 2^n resolution
            public bool powerOfTwo = false;

            // Flag to fill background with colors from content
            public bool expandEdges = false;

            // Color to fill empty cells with
            public Color backgroundColor = new Color(0, 0, 0, 0);

            // Flag to use a floating point texture
            public bool hdr = false;

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

            // Array of number of merged colors per texel
            float[] counts;

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
                                if (superSamplingCount <= 0)
                                {
                                    superSamplingCount = 1;
                                }

                                // Calculate target resolution
                                int textureWidth = (voxels.Width + superSamplingCount - 1) / superSamplingCount;
                                int textureHeight = (voxels.Height + superSamplingCount - 1) / superSamplingCount;
                                int textureDepth = (voxels.Depth + superSamplingCount - 1) / superSamplingCount;

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
                                    counts = new float[textureWidth * textureHeight * textureDepth];

                                    hdr |= voxels.HasHDR();

                                    // Create new texture instance
                                    texture = new UnityEngine.Texture3D(textureWidth, textureHeight, textureDepth, hdr ? TextureFormat.RGBAHalf : TextureFormat.RGBA32, 4);
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
                                        var index = x / superSamplingCount + (y / superSamplingCount + z / superSamplingCount * texture.height) * texture.width;

                                        // Store color to texels array
                                        texels[index] += color;
                                        ++counts[index];
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
                                    // Calculate weight factor for every source cell
                                    var samplingFactor = 1f / (superSamplingCount * superSamplingCount * superSamplingCount);

                                    // Normalize colors and expand edges or blend with background
                                    for (int index = 0; index < texels.Length; ++index)
                                    {
                                        if (counts[index] > 0)
                                        {
                                            if (expandEdges)
                                            {
                                                texels[index].r /= counts[index];
                                                texels[index].g /= counts[index];
                                                texels[index].b /= counts[index];
                                                texels[index].a *= samplingFactor;
                                            }
                                            else
                                            {
                                                texels[index] /= counts[index];
                                                texels[index] += backgroundColor * (1 - texels[index].a);
                                            }
                                        }
                                        else
                                        {
                                            if (!expandEdges)
                                            {
                                                texels[index] = backgroundColor;
                                            }
                                        }
                                    }

                                    if (expandEdges)
                                    {
                                        bool repeat;

                                        do
                                        {
                                            repeat = false;

                                            // Process all cells
                                            for (int index = 0; index < texels.Length; ++index)
                                            {
                                                // Check if current cell is empty
                                                if (counts[index] == 0)
                                                {
                                                    var column = index % texture.width;
                                                    var row = index / texture.width % texture.height;
                                                    var slice = index / texture.width / texture.height;

                                                    var color = new Color(0, 0, 0, 0);
                                                    var count = 0f;

                                                    // Sum up all colors of direct neighbor cells 
                                                    for (int offset = 0; offset < 6; ++offset)
                                                    {
                                                        // Get offset by current index
                                                        var offsetX = offset == 0 ? -1 : offset == 1 ? 1 : 0;
                                                        var offsetY = offset == 2 ? -1 : offset == 3 ? 1 : 0;
                                                        var offsetZ = offset == 4 ? -1 : offset == 5 ? 1 : 0;

                                                        var offsetColumn = column + offsetX;
                                                        if (offsetColumn >= 0 && offsetColumn < texture.width)
                                                        {
                                                            var offsetRow = row + offsetY;
                                                            if (offsetRow >= 0 && offsetRow < texture.height)
                                                            {
                                                                var offsetSlice = slice + offsetZ;
                                                                if (offsetSlice >= 0 && offsetSlice < texture.depth)
                                                                {
                                                                    var offsetIndex = offsetColumn + (offsetRow + offsetSlice * texture.height) * texture.width;

                                                                    // Check if neighbor includes an original color or one that has been set in a previous iteration
                                                                    if (counts[offsetIndex] > 0)
                                                                    {
                                                                        // Sum color components and increase quantity counter for later normalization
                                                                        color += texels[offsetIndex];
                                                                        ++count;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }

                                                    if (count > 0)
                                                    {
                                                        // Normalize target color but set full transparency
                                                        texels[index].r = color.r / count;
                                                        texels[index].g = color.g / count;
                                                        texels[index].b = color.b / count;
                                                        texels[index].a = 0;

                                                        // Flag index as processed in this loop and enable next one
                                                        counts[index] = -count;
                                                        repeat = true;
                                                    }
                                                }
                                            }

                                            // Unset processing flags for next iteration
                                            for (int index = 0; index < texels.Length; ++index)
                                            {
                                                if (counts[index] < 0)
                                                {
                                                    counts[index] = -counts[index];
                                                }
                                            }
                                        }
                                        while (repeat);
                                    }

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
        public bool fileStoring = false;
        public FileFormat fileFormat = FileFormat.Asset;

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

        // Number of samples per axis to merge into one voxel
        public int superSamplingCount = 1;

        // Access power-of-two creation flag at the processor
        public bool powerOfTwo = false;

        // Flag to fill background with colors from content
        public bool expandEdges = false;

        // Color to fill empty cells with
        public Color backgroundColor = Color.black;


        // Return increased priority to process before VoxelMesh
        public override int GetPriority()
        {
            return 1;
        }

        // Build voxel object
        public override float Build(Storage voxels, Bounds bounds, Informer informer, object parameter)
        {
            processor.superSamplingCount = superSamplingCount;
            processor.powerOfTwo = powerOfTwo;
            processor.expandEdges = expandEdges;
            processor.backgroundColor = backgroundColor;

#if UNITY_2020_2_OR_NEWER

            processor.hdr = fileFormat == FileFormat.EXR;

#endif

            // Execute real build-up method
            float progress = processor.Build(voxels, bounds);

            // Check if processing has been finished
            if (progress >= 1)
            {
                // Store file, if it is specified
                if (fileStoring && filePath != null && filePath.Length > 0 && Texture != null)
                {
                    try
                    {
                        // Build target path
                        switch (fileFormat)
                        {

#if UNITY_2020_2_OR_NEWER

                            case FileFormat.JPG:
                            case FileFormat.PNG:
                            case FileFormat.TGA:
                            case FileFormat.EXR:

                                var source = processor.Texture;
                                if (source)
                                {
                                    var sourceData = source.GetPixelData<byte>(0);
                                    if (sourceData != null)
                                    {
                                        var length = source.width * source.height * source.depth;
                                        //var textureWidth = Mathf.CeilToInt(Mathf.Sqrt(length));

                                        var textureWidth = (int)Math.Pow(2, Math.Ceiling(Math.Log(Mathf.Sqrt(length)) / Math.Log(2)));

                                        var columnsCount = Mathf.CeilToInt((float)textureWidth / source.width);
                                        var rowsCount = Mathf.CeilToInt((float)source.depth / columnsCount);

                                        textureWidth = source.width * columnsCount;
                                        var textureHeight = source.height * rowsCount;

                                        var pixelSize = voxels.HasHDR() ? 8 : 4;
                                        var lineSize = source.width * pixelSize;

                                        var buffer = new byte[lineSize];
                                        var targetData = new byte[textureWidth * textureHeight * pixelSize];
                                        if (targetData != null)
                                        {
                                            var sourceOffset = 0;

                                            // Process depth slices
                                            for (int slice = 0; slice < source.depth; ++slice)
                                            {
                                                // Calculate column and row of the target sub image
                                                var column = slice % columnsCount;
                                                var row = rowsCount - slice / columnsCount - 1;

                                                // Process lines of the current slice
                                                for (int line = 0; line < source.height; ++line)
                                                {
                                                    // Get data for line into the cache
                                                    sourceData.GetSubArray(sourceOffset, lineSize).CopyTo(buffer);

                                                    // Copy it to the target buffer
                                                    Array.Copy(buffer, 0, targetData, (column + (line + row * source.height) * columnsCount) * lineSize, lineSize);

                                                    sourceOffset += lineSize;
                                                }
                                            }

                                            byte[] data;
                                            switch (fileFormat)
                                            {
                                                case FileFormat.JPG:
                                                    // Convert image data to JPEG data
                                                    data = ImageConversion.EncodeArrayToJPG(targetData, source.graphicsFormat, (uint)textureWidth, (uint)textureHeight, 0, 100);
                                                    break;

                                                case FileFormat.PNG:
                                                    // Convert image data to PNG data
                                                    data = ImageConversion.EncodeArrayToPNG(targetData, source.graphicsFormat, (uint)textureWidth, (uint)textureHeight);
                                                    break;

                                                case FileFormat.TGA:
                                                    // Convert image data to TGA data
                                                    data = ImageConversion.EncodeArrayToTGA(targetData, source.graphicsFormat, (uint)textureWidth, (uint)textureHeight);
                                                    break;

                                                case FileFormat.EXR:
                                                    // Convert image data to EXR data
                                                    data = ImageConversion.EncodeArrayToEXR(targetData, source.graphicsFormat, (uint)textureWidth, (uint)textureHeight, 0, UnityEngine.Texture2D.EXRFlags.CompressZIP);
                                                    break;

                                                default:
                                                    data = null;
                                                    break;
                                            }

                                            if (data != null)
                                            {
                                                // Make sure the target folders exist
                                                var path = System.IO.Path.Combine(Application.dataPath, filePath);
                                                if (Helper.CreateDirectory(path, true))
                                                {

#if UNITY_EDITOR

                                                    // Remove existing asset file
                                                    if (System.IO.File.Exists(path))
                                                    {
                                                        System.IO.File.Delete(path);
                                                    }

                                                    // Update database
                                                    UnityEditor.AssetDatabase.Refresh();

#endif

                                                    // Save data to file
                                                    System.IO.File.WriteAllBytes(path, data);

#if UNITY_EDITOR

                                                    // Store columns and rows for the asset to be applied later
                                                    voxelTexturePaths.Add(System.IO.Path.GetFullPath(path), new Vector2Int(columnsCount, rowsCount));

                                                    // Update database to enable changing import settings
                                                    UnityEditor.AssetDatabase.Refresh();

#endif

                                                }
                                            }
                                        }
                                    }
                                }

                                break;

#endif

                            default:
                                // Save texture as asset file
                                Helper.StoreAsset(processor.Texture, filePath, null);
                                break;
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
                    UnityEditor.Undo.RegisterCreatedObjectUndo(processor.Texture, "\"VoxelTexture3D\" Creation");
                }

#endif

                // Execute informer callback
                informer?.Invoke(new UnityEngine.Object[] { processor.Texture }, parameter);
            }

            return progress;
        }

#if UNITY_EDITOR

        /// <summary>
        /// Hash table with import settings for added texture assets
        /// </summary>
        protected static Dictionary<string, Vector2Int> voxelTexturePaths = new Dictionary<string, Vector2Int>();

        /// <summary>
        /// Retrieve import settings for texture with given path
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>Import settings</returns>
        public static Vector2Int GetTilingDimension(string path)
        {
            // Get full path for comparison
            path = System.IO.Path.GetFullPath(path);

            // Try to get settings from hash table
            Vector2Int dimension;
            if (voxelTexturePaths.TryGetValue(path, out dimension))
            {
                // Remove element
                voxelTexturePaths.Remove(path);

                return dimension;
            }

            return Vector2Int.zero;
        }

#endif

    }

#if UNITY_EDITOR && UNITY_2020_2_OR_NEWER

    /// <summary>
    /// Class to change import settings for volume textures
    /// </summary>
    public class Texture3DPostProcessor : UnityEditor.AssetPostprocessor
    {

        /// <summary>
        /// Prepare texture for importing
        /// </summary>
        void OnPreprocessTexture()
        {
            // Try to get import setting for the texture
            var dimension = Texture3D.GetTilingDimension(assetPath);
            if (dimension.x > 0 && dimension.y > 0)
            {
                var textureImporter = assetImporter as UnityEditor.TextureImporter;
                if (textureImporter)
                {
                    // Change import settings to 3D texture using the columns and rows
                    var settings = new UnityEditor.TextureImporterSettings();
                    textureImporter.ReadTextureSettings(settings);
                    settings.textureShape = UnityEditor.TextureImporterShape.Texture3D;
                    settings.flipbookColumns = dimension.x;
                    settings.flipbookRows = dimension.y;
                    settings.sRGBTexture = false;
                    settings.alphaSource = UnityEditor.TextureImporterAlphaSource.FromInput;
                    settings.alphaIsTransparency = true;
                    settings.mipmapEnabled = true;
                    settings.filterMode = FilterMode.Trilinear;
                    textureImporter.SetTextureSettings(settings);
                }
            }
        }

    }

#endif

}