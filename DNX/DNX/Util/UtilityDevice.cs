using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace DNX.Util
{

    using Device = SharpDX.Direct3D11.Device;

    /// <summary>
    /// a device that does not output to the screen
    /// </summary>
    /// <remarks>this is used for creating things like textures that are not used for rendering but saving to file after writing to them</remarks>
    public class UtilityDevice : IDisposable
    {
        #region static

        static object locker = new object();

        #endregion

        Device d;
        /// <summary>
        /// gets the Device
        /// </summary>
        public Device GraphicsDevice { get { return d; } }
        /// <summary>
        /// gets the device context
        /// </summary>
        public DeviceContext Context { get { return d.ImmediateContext; } }
       

        public UtilityDevice(SharpDX.Direct3D.DriverType type)
        {
            lock (locker)
            {
#if DEBUG
                d = new Device(type, DeviceCreationFlags.Debug);
#else 
                d = new Device(type);
#endif
            }
        }

        public void Dispose()
        {
            lock (locker)
            {
                if (d != null) d.Dispose();
            }
        }
    }
}
