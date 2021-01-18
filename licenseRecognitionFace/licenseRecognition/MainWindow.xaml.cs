               using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
//using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

using Microsoft.Win32;
using System.Drawing; //需要在解决方案的引用中添加“System.Drawing”
using System.Drawing.Imaging;
using System.IO;

namespace licenseRecognition
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Bitmap m_Bitmap;//存放最先打开的图片 // 需要在解决方案的引用中添加“System.Drawing” 
        private Bitmap c_Bitmap; //车牌图像

        private Bitmap always_Bitmap;//永远是彩色图片，用以备用
        private Bitmap other_c_Bitmap;
        private Bitmap extract_Bitmap_one;
        private Bitmap extract_Bitmap_two;
        private Bitmap z_Bitmap0;
        private Bitmap z_Bitmap1;
        private Bitmap z_Bitmap2;
        private Bitmap z_Bitmap3;
        private Bitmap z_Bitmap4;
        private Bitmap z_Bitmap5;
        private Bitmap z_Bitmap6;
        //private Bitmap z_Bitmap7;
        private Bitmap objNewPic;
       

        private BitmapData bmData;

        private Bitmap[] z_Bitmaptwo = new Bitmap[7];//用于储存最终的黑白字体

        private Bitmap[] charFont;
        private Bitmap[] provinceFont;
        string[] charString;//存储的路径
        string[] provinceString;//省份字体
        string[] charDigitalString;
        string[] provinceDigitalString;
        System.Drawing.Pen pen1 = new System.Drawing.Pen(System.Drawing.Color.Black);
        private String name;  // pictureName;
        private float count;
        private float[] gl = new float[256];
        int[] gray = new int[256]; //灰度化
        int[] rr = new int[256];
        int[] gg = new int[256];
        int[] bb = new int[256];

        int[] XLeft = new int[20];
        int[] XRight = new int[20];
        int Yheight, YBottom;

        float[,] m = new float[5000, 5000];
        int flag = 0, flag1 = 0;
        int Yr, Yl;
        int xx = -1;
        //private bool aline = false;
        public static string SourceBathOne = "G:\\licensePlate\\";//备用
        public static string charSourceBath = "MYsource\\char\\";
        public static string provinceSourceBath = "MYsource\\font\\";

        public MainWindow()
        {
            InitializeComponent();
        }

        private byte getMin(byte a, byte b, byte c)
        {
            byte ans;
            if (a > b)
                ans = a;
            else
                ans = b;

            if (ans < c)
                ans = c;

            return ans;
        }
        private byte getMax(byte a, byte b, byte c)
        {
            byte ans;
            if (a < b)
                ans = a;
            else
                ans = b;

            if (ans > c)
                ans = c;

            return ans;
        }

        //打开图片
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {   
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Jpeg文件（*.jpg）|*.jpg|Bitamp文件(*.bmp)|*.bmp|所有合适文件（*.bmp/*.jpg）|*.bmp/*/jpg";
            openFileDialog.FilterIndex = 2;
            openFileDialog.RestoreDirectory = true;

            if(openFileDialog.ShowDialog() == true)
            {
                name = openFileDialog.FileName;
                m_Bitmap = (Bitmap)Bitmap.FromFile(name, false);
                this.always_Bitmap = m_Bitmap.Clone(new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height),PixelFormat.DontCare);

                IntPtr ip = m_Bitmap.GetHbitmap();
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                imgLoad.Source = bitmapSource;
            }
        }
