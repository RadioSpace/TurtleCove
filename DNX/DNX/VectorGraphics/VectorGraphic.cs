using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpDX;

namespace DNX.VectorGraphics
{


    [Serializable]
    public class VectorGraphic
    {
        List<List<GLine>> Xgraph_R;
        List<List<GLine>> Xgraph_G;
        List<List<GLine>> Xgraph_B;
        List<List<GLine>> Xgraph_A;

        List<List<GLine>> Ygraph_R;
        List<List<GLine>> Ygraph_G;
        List<List<GLine>> Ygraph_B;
        List<List<GLine>> Ygraph_A;

        double XUnit;
        double YUnit;

        double dataWidth;
        double dataHeight;

        /// <summary>
        /// creates a vectorized graphic
        /// </summary>
        /// <param name="rawData">the raw data from a directx texture2d</param>
        /// <param name="ogWidth">the width of the image</param>
        /// <param name="ogHeight">the height of the image</param>
        /// <param name="rowPitch">the number of bytes in a row (get from SharpDX.DataBox.RowPitch) NOTE: if zero will match ogWidth</param>
        public VectorGraphic(byte[] rawData, double ogWidth, double ogHeight, double rowPitch = 0)
        {

            if (rowPitch == 0) rowPitch = ogWidth;

            XUnit = 1.0;
            YUnit = 1.0;

            if (ogWidth != 0 && ogHeight != 0)
            {
                XUnit = 1.0 / (double)ogWidth;
                YUnit = 1.0 / (double)ogHeight;
            }

            Xgraph_R = new List<List<GLine>>();
            Xgraph_G = new List<List<GLine>>();
            Xgraph_B = new List<List<GLine>>();
            Xgraph_A = new List<List<GLine>>();

            Ygraph_R = new List<List<GLine>>();
            Ygraph_G = new List<List<GLine>>();
            Ygraph_B = new List<List<GLine>>();
            Ygraph_A = new List<List<GLine>>();

            int dataSize = (int)(rowPitch * ogHeight);



            for (double x = 0; x < ogWidth; x++)
            {
                List<GLine> Col_R = new List<GLine>();
                List<GLine> Col_G = new List<GLine>();
                List<GLine> Col_B = new List<GLine>();
                List<GLine> Col_A = new List<GLine>();

                for (double y = 1; y < ogHeight; y++)
                {
                    double y_start = (y - 1.0) / ogHeight;
                    double y_end = y / ogHeight;

                    int index_cur = (int)((x * 4.0) + (y * (double)rowPitch));
                    int index_pre = index_cur - (int)rowPitch;


                    //current color
                    double cr = rawData[index_cur];
                    double cg = rawData[index_cur + 1];
                    double cb = rawData[index_cur + 2];
                    double ca = rawData[index_cur + 3];


                    //previous color
                    double pr = rawData[index_pre];
                    double pg = rawData[index_pre + 1];
                    double pb = rawData[index_pre + 2];
                    double pa = rawData[index_pre + 3];



                    //now add the data to the Vector graphic
                    //divide by 4 ? or even 8? because of the y data that will be added?
                    //

                    Col_R.Add(new GLine(y_start, y_end, pr, cr));
                    Col_G.Add(new GLine(y_start, y_end, pg, cg));
                    Col_B.Add(new GLine(y_start, y_end, pb, cb));
                    Col_A.Add(new GLine(y_start, y_end, pa, ca));

                }

                Ygraph_R.Add(Col_R);
                Ygraph_G.Add(Col_G);
                Ygraph_B.Add(Col_B);
                Ygraph_A.Add(Col_A);
            }



            for (double y = 0; y < ogHeight; y++)
            {
                List<GLine> Row_R = new List<GLine>();
                List<GLine> Row_G = new List<GLine>();
                List<GLine> Row_B = new List<GLine>();
                List<GLine> Row_A = new List<GLine>();

                for (double x = 1; x < ogWidth; x++)
                {
                    double x_start = (x - 1.0) / ogWidth;
                    double x_end = x / ogWidth;

                    int index_cur = (int)((x * 4.0) + (y * (double)rowPitch));
                    int index_pre = index_cur - 4;

                    //current color
                    double cr = rawData[index_cur];
                    double cg = rawData[index_cur + 1];
                    double cb = rawData[index_cur + 2];
                    double ca = rawData[index_cur + 3];

                    //previous color
                    double pr = rawData[index_pre];
                    double pg = rawData[index_pre + 1];
                    double pb = rawData[index_pre + 2];
                    double pa = rawData[index_pre + 3];

                    //now add the data to the Vector graphic

                    Row_R.Add(new GLine(x_start, x_end, pr, cr));
                    Row_G.Add(new GLine(x_start, x_end, pg, cg));
                    Row_B.Add(new GLine(x_start, x_end, pb, cb));
                    Row_A.Add(new GLine(x_start, x_end, pa, ca));
                }

                Xgraph_R.Add(Row_R);
                Xgraph_G.Add(Row_G);
                Xgraph_B.Add(Row_B);
                Xgraph_A.Add(Row_A);
            }

            dataWidth = Xgraph_R.Count;
            dataHeight = Ygraph_R.Count;

        }

        public byte[] GetImage(double width, double height, double SampleDepth)
        {
            
            int rowPitch = (int)width * 4;
            int imageLength = (int)(rowPitch * height);

            List<byte> imageData = new List<byte>(imageLength);

            double stepWidth = width / dataWidth;
            double stepHeight = height / dataHeight;

            double dataStep = 1.0 / stepWidth;

            for (int y = 0; y < height; y++)
            {

                byte[] newrow = new byte[rowPitch];

                GLine[] rowData = Xgraph_R[(int)((double)y / stepHeight)].ToArray();



                for (int i = 0; i < rowData.Length; i++)
                {

                    int startx = (int)(rowData[i].getPosition(0.0) * (double)width);                    
                    int endx = (int)( rowData[i].getPosition(1.0) * (double)width);                    

                    for (int x = startx; x < endx; x++)
                    {
                        double u = (double)x - (double)startx / stepWidth;

                        int index = x * 4;

                        newrow[index] = (byte)(rowData[i].getAmount(u) % 256.0);
                        newrow[index + 1] = 255;
                        newrow[index + 2] = 255;
                        newrow[index + 3] = 255;
                    }

                }

                imageData.AddRange(newrow);
            }
            


            return imageData.ToArray();
        }


 





    }
}

