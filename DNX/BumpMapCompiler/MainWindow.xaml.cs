using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using SharpDX;
using SharpDX.Direct3D11;

namespace BumpMapCompiler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ObservableCollection<string> paths;
        int redIndex = -1, greenIndex = -1, blueIndex = -1;


        public MainWindow()
        {
            InitializeComponent();
            paths = new ObservableCollection<string>();
            imageList.DataContext = paths;

        }

        private void CompileButton_Click(object sender, RoutedEventArgs e)
        {
            //make the width and height flexible

            if (paths.Count > 2 && redIndex > -1 && greenIndex > -1 && blueIndex > -1)
            {

                Device d = new Device(SharpDX.Direct3D.DriverType.Hardware);

                ImageLoadInformation loadInfo = new ImageLoadInformation()
                {
                    BindFlags = BindFlags.None,
                    CpuAccessFlags = CpuAccessFlags.Read,
                    Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                    OptionFlags = ResourceOptionFlags.None,
                    Usage = ResourceUsage.Staging
                };
                

                Texture2D red, green, blue,final;

                red = Texture2D.FromFile<Texture2D>(d,paths[redIndex],loadInfo);
                green = Texture2D.FromFile<Texture2D>(d, paths[greenIndex],loadInfo);
                blue = Texture2D.FromFile<Texture2D>(d, paths[blueIndex],loadInfo);

                final = new Texture2D(d, new Texture2DDescription() 
                {
                    ArraySize = 1,
                    BindFlags = BindFlags.None,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm,
                    Height = 3456,
                    MipLevels = 0,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new SharpDX.DXGI.SampleDescription(1,0),
                    Usage = ResourceUsage.Staging,
                    Width = 4608                
                });


                DataBox redDataBox = d.ImmediateContext.MapSubresource(red,0,MapMode.Read,MapFlags.None);
                DataBox greenDataBox = d.ImmediateContext.MapSubresource(green, 0, MapMode.Read, MapFlags.None);
                DataBox blueDataBox = d.ImmediateContext.MapSubresource(blue, 0, MapMode.Read, MapFlags.None);
                DataBox finalDataBox = d.ImmediateContext.MapSubresource(final, 0, MapMode.Write, MapFlags.None);


                byte[] 
                    redData = new byte[redDataBox.RowPitch * red.Description.Height],
                    greenData = new byte[greenDataBox.RowPitch * green.Description.Height],
                    blueData =new byte[blueDataBox.RowPitch * blue.Description.Height],
                    finalData = new byte[finalDataBox.RowPitch * final.Description.Height];

                
                Utilities.Read(redDataBox.DataPointer, redData, 0, redData.Length);
                Utilities.Read(greenDataBox.DataPointer, greenData, 0, greenData.Length);
                Utilities.Read(blueDataBox.DataPointer, blueData, 0, blueData.Length);
                
                


                for (int x = 0; x < final.Description.Width; x++)
                {
                    for (int y = 0; y < final.Description.Height; y++)
                    {
                        int index = (x*4) + (y * finalDataBox.RowPitch);

                        finalData[index]     = redData[index];
                        finalData[index + 1] = greenData[index];
                        finalData[index + 2] = blueData[index];
                        finalData[index + 3] = 255;//A

                    }
                }

                Utilities.Write(finalDataBox.DataPointer, finalData, 0, finalData.Length);

                d.ImmediateContext.UnmapSubresource(red, 0);
                d.ImmediateContext.UnmapSubresource(green, 0);
                d.ImmediateContext.UnmapSubresource(blue, 0);
                d.ImmediateContext.UnmapSubresource(final, 0);

                Texture2D.ToFile(d.ImmediateContext, final, ImageFileFormat.Png, "C:\\dan\\projectmedia\\BumpMap\\final.png");

                

                final.Dispose();
                red.Dispose();
                green.Dispose();
                blue.Dispose();
                d.Dispose();
            }
            else MessageBox.Show("must choose at least 3 files and select 3 to be the red green and blue channels");
        }

        private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "Image Files(png,jpeg,bmp)|*.png;*.jpg;*.bmp";
            ofd.Multiselect = true;
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Title = "Choose 3 images";

            if (ofd.ShowDialog() ?? false)
            {
                foreach (string path in ofd.FileNames)
                {
                    paths.Add(path);
                }
            }
            
        }

        private void chooseRed_Click(object sender, RoutedEventArgs e)
        {
            if (imageList.SelectedIndex > -1)
            {
                redIndex = imageList.SelectedIndex;
            }

        }


        private void chooseGreen_Click(object sender, RoutedEventArgs e)
        {
            if (imageList.SelectedIndex > -1)
            {
                greenIndex = imageList.SelectedIndex;
            }
        }

        private void chooseBlue_Click(object sender, RoutedEventArgs e)
        {
            if (imageList.SelectedIndex > -1)
            {
                blueIndex = imageList.SelectedIndex;
            }
        }


    }
}
