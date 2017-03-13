using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Data.SqlClient;
using System.Configuration;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;

namespace WindowsFormsApplication42
{
    public partial class Form1 : Form
    {
        int num=0;
        string[] sArray;
        int page = 0;
        private string curFileName;
        private System.Drawing.Bitmap curBitmap1;
        System.Drawing.Image img;

        Bitmap imgOutput;
        static string GetConnectString()
        {
            return "Data Source=(local);Initial Catalog=test;Integrated Security=SSPI;";

        }



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
        // Compare hash
        public static Int32 CalcSimilarDegree(string a, string b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException();
            int count = 0;
            for (int i = 0; i < 64; i++)
            {
                if (a[i] != b[i])
                    count++;
            }
            return count;
        }


        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            button4.Enabled = false;
            pictureBox1.Image = null;
            pictureBox2.Image = null;
            pictureBox3.Image = null;
            pictureBox4.Image = null;
            pictureBox5.Image = null;
            textBox1.Text = "";
            OpenFileDialog opnDlg = new OpenFileDialog();//创建OpenFileDialog对象

            //为图像选择一个筛选器

            opnDlg.Filter = "所有图像文件 | *.bmp; *.pcx; *.png; *.jpg; *.gif;" +

                "*.tif; *.ico; *.dxf; *.cgm; *.cdr; *.wmf; *.eps; *.emf|" +

                "位图( *.bmp; *.jpg; *.png;...) | *.bmp; *.pcx; *.png; *.jpg; *.gif; *.tif; *.ico|" +

                "矢量图( *.wmf; *.eps; *.emf;...) | *.dxf; *.cgm; *.cdr; *.wmf; *.eps; *.emf";

            opnDlg.Title = "打开图像文件";

            opnDlg.ShowHelp = true;//启动帮助按钮

            if (opnDlg.ShowDialog() == DialogResult.OK)
            {

                curFileName = opnDlg.FileName;

                try
                {

                    curBitmap1 = (Bitmap)Image.FromFile(curFileName);//使用Image.FromFile创建图像对象
                    img = Image.FromFile(curFileName);
                    imgOutput = new Bitmap(img, 8, 8);

                }

                catch (Exception exp)
                {

                    MessageBox.Show(exp.Message);

                }

            }
            button2.Enabled = true;
            //使控件的整个图面无效并导致重绘控件

            Invalidate();//对窗体进行重新绘制,这将强制执行Paint事件处理程序   
            
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g1 = e.Graphics;//获取Graphics对象
            
            if (curBitmap1 != null)
            {

                g1.DrawImage(curBitmap1, 864, 10, 200, 200);//使用DrawImage方法绘制图像

            }
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectString()))
            {
                conn.Open();//打开数据库  
                //Console.WriteLine("数据库打开成功!");  
                //创建数据库命令  
                SqlCommand cmd = conn.CreateCommand();
                //创建查询语句  
                cmd.CommandText = "SELECT * FROM picture";
                //从数据库中读取数据流存入reader中  
                SqlDataReader reader = cmd.ExecuteReader();
                //从reader中读取下一行数据,如果没有数据,reader.Read()返回flase  
                string resultpath="";
                int i = 0;
                while (reader.Read())
                {
                    byte[] histogram = new byte[64];
                    string strFileName = reader.GetString(reader.GetOrdinal("path"));
                    string result1 = reader.GetString(reader.GetOrdinal("histogram"));
                    histogram = ReduceColor(imgOutput);
                    byte average = CalcAverage(histogram);
                    string result = ComputeBits(histogram, average);
                    int count = CalcSimilarDegree(result1, result);
                    if (count <= 10)//调节相似度改这里，值越小相似度要求越高
                    {
                        resultpath +=  ","+strFileName ;
                        i++;
                    }
                   
                }

                 sArray = resultpath.Split(',');
                 num = sArray.Length - 1;
             
                     textBox1.Text = "有" + num + "张相似的图片";
                     
               
                if (sArray[1] != null)
                    pictureBox1.Image = Image.FromFile(sArray[1]);
                if (sArray.Length - 1>=2)
                    pictureBox2.Image = Image.FromFile(sArray[2]);
                if (sArray.Length - 1 >= 3)
                    pictureBox3.Image = Image.FromFile(sArray[3]);
                if (sArray.Length - 1 >= 4)
                    pictureBox4.Image = Image.FromFile(sArray[4]);
                if (sArray.Length - 1 >= 5)
                    pictureBox5.Image = Image.FromFile(sArray[5]);
              if(sArray.Length-6<=0)
                  button3.Enabled=false;
              else
                  button3.Enabled = true;
              
            }
            
            imgOutput.Dispose();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            page++;
            if (sArray.Length - (1 + page * 5) >= 1)
                pictureBox1.Image = Image.FromFile(sArray[1+page*5]);
            if (sArray.Length - (1+page*5) >= 2)
                pictureBox2.Image = Image.FromFile(sArray[2 + page * 5]);
            else
                pictureBox2.Image = null;
            if (sArray.Length - (1 + page * 5) >= 3)
                pictureBox3.Image = Image.FromFile(sArray[3 + page * 5]);
             else
                pictureBox3.Image = null;
            if (sArray.Length - (1 + page * 5) >= 4)
                pictureBox4.Image = Image.FromFile(sArray[4 + page * 5]);
            else
                pictureBox4.Image = null;
            if (sArray.Length - (1 + page * 5) >= 5)
                pictureBox5.Image = Image.FromFile(sArray[5 + page * 5]);
            else
                pictureBox5.Image = null;
           
            if (sArray.Length - (6+page*5) <= 0)
                button3.Enabled = false;
            
            if (page > 0)
                button4.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            page--;
            if (sArray.Length - (1 + page * 5) >= 1)
                pictureBox1.Image = Image.FromFile(sArray[1 + page * 5]);
            if (sArray.Length - (1 + page * 5) >= 2)
                pictureBox2.Image = Image.FromFile(sArray[2 + page * 5]);
            else
                pictureBox2.Image = null;
            if (sArray.Length - (1 + page * 5) >= 3)
                pictureBox3.Image = Image.FromFile(sArray[3 + page * 5]);
            else
                pictureBox3.Image = null;
            if (sArray.Length - (1 + page * 5) >= 4)
                pictureBox4.Image = Image.FromFile(sArray[4 + page * 5]);
            else
                pictureBox4.Image = null;
            if (sArray.Length - (1 + page * 5) >= 5)
                pictureBox5.Image = Image.FromFile(sArray[5 + page * 5]);
            else
                pictureBox5.Image = null;

            if (sArray.Length - (6 + page * 5) <= 0)
                button3.Enabled = false;
            if (page > 0)
                button4.Enabled = true;
            else
                button4.Enabled = false;
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        
    }
}
