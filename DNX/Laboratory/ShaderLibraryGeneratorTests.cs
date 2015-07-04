using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using ShaderClasses;

using SharpDX;

namespace Laboratory
{
    [TestClass]
    public class ShaderLibraryGeneratorTests
    {
        [TestMethod]
        public void TestSerialization()
        {   

            BinaryFormatter bf = new BinaryFormatter();

            MemoryStream ms = new MemoryStream();

            TestVertexShader_Vertex testWriteVertex = new TestVertexShader_Vertex(new Vector4(0, 1, 0, 1), new Vector2(0,1));
            TestVertexShader_Vertex testReadVertex;

            bf.Serialize(ms,testWriteVertex);

            ms.Close();
            byte[] testVertexData = ms.ToArray();
            ms.Dispose();

            using (MemoryStream ms2 = new MemoryStream(testVertexData))
            {
                testReadVertex = (TestVertexShader_Vertex)bf.Deserialize(ms2);
            }

            Assert.IsFalse(testWriteVertex.Equals(testReadVertex));
            
            

        }
    }
}