//保存图片
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Bitmap文件(*.bmp)|*.bmp| Jpeg文件(*.jpg)|*.jpg| 所有合适文件(*.bmp/*.jpg)|*.bmp/*.jpg";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == true)
            {
                c_Bitmap.Save(saveFileDialog.FileName);
            }

        }

//整个图片灰度化
        private void btnPictureGray_Click(object sender, RoutedEventArgs e)
        { 
            if (m_Bitmap != null)
            {
                int tt = 0;
                for (int i = 0; i < 256; i++)//清掉数组gray里的数据
                    gray[i] = 0;
                for (int i = 0; i < 256; i++)//清掉数组rr里的数据
                    rr[i] = 0;
                for (int i = 0; i < 256; i++)//清掉数组gg里的数据
                    gg[i] = 0;
                for (int i = 0; i < 256; i++)//清掉数组bb里的数据
                    bb[i] = 0;

                BitmapData bmData = m_Bitmap.LockBits(new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                int stride = bmData.Stride;//获取或设置 Bitmap 对象的跨距宽度（也称为扫描宽度）。 
                System.IntPtr Scan0 = bmData.Scan0;//获取或设置位图中第一个像素数据的地址。 它也可以看成是位图中的第一个扫描行
                unsafe  //"生成"选择"允许不安全代码" 
                {
                    byte* p = (byte*)(void*)Scan0;
                    int nOffset = stride - m_Bitmap.Width * 3;
                    byte red, green, blue;
                    int nWidth = m_Bitmap.Width;
                    int nHeight = m_Bitmap.Height;
                    for (int y = 0; y < nHeight; ++y)
                    {
                        for (int x = 0; x < nWidth; ++x)
                        {
                            blue = p[0];
                            green = p[1];
                            red = p[2];

                            // 加权平均值法
                            // tt = p[0] = p[1] = p[2] = (byte)(.299 * red + .587 * green + .114 * blue);
                            //平均值法
                            //tt = p[0] = p[1] = p[2] = (byte)((red + green + blue)/3);
                            //最小值法
                            // tt = p[0] = p[1] = p[2] = (byte)(getMin(red, green , blue));
                            //最大值法
                            // tt = p[0] = p[1] = p[2] = (byte)(getMax(red, green, blue));
                            //去饱和
                             tt = p[0] = p[1] = p[2] = (byte)((getMin(red, green, blue)+getMax(red, green, blue))/2);

                            rr[red]++;
                            gg[green]++;
                            bb[blue]++;
                            gray[tt]++;   //统计灰度值为tt的象素点数目  
                            p += 3;
                        }
                        p += nOffset;
                    }
                }

                m_Bitmap.UnlockBits(bmData);
                count = m_Bitmap.Width * m_Bitmap.Height;
                IntPtr ip = m_Bitmap.GetHbitmap();//将Bitmap转换为BitmapSource
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                imgLoad.Source = bitmapSource;
            }
        }


//传统直方图均衡化 -- 灰度均衡化
        private void btnGrayScales_Click(object sender, RoutedEventArgs e)
        { 
            if (m_Bitmap != null)
            {
                BitmapData bmData = m_Bitmap.LockBits(new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                //加入内存进行处理
                int stride = bmData.Stride;
                System.IntPtr Scan0 = bmData.Scan0;//扫描的第一行
                int tt = 0;
                int[] SumGray = new int[256];

                for (int i = 0; i < 256; i++)
                {
                    SumGray[i] = 0;
                }
                unsafe
                {
                    byte* p = (byte*)(void*)Scan0;
                    int nOffset = stride - m_Bitmap.Width * 3;
                    int nHeight = m_Bitmap.Height;
                    int nWidth = m_Bitmap.Width;
                    SumGray[0] = gray[0];//灰度均衡化 
                    for (int i = 1; i < 256; ++i)//灰度级频度数累加                         
                        SumGray[i] = SumGray[i - 1] + gray[i];
                    for (int i = 0; i < 256; ++i) //计算调整灰度值     频率乘以灰度总级数得出该灰度变换后的灰度级                   
                        SumGray[i] = (int)(SumGray[i] * 255 / count);
                    for (int i = 0; i < 256; i++)
                        gray[i] = 0;

                    for (int y = 0; y < nHeight; ++y)
                    {
                        for (int x = 0; x < nWidth; ++x)
                        {
                            tt = p[0] = p[1] = p[2] = (byte)(SumGray[p[0]]);
                            gray[tt]++;
                            p += 3;
                        }
                        p += nOffset;
                    }
                }
                m_Bitmap.UnlockBits(bmData);

                IntPtr ip = m_Bitmap.GetHbitmap();//将Bitmap转换为BitmapSource
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                imgLoad.Source = bitmapSource;
            }

        }

// 高斯平滑滤波滤波去噪
        private void btnMedianFilter_Click(object sender, RoutedEventArgs e)
        {
            BitmapData bmData = m_Bitmap.LockBits(new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            for (int i = 0; i < 256; i++)
            {
                gray[i] = 0;
            }
            unsafe
            {
                int stride = bmData.Stride;
                System.IntPtr Scan0 = bmData.Scan0;
                byte* p = (byte*)(void*)Scan0;
                byte* pp;
                int tt;
                int nOffset = stride - m_Bitmap.Width * 3;
                int nWidth = m_Bitmap.Width;
                int nHeight = m_Bitmap.Height;
                long sum = 0;
                int[,] gaussianMatrix = { { 1, 2, 3, 2, 1 }, { 2, 4, 6, 4, 2 }, { 3, 6, 7, 6, 3 }, { 2, 4, 6, 4, 2 }, { 1, 2, 3, 2, 1 } };//高斯滤波器所选的n=5模板
                for (int y = 0; y < nHeight; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        if (!(x <= 1 || x >= nWidth - 2 || y <= 1 || y >= nHeight - 2))
                        {
                            pp = p;
                            sum = 0;
                            int dividend = 79;
                            for (int i = -2; i <= 2; i++)
                                for (int j = -2; j <= 2; j++)
                                {
                                    pp += (j * 3 + stride * i);
                                    sum += pp[0] * gaussianMatrix[i + 2, j + 2];
                                    if (i == 0 && j == 0)
                                    {
                                        if (pp[0] > 240)//如果模板中心的灰度大于240
                                        {
                                            sum += p[0] * 30;
                                            dividend += 30;
                                        }
                                        else if (pp[0] > 230)
                                        {
                                            sum += pp[0] * 20;
                                            dividend += 20;
                                        }
                                        else if (pp[0] > 220)
                                        {
                                            sum += p[0] * 15;
                                            dividend += 15;
                                        }
                                        else if (pp[0] > 210)
                                        {
                                            sum += pp[0] * 10;
                                            dividend += 10;
                                        }
                                        else if (p[0] > 200)
                                        {
                                            sum += pp[0] * 5;
                                            dividend += 5;
                                        }
                                    }
                                    pp = p;
                                }
                            sum = sum / dividend;
                            if (sum > 255)
                                sum = 255;
                            
                            p[0] = p[1] = p[2] = (byte)(sum);
                        }
                        tt = p[0];
                        gray[tt]++;
                        p += 3;
                    }
                    p += nOffset;
                }
            }
            m_Bitmap.UnlockBits(bmData);
            IntPtr ip = m_Bitmap.GetHbitmap();//将Bitmap转换为BitmapSource
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            imgLoad.Source = bitmapSource;

        }

 //边缘检测
        private void btnEdgeDetection_Click(object sender, RoutedEventArgs e)
        {
            //
            BitmapData bmData = m_Bitmap.LockBits(new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            float valve = 67;
            for (int i = 0; i < 256; i++)
                gray[i] = 0;
          
            unsafe
            {
                int stride = bmData.Stride;
                System.IntPtr Scan0 = bmData.Scan0;
                byte* p = (byte*)(void*)Scan0;
                byte* pp;
                int tt;
                int nOffset = stride - m_Bitmap.Width * 3;
                int nWidth = m_Bitmap.Width;
                int nHeight = m_Bitmap.Height;
                int Sx = 0;
                int Sy = 0;
                //	float max = 0;
                double sumM = 0;
                double sumCount = 0;
                int[] marginalMx = { -1, 0, 1, -2, 0, 2, -1, 0, 1 };   //sobel模板
                int[] marginalMy = { 1, 2, 1, 0, 0, 0, -1, -2, -1 };
                int[,] dlta = new int[nHeight, nWidth];
                for (int y = 0; y < nHeight; ++y)      //sobel算子
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        if (!(x <= 0 || x >= nWidth - 1 || y <= 0 || y >= nHeight - 1))
                        {
                            pp = p;
                            Sx = 0;
                            Sy = 0;
                            for (int i = -1; i <= 1; i++)
                                for (int j = -1; j <= 1; j++)
                                {
                                    pp += (j * 3 + stride * i);
                                    Sx += pp[0] * marginalMx[(i + 1) * 3 + j + 1];
                                    Sy += pp[0] * marginalMy[(i + 1) * 3 + j + 1];
                                    pp = p;
                                }
                            m[y, x] = (int)(Math.Sqrt(Sx * Sx + Sy * Sy));
                            if (m[y, x] > valve / 2) //增强白点
                            {
                                if (p[0] > 240)
                                {
                                    m[y, x] += valve;
                                }
                                else if (p[0] > 220)
                                {
                                    m[y, x] += (float)(valve * 0.8);
                                }
                                else if (p[0] > 200)
                                {
                                    m[y, x] += (float)(valve * 0.6);
                                }
                                else if (p[0] > 180)
                                {
                                    m[y, x] += (float)(valve * 0.4);
                                }
                                else if (p[0] > 160)
                                {
                                    m[y, x] += (float)(valve * 0.2);
                                }
                            }
                            float tan;
                            if (Sx != 0)
                            {
                                tan = Sy / Sx;
                            }
                            else tan = 10000;
                            if (-0.41421356 <= tan && tan < 0.41421356)//角度为-22.5度到22.5度之间
                            {
                                dlta[y, x] = 0; //	m[y,x]+=valve;                                 
                            }
                            else if (0.41421356 <= tan && tan < 2.41421356)//角度为22.5度到67.5度之间
                            {
                                dlta[y, x] = 1; //m[y,x] = 0;
                            }
                            else if (tan >= 2.41421356 || tan < -2.41421356)//角度为67.5度到90度之间或-90度到-67.5度
                            {
                                dlta[y, x] = 2;    //	m[y,x]+=valve;
                            }
                            else
                            {
                                dlta[y, x] = 3;//m[y,x] = 0;     
                            }
                        }
                        else
                            m[y, x] = 0;
                        p += 3;
                        if (m[y, x] > 0)
                        {
                            sumCount++;
                            sumM += m[y, x];
                        }
                    }
                    p += nOffset;
                }
                unsafe
                {
                    p = (byte*)(void*)Scan0; //非极大值抑制和阀值
                    for (int y = 0; y < nHeight; ++y)
                    {
                        for (int x = 0; x < nWidth; ++x)
                        {
                            if (m[y, x] > sumM / sumCount * 1.2)
                            {
                                p[0] = p[1] = p[2] = (byte)(m[y, x]); //m[y,x]=1;                            
                            }
                            else
                            {
                                m[y, x] = 0;
                                p[0] = p[1] = p[2] = 0;
                            }
                            if (x >= 1 && x <= nWidth - 1 && y >= 1 && y <= nHeight - 1 && m[y, x] > valve)
                            {
                                switch (dlta[y, x])
                                {
                                    case 0:
                                        if (m[y, x] >= m[y, x - 1] && m[y, x] >= m[y, x + 1])//水平边缘
                                        {
                                            p[0] = p[1] = p[2] = 255;
                                        }
                                        break;

                                    case 1:
                                        if (m[y, x] >= m[y + 1, x - 1] && m[y, x] >= m[y - 1, x + 1])//正斜45度边缘
                                        {
                                            p[0] = p[1] = p[2] = 255;
                                        }
                                        break;

                                    case 2:
                                        if (m[y, x] >= m[y - 1, x] && m[y, x] >= m[y + 1, x])//垂直边缘
                                        {
                                            p[0] = p[1] = p[2] = 255;
                                        }
                                        break;

                                    case 3:
                                        if (m[y, x] >= m[y + 1, x + 1] && m[y, x] >= m[y - 1, x - 1])//反斜45度边缘
                                        {
                                            p[0] = p[1] = p[2] = 255;
                                        }
                                        break;
                                }
                            }
                            if (p[0] == 255)
                            {
                                m[y, x] = 1;
                            }
                            else
                            {
                                m[y, x] = 0;
                                p[0] = p[1] = p[2] = 0;
                            }

                            tt = p[0];
                            gray[tt]++;
                            p += 3;
                        }
                        //  p += nOffset;
                    }
                    m_Bitmap.UnlockBits(bmData);
                    IntPtr ip = m_Bitmap.GetHbitmap();//将Bitmap转换为BitmapSource
                    BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    imgLoad.Source = bitmapSource;
                }
            }
        }

 //车牌定位
        private void btnLocation_Click(object sender, RoutedEventArgs e)
        {
            //  Recoginzation re = new Recoginzation();
            this.c_Bitmap = Recoginzation.licensePlateLocation(m_Bitmap, always_Bitmap, m);
            extract_Bitmap_one = c_Bitmap.Clone(new Rectangle(0, 0, c_Bitmap.Width, c_Bitmap.Height), PixelFormat.DontCare);


            IntPtr ip = m_Bitmap.GetHbitmap();//将Bitmap转换为BitmapSource
            BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            imgLoad.Source = bitmapSource;

            IntPtr ip2 = c_Bitmap.GetHbitmap();//将Bitmap转换为BitmapSource
            BitmapSource bitmapSource2 = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip2, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            imgLicence.Source = bitmapSource2;


        }

 //车牌灰度化
        private void btnLicenceGray_Click1(object sender, RoutedEventArgs e)
        {
            if (m_Bitmap != null)
            {
                int tt = 0;
                for (int i = 0; i < 256; i++)//清掉数组gray里的数据
                    gray[i] = 0;
                for (int i = 0; i < 256; i++)//清掉数组rr里的数据
                    rr[i] = 0;
                for (int i = 0; i < 256; i++)//清掉数组gg里的数据
                    gg[i] = 0;
                for (int i = 0; i < 256; i++)//清掉数组bb里的数据
                    bb[i] = 0;

                BitmapData bmData = m_Bitmap.LockBits(new Rectangle(0, 0, m_Bitmap.Width, m_Bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                int stride = bmData.Stride;//获取或设置 Bitmap 对象的跨距宽度（也称为扫描宽度）。 
                System.IntPtr Scan0 = bmData.Scan0;//获取或设置位图中第一个像素数据的地址。 它也可以看成是位图中的第一个扫描行
                unsafe  //"生成"选择"允许不安全代码" 
                {
                    byte* p = (byte*)(void*)Scan0;
                    int nOffset = stride - m_Bitmap.Width * 3;
                    byte red, green, blue;
                    int nWidth = m_Bitmap.Width;
                    int nHeight = m_Bitmap.Height;
                    for (int y = 0; y < nHeight; ++y)
                    {
                        for (int x = 0; x < nWidth; ++x)
                        {
                            blue = p[0];
                            green = p[1];
                            red = p[2];
                            tt = p[0] = p[1] = p[2] = (byte)(.299 * red + .587 * green + .114 * blue);
                            rr[red]++;
                            gg[green]++;
                            bb[blue]++;
                            gray[tt]++;   //统计灰度值为tt的象素点数目  
                            p += 3;
                        }
                        p += nOffset;
                    }
                }

                m_Bitmap.UnlockBits(bmData);
                count = m_Bitmap.Width * m_Bitmap.Height;
                IntPtr ip = m_Bitmap.GetHbitmap();//将Bitmap转换为BitmapSource
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                imgLoad.Source = bitmapSource;
            }

        }
        private void btnLicenceGray_Click(object sender, RoutedEventArgs e)
        {
            if (c_Bitmap != null)
            {
                int tt = 0;
                for (int i = 0; i < 256; i++)//清掉数组gray里的数据
                    gray[i] = 0;
                for (int i = 0; i < 256; i++)//清掉数组rr里的数据
                    rr[i] = 0;
                for (int i = 0; i < 256; i++)//清掉数组gg里的数据
                    gg[i] = 0;
                for (int i = 0; i < 256; i++)//清掉数组bb里的数据
                    bb[i] = 0;

                BitmapData bmData = c_Bitmap.LockBits(new Rectangle(0, 0, c_Bitmap.Width, c_Bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                int stride = bmData.Stride;//获取或设置 Bitmap 对象的跨距宽度（也称为扫描宽度）。 
                System.IntPtr Scan0 = bmData.Scan0;//获取或设置位图中第一个像素数据的地址。 它也可以看成是位图中的第一个扫描行
                unsafe  //"生成"选择"允许不安全代码" 
                {
                    byte* p = (byte*)(void*)Scan0;
                    int nOffset = stride - c_Bitmap.Width * 3;
                    byte red, green, blue;
                    int nWidth = c_Bitmap.Width;
                    int nHeight = c_Bitmap.Height;
                    for (int y = 0; y < nHeight; ++y)
                    {
                        for (int x = 0; x < nWidth; ++x)
                        {
                            blue = p[0];
                            green = p[1];
                            red = p[2];
                            tt = p[0] = p[1] = p[2] = (byte)(.299 * red + .587 * green + .114 * blue);
                            rr[red]++;
                            gg[green]++;
                            bb[blue]++;
                            gray[tt]++;   //统计灰度值为tt的象素点数目  
                            p += 3;
                        }
                        p += nOffset;
                    }
                }

                c_Bitmap.UnlockBits(bmData);
                count = c_Bitmap.Width * c_Bitmap.Height;
                IntPtr ip = c_Bitmap.GetHbitmap();//将Bitmap转换为BitmapSource
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                imgLoad.Source = bitmapSource;
            }

        }
 
//车牌切割
       private void LicenceSplit(object sender, RoutedEventArgs e)
        {
            flag1 = 1;
            bmData = c_Bitmap.LockBits(new Rectangle(0, 0, c_Bitmap.Width, c_Bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            //现在的c_Bitmap是车牌图片
            unsafe
            {
                int stride = bmData.Stride;
                System.IntPtr Scan0 = bmData.Scan0;
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - c_Bitmap.Width * 3;

                int nWidth = c_Bitmap.Width;
                int nHeight = c_Bitmap.Height;
                int[] countHeight = new int[nHeight];
                int[] countWidth = new int[nWidth];
                Yheight = nHeight;
                YBottom = 0;

                for (int i = 0; i < nHeight; i++)
                {
                    countHeight[i] = 0;
                }
                for (int y = 0; y < nHeight; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        if ((p[0] == 0 && p[3] == 255) || (p[0] == 255 && p[3] == 0))
                            countHeight[y]++;

                        p += 3;
                    }
                    Console.WriteLine(y + "*******************跳变数     " + countHeight[y]);
                    p += nOffset;
                }

                //计算车牌号的上边缘
                for (int y = nHeight / 2; y > 0; y--)
                {
                    if (countHeight[y] >= 16 && countHeight[(y + 1) % nHeight] >= 12)//12,6,11
                    {
                        if (Yheight > y)
                        {
                            Yheight = y;
                            //YBottom = y;
                        }//此if语句只执行了好多次
                        if ((Yheight - y) == 1)
                        { Yheight = y - 3; Console.WriteLine("------------" + Yheight); }
                    }
                    //Console.WriteLine("现在图片的顶部是：" + Yheight);

                }
                //计算车牌号的下边缘
                for (int y = nHeight / 2; y < nHeight; y++)
                {
                    if (countHeight[y] >= 12 && countHeight[(y + 1) % nHeight] >= 12)//12,6,11
                    {
                        if (YBottom < y)
                        {
                            YBottom = y;
                            //YBottom = y;
                        }//此if语句只执行了一次
                        if ((y - YBottom) == 1)
                        { YBottom = y + 3; Console.WriteLine("------------" + YBottom); }
                    }
                    // Console.WriteLine("NOW图片的底部是：" + YBottom);

                }
                YBottom += 1;//主要目的是由于计算时少算了1
                byte* p1 = (byte*)(void*)Scan0;
                p1 += stride * (Yheight - 1);
                for (int y = Yheight; y < YBottom; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {

                        if (p1[0] == 255)
                            countWidth[x]++;

                        p1 += 3;
                    }
                    p1 += nOffset;
                }
                int contg = 0, contd = 0, countRightEdge = 0, countLeftEdge = 0;
                Yl = nWidth;
                Yr = 0;
                //int[] XLeft = new int[20];
                //int[] XRight = new int[20];
                foreach (int i in XRight)
                {
                    XRight[i] = 0;
                }
                foreach (int i in XLeft)
                {
                    XLeft[i] = 0;
                }
                for (int y = 1; y < YBottom - Yheight; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        if (countWidth[(x + 1) % nWidth] < y && countWidth[x] >= y && countWidth[Math.Abs((x - 1) % nWidth)] >= y && contg >= 2)
                        {
                            //if (countWidth[(x + 1)] < y && countWidth[x] >= y && countWidth[Math.Abs((x - 1) )] >= y && contg >= 2)
                            //{
                            if (countRightEdge == 6)
                            { Yr = x; }
                            if ((countRightEdge == 2 && (x >= XLeft[2] && XLeft[2] > 0)))
                            {
                                XRight[countRightEdge] = x;
                                countRightEdge++;
                                contd = 0;
                            }
                            else
                            {
                                if ((countRightEdge != 2))
                                {
                                    if (countRightEdge == 0 && contg < 4)
                                    {
                                        XLeft[0] = 0;
                                        countLeftEdge = 0;
                                    }
                                    if ((x >= XLeft[0] && XLeft[0] > 0))
                                    {
                                        XRight[countRightEdge] = x;
                                        countRightEdge++;
                                        contd = 0;
                                    }
                                }
                            }
                        }
                        if (countWidth[Math.Abs((x - 1) % nWidth)] < y && countWidth[x] >= y && countWidth[(x + 1) % nWidth] >= y && contd >= 2)
                        {
                            if (countLeftEdge == 0 && countWidth[(x + 2) % nWidth] >= y)
                            { Yl = x; }
                            if ((countLeftEdge == 2 && contd > 5))
                            {
                                XLeft[countLeftEdge] = x;
                                countLeftEdge++;
                            }
                            else
                            {
                                if ((countLeftEdge != 2))
                                {
                                    XLeft[countLeftEdge] = x;
                                    countLeftEdge++;
                                    contg = 0;
                                    if (countLeftEdge == 0 && countWidth[(x + 2) % nWidth] < y)
                                    {
                                        XLeft[0] = 0;
                                        countLeftEdge = 0;
                                    }

                                }
                            }
                        }
                        contg++;
                        contd++;

                    }
                    if (countRightEdge + countLeftEdge >= 14)
                        break;

                    countRightEdge = 0;
                    countLeftEdge = 0;
                    for (int i = 0; i < XRight.Length; i++)
                    {
                        XRight[i] = 0;
                    }
                    for (int i = 0; i < XLeft.Length; i++)
                    {
                        XLeft[i] = 0;
                    }
                }
            }

        }

//车牌二值化

        private int getYu1() //灰度平均值
        {
            int sum = 0;
            int count = 0;
            for (int i = 0; i < 256; i++)//像素个数与灰度等级的乘积除以像素个数
            {
                sum += gray[i] * i;
                count += gray[i];
            }
            return sum / count;
        }

        private int getYu2()//百分比阈值
        {

            int tile = 50;
            int sum = 0, count = 0;
            for (int i = 0; i < 256; i++)
                count += gray[i];
            for (int i = 0; i < 256; i++)
            {
                sum = sum + gray[i];
                if (sum >= count * tile / 100)
                    return i;
            }
            return -1;
        }

        private int getYu3()   //基于谷底最小值的阈值
        {
            int Y, Iter = 0;
            double[] HistGramC = new double[256];           // 基于精度问题，一定要用浮点数来处理，否则得不到正确的结果
            double[] HistGramCC = new double[256];          // 求均值的过程会破坏前面的数据，因此需要两份数据
            for (Y = 0; Y < 256; Y++)
            {
                HistGramC[Y] = gray[Y];
                HistGramCC[Y] = gray[Y];
            }

            // 通过三点求均值来平滑直方图
            while (IsDimodal(HistGramCC) == false)                                        // 判断是否已经是双峰的图像了      
            {
                HistGramCC[0] = (HistGramC[0] + HistGramC[0] + HistGramC[1]) / 3;                 // 第一点
                for (Y = 1; Y < 255; Y++)
                    HistGramCC[Y] = (HistGramC[Y - 1] + HistGramC[Y] + HistGramC[Y + 1]) / 3;     // 中间的点
                HistGramCC[255] = (HistGramC[254] + HistGramC[255] + HistGramC[255]) / 3;         // 最后一点
                System.Buffer.BlockCopy(HistGramCC, 0, HistGramC, 0, 256 * sizeof(double));
                Iter++;
                if (Iter >= 1000) return -1;                                                   // 直方图无法平滑为双峰的，返回错误代码
            }
            // 阈值极为两峰之间的最小值 
            bool Peakfound = false;
            for (Y = 1; Y < 255; Y++)
            {
                if (HistGramCC[Y - 1] < HistGramCC[Y] && HistGramCC[Y + 1] < HistGramCC[Y]) Peakfound = true;
                if (Peakfound == true && HistGramCC[Y - 1] >= HistGramCC[Y] && HistGramCC[Y + 1] >= HistGramCC[Y])
                    return Y - 1;
            }
            return -1;
        }

        private static bool IsDimodal(double[] HistGram)       // 检测直方图是否为双峰的
        {
            // 对直方图的峰进行计数，只有峰数位2才为双峰 
            int Count = 0;
            for (int Y = 1; Y < 255; Y++)
            {
                if (HistGram[Y - 1] < HistGram[Y] && HistGram[Y + 1] < HistGram[Y])
                {
                    Count++;
                    if (Count > 2) return false;
                }
            }
            if (Count == 2)
                return true;
            else
                return false;
        }

        private int getYu4()  //基于双峰平均值的阈值
        {
            int Y, Iter = 0, Index;
            double[] HistGramC = new double[256];           // 基于精度问题，一定要用浮点数来处理，否则得不到正确的结果
            double[] HistGramCC = new double[256];          // 求均值的过程会破坏前面的数据，因此需要两份数据
            for (Y = 0; Y < 256; Y++)
            {
                HistGramC[Y] = gray[Y];
                HistGramCC[Y] = gray[Y];
            }
            // 通过三点求均值来平滑直方图
            while (IsDimodal(HistGramCC) == false)                                                  // 判断是否已经是双峰的图像了      
            {
                HistGramCC[0] = (HistGramC[0] + HistGramC[0] + HistGramC[1]) / 3;                   // 第一点
                for (Y = 1; Y < 255; Y++)
                    HistGramCC[Y] = (HistGramC[Y - 1] + HistGramC[Y] + HistGramC[Y + 1]) / 3;       // 中间的点
                HistGramCC[255] = (HistGramC[254] + HistGramC[255] + HistGramC[255]) / 3;           // 最后一点
                System.Buffer.BlockCopy(HistGramCC, 0, HistGramC, 0, 256 * sizeof(double));         // 备份数据，为下一次迭代做准备
                Iter++;
                if (Iter >= 10000) return -1;                                                       // 似乎直方图无法平滑为双峰的，返回错误代码
            }
            // 阈值为两峰值的平均值
            int[] Peak = new int[2];
            for (Y = 1, Index = 0; Y < 255; Y++)
                if (HistGramCC[Y - 1] < HistGramCC[Y] && HistGramCC[Y + 1] < HistGramCC[Y]) Peak[Index++] = Y - 1;
            return ((Peak[0] + Peak[1]) / 2);
        }

        private int getYu5()  //迭代最佳阈值
        {
            int X, Iter = 0;
            int MeanValueOne, MeanValueTwo, SumOne, SumTwo, SumIntegralOne, SumIntegralTwo;
            int MinValue, MaxValue;
            int Threshold, NewThreshold;

            for (MinValue = 0; MinValue < 256 && gray[MinValue] == 0; MinValue++) ;
            for (MaxValue = 255; MaxValue > MinValue && gray[MinValue] == 0; MaxValue--) ;

            if (MaxValue == MinValue) return MaxValue;          // 图像中只有一个颜色             
            if (MinValue + 1 == MaxValue) return MinValue;      // 图像中只有二个颜色

            Threshold = MinValue;
            NewThreshold = (MaxValue + MinValue) >> 1;
            while (Threshold != NewThreshold)    // 当前后两次迭代的获得阈值相同时，结束迭代    
            {
                SumOne = 0; SumIntegralOne = 0;
                SumTwo = 0; SumIntegralTwo = 0;
                Threshold = NewThreshold;
                for (X = MinValue; X <= Threshold; X++)         //根据阈值将图像分割成目标和背景两部分，求出两部分的平均灰度值      
                {
                    SumIntegralOne += gray[X] * X;
                    SumOne += gray[X];
                }
                MeanValueOne = SumIntegralOne / SumOne;
                for (X = Threshold + 1; X <= MaxValue; X++)
                {
                    SumIntegralTwo += gray[X] * X;
                    SumTwo += gray[X];
                }
                MeanValueTwo = SumIntegralTwo / SumTwo;
                NewThreshold = (MeanValueOne + MeanValueTwo) >> 1;       //求出新的阈值
                Iter++;
                if (Iter >= 1000) return -1;
            }
            return Threshold;
        }

        private int getYu6()   //OSTU大律法
        {
            int X, Y, Amount = 0;
            int PixelBack = 0, PixelFore = 0, PixelIntegralBack = 0, PixelIntegralFore = 0, PixelIntegral = 0;
            double OmegaBack, OmegaFore, MicroBack, MicroFore, SigmaB, Sigma;              // 类间方差;
            int MinValue, MaxValue;
            int Threshold = 0;

            for (MinValue = 0; MinValue < 256 && gray[MinValue] == 0; MinValue++) ;
            for (MaxValue = 255; MaxValue > MinValue && gray[MinValue] == 0; MaxValue--) ;
            if (MaxValue == MinValue) return MaxValue;          // 图像中只有一个颜色             
            if (MinValue + 1 == MaxValue) return MinValue;      // 图像中只有二个颜色

            for (Y = MinValue; Y <= MaxValue; Y++) Amount += gray[Y];        //  像素总数

            PixelIntegral = 0;
            for (Y = MinValue; Y <= MaxValue; Y++) PixelIntegral += gray[Y] * Y;
            SigmaB = -1;
            for (Y = MinValue; Y < MaxValue; Y++)
            {
                PixelBack = PixelBack + gray[Y];
                PixelFore = Amount - PixelBack;
                OmegaBack = (double)PixelBack / Amount;
                OmegaFore = (double)PixelFore / Amount;
                PixelIntegralBack += gray[Y] * Y;
                PixelIntegralFore = PixelIntegral - PixelIntegralBack;
                MicroBack = (double)PixelIntegralBack / PixelBack;
                MicroFore = (double)PixelIntegralFore / PixelFore;
                Sigma = OmegaBack * OmegaFore * (MicroBack - MicroFore) * (MicroBack - MicroFore);
                if (Sigma > SigmaB)
                {
                    SigmaB = Sigma;
                    Threshold = Y;
                }
            }
            return Threshold;
        }
        private int getYu7()  //一维最大熵
        {
            int X, Y, Amount = 0;
            double[] grayD = new double[256];
            double SumIntegral, EntropyBack, EntropyFore, MaxEntropy;
            int MinValue = 255, MaxValue = 0;
            int m = 0;

            for (MinValue = 0; MinValue < 256 && gray[MinValue] == 0; MinValue++) ;
            for (MaxValue = 255; MaxValue > MinValue && gray[MinValue] == 0; MaxValue--) ;
            if (MaxValue == MinValue) return MaxValue;          // 图像中只有一个颜色             
            if (MinValue + 1 == MaxValue) return MinValue;      // 图像中只有二个颜色

            for (Y = MinValue; Y <= MaxValue; Y++) Amount += gray[Y];        //  像素总数

            for (Y = MinValue; Y <= MaxValue; Y++) grayD[Y] = (double)gray[Y] / Amount + 1e-17;

            MaxEntropy = double.MinValue; ;
            for (Y = MinValue + 1; Y < MaxValue; Y++)
            {
                SumIntegral = 0;
                for (X = MinValue; X <= Y; X++) SumIntegral += grayD[X];
                EntropyBack = 0;
                for (X = MinValue; X <= Y; X++) EntropyBack += (-grayD[X] / SumIntegral * Math.Log(grayD[X] / SumIntegral));
                EntropyFore = 0;
                for (X = Y + 1; X <= MaxValue; X++) EntropyFore += (-grayD[X] / (1 - SumIntegral) * Math.Log(grayD[X] / (1 - SumIntegral)));
                if (MaxEntropy < EntropyBack + EntropyFore)
                {
                    m = Y;
                    MaxEntropy = EntropyBack + EntropyFore;
                }
            }
            return m;
        }

        private int getYu8()  //力矩保持法 
        {
            int X, Y, Index = 0, Amount = 0;
            double[] Avec = new double[256];
            double X2, X1, X0, Min;

            for (Y = 0; Y <= 255; Y++) Amount += gray[Y];        //  像素总数
            for (Y = 0; Y < 256; Y++) Avec[Y] = (double)A(gray, Y) / Amount;       // The threshold is chosen such that A(y,t)/A(y,n) is closest to x0.

            // The following finds x0.

            X2 = (double)(B(gray, 255) * C(gray, 255) - A(gray, 255) * D(gray, 255)) / (double)(A(gray, 255) * C(gray, 255) - B(gray, 255) * B(gray, 255));
            X1 = (double)(B(gray, 255) * D(gray, 255) - C(gray, 255) * C(gray, 255)) / (double)(A(gray, 255) * C(gray, 255) - B(gray, 255) * B(gray, 255));
            X0 = 0.5 - (B(gray, 255) / A(gray, 255) + X2 / 2) / Math.Sqrt(X2 * X2 - 4 * X1);

            for (Y = 0, Min = double.MaxValue; Y < 256; Y++)
            {
                if (Math.Abs(Avec[Y] - X0) < Min)
                {
                    Min = Math.Abs(Avec[Y] - X0);
                    Index = Y;
                }
            }
            return (byte)Index;
        }

        private static double A(int[] HistGram, int Index)
        {
            double Sum = 0;
            for (int Y = 0; Y <= Index; Y++)
                Sum += HistGram[Y];
            return Sum;
        }

        private static double B(int[] HistGram, int Index)
        {
            double Sum = 0;
            for (int Y = 0; Y <= Index; Y++)
                Sum += (double)Y * HistGram[Y];
            return Sum;
        }

        private static double C(int[] HistGram, int Index)
        {
            double Sum = 0;
            for (int Y = 0; Y <= Index; Y++)
                Sum += (double)Y * Y * HistGram[Y];
            return Sum;
        }

        private static double D(int[] HistGram, int Index)
        {
            double Sum = 0;
            for (int Y = 0; Y <= Index; Y++)
                Sum += (double)Y * Y * Y * HistGram[Y];
            return Sum;
        }

        public int getYu9()
        {
            int X, Y;
            int First, Last;
            int Threshold = -1;
            double BestEntropy = Double.MaxValue, Entropy;
            //   找到第一个和最后一个非0的色阶值
            for (First = 0; First < gray.Length && gray[First] == 0; First++) ;
            for (Last = gray.Length - 1; Last > First && gray[Last] == 0; Last--) ;
            if (First == Last) return First;                // 图像中只有一个颜色
            if (First + 1 == Last) return First;            // 图像中只有二个颜色

            // 计算累计直方图以及对应的带权重的累计直方图
            int[] S = new int[Last + 1];
            int[] W = new int[Last + 1];            // 对于特大图，此数组的保存数据可能会超出int的表示范围，可以考虑用long类型来代替
            S[0] = gray[0];
            for (Y = First > 1 ? First : 1; Y <= Last; Y++)
            {
                S[Y] = S[Y - 1] + gray[Y];
                W[Y] = W[Y - 1] + Y * gray[Y];
            }

            // 建立公式（4）及（6）所用的查找表
            double[] Smu = new double[Last + 1 - First];
            for (Y = 1; Y < Smu.Length; Y++)
            {
                double mu = 1 / (1 + (double)Y / (Last - First));               // 公式（4）
                Smu[Y] = -mu * Math.Log(mu) - (1 - mu) * Math.Log(1 - mu);      // 公式（6）
            }

            // 迭代计算最佳阈值
            for (Y = First; Y <= Last; Y++)
            {
                Entropy = 0;
                int mu = (int)Math.Round((double)W[Y] / S[Y]);             // 公式17
                for (X = First; X <= Y; X++)
                    Entropy += Smu[Math.Abs(X - mu)] * gray[X];
                mu = (int)Math.Round((double)(W[Last] - W[Y]) / (S[Last] - S[Y]));  // 公式18
                for (X = Y + 1; X <= Last; X++)
                    Entropy += Smu[Math.Abs(X - mu)] * gray[X];       // 公式8
                if (BestEntropy > Entropy)
                {
                    BestEntropy = Entropy;      // 取最小熵处为最佳阈值
                    Threshold = Y;
                }
            }
            return Threshold;
        }


        private int getYu10()   //Kittler最小错误分类法
        {
            int X, Y;
            int MinValue, MaxValue;
            int Threshold;
            int PixelBack, PixelFore;
            double OmegaBack, OmegaFore, MinSigma, Sigma, SigmaBack, SigmaFore;
            for (MinValue = 0; MinValue < 256 && gray[MinValue] == 0; MinValue++) ;
            for (MaxValue = 255; MaxValue > MinValue && gray[MinValue] == 0; MaxValue--) ;
            if (MaxValue == MinValue) return MaxValue;          // 图像中只有一个颜色             
            if (MinValue + 1 == MaxValue) return MinValue;      // 图像中只有二个颜色
            Threshold = -1;
            MinSigma = 1E+20;
            for (Y = MinValue; Y < MaxValue; Y++)
            {
                PixelBack = 0; PixelFore = 0;
                OmegaBack = 0; OmegaFore = 0;
                for (X = MinValue; X <= Y; X++)
                {
                    PixelBack += gray[X];
                    OmegaBack = OmegaBack + X * gray[X];
                }
                for (X = Y + 1; X <= MaxValue; X++)
                {
                    PixelFore += gray[X];
                    OmegaFore = OmegaFore + X * gray[X];
                }
                OmegaBack = OmegaBack / PixelBack;
                OmegaFore = OmegaFore / PixelFore;
                SigmaBack = 0; SigmaFore = 0;
                for (X = MinValue; X <= Y; X++) SigmaBack = SigmaBack + (X - OmegaBack) * (X - OmegaBack) * gray[X];
                for (X = Y + 1; X <= MaxValue; X++) SigmaFore = SigmaFore + (X - OmegaFore) * (X - OmegaFore) * gray[X];
                if (SigmaBack == 0 || SigmaFore == 0)
                {
                    if (Threshold == -1)
                        Threshold = Y;
                }
                else
                {
                    SigmaBack = Math.Sqrt(SigmaBack / PixelBack);
                    SigmaFore = Math.Sqrt(SigmaFore / PixelFore);
                    Sigma = 1 + 2 * (PixelBack * Math.Log(SigmaBack / PixelBack) + PixelFore * Math.Log(SigmaFore / PixelFore));
                    if (Sigma < MinSigma)
                    {
                        MinSigma = Sigma;
                        Threshold = Y;
                    }
                }
            }
            return Threshold;
        }

        private int getYu11()  //ISODATA(也叫做intermeans法）
        {
            int i, l, toth, totl, h, g = 0;
            for (i = 1; i < gray.Length; i++)
            {
                if (gray[i] > 0)
                {
                    g = i + 1;
                    break;
                }
            }
            while (true)
            {
                l = 0;
                totl = 0;
                for (i = 0; i < g; i++)
                {
                    totl = totl + gray[i];
                    l = l + (gray[i] * i);
                }
                h = 0;
                toth = 0;
                for (i = g + 1; i < gray.Length; i++)
                {
                    toth += gray[i];
                    h += (gray[i] * i);
                }
                if (totl > 0 && toth > 0)
                {
                    l /= totl;
                    h /= toth;
                    if (g == (int)Math.Round((l + h) / 2.0))
                        break;
                }
                g++;
                if (g > gray.Length - 2)
                {
                    return 0;
                }
            }
            return g;
        }

        public int getYu12()  //Shanbhag 法
        {
            int threshold;
            int ih, it;
            double crit;
            double max_crit;
            double[] norm_histo = new double[gray.Length]; /* normalized histogram */
            double[] P1 = new double[gray.Length]; /* cumulative normalized histogram */
            double[] P1_sq = new double[gray.Length];
            double[] P2_sq = new double[gray.Length];

            int total = 0;
            for (ih = 0; ih < gray.Length; ih++)
                total += gray[ih];

            for (ih = 0; ih < gray.Length; ih++)
                norm_histo[ih] = (double)gray[ih] / total;

            P1[0] = norm_histo[0];
            for (ih = 1; ih < gray.Length; ih++)
                P1[ih] = P1[ih - 1] + norm_histo[ih];

            P1_sq[0] = norm_histo[0] * norm_histo[0];
            for (ih = 1; ih < gray.Length; ih++)
                P1_sq[ih] = P1_sq[ih - 1] + norm_histo[ih] * norm_histo[ih];

            P2_sq[gray.Length - 1] = 0.0;
            for (ih = gray.Length - 2; ih >= 0; ih--)
                P2_sq[ih] = P2_sq[ih + 1] + norm_histo[ih + 1] * norm_histo[ih + 1];

            /* Find the threshold that maximizes the criterion */
            threshold = -1;
            max_crit = Double.MinValue;
            for (it = 0; it < gray.Length; it++)
            {
                crit = -1.0 * ((P1_sq[it] * P2_sq[it]) > 0.0 ? Math.Log(P1_sq[it] * P2_sq[it]) : 0.0) + 2 * ((P1[it] * (1.0 - P1[it])) > 0.0 ? Math.Log(P1[it] * (1.0 - P1[it])) : 0.0);
                if (crit > max_crit)
                {
                    max_crit = crit;
                    threshold = it;
                }
            }
            return threshold;
        }
        public int getYu13()  //Yen法
        {
            int threshold;
            int ih, it;
            double crit;
            double max_crit;
            double[] norm_histo = new double[gray.Length]; /* normalized histogram */
            double[] P1 = new double[gray.Length]; /* cumulative normalized histogram */
            double[] P1_sq = new double[gray.Length];
            double[] P2_sq = new double[gray.Length];

            int total = 0;
            for (ih = 0; ih < gray.Length; ih++)
                total += gray[ih];

            for (ih = 0; ih < gray.Length; ih++)
                norm_histo[ih] = (double)gray[ih] / total;

            P1[0] = norm_histo[0];
            for (ih = 1; ih < gray.Length; ih++)
                P1[ih] = P1[ih - 1] + norm_histo[ih];

            P1_sq[0] = norm_histo[0] * norm_histo[0];
            for (ih = 1; ih < gray.Length; ih++)
                P1_sq[ih] = P1_sq[ih - 1] + norm_histo[ih] * norm_histo[ih];

            P2_sq[gray.Length - 1] = 0.0;
            for (ih = gray.Length - 2; ih >= 0; ih--)
                P2_sq[ih] = P2_sq[ih + 1] + norm_histo[ih + 1] * norm_histo[ih + 1];

            /* Find the threshold that maximizes the criterion */
            threshold = -1;
            max_crit = Double.MinValue;
            for (it = 0; it < gray.Length; it++)
            {
                crit = -1.0 * ((P1_sq[it] * P2_sq[it]) > 0.0 ? Math.Log(P1_sq[it] * P2_sq[it]) : 0.0) + 2 * ((P1[it] * (1.0 - P1[it])) > 0.0 ? Math.Log(P1[it] * (1.0 - P1[it])) : 0.0);
                if (crit > max_crit)
                {
                    max_crit = crit;
                    threshold = it;
                }
            }
            return threshold;
        }

        public int getYu14()//最大类间方差算法
        {
            int Mr = 0;//灰度均值
            long sum = 0;
            int count = 0;
            for (int i = 0; i < 256; i++)//像素个数与灰度等级的乘积除以像素个数
            {
                sum += gray[i] * i;
                count += gray[i];
            }
            Mr = (int)(sum / count);
            int sum1 = 0;
            int count1 = 0;
            for (int i = 0; i <= Mr; i++)
            {
                sum1 += gray[i] * i;
                count1 += gray[i];
            }
            int g1 = sum1 / count1;

            int sum2 = 0;
            int count2 = 0;
            for (int i = Mr; i <= 255; i++)
            {
                sum2 += gray[i] * i;
                count2 += gray[i];
            }
            int g2 = sum2 / count2;

            //求阀值
            int va;
            if (count1 < count2)
            {//白底黑字
                va = Mr - count1 / count2 * Math.Abs(g1 - Mr);
            }
            else                //黑底白字
                va = Mr + count2 / count1 * Math.Abs(g2 - Mr);

            return va;

        }

        private void btnLicenceBinary_Click(object sender, RoutedEventArgs e)
        {
            //int va = getYu1();
            //int va = getYu2();//百分比阈值
            //int va = getYu3();  //基于谷底最小值的阈值
            //int va = getYu4();   //基于双峰平均值的阈值
            //int va = getYu5();    //迭代最佳阈值
            //int va = getYu6();   //OSTU大律法
            //int va = getYu7();  //一维最大熵
            //int va = getYu8();  //力矩保持法 
            //int va = getYu9();
            //int va = getYu10();   //Kittler最小错误分类法
            //int va = getYu11();   //ISODATA(也叫做intermeans法）
            //int va = getYu12();
            //int va = getYu13();
            int va = getYu14();//最大类间方差法

            BitmapData bmData = c_Bitmap.LockBits(new Rectangle(0, 0, c_Bitmap.Width, c_Bitmap.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            unsafe
            {
                int stride = bmData.Stride;
                System.IntPtr Scan0 = bmData.Scan0;
                byte* p = (byte*)(void*)Scan0;
                int nOffset = stride - c_Bitmap.Width * 3;

                int nWidth = c_Bitmap.Width;
                int nHeight = c_Bitmap.Height;

                for (int y = 0; y < nHeight; ++y)
                {
                    for (int x = 0; x < nWidth; ++x)
                    {
                        if (p[0] > va)
                        {
                            p[0] = p[1] = p[2] = 255;
                        }
                        else
                            p[0] = p[1] = p[2] = 0;

                        p += 3;
                    }
                    p += nOffset;
                }
            }
            c_Bitmap.UnlockBits(bmData);

            IntPtr ip2 = c_Bitmap.GetHbitmap();//将Bitmap转换为BitmapSource
            BitmapSource bitmapSource2 = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip2, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            imgLicence.Source = bitmapSource2;

        }

// 字符切割
        private void btnCharSplit_Click(object sender, RoutedEventArgs e)
        {
            this.LicenceSplit(sender,e);
           c_Bitmap.UnlockBits(bmData);
            if ((YBottom - Yheight) > 1 && (Yr - Yl) > 1)
            {
                Rectangle sourceRectangle = new Rectangle(Yl, Yheight, Yr - Yl, YBottom - Yheight);
                //c_Bitmap2是画线的那个图片
                extract_Bitmap_two = extract_Bitmap_one.Clone(sourceRectangle, PixelFormat.DontCare);
                BitmapData bmData2 = extract_Bitmap_two.LockBits(new Rectangle(0, 0, extract_Bitmap_two.Width, extract_Bitmap_two.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                int stride2 = bmData2.Stride;
                System.IntPtr Scan02 = bmData2.Scan0;

                unsafe
                {
                    byte* p2 = (byte*)(void*)Scan02;
                    int nOffset2 = stride2 - extract_Bitmap_two.Width * 3;

                    int nWidth2 = extract_Bitmap_two.Width;
                    int nHeight2 = extract_Bitmap_two.Height;
                    for (int y = 0; y < nHeight2; ++y)
                    {
                        for (int x = 0; x < nWidth2; ++x)
                        {

                            if (x == (XRight[0] - Yl) || x == (XLeft[0] - Yl) || x == (XRight[1] - Yl) || x == (XLeft[1] - Yl) || x == (XRight[2] - Yl) || x == (XLeft[2] - Yl) || x == (XRight[3] - Yl) || x == (XLeft[3] - Yl) || x == (XRight[4] - Yl) || x == (XLeft[4] - Yl) || x == (XRight[5] - Yl) || x == (XLeft[5] - Yl) || x == (XRight[6] - Yl) || x == (XLeft[6] - Yl) || x == (XRight[7] - Yl) || x == (XLeft[7] - Yl))
                            {
                                if (x != 0)
                                    p2[2] = 255; p2[0] = p2[1] = 0;
                            }

                            p2 += 3;
                        }
                        p2 += nOffset2;
                    } 
                }   
               
                extract_Bitmap_two.UnlockBits(bmData2);
                IntPtr ip = extract_Bitmap_two.GetHbitmap();//将Bitmap转换为BitmapSource
                BitmapSource bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                imgSplit.Source = bitmapSource;
                //this.ExtractPanel.Invalidate();
                if ((YBottom - Yheight) > 1 && (XRight[1] - XLeft[1]) > 1)
                {

                    Rectangle sourceRectangle2 = new Rectangle(XLeft[1], Yheight, XRight[1] - XLeft[1], YBottom - Yheight);
                    z_Bitmap1 = extract_Bitmap_one.Clone(sourceRectangle2, PixelFormat.DontCare);
                    z_Bitmaptwo[1] = c_Bitmap.Clone(sourceRectangle2, PixelFormat.DontCare);
                    objNewPic = new System.Drawing.Bitmap(z_Bitmaptwo[1], 9, 16);
                    z_Bitmaptwo[1] = objNewPic;
                    //objNewPic.Save("E:\\1.bmp");
                    //objNewPic = null;
                    //FontPanel2.Invalidate();
                    ip = z_Bitmaptwo[1].GetHbitmap();//将Bitmap转换为BitmapSource
                    bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    imgChar1.Source = bitmapSource;
                }
                if ((YBottom - Yheight) > 1 && (XRight[2] - XLeft[2]) > 1)
                {

                    Rectangle sourceRectangle3 = new Rectangle(XLeft[2], Yheight, XRight[2] - XLeft[2], YBottom - Yheight);
                    z_Bitmap2 = extract_Bitmap_one.Clone(sourceRectangle3, PixelFormat.DontCare);
                    z_Bitmaptwo[2] = c_Bitmap.Clone(sourceRectangle3, PixelFormat.DontCare);
                    objNewPic = new System.Drawing.Bitmap(z_Bitmaptwo[2], 9, 16);
                    z_Bitmaptwo[2] = objNewPic;
                    //objNewPic.Save("E:\\2.bmp");
                    //objNewPic = null;
                    //FontPanel3.Invalidate();
                    ip = z_Bitmaptwo[2].GetHbitmap();//将Bitmap转换为BitmapSource
                    bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    imgChar2.Source = bitmapSource;
                }
                if ((YBottom - Yheight) > 1 && (XRight[3] - XLeft[3]) > 1)
                {
                    Rectangle sourceRectangle4 = new Rectangle(XLeft[3], Yheight, XRight[3] - XLeft[3], YBottom - Yheight);
                    z_Bitmap3 = extract_Bitmap_one.Clone(sourceRectangle4, PixelFormat.DontCare);
                    z_Bitmaptwo[3] = c_Bitmap.Clone(sourceRectangle4, PixelFormat.DontCare);
                    objNewPic = new System.Drawing.Bitmap(z_Bitmaptwo[3], 9, 16);
                    z_Bitmaptwo[3] = objNewPic;
                    //objNewPic.Save("E:\\3.bmp");
                    //objNewPic = null;
                    //FontPanel4.Invalidate();
                    ip = z_Bitmaptwo[3].GetHbitmap();//将Bitmap转换为BitmapSource
                    bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    imgChar3.Source = bitmapSource;
                }
                if ((YBottom - Yheight) > 1 && (XRight[4] - XLeft[4]) > 1)
                {
                    Rectangle sourceRectangle5 = new Rectangle(XLeft[4], Yheight, XRight[4] - XLeft[4], YBottom - Yheight);
                    z_Bitmap4 = extract_Bitmap_one.Clone(sourceRectangle5, PixelFormat.DontCare);
                    z_Bitmaptwo[4] = c_Bitmap.Clone(sourceRectangle5, PixelFormat.DontCare);
                    objNewPic = new System.Drawing.Bitmap(z_Bitmaptwo[4], 9, 16);
                    z_Bitmaptwo[4] = objNewPic;
                    //objNewPic.Save("E:\\4.bmp");
                    //objNewPic = null;
                    //FontPanel5.Invalidate();
                    ip = z_Bitmaptwo[4].GetHbitmap();//将Bitmap转换为BitmapSource
                    bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    imgChar4.Source = bitmapSource;
                }
                if ((YBottom - Yheight) > 1 && (XRight[5] - XLeft[5]) > 1)
                {
                    Rectangle sourceRectangle6 = new Rectangle(XLeft[5], Yheight, XRight[5] - XLeft[5], YBottom - Yheight);
                    z_Bitmap5 = extract_Bitmap_one.Clone(sourceRectangle6, PixelFormat.DontCare);
                    z_Bitmaptwo[5] = c_Bitmap.Clone(sourceRectangle6, PixelFormat.DontCare);
                    objNewPic = new System.Drawing.Bitmap(z_Bitmaptwo[5], 9, 16);
                    z_Bitmaptwo[5] = objNewPic;
                    //objNewPic.Save("E:\\5.bmp");
                    //objNewPic = null;
                    //FontPanel6.Invalidate();
                    ip = z_Bitmaptwo[5].GetHbitmap();//将Bitmap转换为BitmapSource
                    bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    imgChar5.Source = bitmapSource;
                }
                if ((YBottom - Yheight) > 1 && (XRight[6] - XLeft[6]) > 1)
                {

                    Rectangle sourceRectangle7 = new Rectangle(XLeft[6], Yheight, XRight[6] - XLeft[6], YBottom - Yheight);
                    z_Bitmap6 = extract_Bitmap_one.Clone(sourceRectangle7, PixelFormat.DontCare);
                    z_Bitmaptwo[6] = c_Bitmap.Clone(sourceRectangle7, PixelFormat.DontCare);
                    objNewPic = new System.Drawing.Bitmap(z_Bitmaptwo[6], 9, 16);
                    z_Bitmaptwo[6] = objNewPic;
                    //objNewPic.Save("E:\\6.bmp");
                    //objNewPic = null;
                    //FontPanel7.Invalidate();
                    ip = z_Bitmaptwo[6].GetHbitmap();//将Bitmap转换为BitmapSource
                    bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    imgChar6.Source = bitmapSource;
                }
                if ((YBottom - Yheight) > 1 && (XRight[0] - XLeft[0]) > 1)
                {

                    Rectangle sourceRectangle0 = new Rectangle(XLeft[0], Yheight, XRight[0] - XLeft[0], YBottom - Yheight);
                    z_Bitmap0 = extract_Bitmap_one.Clone(sourceRectangle0, PixelFormat.DontCare);
                    z_Bitmaptwo[0] = c_Bitmap.Clone(sourceRectangle0, PixelFormat.DontCare);
                    objNewPic = new System.Drawing.Bitmap(z_Bitmaptwo[0], 9, 16);
                    z_Bitmaptwo[0] = objNewPic;
                    //objNewPic.Save("E:\\0.bmp");
                    //objNewPic = null;
                    //FontPanel1.Invalidate();
                    ip = z_Bitmaptwo[0].GetHbitmap();//将Bitmap转换为BitmapSource
                    bitmapSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    imgChar0.Source = bitmapSource;
                }

            }


        }

//图像归一化
        private int TransformFiles(string path)
        {
            //throw new NotImplementedException();
            DirectoryInfo dir = new DirectoryInfo(path);
            //DirectoryInfo[] dirs = dir.GetDirectories();
            FileInfo[] files = dir.GetFiles("*.bmp");

            int i = 0, j = 0;
            try
            {
                foreach (FileInfo f in files)
                {
                    //this.listBox1.Items.Add(dir + f.ToString());
                    i++;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            if (path.Equals(charSourceBath))
            {
                this.charString = new string[i];
                this.charDigitalString = new string[i];
                try
                {

                    foreach (FileInfo f in files)
                    {
                        charString[j] = (dir + f.ToString());
                        charDigitalString[j] = Path.GetFileNameWithoutExtension(charString[j]);
                        //Console.WriteLine(charDigitalString[j]);
                        j++;

                    }
                    //Console.WriteLine(j);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
            else
            {
                provinceString = new string[i];
                provinceDigitalString = new string[i];

                try
                {

                    foreach (FileInfo f in files)
                    {
                        provinceString[j] = (dir + f.ToString());
                        provinceDigitalString[j] = Path.GetFileNameWithoutExtension(provinceString[j]);
                        //Console.WriteLine(provinceString[j]);
                        j++;
                    }
                    // Console.WriteLine(j);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString());
                }
            }
            return i;
        }

 //字符识别
        private void btnCharIdentify_Click(object sender, RoutedEventArgs e)
        {
            int charBmpCount = this.TransformFiles(charSourceBath); //图像归一化
            int provinceBmpCount = this.TransformFiles(provinceSourceBath);
            int[] charMatch = new int[charBmpCount];//存储当前图片和资源库中图片比对后所得的像素不同的个数
            int[] provinceMatch = new int[provinceBmpCount];

            charFont = new Bitmap[charBmpCount];
            provinceFont = new Bitmap[provinceBmpCount];//这两个数组存储的是资源库中的bitmap文件
                                                        //Console.WriteLine("一共有这么几个资源文件" + (charBmpCount + provinceBmpCount));
            for (int i = 0; i < charBmpCount; i++)
            {
                charMatch[i] = 0;
            }
            for (int i = 0; i < provinceBmpCount; i++)
            {
                provinceMatch[i] = 0;
            }
            for (int i = 0; i < charBmpCount; i++)
            {
                charFont[i] = (Bitmap)Bitmap.FromFile(charString[i], false);//使用该文件中的嵌入颜色管理信息，从指定的文件创建m_Bitmap
            }                                                               //charString存储的是路径
            for (int i = 0; i < provinceBmpCount; i++)
            {
                provinceFont[i] = (Bitmap)Bitmap.FromFile(provinceString[i], false);//使用该文件中的嵌入颜色管理信息，从指定的文件创建m_Bitmap
            }

            int matchIndex = 0;//最终匹配索引
            string[] digitalFont = new string[7];
            unsafe
            {
                if (z_Bitmaptwo[0] != null)
                {
                    BitmapData bmData = z_Bitmaptwo[0].LockBits(new Rectangle(0, 0, z_Bitmaptwo[0].Width, z_Bitmaptwo[0].Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                    int stride = bmData.Stride;
                    System.IntPtr Scan = bmData.Scan0;
                    // byte* p = (byte*)(void*)Scan;
                    int nOffset = stride - z_Bitmaptwo[0].Width * 3;
                    int nWidth = z_Bitmaptwo[0].Width;
                    int nHeight = z_Bitmaptwo[0].Height;
                    int lv, lc = 30;

                    for (int i = 0; i < provinceBmpCount; i++)
                    {
                        byte* p = (byte*)(void*)Scan;
                        BitmapData bmData1 = provinceFont[i].LockBits(new Rectangle(0, 0, provinceFont[i].Width, provinceFont[i].Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                        int stride1 = bmData1.Stride;
                        System.IntPtr Scan1 = bmData1.Scan0;
                        byte* p1 = (byte*)(void*)Scan1;
                        int nOffset1 = stride1 - provinceFont[i].Width * 3;
                        int nWidth1 = provinceFont[i].Width;
                        int nHeight1 = provinceFont[i].Height;
                        int ccc0 = 0, ccc1 = 0;
                        lv = 0;//两个图片匹配不相同的地方
                        for (int y = 0; y < nHeight; ++y)
                        {
                            for (int x = 0; x < nWidth; ++x)
                            {

                                if ((p[0] - p1[0]) != 0)
                                    provinceMatch[i]++;
                          

                                p1 += 3;
                                p += 3;

                            }
                            p1 += nOffset;
                            p += nOffset;
                        }
                        //Console.WriteLine(provinceDigitalString[i] + "不相同的像素数值" + provinceMatch[i]);
                        //lv = lv + Math.Abs(ccc0 - ccc1);
                        matchIndex = this.minNumber(provinceMatch);
                        digitalFont[0] = provinceDigitalString[matchIndex].Substring(0, 1);//文件的名字和图片信息匹配，所以得到的文件名就是图片上的文字

                        provinceFont[i].UnlockBits(bmData1);

                    }
                    z_Bitmaptwo[0].UnlockBits(bmData);
                }



                if (z_Bitmaptwo[1] != null && z_Bitmaptwo[2] != null && z_Bitmaptwo[3] != null && z_Bitmaptwo[4] != null && z_Bitmaptwo[5] != null && z_Bitmaptwo[6] != null)
                {
                    for (int j = 1; j < 7; j++)
                    {
                        BitmapData bmData = z_Bitmaptwo[j].LockBits(new Rectangle(0, 0, z_Bitmaptwo[j].Width, z_Bitmaptwo[j].Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                        int stride = bmData.Stride;
                        System.IntPtr Scan = bmData.Scan0;
                        // byte* p = (byte*)(void*)Scan;
                        int nOffset = stride - z_Bitmaptwo[j].Width * 3;
                        int nWidth = z_Bitmaptwo[j].Width;
                        int nHeight = z_Bitmaptwo[j].Height;
                        int lv, lc = 0;
                        //Console.WriteLine("======================================" + j);
                        for (int i = 0; i < charBmpCount; i++)
                        {
                            charMatch[i] = 0;
                        }
                        for (int i = 0; i < charBmpCount; i++)
                        {
                            byte* p = (byte*)(void*)Scan;
                            BitmapData bmData1 = charFont[i].LockBits(new Rectangle(0, 0, charFont[i].Width, charFont[i].Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                            int stride1 = bmData1.Stride;
                            System.IntPtr Scan1 = bmData1.Scan0;
                            byte* p1 = (byte*)(void*)Scan1;
                            int nOffset1 = stride1 - charFont[i].Width * 3;
                            int nWidth1 = charFont[i].Width;
                            int nHeight1 = charFont[i].Height;
                            int ccc0 = 0, ccc1 = 0;
                            lv = 0;
                            for (int y = 0; y < nHeight; ++y)
                            {
                                for (int x = 0; x < nWidth; ++x)
                                {

                                    if ((p[0] - p1[0]) != 0)
                                    {
                                        charMatch[i]++;
                                        // Console.WriteLine(ccc0++);
                                    }
                                    lv++;
                                    p1 += 3;
                                    p += 3;
                                }
                                p1 += nOffset;
                                p += nOffset;
                            }
                            // Console.WriteLine("图像尺寸" + lv);
                            Console.WriteLine(charDigitalString[i] + "数字中不相同的像素数值" + charMatch[i]);
                            matchIndex = this.minNumber(charMatch);
                            digitalFont[j] = charDigitalString[matchIndex].Substring(0, 1);//截取文件名的第一个字符就行了

                            charFont[i].UnlockBits(bmData1);

                        }

                        z_Bitmaptwo[j].UnlockBits(bmData);
                    }
                }
            }
            //this.ResultLabel.Text = "" + digitalFont[0] + digitalFont[1] + digitalFont[2] + digitalFont[3] + digitalFont[4] + digitalFont[5] + digitalFont[6];
            this.txtResult.Text = "" + digitalFont[0] + digitalFont[1] + digitalFont[2] + digitalFont[3] + digitalFont[4] + digitalFont[5] + digitalFont[6];


        }

        private int minNumber(int[] provinceMatch)
        {
            // throw new NotImplementedException();
            int minn = 300;
            int tag = 0;
            int len = provinceMatch.Length;
            for (int i = 0; i < len; i++)
            {
                if (provinceMatch[i] < minn)
                {
                     minn = provinceMatch[i];
                    tag = i;
                }
                   
            }

            return tag;
        }
    }
}
