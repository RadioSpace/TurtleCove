using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;
using SharpDX.Direct3D11;

using DNX.Util;

namespace DNX.VectorGraphics
{
    public class ImageAnalyzer
    {
        /// <summary>
        /// creates the vectordata of an image
        /// </summary>
        /// <param name="ud">the device that created the Texture </param>
        /// <param name="data">the image to analyze</param>
        /// <remarks>
        /// Texture expected to be be R8G8B8A8_UInt format
        /// </remarks>
        public VectorGraphic AnalyzeImage(UtilityDevice ud, Texture2D data)
        {

            DataBox dataBox = ud.Context.MapSubresource(data,0,MapMode.Read,MapFlags.None);
            int dataSize = dataBox.RowPitch * data.Description.Height;
            
            byte[] rawData = new byte[dataSize];
            
            System.Runtime.InteropServices.Marshal.Copy(dataBox.DataPointer,rawData,0,dataSize - 1);

            VectorGraphic VG = new VectorGraphic(rawData, data.Description.Width, data.Description.Height, dataBox.RowPitch);          
            
            ud.Context.UnmapSubresource(data, 0);

            return VG;

        }

    }
}

