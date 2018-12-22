using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace Neyrosetka
{
    public partial class Form1 : Form
    {
        public IBZ bazaZnaniy;
        private Bitmap cartina, vectorBitmap;
        private int[] vector = new int[225];

        public Form1()
        {
            InitializeComponent();
            bazaZnaniy = new BZ();
        }

        private bool IsClicked { get; set; }

        private void Form1_Load(object sender, EventArgs e)
        {
            cartina = new Bitmap(pictureBox1.Width, pictureBox1.Height); //создание нового изображения
            var gr = Graphics.FromImage(cartina);
            gr.FillRectangle(new SolidBrush(Color.Bisque), 0, 0, cartina.Width, cartina.Height); //заполняем цветом
            pictureBox1.Image = cartina;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            Risovatel.Painter(e, Graphics.FromImage(cartina));
            pictureBox1.Image = cartina;

            IsClicked = true;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsClicked)
                Risovatel.Painter(e, Graphics.FromImage(cartina));
            pictureBox1.Image = cartina;
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (IsClicked)
            {
                Risovatel.Painter(e, Graphics.FromImage(cartina));
                pictureBox1.Image = cartina;

                vectorBitmap = new Bitmap(Risovatel.ResizeImageMinImage(cartina, 15));
                pictureBox2.Image = Risovatel.ResizeImageMaxImage(vectorBitmap, 300);
                var arr = Parser.BMPToArray(vectorBitmap);
                vector = Parser.ArrayToVector(arr);

                IsClicked = false;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var gr = Graphics.FromImage(cartina);
            gr.FillRectangle(new SolidBrush(Color.Bisque), 0, 0, cartina.Width, cartina.Height);
            pictureBox1.Image = cartina;
        }


        //тренировать
        private void button3_Click(object sender, EventArgs e)
        {
            if (bazaZnaniy.Brain == null)
                bazaZnaniy.CreateNewBrain(vector.Length); //если персептрона нет, создаем новый
            bazaZnaniy.Train(vector, textBox1.Text);
            button2.Enabled = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 1) textBox1.Text = textBox1.Text.Substring(0, 1);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            var c = e.KeyChar;
            var a = !(c >= 'а' && c <= 'я' || c >= 'А' && c <= 'Я' || c == 'Ё' || c == 'ё' || c == 8 || c == 46);
            e.Handled = a && !(c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9');
        }

        private void загрузитьБЗToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!bazaZnaniy.LoadBrain("baza.xml")) return;
            MessageBox.Show("База загружена!");
            button2.Enabled = true;
        }

        private void сохранитьБЗToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (bazaZnaniy.SaveBrain("baza.xml"))
                MessageBox.Show("База сохранена!");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (bazaZnaniy.Brain == null)
                bazaZnaniy.CreateNewBrain(vector.Length); //если персептрона нет, создаем новый

            var vyborka = new Bitmap("vyborkaTraining.bmp");
            var y = 0;
            var width = 16;
            var height = 20;
            var chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            TrainingSet.Chars = new List<string>();
            TrainingSet.Vectors = new List<int[]>();
            foreach (var bukva in chars)
            {
                for (var i = 0; i < 39; i++)
                {
                    var oblastClone = vyborka.Clone(new Rectangle(i * (width + 3), y, width, height),
                        vyborka.PixelFormat);
                    var currentImg = new Bitmap(20, 20);
                    var gr = Graphics.FromImage(currentImg);
                    gr.FillRectangle(new SolidBrush(Color.White), 0, 0, 20, 20);
                    gr.DrawImage(oblastClone, new Rectangle(2, 0, width, height));
                    currentImg = Risovatel.ResizeImageMinImage(currentImg, 15);
                    var vect = Parser.ArrayToVector(Parser.BMPToArray(currentImg));
                    TrainingSet.Chars.Add(bukva.ToString());
                    TrainingSet.Vectors.Add(vect);
                }
                y += 23;
            }

            bazaZnaniy.TrainingFromFile();
            button2.Enabled = true;
            MessageBox.Show("OK");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            /*
            string str = "Array ===================\r\n";
            for (int i = 0; i < vectorBitmap.Height; i++)
            {
                for (int j = 0; j < vectorBitmap.Width; j++)
                    str += "{" + arr[i, j] + "};";
                str += "\r\n";
            }
            str += "List ===================\r\n";
            for (int i = 0; i < vectorBitmap.Height; i++)
            {
                for (int j = i*15; j < vectorBitmap.Width+(i*15); j++)
                    str += "{" + list[j] + "};";
                str += "\r\n";
            }
            richTextBox1.Text = str;*/
            string answer, values;
            Dictionary<string, string> neurons;
            bazaZnaniy.ReadChar(vector, out answer, out neurons);
            values = "";
            foreach (var valReadChar in neurons)
                values += "Буква {" + valReadChar.Key + "} : " + valReadChar.Value + "\r\n";
            
            richTextBox1.Text = "===================ОТВЕТ================\r\n             " + answer +
                                "\r\n=============Выходные сигналы (удельный вес)============\r\n" + values;
        }
    }


    public class Parser
    {
        public static int[,] BMPToArray(Bitmap image)
        {
            var arr = new string[image.Height, image.Width];

            for (var i = 0; i < image.Height; i++)
            for (var j = 0; j < image.Width; j++)
                arr[i, j] = image.GetPixel(j, i).Name; //ff000000

            var arr1 = new int[image.Height, image.Width];

            for (var i = 0; i < image.Height; i++)
            for (var j = 0; j < image.Width; j++)
                if (arr[i, j] == "ff000000")
                    arr1[i, j] = 1;
            return arr1;
        }

        public static int[] ArrayToVector(int[,] array)
        {
            return array.Cast<int>().ToArray();
        }
    }

    public class Risovatel
    {
        public static void Painter(MouseEventArgs e, Graphics gr)
        {
            var pp = new SolidBrush(Color.Black);
            gr.FillRectangle(pp, e.X, e.Y, 20, 20);
        }

        public static Bitmap ResizeImageMinImage(Image originImage, int newSize)
        {
            float width = newSize;
            float height = newSize;
            var image = new Bitmap(originImage);
            var scale = Math.Min(width / image.Width, height / image.Height);
            var bmp = new Bitmap((int) width, (int) height);
            var graph = Graphics.FromImage(bmp);
            graph.InterpolationMode = InterpolationMode.NearestNeighbor;
            var scaleWidth = (int) (image.Width * scale);
            var scaleHeight = (int) (image.Height * scale);
            graph.DrawImage(image, new Rectangle(0, 0, scaleWidth, scaleHeight));
            return bmp;
        }


        public static Bitmap ResizeImageMaxImage(Image originImage, int newSize)
        {
            float width = newSize;
            float height = newSize;
            var image = new Bitmap(originImage);
            var scale = Math.Min(width / image.Width, height / image.Height);
            var bmp = new Bitmap((int) width, (int) height);
            var graph = Graphics.FromImage(bmp);
            graph.InterpolationMode = InterpolationMode.NearestNeighbor;
            var scaleWidth = (int) (image.Width * scale * 1.05);
            var scaleHeight = (int) (image.Height * scale * 1.05);
            graph.DrawImage(image, new Rectangle(0, 0, scaleWidth, scaleHeight));
            return bmp;
        }
    }


    public static class TrainingSet
    {
        public static List<string> Chars = new List<string>();
        public static List<int[]> Vectors = new List<int[]>();
    }
}