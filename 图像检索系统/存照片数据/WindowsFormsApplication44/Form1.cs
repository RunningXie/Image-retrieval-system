using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using System.Drawing.Imaging;
namespace WindowsFormsApplication44
{
    public partial class Form1 : Form
    {
        static string GetConnectString()
        {
            return "Data Source=(local);Initial Catalog=test;Integrated Security=SSPI;";

        }
        Bitmap imgOutput;
        System.Drawing.Image img;
      
        // Step 2 : Reduce Color
        private Byte[] ReduceColor(Image image)
        {
            Bitmap bitMap = new Bitmap(image);
            Byte[] grayValues = new Byte[image.Width * image.Height];

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {

                    Color color = bitMap.GetPixel(x, y);
                    byte grayValue = (byte)((color.R * 30 + color.G * 59 + color.B * 11) / 100);
                    grayValues[x * image.Width + y] = grayValue;
                }
            }
            return grayValues;
        }
        // Step 3 : Average the colors
        private Byte CalcAverage(byte[] values)
        {
            int sum = 0;
            for (int i = 0; i < 64; i++)
                sum += (int)values[i];
            return Convert.ToByte(sum / values.Length);
        }
        // Step 4 : Compute the bits
        private String ComputeBits(byte[] values, byte averageValue)
        {
            char[] result = new char[values.Length];
            for (int i = 0; i < 64; i++)
            {
                if (values[i] < averageValue)
                    result[i] = '0';
                else
                    result[i] = '1';
            }
            return new String(result);
        }
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectString()))
            {
                conn.Open();//打开数据库  
                //Console.WriteLine("数据库打开成功!");  
                //创建数据库命令  

                SqlDataAdapter d = new SqlDataAdapter();

                DirectoryInfo dicInfo = new DirectoryInfo(@"E:\ski");//注意这种@写路径的方法，这里修改你的文件夹路径
                FileInfo[] textFiles = dicInfo.GetFiles("*.*", SearchOption.AllDirectories);

                foreach (FileInfo fileInfo in textFiles)
                {
                    img = Image.FromFile(fileInfo.FullName);
                    imgOutput = new Bitmap(img, 8, 8);//Step 1:Reduce  picture
                    byte[] histogram = new byte[64];

                    histogram = ReduceColor(imgOutput);


                    byte average = CalcAverage(histogram);

                    string result = ComputeBits(histogram, average);



                    string strFileName = "../../../../../ski/" + fileInfo.Name;



                    d.InsertCommand = new SqlCommand("INSERT INTO picture(path,histogram)Values('" + strFileName + "',+'" + result + "')", conn);//非常经典的存储方法
                    d.InsertCommand.ExecuteNonQuery();


                }
                conn.Close();
                textBox1.Text = "end";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
    }
}

