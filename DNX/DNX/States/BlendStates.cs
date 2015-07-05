using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;
using SharpDX.Direct3D11;

namespace DNX.States
{
    public enum BlendStyle { Disabled, AlphaBlend,Overwrite};

    public static class BlendStateDescriptions
    {


        static RenderTargetBlendDescription AlphaBlendDescription;
        static RenderTargetBlendDescription OverwriteBlendDescription;

        static RenderTargetBlendDescription DisabledBlendDescription;

        static BlendStateDescriptions()
        {
            AlphaBlendDescription = new RenderTargetBlendDescription() 
            {
                IsBlendEnabled = true,

                RenderTargetWriteMask = ColorWriteMaskFlags.All,
                

                AlphaBlendOperation = BlendOperation.Add,
                BlendOperation = BlendOperation.Add,
                

                SourceAlphaBlend = BlendOption.SourceAlpha,
                SourceBlend = BlendOption.SourceAlpha,



                DestinationAlphaBlend = BlendOption.Zero,
                DestinationBlend = BlendOption.InverseSourceAlpha               
            };

            OverwriteBlendDescription = new RenderTargetBlendDescription()
            {

                IsBlendEnabled = true,

                RenderTargetWriteMask = ColorWriteMaskFlags.All,


                AlphaBlendOperation = BlendOperation.Add,
                BlendOperation = BlendOperation.Add,


                SourceAlphaBlend = BlendOption.One,
                SourceBlend = BlendOption.One,



                DestinationAlphaBlend = BlendOption.Zero,
                DestinationBlend = BlendOption.Zero

            };

            DisabledBlendDescription = new RenderTargetBlendDescription()
            {
                IsBlendEnabled = false

            };
            

        }


        public static SharpDX.Direct3D11.BlendState GetBlendState(Device d,params BlendStyle[] targets)
        { 
            BlendStateDescription bsd = new BlendStateDescription();

            for (int i = 0; i < targets.Length; i++)
            {


                if (i < bsd.RenderTarget.Length)
                {
                    switch(targets[i])
                    {
                        case BlendStyle.AlphaBlend:
                            bsd.RenderTarget[i] = AlphaBlendDescription;
                            break;
                        case BlendStyle.Overwrite:
                            bsd.RenderTarget[i] = OverwriteBlendDescription;
                            break;
                        case BlendStyle.Disabled:
                            bsd.RenderTarget[i] = DisabledBlendDescription;
                            break;
                        default:
                            throw new ArgumentException("Unknown Blend Style " + targets[i].ToString());
                    }

                   
                }
                else break;
            }


            return new BlendState(d, bsd);
        }

    }
}
