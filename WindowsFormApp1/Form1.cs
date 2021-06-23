using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CsPotrace;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections;
using System.IO;
using System.Windows.Forms;

namespace WindowsFormsApp1
{

    public class ButtonSettings
    {
        public void ButtonSetting(Button button, string button_text, int button_width, int button_height)
        {
            button.Text = button_text;
            button.Size = new Size(button_width, button_height);
            button.BackColor = Color.FromArgb(230, 230, 230);
            button.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            button.TabStop = false;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.UseCompatibleTextRendering = true;
            button.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 200);
        }
    }

    public class Function
    {
        public unsafe bool IsGrayScale(Image image)
        {
            using (var bmp = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.DrawImage(image, 0, 0);
                }

                var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

                var pt = (int*)data.Scan0;
                var res = true;

                for (var i = 0; i < data.Height * data.Width; i++)
                {
                    var color = Color.FromArgb(pt[i]);

                    if (color.A != 0 && (color.R != color.G || color.G != color.B))
                    {
                        res = false;
                        break;
                    }
                }

                bmp.UnlockBits(data);
                return res;
            }
        }
    }

    public class TopControl
    {
        ButtonSettings btn = new ButtonSettings();
        public TextBox filedirectory = new TextBox();
        public Label label1 = new Label();

        public void Top_Control(Form form)
        {

            label1.AutoSize = true;
            label1.Location = new Point(form.Width / 64, form.Height / 32);
            label1.BackColor = Color.FromArgb(240, 240, 240);
            label1.Text = "File Selected :";
            label1.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            label1.TextAlign = ContentAlignment.MiddleCenter;
            form.Controls.Add(label1);

            Button browse = new Button();
            btn.ButtonSetting(browse, "Browse", form.Width / 9, label1.Size.Height + 1);
            browse.Location = new Point(form.Width - form.Width / 32 - browse.Size.Width, form.Height / 32 - 1);
            form.Controls.Add(browse);
            browse.Click += new EventHandler(browse_Click);


            filedirectory.Size = new Size(form.Width - label1.Width - browse.Width - form.Width / 16, label1.Height);
            filedirectory.Location = new Point(label1.Location.X + label1.Size.Width, form.Height / 32);
            filedirectory.Multiline = true;
            filedirectory.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            filedirectory.BorderStyle = 0;
            filedirectory.Name = "txtbox1";
            filedirectory.ReadOnly = true;
            form.Controls.Add(filedirectory);
        }


        public void browse_Click(object sender, EventArgs e)
        {
            string file = "";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Filter = "Image(*.png .jpg .jpeg .bmp .pbm)|*.png;*.jpeg;*.jpg;*.bmp;*.pbm";


            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                file = openFileDialog1.FileName;
                filedirectory.Text = file;
                try
                {
                }
                catch (IOException)
                {
                }
            }
            Console.WriteLine(result); // <-- For debugging use.
        }

        public string filedirectory_GetText()
        {
            TextBox t = Application.OpenForms["Main Form"].Controls["txtbox1"] as TextBox;
            return t.Text;
        }
    }



    public class Convert
    {
        ButtonSettings btn = new ButtonSettings();
        TopControl top = new TopControl();
        RichTextBox main = new RichTextBox();
        TextBox morph_size1 = new TextBox();
        TextBox morph_size2 = new TextBox();


        public void ConvertButton(Form form)
        {

            Button convert = new Button();
            btn.ButtonSetting(convert, "Convert", form.Width / 9, top.label1.Size.Height + 1);
            convert.Location = new Point(form.Width / 64, form.Height / 32 + top.label1.Size.Height + 20);
            form.Controls.Add(convert);

            CheckBox colour = new CheckBox();
            colour.Text = "Colour Image";
            colour.Size = new Size(100, 20);
            colour.Enabled = true;
            colour.Location = new Point(convert.Location.X + convert.Size.Width + 20, convert.Location.Y + convert.Size.Height - convert.Size.Height);

            colour.MouseHover += new EventHandler(colour_MouseHover);
            void colour_MouseHover(object sender, EventArgs e)
            {
                ToolTip ToolTip1 = new ToolTip();
                ToolTip1.SetToolTip(colour, "Check if source image has colour\nUse Canny Edge Detection (by John F.Canny)\nWill always result in double lines");
            }
            form.Controls.Add(colour);


            CheckBox potrace = new CheckBox();
            potrace.Text = "Potrace";
            potrace.Size = new Size(80, 20);
            potrace.Location = new Point(colour.Location.X + colour.Size.Width + 20, colour.Location.Y + colour.Size.Height - colour.Size.Height);
            form.Controls.Add(potrace);


            potrace.MouseHover += new EventHandler(potrace_MouseHover);
            void potrace_MouseHover(object sender, EventArgs e)
            {
                ToolTip ToolTip1 = new ToolTip();
                ToolTip1.SetToolTip(potrace, "Use Potrace Edge Detection (by Peter Selinger)\nTLDR: More accurate but result in double lines (lack centerline tracing)\nMeaning a lot more equations");
            }


            main.Multiline = true;
            main.BorderStyle = BorderStyle.None;
            main.Size = new Size(form.Width - convert.Location.X * 3, form.Height - convert.Location.Y - convert.Size.Height - 80);
            main.Location = new Point(convert.Location.X, convert.Location.Y + convert.Size.Height + 20);
            main.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            form.Controls.Add(main);


            CheckBox autotrace = new CheckBox();
            autotrace.Text = "Autotrace";
            autotrace.Size = new Size(80, 20);
            autotrace.Location = new Point(potrace.Location.X + potrace.Size.Width + 20, potrace.Location.Y + potrace.Size.Height - potrace.Size.Height);
            form.Controls.Add(autotrace);

            autotrace.MouseHover += new EventHandler(autotrace_MouseHover);
            void autotrace_MouseHover(object sender, EventArgs e)
            {
                ToolTip ToolTip1 = new ToolTip();
                ToolTip1.SetToolTip(autotrace, "Use AutoTrace Edge Detection (by Martin Weber)\nTLDR: Much less accurate but has centerline tracing (no double line)\nMeaning much less equations");
            }


            Label important_info = new Label();
            important_info.Location = new Point(form.Size.Width / 2 - 40, convert.Location.Y);
            important_info.AutoSize = true;
            important_info.Font = new Font("Segoe UI", 8, FontStyle.Regular);
            important_info.Text = "Using Potrace with a coloured image will result in quadruple lines\nbecause it is passed through both Canny and Potrace";
            form.Controls.Add(important_info);

            Label morph = new Label();
            morph.Location = new Point(important_info.Location.X + important_info.Width + 20, convert.Location.Y);
            morph.AutoSize = true;
            morph.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            morph.Text = "Morph Size:";
            form.Controls.Add(morph);

            morph.MouseHover += new EventHandler(morph_hover);
            void morph_hover(object sender, EventArgs e)
            {
                ToolTip ToolTip1 = new ToolTip();
                ToolTip1.SetToolTip(morph, "MorphologyEx Size\nDo not input anything unless you really know what you are doing\nTLDR: The larger the value the less quality the output, which means less equation\nOnly applied to coloured image\nDefault value for Potrace is (1, 1), for AutoTrace is (2, 2)");
            }

            morph_size1.Size = new Size(20, convert.Height);
            morph_size1.Location = new Point(morph.Location.X + morph.Width, convert.Location.Y);
            morph_size1.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            morph_size1.Text = "";
            morph_size1.MaxLength = 1;
            form.Controls.Add(morph_size1);
            morph_size1.KeyPress += new KeyPressEventHandler(morph_size1_keypress);
            void morph_size1_keypress(object sender, KeyPressEventArgs e)
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                }
            }

            morph_size2.Size = new Size(20, convert.Height);
            morph_size2.Location = new Point(morph_size1.Location.X + morph_size1.Width + 10, convert.Location.Y);
            morph_size2.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            morph_size2.Text = "";
            morph_size2.MaxLength = 1;
            form.Controls.Add(morph_size2);
            morph_size2.KeyPress += new KeyPressEventHandler(morph_size2_keypress);
            void morph_size2_keypress(object sender, KeyPressEventArgs e)
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                {
                    e.Handled = true;
                }
            }



            TextBox t = Application.OpenForms["Main Form"].Controls["txtbox1"] as TextBox;
            t.TextChanged += new EventHandler(check_image);

            void check_image(object sender, EventArgs e)
            {
                Function gray = new Function();
                bool gray_or_not;
                Image img = Image.FromFile(t.Text);
                gray_or_not = gray.IsGrayScale(img);
                img.Dispose();
                if (gray_or_not == false)
                    colour.Checked = true;  
                else
                    colour.Checked = false;
            }


            convert.Click += new EventHandler(convert_Click);
            void convert_Click(object sender, EventArgs e)
            {
                if (top.filedirectory_GetText() == "")
                {
                    MessageBox.Show("Please enter a file directory");
                    return;
                }

                if (File.Exists(top.filedirectory_GetText()) == false)
                {
                    MessageBox.Show("The file does not exist");
                    return;
                }

                if (potrace.Checked == autotrace.Checked)
                {
                    MessageBox.Show("Please select either AutoTrace or Potrace");
                    return;
                }


                Mat ver_flipped = new Mat();

                if (colour.Checked == true)
                {
                    Image<Gray, Byte> greyImg = new Image<Gray, Byte>(top.filedirectory_GetText());
                    Image<Gray, Byte> blurredImg = greyImg.SmoothGaussian(5, 5, 0, 0);
                    UMat cannyImg = new UMat();
                    CvInvoke.Canny(blurredImg, cannyImg, 50, 150);
                    CvInvoke.Flip(cannyImg, ver_flipped, FlipType.Vertical);
                    greyImg.Dispose();
                    blurredImg.Dispose();
                    cannyImg.Dispose();
                }
                else
                {
                    Image<Gray, Byte> OG_img = new Image<Gray, Byte>(top.filedirectory_GetText());
                    CvInvoke.Flip(OG_img, ver_flipped, FlipType.Vertical);
                    OG_img.Dispose();
                }

                if (potrace.Checked == true)
                {
                    Mat morphed = new Mat();
                    var cannyImg_bitmap = ver_flipped.ToBitmap();

                    if (colour.Checked == true)
                    {
                        Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(1, 1), new Point(-1, -1));
                        if ((morph_size1.Text != "") && (morph_size2.Text != ""))
                        {
                            int x = Int32.Parse(morph_size1.Text);
                            int y = Int32.Parse(morph_size2.Text);
                            kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(x, y), new Point(-1, -1));
                        }
                        CvInvoke.MorphologyEx(ver_flipped, morphed, MorphOp.Close, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
                        cannyImg_bitmap = morphed.ToBitmap();

                    }
                    morphed.Dispose();
                    ver_flipped.Dispose();

                    var binary_canny = Potrace.BitMapToBinary(cannyImg_bitmap, 120);
                    ArrayList ListOfCurveArrays = new ArrayList();
                    Potrace.potrace_trace(binary_canny, ListOfCurveArrays);

                    ArrayList parametric_array = new ArrayList();

                    for (int i = 0; i < ListOfCurveArrays.Count; i++)
                    {
                        ArrayList CurveArray = (ArrayList)ListOfCurveArrays[i];
                        for (int j = 0; j < CurveArray.Count; j++)
                        {
                            Potrace.Curve[] Curves = (Potrace.Curve[])CurveArray[j];
                            for (int k = 0; k < Curves.Length; k++)
                            {
                                if (Curves[k].Kind == Potrace.CurveKind.Bezier)
                                {
                                    string x = "(" + (float)Curves[k].A.X + "(1 - t)^3 + " + (float)Curves[k].ControlPointA.X * 3 + "t(1 - t)^2 + " + (float)Curves[k].ControlPointB.X * 3 + "t^2(1 - t) + " + (float)Curves[k].B.X + "t^3" + ",";
                                    string y = (float)Curves[k].A.Y + "(1 - t)^3 + " + (float)Curves[k].ControlPointA.Y * 3 + "t(1 - t)^2 + " + (float)Curves[k].ControlPointB.Y * 3 + "t^2(1 - t) + " + (float)Curves[k].B.Y + "t^3" + ")";
                                    string line = x + y;
                                    parametric_array.Add(line);
                                }
                                else
                                {
                                    string line = "(" + (float)Curves[k].A.X + "(1 - t) + " + (float)Curves[k].B.X + "t, " + (float)Curves[k].A.Y + "(1 - t) + " + (float)Curves[k].B.Y + "t)";
                                    parametric_array.Add(line);
                                }
                            }
                        }
                    }

                    int count = 1;

                    string file_name = Path.GetFileNameWithoutExtension(top.filedirectory_GetText());
                    while (File.Exists("Output/" + file_name + "-potrace-output.txt"))
                    {
                        string file_renamed = string.Format(file_name + " {0}", count++);
                        file_name = file_renamed;
                    }
                    string file_directory = "Output/" + file_name + "-potrace-output.txt";
                    File.WriteAllLines(file_directory, parametric_array.Cast<string>());

                    main.Text = File.ReadAllText("Output/" + file_name + "-potrace-output.txt");
                }



                if (autotrace.Checked == true)
                {
                    if (colour.Checked == true)
                    {
                        Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(2, 2), new Point(-1, -1));
                        Mat morphed = new Mat();
                        if ((morph_size1.Text != "") && (morph_size2.Text != ""))
                        {
                            int x = Int32.Parse(morph_size1.Text);
                            int y = Int32.Parse(morph_size2.Text);
                            kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(x, y), new Point(-1, -1));
                        }
                        CvInvoke.MorphologyEx(ver_flipped, morphed, MorphOp.Close, kernel, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());

                        Image<Gray, Byte> morph_image = new Image<Gray, byte>(morphed.Width, morphed.Height);
                        morph_image = morphed.ToImage<Gray, Byte>();

                        for (int x = 0; x < morphed.Width; x++) //Convert Black to White vice versa
                        {
                            for (int y = 0; y < morphed.Height; y++)
                            {
                                byte Gray = morph_image.Data[y, x, 0]; //x, y are inverted in Data array
                                morph_image.Data[y, x, 0] = (byte)(255 - Gray);
                            }
                        }

                        morph_image.Save("Output/morphed.pbm");

                        morph_image.Dispose();
                        morphed.Dispose();
                    }
                    else
                    {
                        Image<Gray, Byte> img = ver_flipped.ToImage<Gray, Byte>();
                        for (int x = 0; x < img.Width; x++) //Convert all Gray to Black
                        {
                            for (int y = 0; y < img.Height; y++)
                            {
                                byte Gray = img.Data[y, x, 0]; //x, y are inverted in Data array
                                if (Gray < 130)
                                    img.Data[y, x, 0] = (byte)(0);
                                else
                                    img.Data[y, x, 0] = (byte)(255);
                            }
                        }
                        Image<Gray, Byte> scaled = img.Resize(img.Width * 10, img.Height * 10, Inter.Linear);
                        img.Save("Output/morphed.pbm");

                        img.Dispose();
                    }

                    ver_flipped.Dispose();

                    Process process = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = "cmd.exe";
                    startInfo.CreateNoWindow = true;

                    startInfo.Arguments = "/C autotrace -centerline -output-file Output/output.svg -input-format pbm Output/morphed.pbm";
                    process.StartInfo = startInfo;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd();
                    Console.WriteLine(output);
                    process.WaitForExit();


                    string thirdLine;
                    using (var reader = new StreamReader("Output/output.svg"))
                    {
                        reader.ReadLine();
                        reader.ReadLine();
                        thirdLine = reader.ReadLine();
                    }

                    ArrayList array = new ArrayList();

                    for (int int_start = 0; int_start < thirdLine.Length; int_start++)
                    {
                        int index_M = thirdLine.IndexOf("M", int_start);
                        int_start = index_M + 1;
                        if (index_M == -1)
                            break;
                        array.Add(index_M);
                    }

                    for (int int_start = 0; int_start < thirdLine.Length; int_start++)
                    {
                        int index_C = thirdLine.IndexOf("C", int_start);
                        int_start = index_C + 1;
                        if (index_C == -1)
                            break;
                        array.Add(index_C);
                    }

                    for (int int_start = 0; int_start < thirdLine.Length; int_start++)
                    {
                        int index_L = thirdLine.IndexOf("L", int_start);
                        int_start = index_L + 1;
                        if (index_L == -1)
                            break;
                        array.Add(index_L);
                    }

                    array.Sort();

                    string current_positionX = "";
                    string current_positionY = "";

                    ArrayList parametric_array = new ArrayList();

                    for (int i = 0; i < array.Count; i++)
                    {
                        int first_char_of_command = (int)array[i];
                        string command;
                        if (i == array.Count - 1)
                        {
                            command = thirdLine.Substring(first_char_of_command, thirdLine.Length - first_char_of_command - 3);
                        }
                        else
                        {
                            int first_char_of_next_command = (int)array[i + 1];
                            command = thirdLine.Substring(first_char_of_command, first_char_of_next_command - first_char_of_command);
                        }

                        string previous_positionX;
                        string previous_positionY;
                        string command_word = command.Substring(0, 1);
                        if (command_word == "M")
                        {
                            int index_space = command.IndexOf(" ");
                            previous_positionX = current_positionX;
                            previous_positionY = current_positionY;
                            current_positionX = command.Substring(1, index_space - 1);
                            current_positionY = command.Substring(index_space + 1);
                        }
                        else if (command_word == "L")
                        {
                            int index_space = command.IndexOf(" ");
                            previous_positionX = current_positionX;
                            previous_positionY = current_positionY;
                            current_positionX = command.Substring(1, index_space - 1);
                            current_positionY = command.Substring(index_space + 1);
                            string result = "(" + current_positionX + "(1 - t) + " + previous_positionX + "t, " + current_positionY + "(1 - t) + " + previous_positionY + "t)";
                            parametric_array.Add(result);

                        }
                        else if (command_word == "C")
                        {
                            ArrayList space_array = new ArrayList();
                            for (int int_start = 0; int_start < command.Length; int_start++)
                            {
                                int index_space = command.IndexOf(" ", int_start);
                                int_start = index_space + 1;
                                if (index_space == -1)
                                    break;
                                space_array.Add(index_space);
                            }


                            string ControlA_X = command.Substring(1, (int)space_array[0] - 1);
                            string ControlA_Y = command.Substring((int)space_array[0] + 1, (int)space_array[1] - (int)space_array[0] - 1);
                            string ControlB_X = command.Substring((int)space_array[1] + 1, (int)space_array[2] - (int)space_array[1] - 1);
                            string ControlB_Y = command.Substring((int)space_array[2] + 1, (int)space_array[3] - (int)space_array[2] - 1);
                            string endpoint_X = command.Substring((int)space_array[3] + 1, (int)space_array[4] - (int)space_array[3] - 1);
                            string endpoint_Y = command.Substring((int)space_array[4] + 1);

                            previous_positionX = current_positionX;
                            previous_positionY = current_positionY;

                            current_positionX = endpoint_X;
                            current_positionY = endpoint_Y;

                            string parametric_X = "(" + previous_positionX + "(1 - t)^3 + " + float.Parse(ControlA_X) * 3 + "t(1 - t)^2 + " + float.Parse(ControlB_X) * 3 + "(1 - t)t^2 + " + current_positionX + "t^3" + ", ";
                            string parametric_Y = previous_positionY + "(1 - t)^3 + " + float.Parse(ControlA_Y) * 3 + "t(1 - t)^2 + " + float.Parse(ControlB_Y) * 3 + "(1 - t)t^2 + " + current_positionY + "t^3" + ")";
                            string result = parametric_X + parametric_Y;
                            parametric_array.Add(result);
                        }
                    }

                    File.Delete("Output/morphed.pbm");
                    File.Delete("Output/output.svg");

                    int count = 1;

                    string file_name = Path.GetFileNameWithoutExtension(top.filedirectory_GetText());
                    while (File.Exists("Output/" + file_name + "-potrace-output.txt"))
                    {
                        string file_renamed = string.Format(file_name + " {0}", count++);
                        file_name = file_renamed;
                        Console.WriteLine(file_renamed);
                    }
                    string file_directory = "Output/" + file_name + "-autotrace-output.txt";
                    File.WriteAllLines(file_directory, parametric_array.Cast<string>());

                    main.Text = File.ReadAllText("Output/" + file_name + "-autotrace-output.txt"); //could have tried to write each object in the arraylist to the textbox and idk but that takes very long to complete
                }
            }
        }
    }


    public partial class Form1 : Form
    {
        int screenWidth = Screen.PrimaryScreen.Bounds.Width;
        int screenHeight = Screen.PrimaryScreen.Bounds.Height;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TopControl topcontrol = new TopControl();
            Convert convert = new Convert();
            this.Text = "Image to Graph";
            this.Name = "Main Form";
            this.ResizeRedraw = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Size = new Size(screenWidth / 2, screenHeight / 2);
            this.Location = new Point(screenWidth / 4, screenHeight / 4);
            topcontrol.Top_Control(this);
            convert.ConvertButton(this);
        }
    }
}
