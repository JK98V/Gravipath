using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.IO;

namespace LiMiDe_DX_Engine
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region Переменные
        public Device device;
        public PresentParameters present = new PresentParameters();
        public Sprite sprite;

        public SettingsContainer SContainer = new SettingsContainer();
        public ContentContainer CContainer = new ContentContainer();
        public GameManager GManager = new GameManager();
        #endregion
        #region Инициализация Директа
        public Boolean DXInit(DepthFormat DF, DeviceType DT, CreateFlags CF)
        {
            //try
            //{
                present.Windowed = true;
                present.SwapEffect = SwapEffect.Discard;
                present.EnableAutoDepthStencil = true;
                present.AutoDepthStencilFormat = DF;
                present.MultiSample = MultiSampleType.FourSamples;
                device = new Device(0, DT, this, CF, present);
                sprite = new Sprite(device);

                return true;
            //}
            //catch
            //{
            //    return false;
            //}
        }
        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            #region Запуск инициализации DX11
            if (DXInit(DepthFormat.D24S8, DeviceType.Hardware, CreateFlags.HardwareVertexProcessing) == false)
            {
                MessageBox.Show("Ошибка инициализации DirectX 11. Программа будет закрыта.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
            #endregion

            SContainer.RealScreen = new PointF(this.Width, this.Height);
            SContainer.ScreenAsRa = new PointF(this.Width / SContainer.IdealScreen.X, this.Height / SContainer.IdealScreen.Y); // вычисляем соотношение сторон
            ContentLoader.RunWorkerAsync(); // вызываем загрузку контента
            Cursor.Hide(); // прячем основной курсор

            Update.Start(); // Запускаем процесс игры
            FPS_Checker.Start();
        }

        private void Update_Tick(object sender, EventArgs e) // основной контроль передан классу GameManager (GManager)
        {
            try
            {
                #region Подготовка к отрисовке
                device.BeginScene();
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, SContainer.ClearColor, 1f, 0);

                device.RenderState.CullMode = Cull.None; // отрисовка 3D объектов: все грани
                device.RenderState.ZBufferEnable = true; // двойная буферизация: да
                device.RenderState.NormalizeNormals = true; // стандартрные нормали: да

                device.RenderState.AlphaBlendEnable = true;
                device.RenderState.AlphaTestEnable = true;
                device.RenderState.ReferenceAlpha = 15;
                device.RenderState.AlphaFunction = Compare.Greater;
                device.RenderState.SourceBlend = Blend.SourceAlpha;
                device.RenderState.DestinationBlend = Blend.InvSourceAlpha;

                device.RenderState.FogColor = SContainer.ClearColor;
                device.RenderState.FogDensity = 100f;
                device.RenderState.FogStart = 15f;
                device.RenderState.FogEnd = 16f;
                device.RenderState.FogTableMode = FogMode.Linear;
                device.RenderState.FogEnable = true;

                device.Transform.View = Matrix.LookAtLH(SContainer.Camera3DPosition, SContainer.Camera3DDirection, new Vector3(0, 0, 1));
                device.Transform.Projection = Matrix.PerspectiveFovLH(1f, SContainer.IdealScreen.X / SContainer.IdealScreen.Y, 0.01f, 1000f);

                #region Стандартные источники фонового освещения
                device.Lights[0].Type = LightType.Directional;
                device.Lights[0].Diffuse = Color.White;
                device.Lights[0].Direction = new Vector3(1, 2, 3);
                device.Lights[0].Enabled = true;

                device.Lights[1].Type = LightType.Directional;
                device.Lights[1].Diffuse = Color.White;
                device.Lights[1].Direction = new Vector3(-1.5f, -2.5f, -3.5f);
                device.Lights[1].Enabled = true;
                #endregion
                #endregion
                // //////////////////////////////////////////////////////////////// отрисовка
                GManager.Draw3D(device, sprite, SContainer, CContainer); // функция для отрисовки 3D
                #region Отрисовка 2D объектов
                #region подготовка к отрисовке
                sprite.Begin(SpriteFlags.AlphaBlend);
                #endregion
                // /////////////////////////////////////////////////////////// отрисовка 2D

                GManager.Update(device, sprite, SContainer, CContainer); // передаём возможность управлять GameManager'у
                GManager.Draw(device, sprite, SContainer, CContainer); // передаём возможность рисовать GameManager'у

                // ///////////////////////////////////////////////////////////
                #region конец отрисовки
                sprite.End();
                #endregion
                #endregion
                // ////////////////////////////////////////////////////////////////
                #region Завершение отрисовки и вывод на экран
                device.EndScene();

                float AsRaMin = 0f;
                if (SContainer.ScreenAsRa.X < SContainer.ScreenAsRa.Y)
                {
                    AsRaMin = SContainer.ScreenAsRa.X;
                }
                else
                {
                    AsRaMin = SContainer.ScreenAsRa.Y;
                }

                device.Present(new Rectangle(Screen.PrimaryScreen.Bounds.Width / 2 - (int)(SContainer.IdealScreen.X * AsRaMin) / 2, Screen.PrimaryScreen.Bounds.Height / 2 - (int)(SContainer.IdealScreen.Y * AsRaMin) / 2, (int)(SContainer.IdealScreen.X * AsRaMin), (int)(SContainer.IdealScreen.Y * AsRaMin)), false); // сохраняем пропорции экрана
                //device.Present();
                #endregion
            }
            catch
            {
            }
        }

        #region Загрузчик и счётчик FPS
        private void ContentLoader_DoWork(object sender, DoWorkEventArgs e) // производит загрузку контента в бэкграунде
        {
            #region удаляем предыдущий лог
            if (File.Exists(SContainer.PathToContent + "/ScriptLog.txt") == true)
            {
                File.Delete(SContainer.PathToContent + "/ScriptLog.txt");
            }
            GManager.scriptController.SetDebugConsole(SContainer, CContainer, System.Environment.UserName);
            GManager.scriptController.SetDebugConsole(SContainer, CContainer, System.Environment.MachineName);
            GManager.scriptController.SetDebugConsole(SContainer, CContainer, System.Environment.OSVersion.ToString());
            GManager.scriptController.SetDebugConsole(SContainer, CContainer, SContainer.EDEL_Version);
            GManager.scriptController.SetDebugConsole(SContainer, CContainer, "Начало ведения журнала");
            #endregion

            CContainer.ContentLoader(device, SContainer).ToString(); // загружаем контент
            CContainer.LanguageLoader(SContainer);
            GManager.NextLocation("banner");

            //GManager.scriptController.LoadScript(SContainer, CContainer, device, "start"); // запуск главного скрипта
        }
        private void FPS_Checker_Tick(object sender, EventArgs e) // замер FPS и расчёт коэфицента стабилизации FPS
        {
            SContainer.FPS_Now = SContainer.FPS;
            SContainer.FPS = 0f;
            SContainer.FPS_Stab = SContainer.FPS_Default / SContainer.FPS_Now;
        }
        #endregion
        #region Информация от устройств ввода
        #region KeyDown
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) { Application.Exit(); } // Экстренный выход из игры
            if (e.KeyCode == Keys.F1)
            {
                if (SContainer.DebugConsoleShow == false)
                {
                    SContainer.DebugConsoleShow = true;
                }
                else
                {
                    SContainer.DebugConsoleShow = false;
                }
            }

            #region KEY
            Boolean value = true;
            if (e.KeyCode == Keys.W) { SContainer.KEY_W = value; }
            if (e.KeyCode == Keys.A) { SContainer.KEY_A = value; }
            if (e.KeyCode == Keys.S) { SContainer.KEY_S = value; }
            if (e.KeyCode == Keys.D) { SContainer.KEY_D = value; }
            if (e.KeyCode == Keys.ShiftKey) { SContainer.KEY_SHIFT = value; }
            if (e.KeyCode == Keys.ControlKey) { SContainer.KEY_CTRL = value; }
            if (e.KeyCode == Keys.Space) { SContainer.KEY_SPACE = value; }
            #endregion

            //if (e.KeyCode == Keys.Left) { SContainer.CameraPosition.X -= 5f; }
            //if (e.KeyCode == Keys.Right) { SContainer.CameraPosition.X += 5f; }
            //if (e.KeyCode == Keys.Up) { SContainer.CameraPosition.Y -= 5f; }
            //if (e.KeyCode == Keys.Down) { SContainer.CameraPosition.Y += 5f; }
        }
        #endregion
        #region KeyUp
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            #region KEY
            Boolean value = false;
            if (e.KeyCode == Keys.W) { SContainer.KEY_W = value; }
            if (e.KeyCode == Keys.A) { SContainer.KEY_A = value; }
            if (e.KeyCode == Keys.S) { SContainer.KEY_S = value; }
            if (e.KeyCode == Keys.D) { SContainer.KEY_D = value; }
            if (e.KeyCode == Keys.ShiftKey) { SContainer.KEY_SHIFT = value; }
            if (e.KeyCode == Keys.ControlKey) { SContainer.KEY_CTRL = value; }
            if (e.KeyCode == Keys.Space) { SContainer.KEY_SPACE = value; }
            #endregion
        }
        #endregion
        #region MouseDown
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                SContainer.MouseLeft = true;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                SContainer.MouseRight = true;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                SContainer.MouseMiddle = true;
            }
        }
        #endregion
        #region MouseUp
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                SContainer.MouseLeft = false;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                SContainer.MouseRight = false;
            }
            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                SContainer.MouseMiddle = false;
            }
        }
        #endregion
        #endregion
    }
}
