using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;




namespace DNX.Util
{

    public interface IVertexShaderDevice
    {
        void PlaceInContext(SharpDX.Direct3D11.DeviceContext deviceContext);
        
        void UpdateVertexBuffer();
        void UpdateIndexBuffer();
        


        


    }
}
