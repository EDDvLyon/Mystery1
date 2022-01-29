using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Security.Cryptography;


namespace PicCrypto
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //Boolean encry = false, decrypt = false;

        Bitmap myImg;
        List<byte[]> myRed = new List<byte[]>();
        List<byte[]> myGreen = new List<byte[]>();
        List<byte[]> myBlue = new List<byte[]>();

        int OrigImgW = 0;
        int OrigImgH = 0;

        byte[] mySeed = new byte[16];
        byte[] myPass = new byte[16];

        Boolean decrypt = false;

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (false == decrypt)
            {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "image files|*.jpg;*.png;*.gif;*bmp;";
            DialogResult dr = ofd.ShowDialog();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            if (dr == DialogResult.OK)
            {
                myImg = new Bitmap(ofd.FileName);

                OrigImgW = myImg.Width;
                OrigImgH = myImg.Height;
                float ratio = (float)OrigImgW / OrigImgH;

                this.Width = (int)(ratio * this.Height);


                pictureBox1.Image = Image.FromFile(ofd.FileName);

                int size = myImg.Width * myImg.Height;
                if ((size % 16) != 0)
                {
                    size = ((size / 16) + 1) * 16;
                }
                byte[] dataRed = new byte[size];
                byte[] dataGreen = new byte[size];
                byte[] dataBlue = new byte[size];

                int cnt = 0;
                for (int i = 0; i < myImg.Width; i++)
                {
                    for (int j = 0; j < myImg.Height; j++)
                    {
                        dataRed[cnt] = myImg.GetPixel(i, j).R;
                        dataGreen[cnt] = myImg.GetPixel(i, j).G;
                        dataBlue[cnt] = myImg.GetPixel(i, j).B;
                        cnt++;
                    }
                }

                myRed.Add(dataRed);
                myGreen.Add(dataGreen);
                myBlue.Add(dataBlue);
            }
            else
            {

            }
        }
        else
        {
                pictureBox2.Visible = true;
                pictureBox1.Visible = false;
            }
        
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            this.Width = 500;
            this.Height = 600;

            //pictureBox1.Size = Form1.
            int w = this.Size.Width - 35;
            int h = this.Size.Height - 70;

            pictureBox1.Size = new Size(w, h);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            int w = this.Size.Width - 35;
            int h = this.Size.Height - 70;

            pictureBox1.Size = new Size(w, h);
        }


        private void pictureBox2_Click(object sender, EventArgs e)
        {
            pictureBox2.Visible = false;
            pictureBox1.Visible = true;
        }

        private void imgToolStripMenuItem1_Click(object sender, EventArgs e)
        {

            int length = myRed[0].Length;
            GetSeed();

            var rfc = new Rfc2898DeriveBytes(textBox2.Text, mySeed);
            byte[] Key = rfc.GetBytes(16);
            byte[] IV = rfc.GetBytes(16);

            byte[] decryptedR, decryptedG, decryptedB;

            using (MemoryStream mStream = new MemoryStream(myRed[0]))
            {
                using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
                {
                    aesProvider.Padding = PaddingMode.None;
                    using (CryptoStream cryptoStream = new CryptoStream(mStream, aesProvider.CreateDecryptor(Key, IV), CryptoStreamMode.Read))
                    {
                        cryptoStream.Read(myRed[0], 0, length);
                    }
                }
                decryptedR = mStream.ToArray().Take(length).ToArray();
            }

            using (MemoryStream mStream = new MemoryStream(myGreen[0]))
            {
                using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
                {
                    aesProvider.Padding = PaddingMode.None;
                    using (CryptoStream cryptoStream = new CryptoStream(mStream, aesProvider.CreateDecryptor(Key, IV), CryptoStreamMode.Read))
                    {
                        cryptoStream.Read(myGreen[0], 0, length);
                    }
                }
                decryptedG = mStream.ToArray().Take(length).ToArray();
            }

            using (MemoryStream mStream = new MemoryStream(myBlue[0]))
            {
                using (AesCryptoServiceProvider aesProvider = new AesCryptoServiceProvider())
                {
                    aesProvider.Padding = PaddingMode.None;
                    using (CryptoStream cryptoStream = new CryptoStream(mStream, aesProvider.CreateDecryptor(Key, IV), CryptoStreamMode.Read))
                    {
                        cryptoStream.Read(myBlue[0], 0, length);
                    }
                }
                decryptedB = mStream.ToArray().Take(length).ToArray();
            }

            Bitmap encodedBmp = new Bitmap(OrigImgW, OrigImgH);
            pictureBox2.Size = new Size(pictureBox1.Width, pictureBox1.Height);
            int cnt = 0;

            for (int i = 0; i < OrigImgW; i++)
            {
                for (int j = 0; j < OrigImgH; j++)
                {
                    Color mycolor = Color.FromArgb(decryptedR[cnt], decryptedG[cnt], decryptedB[cnt]);
                    encodedBmp.SetPixel(i, j, mycolor);

                    cnt++;
                }
            }

            decrypt = true;

            pictureBox2.Image = encodedBmp;
            pictureBox2.Visible = true;
            pictureBox1.Visible = false;
        }

        void GetSeed()
        {
            for(int i = 0; i < textBox1.Text.Length; i++)
            {
                mySeed[i]  = Convert.ToByte(Convert.ToChar(textBox1.Text.Substring(i,1)));
            }
        }

        void GetPassword()
        {
            for (int i = 0; i < textBox2.Text.Length; i++)
            {
                myPass[i] = Convert.ToByte(textBox2.Text.Substring(i, 1));
            }
        }
    }
}
