using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SharpDX;
using SharpDX.Direct3D11;

using DNX.Util;
using DNX.VectorGraphics;

namespace Laboratory
{
    [TestClass]
    public class VectorTests
    {
        [TestMethod]
        public void ImageAnalyzeTest()
        {

            UtilityDevice ud = new UtilityDevice(SharpDX.Direct3D.DriverType.Hardware);

        

            Texture2D inputTex = Texture2D.FromFile<Texture2D>(ud,"Util\\VectorizeTest.png",new ImageLoadInformation() 
            { 
             BindFlags = BindFlags.None,
             CpuAccessFlags = CpuAccessFlags.Read,
             Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
             Usage = ResourceUsage.Staging
            });

            int dWidth = inputTex.Description.Width * 2;
            int dHeight = inputTex.Description.Height * 2;

            Texture2D outputTex = new Texture2D(ud, new Texture2DDescription()
            {
                ArraySize = inputTex.Description.ArraySize,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Write,
                Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                Height = dHeight,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = inputTex.Description.SampleDescription,
                Usage =ResourceUsage.Staging,
                Width = dWidth

            });//make a matching texture
            
            

            ImageAnalyzer analyzer = new ImageAnalyzer();            
            
            VectorGraphic VG = analyzer.AnalyzeImage(ud, inputTex);

            byte[] imageData = VG.GetImage(dWidth,dHeight,2);

            

            DataBox dataBox = ud.Context.MapSubresource(outputTex, 0, MapMode.Write, MapFlags.None);

            byte[] texData = new byte[dataBox.RowPitch * outputTex.Description.Height];

            for (int x = 0; x < outputTex.Description.Width; x++)
            {
                for (int y = 0; y < outputTex.Description.Height; y++)
                {
                    int imageIndex = (x * 4) + (y * (outputTex.Description.Width*4));
                    int texIndex = (x * 4) + (y * dataBox.RowPitch);
                   

                    texData[texIndex] = imageData[imageIndex];
                    texData[texIndex+1] = imageData[imageIndex+1];
                    texData[texIndex+2] = imageData[imageIndex+2];
                    texData[texIndex+3] = imageData[imageIndex+3];

                    
                }
            }

            Utilities.Write(dataBox.DataPointer, texData, 0, texData.Length);

            ud.Context.UnmapSubresource(outputTex, 0);

            Texture2D.ToFile(ud.Context, outputTex, ImageFileFormat.Png, "c:\\dan\\vectorizedOutput.png");

        }
    }
}
