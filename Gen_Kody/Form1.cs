using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Zen.Barcode;

namespace Gen_Kody
{
    public partial class BCQRGen : Form
    {
        // Linijka kodu CIEŃ OKNA
        #region CIEŃ OKNA
        private bool Drag;
        private int MouseX;
        private int MouseY;

        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        private bool m_aeroEnabled;

        private const int CS_DROPSHADOW = 0x00020000;
        private const int WM_NCPAINT = 0x0085;
        private const int WM_ACTIVATEAPP = 0x001C;

        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]

        public static extern int DwmIsCompositionEnabled(ref int pfEnabled);
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeightEllipse
            );

        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }
        protected override CreateParams CreateParams
        {
            get
            {
                m_aeroEnabled = CheckAeroEnabled();
                CreateParams cp = base.CreateParams;
                if (!m_aeroEnabled)
                    cp.ClassStyle |= CS_DROPSHADOW; return cp;
            }
        }
        private bool CheckAeroEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                int enabled = 0; DwmIsCompositionEnabled(ref enabled);
                return (enabled == 1) ? true : false;
            }
            return false;
        }
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCPAINT:
                    if (m_aeroEnabled)
                    {
                        var v = 2;
                        DwmSetWindowAttribute(this.Handle, 2, ref v, 4);
                        MARGINS margins = new MARGINS()
                        {
                            bottomHeight = 1,
                            leftWidth = 0,
                            rightWidth = 0,
                            topHeight = 0
                        }; DwmExtendFrameIntoClientArea(this.Handle, ref margins);
                    }
                    break;
                default: break;
            }
            base.WndProc(ref m);
            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT) m.Result = (IntPtr)HTCAPTION;
        }
        private void PanelMove_MouseDown(object sender, MouseEventArgs e)
        {
            Drag = true;
            MouseX = Cursor.Position.X - this.Left;
            MouseY = Cursor.Position.Y - this.Top;
        }
        private void PanelMove_MouseMove(object sender, MouseEventArgs e)
        {
            if (Drag)
            {
                this.Top = Cursor.Position.Y - MouseY;
                this.Left = Cursor.Position.X - MouseX;
            }
        }
        private void PanelMove_MouseUp(object sender, MouseEventArgs e) { Drag = false; }
        #endregion
        // Koniec linijki kodu CIEŃ OKNA

        public BCQRGen()
        {
            InitializeComponent();
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void guna2GradientButton1_Click(object sender, EventArgs e)
        {
            // GENEROWANIE KODU KRESKOWEGO
            lab_InputText.Text = "Tekst:  " + guna2TextBox2.Text;
            try
            {
                Zen.Barcode.Code128BarcodeDraw barcode = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                pictureBox1.Image = barcode.Draw(guna2TextBox2.Text, 100);
            }
            catch
            {
                MessageBox.Show("Pole nie może pozostać puste, nie potrafię wygenerować kodu, wpisz znaki", "Uwaga!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void guna2GradientButton2_Click(object sender, EventArgs e)
        {
            try
            {
                //Generowanie kodu QR
                lab_InputText.Text = "Tekst:  " + guna2TextBox2.Text;
                Zen.Barcode.CodeQrBarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.CodeQr;
                pictureBox1.Image = qrcode.Draw(guna2TextBox2.Text, 70);
            }
            catch
            {
                MessageBox.Show("Pole nie może pozostać puste, nie potrafię wygenerować kodu, wpisz znaki", "Uwaga!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btn_printCode_Click(object sender, EventArgs e)
        {
            if (printDialog1.ShowDialog() == DialogResult.OK)
            {
                //Ustawienia marginesów
                printDocument1.OriginAtMargins = true;
                //link print Dialog & Print Document
                printDialog1.Document = printDocument1;
                printDocument1.Print();
            }
        }

        private void guna2ComboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (guna2ComboBox1.Text == "Bilet cementowy")
                {
                    guna2TextBox2.Text = "@B11@01ETSC-9001C@02J3280920 @030  @04XX@05 720128";
                }
                if (guna2ComboBox1.Text == "Identyfikator QR")
                {
                    guna2TextBox2.Text = "@B04@01ACP100000 @02ACP100000";
                }
                if (guna2ComboBox1.Text == "Identyfikator Kreskowy")
                {
                    guna2TextBox2.Text = "FACP100000";
                }
            }
            catch
            {
                MessageBox.Show("Informacja!", "Dane zostały prawdopodobnie źle wprowadzone", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        private void guna2GradientButton3_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.ShowDialog();

                Image smartImage = Image.FromFile(openFileDialog1.FileName);
                //Wyświetl zdjęcie
                pictureBox1.Image = smartImage;
            }
            catch
            {
                MessageBox.Show("Nie załadowałeś żadnego pliku. Spróbuj ponownie", "Brak zdjęcia", MessageBoxButtons.OK, MessageBoxIcon.Question);
            }
        }

        private void guna2GradientButton4_Click(object sender, EventArgs e)
        {
            try
            {
                Process Printjob = new Process();

                Printjob.StartInfo.FileName = openFileDialog1.FileName;
                Printjob.StartInfo.UseShellExecute = true;
                Printjob.StartInfo.Verb = "print";
                Printjob.Start();
            }
            catch
            {
                MessageBox.Show("Nie załadowałeś zdjęcia! Ta opcja działa po załadowaniu zdjęcia.", "Uwaga!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Bitmap Mybitmap = new Bitmap(guna2GradientPanel1.Width, guna2GradientPanel1.Height);

            guna2GradientPanel1.DrawToBitmap(Mybitmap, new Rectangle(0, 0, guna2GradientPanel1.Width, guna2GradientPanel1.Height));
            e.Graphics.DrawImage(Mybitmap, 20, 20);
            // Czyszczenie z pamięci
            Mybitmap.Dispose();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                Bitmap img = new Bitmap(pictureBox1.Image);
                img.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Png);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btn_saveCode_Click(object sender, EventArgs e)
        {
            //saveFileDialog1.ShowDialog();
        }

        private void btn_savePic_Click(object sender, EventArgs e)
        {
            saveFileDialog1.ShowDialog();
        }

        private void btn_minimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
    }
}
