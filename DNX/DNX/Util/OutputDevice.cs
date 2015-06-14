using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using DNX.Util;





namespace DNX
{
    using Device = SharpDX.Direct3D11.Device;


    /// <summary>
    /// a device that outputs to the screen
    /// </summary>
    public class OutputDevice : IDisposable
    {
        #region static 

        static object locker = new object();

        #endregion

        //switch to full screen and detect window size change

        Device d;
        SwapChain sc;



        public OutputDevice(System.Windows.Forms.Form form)
        {

            SwapChainDescription scd = new SwapChainDescription()
            {
                BufferCount = 4,
                Flags = SwapChainFlags.None,
                IsWindowed = true,
                ModeDescription = new ModeDescription(form.ClientSize.Width, form.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };


            lock (locker)
            {
#if DEBUG
                Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.Debug, scd, out d, out sc);
#else 
                Device.CreateWithSwapChain(SharpDX.Direct3D.DriverType.Hardware,DeviceCreationFlags.None ,scd,out d,out sc);
#endif
            }            
        }

        #region public

        

        #endregion


        #region IDisposable
        public void Dispose()
        {
            lock (locker)
            {
                if (d != null) d.Dispose();
                if (sc != null) sc.Dispose();
            }
        }
        #endregion
    }
}
