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
    #region Классы используемые именно этой игрой
    #region GraviPointPart
    public class GraviPointPart
    {
        public float position = 0f; // позиция в кругу
        public float radius = 1f; // радиус от центра 
        public float size = 1f; // радиус частицы
        public Color color = Color.Black; // цвет

        #region конструктор
        public GraviPointPart(float position, float radius, float size, Color color)
        {
            this.position = position;
            this.radius = radius;
            this.size = size;
            this.color = color;
        }
        #endregion
    }
    #endregion
    #region GraviPoint
    public class GraviPoint
    {
        public Vector2 Center = new Vector2(0f, 0f); // центр гравиточки
        public float Radius = 10f; // радиус в пределах которого действует гравитация гравиточки
        public float Power = 1f; // сила гравитации
        public Color color = Color.Black; // цвет гравиточки

        Random RAN = new Random();

        #region Distance
        float Distance(Vector2 p1, Vector2 p2)
        {
            return (float)(Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y)));
        }
        #endregion

        public List<GraviPointPart> Parts = new List<GraviPointPart>();
        #region Update
        public void Update()
        {
            #region создание частиц
            for (Int32 ii = 0; ii < 1; ii++)
            {
                float position = RAN.Next(0, 628) / 100f; // случайная точка на позиции (в радианах, получи число от 0 до 6,28 (0-360))
                if (RAN.Next(0, 5) == 0)
                Parts.Add(new GraviPointPart(position, 100f, 10f, Color.FromArgb(0, color)));
                    //Parts.Add(new GraviPointPart(position, Radius, Radius / 10f / 1f, Color.FromArgb(0, RAN.Next(0, 256), RAN.Next(0, 256), RAN.Next(0, 256))));
            }
            #endregion
            #region передвижение частиц
            for (Int32 i = 0; i < Parts.Count; i++)
            {

                Parts[i].position += (1f / 500f) * (200f / Parts[i].radius);
                //Parts[i].size -= 0.005f * Power;
                if (Parts[i].size < 1f) { Parts[i].size = 1f; }
                Parts[i].radius -= Power / 10f;
                if (Parts[i].radius < 0) { Parts[i].radius = 0f; }
                if (Parts[i].size > Radius / 2f) { Parts[i].size -= 0.5f; }
                if (Parts[i].color.A < 10) 
                {
                    Int32 A = Parts[i].color.A + 1;
                    if (A > 10) { A = 10; }
                    Parts[i].color = Color.FromArgb(A, Parts[i].color); 
                }

                #region удаление частиц
                if (Parts[i].radius <= 0f)
                {
                    Parts.RemoveAt(i);
                    i--;
                }
                #endregion
            }
            #endregion
        }
        #endregion

        #region конструктор
        public GraviPoint(Vector2 Center, float Radius, float Power, Color color)
        {
            this.Center = Center;
            this.Radius = Radius;
            this.Power = Power;
            this.color = color;
        }
        #endregion
    }
    #endregion
    #region Ball
    public class Ball
    {
        public Vector2 LastPosition = new Vector2(0, 0); // прошлая позиция
        public Vector2 Position = new Vector2(0, 0); // текущая позиция шара
        public Vector2 Vect = new Vector2(0, 0); // вектр движения шара
        public float Mass = 1f; // масса шара 0 - 1
        public float Friction = 0.5f; // коэфицент трения
        public float Size = 5f; // радиус шара
        public Color color = Color.White; // цвет шара

        #region конструктор
        public Ball(Vector2 Position, Vector2 Vect, float Mass, float Friction, float Size, Color color)
        {
            this.Position = Position;
            this.Vect = Vect;
            this.Mass = Mass;
            this.Friction = Friction;
            this.Size = Size;
            this.color = color;

            this.LastPosition = Position;
        }
        #endregion
    }
    #endregion
    #region Block
    public class Block
    {
        public Vector2 Position = new Vector2(0, 0);
        public Vector2 Size = new Vector2(10, 10);
        public Color color = Color.White;

        #region Конструктор
        public Block(Vector2 Position, Vector2 Size, Color color)
        {
            this.Position = Position;
            this.Size = Size;
            this.color = color;
        }
        #endregion
    }
    #endregion

    #region GameController
    public class GameController
    {
        public List<GraviPoint> graviPoints = new List<GraviPoint>();
        public List<Ball> balls = new List<Ball>();
        public List<Block> blocks = new List<Block>();

        #region CreateGraviPoint
        public void CreateGraviPoint(Vector2 Center, float Radius, float Power, Color color)
        {
            graviPoints.Add(new GraviPoint(Center, Radius, Power, color));
        }
        #endregion
        #region CreateBall
        public void CreateBall(Vector2 Position, Vector2 Vect, float Mass, float Friction, float Size, Color color)
        {
            balls.Add(new Ball(Position, Vect, Mass, Friction, Size, color));
        }
        #endregion
        #region CreateBlock
        public void CreateBlock(Vector2 Position, Vector2 Size, Color color)
        {
            blocks.Add(new Block(Position, Size, color));
        }
        #endregion

        #region GetBallNextVector // получает объект мяч и устанавливает в него координаты следующего шага (так же получает stepcoi - это переменная отвечающая за то, на какой промежуток времени будет проходить расчёт. Значение 0 - 1)
        public void GetBallNextVector(SettingsContainer SContainer, GameManager GManager, Ball ball, float stepcoi)
        {
            Vector2 Vect = new Vector2(0, 0);
            #region расчёт вектора относительно каждого гравипоинта
            for (Int32 j = 0; j < graviPoints.Count; j++)
            {
                float Dist = GManager.Distance(ball.Position, graviPoints[j].Center); // определяем расстояние между объектами
                if (Dist <= graviPoints[j].Radius) // если объект находится за радиусом, то нет смысла его обрабатывать
                {
                    Vector2 LV = GManager.GetLeadingVector(ball.Position, graviPoints[j].Center); // получаем вектр движения к гравипоинту


                    float COI = 1f - (Dist / graviPoints[j].Radius); // коэфицент домножения вектора в зависимости от дальности гравипоинта
                    float SPEED = graviPoints[j].Power * COI * 0.01f; // коэфицент скорости домноженый на стабилизирующее число (дабы небыло сверхфантастических скоростей)

                    SPEED *= (1 - ball.Mass); // домножаем скорость на массу шара

                    SPEED *= stepcoi; // стабилизируем

                    LV.X *= SPEED; // домножаем вектр
                    LV.Y *= SPEED;

                    Vect.X += LV.X; // складываем текущий вектр с общим вектором движения
                    Vect.Y += LV.Y;
                }
            }
            #endregion
            #region расчёт вектора в соответствии со столкновением с другими мячами
            for (Int32 i = 0; i < balls.Count; i++)
            {
                if (balls[i].Position != ball.Position) // ну типо если это не два одинаковых мяча, то просчитываем
                {
                    if (GManager.Distance(ball.Position, balls[i].Position) <= ball.Size / 2f + balls[i].Size / 2f) // проверка на столкновение
                    {

                        Vector2 LV = GManager.GetLeadingVector(ball.Position, balls[i].Position); // получаем вектор столкновения
                        
                        Vector2 LVB = GManager.GetLeadingVector(ball.Position, ball.Vect); // вектор движения мяча
                        Vector2 LVB2 = GManager.GetLeadingVector(balls[i].Position, balls[i].Vect); // вектор движения мяча с которым столкнулись
                        
                        float Depth = (balls[i].Size / 2f + ball.Size / 2f) - GManager.Distance(ball.Position, balls[i].Position); // глубина вхождения одного шара в другой
                        float MassDiff = balls[i].Mass / ball.Mass; // коэфицент того, во сколько раз масса другого объекта больше массы нашего объекта

                        Vect.X += (float)(LVB2.X * LV.X) * MassDiff;
                        Vect.Y += (float)(LVB2.Y * LV.Y) * MassDiff;

                        //balls[i].Vect.X -= (float)(LVB2.X * LV.X) * (1f - MassDiff);
                        //balls[i].Vect.Y -= (float)(LVB2.X * LV.Y) * (1f - MassDiff);

                    }
                }
            }
            #endregion
            #region расчёт вектора в соответствии со столкновением со стенами
            for (Int32 i = 0; i < blocks.Count; i++)
            {

                RectangleF blockrect = new RectangleF(blocks[i].Position.X - blocks[i].Size.X, blocks[i].Position.Y - blocks[i].Size.Y, blocks[i].Size.X * 2f, blocks[i].Size.Y * 2f);
                RectangleF ballrect = new RectangleF(ball.Position.X - ball.Size / 2f - 1, ball.Position.Y - ball.Size / 2f - 1, ball.Size + 2, ball.Size + 2);
                if (blockrect.IntersectsWith(ballrect) == true)
                {

                    #region
                    float Depth = 0f;
                    Vector2 pointpos = new Vector2(0, 0);

                    #region расчёт pointpos
                    if (ball.Position.X <= blocks[i].Position.X && ball.Position.Y >= blocks[i].Position.Y - blocks[i].Size.Y / 2f && ball.Position.Y <= blocks[i].Position.Y + blocks[i].Size.Y / 2f)
                    {
                        pointpos.X = ball.Size / 2f + ball.Position.X;
                        pointpos.Y = ball.Position.Y;
                        float Dist = GManager.Distance(ball.Position, blocks[i].Position);
                        Depth = Dist - (ball.Size / 2f + blocks[i].Size.X / 2f);
                    }
                    else if (ball.Position.X >= blocks[i].Position.X && ball.Position.Y >= blocks[i].Position.Y - blocks[i].Size.Y / 2f && ball.Position.Y <= blocks[i].Position.Y + blocks[i].Size.Y / 2f)
                    {
                        pointpos.X = -ball.Size / 2f + ball.Position.X;
                        pointpos.Y = ball.Position.Y;
                        float Dist = GManager.Distance(ball.Position, blocks[i].Position);
                        Depth = Dist - (ball.Size / 2f + blocks[i].Size.X / 2f);
                    }
                    else if (ball.Position.Y <= blocks[i].Position.Y && ball.Position.X >= blocks[i].Position.X - blocks[i].Size.X / 2f && ball.Position.X <= blocks[i].Position.X + blocks[i].Size.X / 2f)
                    {
                        pointpos.X = ball.Position.X;
                        pointpos.Y = ball.Size / 2f + ball.Position.Y;
                        float Dist = GManager.Distance(ball.Position, blocks[i].Position);
                        Depth = Dist - (ball.Size / 2f + blocks[i].Size.Y / 2f);
                    }
                    else if (ball.Position.Y >= blocks[i].Position.Y && ball.Position.X >= blocks[i].Position.X - blocks[i].Size.X / 2f && ball.Position.X <= blocks[i].Position.X + blocks[i].Size.X / 2f)
                    {
                        pointpos.X = ball.Position.X;
                        pointpos.Y = -ball.Size / 2f + ball.Position.Y;
                        float Dist = GManager.Distance(ball.Position, blocks[i].Position);
                        Depth = Dist - (ball.Size / 2f + blocks[i].Size.Y / 2f);
                    }
                    else if (ball.Position.X <= blocks[i].Position.X && ball.Position.Y <= blocks[i].Position.Y)
                    {
                        Vector2 LV = GManager.GetLeadingVector(ball.Position, new Vector2(blocks[i].Position.X - blocks[i].Size.X, blocks[i].Position.Y - blocks[i].Size.Y));
                        pointpos = new Vector2(LV.X * ball.Size / 2f + ball.Position.X, LV.Y * ball.Size / 2f + ball.Position.Y);
                        float Dist = GManager.Distance(ball.Position, new Vector2(blocks[i].Position.X - blocks[i].Size.X, blocks[i].Position.Y - blocks[i].Size.Y));
                        Depth = ball.Size / 2f - Dist;
                    }
                    else if (ball.Position.X >= blocks[i].Position.X && ball.Position.Y >= blocks[i].Position.Y)
                    {
                        Vector2 LV = GManager.GetLeadingVector(ball.Position, new Vector2(blocks[i].Position.X + blocks[i].Size.X, blocks[i].Position.Y + blocks[i].Size.Y));
                        pointpos = new Vector2(LV.X * ball.Size / 2f + ball.Position.X, LV.Y * ball.Size / 2f + ball.Position.Y);
                        float Dist = GManager.Distance(ball.Position, new Vector2(blocks[i].Position.X + blocks[i].Size.X, blocks[i].Position.Y + blocks[i].Size.Y));
                        Depth = ball.Size / 2f - Dist;
                    }
                    else if (ball.Position.X <= blocks[i].Position.X && ball.Position.Y >= blocks[i].Position.Y)
                    {
                        Vector2 LV = GManager.GetLeadingVector(ball.Position, new Vector2(blocks[i].Position.X - blocks[i].Size.X, blocks[i].Position.Y + blocks[i].Size.Y));
                        pointpos = new Vector2(LV.X * ball.Size / 2f + ball.Position.X, LV.Y * ball.Size / 2f + ball.Position.Y);
                        float Dist = GManager.Distance(ball.Position, new Vector2(blocks[i].Position.X - blocks[i].Size.X, blocks[i].Position.Y + blocks[i].Size.Y));
                        Depth = ball.Size / 2f - Dist;
                    }
                    else if (ball.Position.X >= blocks[i].Position.X && ball.Position.Y <= blocks[i].Position.Y)
                    {
                        Vector2 LV = GManager.GetLeadingVector(ball.Position, new Vector2(blocks[i].Position.X + blocks[i].Size.X, blocks[i].Position.Y - blocks[i].Size.Y));
                        pointpos = new Vector2(LV.X * ball.Size / 2f + ball.Position.X, LV.Y * ball.Size / 2f + ball.Position.Y);
                        float Dist = GManager.Distance(ball.Position, new Vector2(blocks[i].Position.X + blocks[i].Size.X, blocks[i].Position.Y - blocks[i].Size.Y));
                        Depth = ball.Size / 2f - Dist;
                    }
                    #endregion

                    if (pointpos.X >= blockrect.X && pointpos.X <= blockrect.X + blockrect.Width && pointpos.Y >= blockrect.Y && pointpos.Y <= blockrect.Y + blockrect.Height)
                    {

                        Vector2 LV = GManager.GetLeadingVector(ball.Position, blocks[i].Position);
                        Vect.X += -(ball.Vect.X * 2f);
                        Vect.Y += -(ball.Vect.Y * 2f);

                        //ball.Position = ball.LastPosition;
                        //if (ball.Position.X <= blocks[i].Position.X && ball.Position.Y >= blocks[i].Position.Y - blocks[i].Size.Y / 2f && ball.Position.Y <= blocks[i].Position.Y + blocks[i].Size.Y / 2f)
                        //{
                        //    ball.Vect.X *= -1f;
                        //}
                        //else if (ball.Position.X >= blocks[i].Position.X && ball.Position.Y >= blocks[i].Position.Y - blocks[i].Size.Y / 2f && ball.Position.Y <= blocks[i].Position.Y + blocks[i].Size.Y / 2f)
                        //{
                        //    ball.Vect.X *= -1f;
                        //}
                        //else if (ball.Position.Y <= blocks[i].Position.Y && ball.Position.X >= blocks[i].Position.X - blocks[i].Size.X / 2f && ball.Position.X <= blocks[i].Position.X + blocks[i].Size.X / 2f)
                        //{
                        //    ball.Vect.Y *= -1f;
                        //}
                        //else if (ball.Position.Y >= blocks[i].Position.Y && ball.Position.X >= blocks[i].Position.X - blocks[i].Size.X / 2f && ball.Position.X <= blocks[i].Position.X + blocks[i].Size.X / 2f)
                        //{
                        //    ball.Vect.Y *= -1f;
                        //}
                        //else
                        //{
                        //    //Vector2 LVect = GManager.GetLeadingVector(ball.Position, pointpos);
                        //    //ball.Vect.X *= -LVect.X;
                        //    //ball.Vect.Y *= -LVect.Y;
                        //    Vect.X += -(LV.X * stepcoi * Depth);
                        //    Vect.Y += -(LV.Y * stepcoi * Depth);
                        //}

                        //Vector2 LV = GManager.GetLeadingVector(ball.Position, pointpos);

                        //Vect.X *= -LV.X;
                        //Vect.Y *= -LV.Y;

                        //Vect.X *= -LV.X;
                        //Vect.Y *= -LV.Y;
                        //Vect.X *= -1f;

                    }
                    #endregion
                }
            }
            #endregion
            ball.Vect.X += Vect.X; // прибавляем вектр к вектору мяча
            ball.Vect.Y += Vect.Y;

            Vector2 normalvect = GManager.GetLeadingVector(ball.Position, ball.Vect);
            normalvect.X = (float)Math.Abs(normalvect.X);
            normalvect.Y = (float)Math.Abs(normalvect.Y);

            if (ball.Vect.X > 0) { ball.Vect.X -= ball.Friction * stepcoi * normalvect.X; } // применяем трение
            if (ball.Vect.X < 0) { ball.Vect.X += ball.Friction * stepcoi * normalvect.X; }
            if (ball.Vect.Y > 0) { ball.Vect.Y -= ball.Friction * stepcoi * normalvect.Y; }
            if (ball.Vect.Y < 0) { ball.Vect.Y += ball.Friction * stepcoi * normalvect.Y; }

            if (Math.Abs(ball.Vect.X) < ball.Friction * stepcoi) { ball.Vect.X = 0f; } // что бы мяч не ехал бесконечно вперёд
            if (Math.Abs(ball.Vect.Y) < ball.Friction * stepcoi) { ball.Vect.Y = 0f; }

            ball.LastPosition.X = ball.Position.X; // сохраняем прошлую позицию
            ball.LastPosition.Y = ball.Position.Y;

            ball.Position.X += ball.Vect.X * stepcoi; // передвигаем мяч 
            ball.Position.Y += ball.Vect.Y * stepcoi;
        }
        #endregion

        #region Update
        public void Update(SettingsContainer SContainer, GameManager GManager)
        {
            #region передвижение мячей
            for (Int32 i = 0; i < balls.Count; i++)
            {
                for (Int32 j = 0; j < 5; j++) // точность просчёта
                {
                    GetBallNextVector(SContainer, GManager, balls[i], 0.2f);
                }
            }
            #endregion
        }
        #endregion
        #region Draw
        public void Draw(GameManager GManager, SettingsContainer SContainer, Sprite sprite, ContentContainer CContainer)
        {
            #region отрисовка гравиточек
            for (Int32 i = 0; i < graviPoints.Count; i++)
            {
                float size = 5f; // graviPoints[i].Radius / 10f;
                PointF center = new PointF(graviPoints[i].Center.X - size / 2, graviPoints[i].Center.Y - size / 2);
                GManager.EDraw(SContainer, sprite, CContainer.GetTextureByName("round"), new Rectangle(0, 0, 32, 32), new SizeF(size, size), center, graviPoints[i].color, true);
                graviPoints[i].Update();
                #region отрисовка частиц вокруг гравипоинта
                for (Int32 j = 0; j < graviPoints[i].Parts.Count; j++)
                {
                    GraviPointPart part = graviPoints[i].Parts[j];
                    Vector2 pos = new Vector2((float)(Math.Cos(part.position) * part.radius), (float)(Math.Sin(part.position) * part.radius));
                    pos.X += graviPoints[i].Center.X; //+ ((graviPoints[i].Radius / 10f) / 2f);
                    pos.Y += graviPoints[i].Center.Y; //+ ((graviPoints[i].Radius / 10f) / 2f);
                    GManager.EDraw(SContainer, sprite, CContainer.GetTextureByName("round"), new Rectangle(0, 0, 32, 32), new SizeF(part.size, part.size), new PointF(pos.X - part.size / 2, pos.Y - part.size / 2), part.color, true);
                    //GManager.EDraw(SContainer, sprite, CContainer.GetTextureByName("round"), new Rectangle(0, 0, 32, 32), new SizeF(5, 5), new PointF(pos.X, pos.Y), Color.White, true);
                }
                #endregion
                //GManager.DrawCircleP(SContainer, sprite, CContainer, graviPoints[i].Center, new Vector2(graviPoints[i].Radius + (float)(Math.Cos(SContainer.GameTime / 10f) * 10f), graviPoints[i].Radius + (float)(Math.Sin(SContainer.GameTime / 10f) * 10f)), 3f, SContainer.GameTime / 50f, graviPoints[i].color, 5, true);
                //GManager.DrawCircleP(SContainer, sprite, CContainer, graviPoints[i].Center, new Vector2(graviPoints[i].Radius + (float)(Math.Sin(SContainer.GameTime / 10f) * 10f), graviPoints[i].Radius + (float)(Math.Cos(SContainer.GameTime / 10f) * 10f)), 3f, SContainer.GameTime / 50f + 3.14f, graviPoints[i].color, 5, true);
                //GManager.DrawCircleP(SContainer, sprite, CContainer, graviPoints[i].Center, new Vector2(graviPoints[i].Radius, graviPoints[i].Radius), 100f, SContainer.GameTime / 50f + 3.14f, graviPoints[i].color, 5, true);
            }
            #endregion
            #region отрисовка мячей
            for (Int32 i = 0; i < balls.Count; i++)
            {
                GManager.EDraw(SContainer, sprite, CContainer.GetTextureByName("round"), new Rectangle(0, 0, 32, 32), new SizeF(balls[i].Size, balls[i].Size), new PointF(balls[i].Position.X - balls[i].Size / 2f, balls[i].Position.Y - balls[i].Size / 2f), balls[i].color, true);
                //GManager.DrawString(SContainer, sprite, CContainer, balls[i].Vect.ToString(), new PointF(balls[i].Position.X, balls[i].Position.Y), Color.Red, CContainer.DXFont10_15, true);
            }
            #endregion
            #region отрисовка блоков
            for (Int32 i = 0; i < blocks.Count; i++)
            {
                GManager.EDraw(SContainer, sprite, CContainer.GetTextureByName("pixel"), new Rectangle(0, 0, 2, 2), new SizeF(blocks[i].Size.X * 2, blocks[i].Size.Y * 2), new PointF(blocks[i].Position.X - blocks[i].Size.X, blocks[i].Position.Y - blocks[i].Size.Y), blocks[i].color, true);
            }
            #endregion
        }
        #endregion
    }
    #endregion

    #region GameResource
    public class GameResource
    {
        public float banner_location_time = 0f;
    }
    #endregion
    #endregion
    #region GameManager
    public class GameManager // основной класс игры, производит расчёты и отрисовки
    {
        public Random RAN = new Random();
        public PicturePartsController picturePController = new PicturePartsController();
        public PointPartsController pointPController = new PointPartsController();
        public GameResource GResource = new GameResource();
        public ScriptController scriptController = new ScriptController();

        public GameController gameController = new GameController();

        #region вспомогательные функции
        #region GetTimeNow
        public String GetTimeNow()
        {
            return "[" + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString() + ":" + DateTime.Now.Millisecond.ToString() + "]";
        }
        #endregion
        #region SetDebugConsole
        public void SetDebugConsole(SettingsContainer SContainer, ContentContainer CContainer, String text)
        {
            text = text.Replace("\r", "").Replace("\n", "");

            CContainer.DebugText.Add(text);
            CContainer.DebugTextFull.Add(text);

            if (File.Exists(SContainer.PathToContent + "/ScriptLog.txt") == false)
            {
                using (File.Create(SContainer.PathToContent + "/ScriptLog.txt")) ;
            }

            StreamWriter wr = new StreamWriter(SContainer.PathToContent + "/ScriptLog.txt", true);
            Int32 NULLPLUS = 18 - GetTimeNow().Length;
            String NULLSTRING = "";
            for (Int32 i = 0; i < NULLPLUS; i++)
            {
                NULLSTRING += " ";
            }
            wr.WriteLine(GetTimeNow() + NULLSTRING + " > " + text);
            wr.Close();
        }
        #endregion

        #region EDraw
        public void EDraw(SettingsContainer SContainer, Sprite sprite, Texture texture, Rectangle srcRectangle, SizeF destinationSize, PointF position, Color color, Boolean EffectOfCamera) // функция для вывода 2D ихзображений. Отличается автоматической подстройкой под экран. Идеальным разрешением экрана считать 1000 Х 800
        {
            if (texture != null)
            {
                RectangleF R1;
                RectangleF R2;
                if (EffectOfCamera == true)
                {
                    R1 = new RectangleF(0, 0, (SContainer.IdealScreen.X + SContainer.CameraPosition.X) * SContainer.ScreenAsRa.X, (SContainer.IdealScreen.Y + SContainer.CameraPosition.Y) * SContainer.ScreenAsRa.Y);
                    R2 = new RectangleF((position.X + SContainer.CameraPosition.X) * SContainer.ScreenAsRa.X, (position.Y + SContainer.CameraPosition.Y) * SContainer.ScreenAsRa.Y, destinationSize.Width * SContainer.ScreenAsRa.X, destinationSize.Height * SContainer.ScreenAsRa.Y);
                }
                else
                {
                    R1 = new RectangleF(0, 0, SContainer.IdealScreen.X * SContainer.ScreenAsRa.X, SContainer.IdealScreen.Y * SContainer.ScreenAsRa.Y);
                    R2 = new RectangleF(position.X * SContainer.ScreenAsRa.X, position.Y * SContainer.ScreenAsRa.Y, destinationSize.Width * SContainer.ScreenAsRa.X, destinationSize.Height * SContainer.ScreenAsRa.Y);
                }
                if (R1.IntersectsWith(R2) == true)
                {
                    try
                    {
                        if (EffectOfCamera == true)
                        {
                            sprite.Draw2D(texture, srcRectangle, new SizeF(destinationSize.Width * SContainer.ScreenAsRa.X, destinationSize.Height * SContainer.ScreenAsRa.Y), new PointF((position.X + SContainer.CameraPosition.X) * SContainer.ScreenAsRa.X, (position.Y + SContainer.CameraPosition.Y) * SContainer.ScreenAsRa.Y), color);
                        }
                        else
                        {
                            sprite.Draw2D(texture, srcRectangle, new SizeF(destinationSize.Width * SContainer.ScreenAsRa.X, destinationSize.Height * SContainer.ScreenAsRa.Y), new PointF(position.X * SContainer.ScreenAsRa.X, position.Y * SContainer.ScreenAsRa.Y), color);
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
        #endregion
        #region EDraw
        public void EDraw(SettingsContainer SContainer, Sprite sprite, Texture texture, Rectangle srcRectangle, SizeF destinationSize, PointF rotationCenter, float rotationAngle, PointF position, Color color, Boolean EffectOfCamera) // функция для вывода 2D ихзображений. Отличается автоматической подстройкой под экран. Идеальным разрешением экрана считать 1000 Х 800
        {
            if (texture != null)
            {
                RectangleF R1;
                RectangleF R2;
                if (EffectOfCamera == true)
                {
                    R1 = new RectangleF(0, 0, (SContainer.IdealScreen.X + SContainer.CameraPosition.X) * SContainer.ScreenAsRa.X, (SContainer.IdealScreen.Y + SContainer.CameraPosition.Y) * SContainer.ScreenAsRa.Y);
                    R2 = new RectangleF((position.X + SContainer.CameraPosition.X) * SContainer.ScreenAsRa.X, (position.Y + SContainer.CameraPosition.Y) * SContainer.ScreenAsRa.Y, destinationSize.Width * SContainer.ScreenAsRa.X, destinationSize.Height * SContainer.ScreenAsRa.Y);
                }
                else
                {
                    R1 = new RectangleF(0, 0, SContainer.IdealScreen.X * SContainer.ScreenAsRa.X, SContainer.IdealScreen.Y * SContainer.ScreenAsRa.Y);
                    R2 = new RectangleF(position.X * SContainer.ScreenAsRa.X, position.Y * SContainer.ScreenAsRa.Y, destinationSize.Width * SContainer.ScreenAsRa.X, destinationSize.Height * SContainer.ScreenAsRa.Y);
                }
                if (R1.IntersectsWith(R2) == true)
                {
                    try
                    {
                        if (EffectOfCamera == true)
                        {
                            sprite.Draw2D(texture, srcRectangle, new SizeF(destinationSize.Width * SContainer.ScreenAsRa.X, destinationSize.Height * SContainer.ScreenAsRa.Y), new PointF((rotationCenter.X + SContainer.CameraPosition.X) * SContainer.ScreenAsRa.X, (rotationCenter.Y + SContainer.CameraPosition.Y) * SContainer.ScreenAsRa.Y), rotationAngle, position, color);
                        }
                        else
                        {
                            sprite.Draw2D(texture, srcRectangle, new SizeF(destinationSize.Width * SContainer.ScreenAsRa.X, destinationSize.Height * SContainer.ScreenAsRa.Y), new PointF(rotationCenter.X * SContainer.ScreenAsRa.X, rotationCenter.Y * SContainer.ScreenAsRa.Y), rotationAngle, position, color);
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
        #endregion
        #region NextLoacation
        public void NextLocation(String location)
        {
            _NLNLocation = location;
            _NLState = "down";
        }
        #endregion
        #region переменные для переключения между локациями
        String _NLNLocation = "mainmenu";
        float _NLStep = 0f;
        String _NLState = "none";
        #endregion
        #region DrawString
        public void DrawString(SettingsContainer SContainer, Sprite sprite, ContentContainer CContainer, String text, PointF position, Color color, Microsoft.DirectX.Direct3D.Font DXFont, Boolean EffectOfCamera)
        {
            EDraw(SContainer, sprite, CContainer.GetTextureByName("pixel"), new Rectangle(0, 0, 2, 2), new SizeF(2, 2), new PointF(0, 0), Color.FromArgb(0, 0, 0, 0), EffectOfCamera); // текстура, к которой будет привязан текст, текст будет автоматически масштабироваться
            DXFont.DrawText(sprite, text, new Point((int)position.X, (int)position.Y), color);
        }
        #endregion
        #region DrawString
        public void DrawString(SettingsContainer SContainer, Sprite sprite, ContentContainer CContainer, String text, PointF position, Color color, Device device, Int32 Width, Int32 Height, Boolean EffectOfCamera)
        {
            EDraw(SContainer, sprite, CContainer.GetTextureByName("pixel"), new Rectangle(0, 0, 2, 2), new SizeF(2, 2), new PointF(0, 0), Color.FromArgb(0, 0, 0, 0), EffectOfCamera); // текстура, к которой будет привязан текст, текст будет автоматически масштабироваться
            //CContainer.DXFont.DrawText(sprite, text, position, color);
            FontDescription FD = new FontDescription();
            FD.Height = Height;
            FD.Width = Width;
            Microsoft.DirectX.Direct3D.Font FNT = new Microsoft.DirectX.Direct3D.Font(device, FD);
            FNT.DrawText(sprite, text, new Point((int)position.X, (int)position.Y), color);
            FNT.Dispose();
        }
        #endregion
        #region DrawLine
        public void DrawLine(Device device, SettingsContainer SContainer, Vector2 p1, Vector2 p2, Color color)
        {
            CustomVertex.TransformedColored[] verts = new CustomVertex.TransformedColored[2];
            verts[0].Color = color.ToArgb();
            verts[0].Position = new Vector4(p1.X * SContainer.ScreenAsRa.X, p1.Y * SContainer.ScreenAsRa.Y, 0f, 1f);
            verts[1].Color = color.ToArgb();
            verts[1].Position = new Vector4(p2.X * SContainer.ScreenAsRa.X, p2.Y * SContainer.ScreenAsRa.Y, 0f, 1f);
            device.DrawUserPrimitives(PrimitiveType.LineList, 1, verts);
        }
        #endregion
        #region DrawCircle
        public void DrawCircle(Device device, SettingsContainer SContainer, Vector2 center, Vector2 radius, Int32 degree, Color color, float rotation)
        {
            for (Int32 i = 0; i < degree; i++)
            {
                float X = 0f, Y = 0f;
                float X2 = 0f, Y2 = 0f;

                X = (float)(center.X + Math.Cos(6.28f / degree * i + rotation) * radius.X);
                Y = (float)(center.Y + Math.Sin(6.28f / degree * i + rotation) * radius.Y);
                X2 = (float)(center.X + Math.Cos(6.28f / degree * (i + 1) + rotation) * radius.X);
                Y2 = (float)(center.Y + Math.Sin(6.28f / degree * (i + 1) + rotation) * radius.Y);

                DrawLine(device, SContainer, new Vector2(X, Y), new Vector2(X2, Y2), Color.Red);
            }
        }
        #endregion
        #region DrawPoint
        public void DrawPoint(Device device, SettingsContainer SContainer, Vector2 position, Color color)
        {
            CustomVertex.TransformedColored[] verts = new CustomVertex.TransformedColored[1];
            verts[0].Color = color.ToArgb();
            verts[0].Position = new Vector4(position.X * SContainer.ScreenAsRa.X, position.Y * SContainer.ScreenAsRa.Y, 0f, 1f);
            device.DrawUserPrimitives(PrimitiveType.PointList, 1, verts);
        }
        #endregion
        #region Distance
        public float Distance(Vector2 p1, Vector2 p2)
        {
            return (float)(Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y)));
        }
        #endregion
        #region PointDistance3D
        public float PointDistance3D(Vector3 P1, Vector3 P2)
        {
            return (float)(Math.Sqrt((P1.X - P2.X) * (P1.X - P2.X) + (P1.Y - P2.Y) * (P1.Y - P2.Y) + (P1.Z - P2.Z) * (P1.Z - P2.Z)));
        }
        #endregion
        #region GetLeadingVector3D
        public Vector3 GetLeadingVector3D(Vector3 V1, Vector3 V2)
        {
            float SPX = (V2.X - V1.X) / PointDistance3D(V1, V2);
            float SPY = (V2.Y - V1.Y) / PointDistance3D(V1, V2);
            float SPZ = (V2.Z - V1.Z) / PointDistance3D(V1, V2);
            return new Vector3(SPX, SPY, SPZ);
        }
        #endregion
        #region GetLeadingVector
        public Vector2 GetLeadingVector(Vector2 V1, Vector2 V2)
        {
            float SPX = (V2.X - V1.X) / Distance(V1, V2);
            float SPY = (V2.Y - V1.Y) / Distance(V1, V2);
            return new Vector2(SPX, SPY);
        }
        #endregion
        #region DrawLineP
        public void DrawLineP(SettingsContainer SContainer, Sprite sprite, ContentContainer CContainer, Vector2 p1, Vector2 p2, Color color, float width, Boolean EffectOfCamera)
        {
            float DIST = Distance(p1, p2);
            Vector2 vect = GetLeadingVector(p1, p2);
            for (float i = 0; i <= DIST / width; i += 0.5f)
            {
                EDraw(SContainer, sprite, CContainer.GetTextureByName("round"), new Rectangle(0, 0, 32, 32), new SizeF(width, width), new PointF(vect.X * i * width + p1.X - width / 2, vect.Y * i * width + p1.Y - width / 2), color, EffectOfCamera);
            }
        }
        #endregion
        #region DrawLineP
        public void DrawLineP(SettingsContainer SContainer, Sprite sprite, ContentContainer CContainer, Vector2 p1, Vector2 p2, Color color, float width, float density, Boolean EffectOfCamera)
        {
            float DIST = Distance(p1, p2);
            Vector2 vect = GetLeadingVector(p1, p2);
            for (float i = 0; i <= DIST / width; i += density)
            {
                EDraw(SContainer, sprite, CContainer.GetTextureByName("round"), new Rectangle(0, 0, 32, 32), new SizeF(width, width), new PointF(vect.X * i * width + p1.X - width / 2, vect.Y * i * width + p1.Y - width / 2), color, EffectOfCamera);
            }
        }
        #endregion
        #region DrawCircleP
        public void DrawCircleP(SettingsContainer SContainer, Sprite sprite, ContentContainer CContainer, Vector2 center, Vector2 radius, float degree, float rotation, Color color, float width, Boolean EffectOfCamera)
        {
            for (Int32 i = 0; i < degree; i++)
            {
                float X = 0f, Y = 0f;
                float X2 = 0f, Y2 = 0f;

                X = (float)(center.X + Math.Cos(6.28f / degree * i + rotation) * radius.X);
                Y = (float)(center.Y + Math.Sin(6.28f / degree * i + rotation) * radius.Y);
                X2 = (float)(center.X + Math.Cos(6.28f / degree * (i + 1) + rotation) * radius.X);
                Y2 = (float)(center.Y + Math.Sin(6.28f / degree * (i + 1) + rotation) * radius.Y);

                DrawLineP(SContainer, sprite, CContainer, new Vector2(X, Y), new Vector2(X2, Y2), color, width, EffectOfCamera);
            }
        }
        #endregion
        #region DrawRectangleP
        public void DrawRectangleP(SettingsContainer SContainer, Sprite sprite, ContentContainer CContainer, RectangleF rect, Color color, float width, Boolean EffectOfCamera)
        {
            DrawLineP(SContainer, sprite, CContainer, new Vector2(rect.X, rect.Y), new Vector2(rect.X + rect.Width, rect.Y), color, width, EffectOfCamera);
            DrawLineP(SContainer, sprite, CContainer, new Vector2(rect.X, rect.Y), new Vector2(rect.X, rect.Y + rect.Height), color, width, EffectOfCamera);
            DrawLineP(SContainer, sprite, CContainer, new Vector2(rect.X + rect.Width, rect.Y), new Vector2(rect.X + rect.Width, rect.Y + rect.Height), color, width, EffectOfCamera);
            DrawLineP(SContainer, sprite, CContainer, new Vector2(rect.X, rect.Y + rect.Height), new Vector2(rect.X + rect.Width, rect.Y + rect.Height), color, width, EffectOfCamera);
        }
        #endregion
        #region DrawArcP
        public void DrawArcP(SettingsContainer SContainer, Sprite sprite, ContentContainer CContainer, Vector2 center, Vector2 radius, float max, float now, float rotation, Color color, float width, Boolean EffectOfCamera)
        {
            for (Int32 i = 0; i < max; i++)
            {
                if (i <= now)
                {
                    float X = 0f, Y = 0f;
                    float X2 = 0f, Y2 = 0f;

                    X = (float)(center.X + Math.Cos(6.28f / max * i + rotation) * radius.X);
                    Y = (float)(center.Y + Math.Sin(6.28f / max * i + rotation) * radius.Y);
                    X2 = (float)(center.X + Math.Cos(6.28f / max * (i + 1) + rotation) * radius.X);
                    Y2 = (float)(center.Y + Math.Sin(6.28f / max * (i + 1) + rotation) * radius.Y);

                    DrawLineP(SContainer, sprite, CContainer, new Vector2(X, Y), new Vector2(X2, Y2), color, width, EffectOfCamera);
                }
                else
                {
                    break;
                }
            }
        }
        #endregion

        #region DrawTexture3DViewToCam
        /// <summary>
        /// Данная текстура всегда будет смотреть на камеру, желательно сбрасывать матрицу перед отрисовкой
        /// </summary>
        /// <param name="device"></param>
        /// <param name="SContainer"></param>
        /// <param name="CContainer"></param>
        /// <param name="TextureName"></param>
        /// <param name="VU"></param>
        /// <param name="Option"></param>
        public void DrawTexture3DViewToCam(Device device, SettingsContainer SContainer, ContentContainer CContainer, String TextureName, Vector2 VU, String Option, Vector3 Position)
        {
            Vector3 Rotate = new Vector3(0, 0, 0);
            Rotate = GetLeadingVector3D(SContainer.Camera3DPosition, Position);

            device.Transform.World *= Matrix.Translation(-0.5f, -0.5f, 0);
            //device.Transform.World *= Matrix.RotationX(SContainer.GameTime / 200f);
            device.Transform.World *= Matrix.RotationY(3.14f / 2f);
            device.Transform.World *= Matrix.RotationZ((float)Math.Atan2(-SContainer.Camera3DDirection.X, SContainer.Camera3DDirection.Y) + 3.14f / 2f);
            device.Transform.World *= Matrix.Translation(Position);
            DrawTexture3D(device, CContainer, TextureName, new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), VU, "R90" + Option);
        }
        #endregion

        #region DrawTexture3D
        public void DrawTexture3D(Device device, ContentContainer CContainer, String TextureName, Vector3 V1, Vector3 V2, Vector3 V3, Vector3 V4, Vector2 VU, String Option)
        {
            // Option (Опция - один символ. Если нужно одновременно применить несколько опций, запишите в строку несколько символов)
            // N - без опций (можно просто передать пустую строку)
            // X - отражение текстуры по X
            // Y - отражение текстуры по Y
            // R90 - поворот на 90 градусов
            try
            {
                Texture TXTR = CContainer.GetTextureByName(TextureName);
                if (TXTR != null)
                {
                    #region поворот
                    if (Option.IndexOf("R90") >= 0)
                    {
                        Vector3 v1 = V2;
                        Vector3 v2 = V4;
                        Vector3 v3 = V1;
                        Vector3 v4 = V3;

                        V1 = v1;
                        V2 = v2;
                        V3 = v3;
                        V4 = v4;
                    }

                    if (Option.IndexOf("R180") >= 0)
                    {
                        for (Int32 i = 0; i < 2; i++)
                        {
                            Vector3 v1 = V2;
                            Vector3 v2 = V4;
                            Vector3 v3 = V1;
                            Vector3 v4 = V3;

                            V1 = v1;
                            V2 = v2;
                            V3 = v3;
                            V4 = v4;
                        }
                    }

                    if (Option.IndexOf("R270") >= 0)
                    {
                        for (Int32 i = 0; i < 3; i++)
                        {
                            Vector3 v1 = V2;
                            Vector3 v2 = V4;
                            Vector3 v3 = V1;
                            Vector3 v4 = V3;

                            V1 = v1;
                            V2 = v2;
                            V3 = v3;
                            V4 = v4;
                        }
                    }
                    #endregion

                    device.RenderState.Lighting = false;

                    device.SetTexture(0, TXTR);
                    Material mat = new Material();
                    mat.Diffuse = Color.FromArgb(100, 255, 255, 255);
                    device.Material = mat;

                    CustomVertex.PositionColoredTextured[] verts = new CustomVertex.PositionColoredTextured[3];

                    verts[0].Color = Color.FromArgb(100, 255, 255, 255).ToArgb();
                    verts[0].X = V1.X;
                    verts[0].Y = V1.Y;
                    verts[0].Z = V1.Z;
                    verts[0].Tu = 0;
                    verts[0].Tv = 0;

                    verts[1].Color = Color.FromArgb(100, 255, 255, 255).ToArgb();
                    verts[1].X = V2.X;
                    verts[1].Y = V2.Y;
                    verts[1].Z = V2.Z;
                    verts[1].Tu = -1 * VU.X;
                    verts[1].Tv = 0;

                    if (Option.IndexOf("X") >= 0)
                    {
                        verts[1].Tu = 1 * VU.X;
                    }

                    verts[2].Color = Color.FromArgb(100, 255, 255, 255).ToArgb();
                    verts[2].X = V3.X;
                    verts[2].Y = V3.Y;
                    verts[2].Z = V3.Z;
                    verts[2].Tu = 0;
                    verts[2].Tv = -1 * VU.Y;

                    if (Option.IndexOf("Y") >= 0)
                    {
                        verts[2].Tv = 1 * VU.Y;
                    }

                    device.DrawUserPrimitives(PrimitiveType.TriangleFan, 1, verts);

                    verts[0].Color = Color.FromArgb(100, 255, 255, 255).ToArgb();
                    verts[0].Position = V4;
                    verts[0].Tu = 0;
                    verts[0].Tv = 0;

                    verts[1].Color = Color.FromArgb(100, 255, 255, 255).ToArgb();
                    verts[1].Position = V3;
                    verts[1].Tu = 1 * VU.X;
                    verts[1].Tv = 0;

                    if (Option.IndexOf("X") >= 0)
                    {
                        verts[1].Tu = -1 * VU.X;
                    }

                    verts[2].Color = Color.FromArgb(100, 255, 255, 255).ToArgb();
                    verts[2].Position = V2;
                    verts[2].Tu = 0;
                    verts[2].Tv = 1 * VU.Y;

                    if (Option.IndexOf("Y") >= 0)
                    {
                        verts[2].Tv = -1 * VU.Y;
                    }

                    device.DrawUserPrimitives(PrimitiveType.TriangleFan, 1, verts);

                    device.RenderState.Lighting = true;
                }
            }
            catch
            {
            }
        }
        #endregion
        #region DrawTexture3D
        public void DrawTexture3D(Device device, ContentContainer CContainer, String TextureName, Vector2 VU, String Option)
        {
            DrawTexture3D(device, CContainer, TextureName, new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), VU, Option);
        }
        #endregion
        #region DrawCube3D 1 texture
        /// <summary>
        /// Передаётся имя одной текстуры которой будет покрыт весь блок
        /// </summary>
        /// <param name="device"></param>
        /// <param name="Size"></param>
        /// <param name="TextureName"></param>
        public void DrawCube3D(Device device, ContentContainer CContainer, Vector2 VU, String TextureName)
        {
            Vector3 Size = new Vector3(1f, 1f, 1f);

            DrawTexture3D(device, CContainer, TextureName, new Vector3(0 * Size.X, 1 * Size.Y, 1 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 1 * Size.Z), new Vector3(0 * Size.X, 1 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 0 * Size.Z), VU, "XY");
            DrawTexture3D(device, CContainer, TextureName, new Vector3(0 * Size.X, 0 * Size.Y, 1 * Size.Z), new Vector3(1 * Size.X, 0 * Size.Y, 1 * Size.Z), new Vector3(0 * Size.X, 0 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 0 * Size.Y, 0 * Size.Z), VU, "Y");
            DrawTexture3D(device, CContainer, TextureName, new Vector3(0 * Size.X, 1 * Size.Y, 0 * Size.Z), new Vector3(0 * Size.X, 0 * Size.Y, 0 * Size.Z), new Vector3(0 * Size.X, 1 * Size.Y, 1 * Size.Z), new Vector3(0 * Size.X, 0 * Size.Y, 1 * Size.Z), VU, "N");
            DrawTexture3D(device, CContainer, TextureName, new Vector3(1 * Size.X, 0 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 0 * Size.Y, 1 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 1 * Size.Z), VU, "N");

            DrawTexture3D(device, CContainer, TextureName, new Vector3(0 * Size.X, 0 * Size.Y, 1 * Size.Z), new Vector3(1 * Size.X, 0 * Size.Y, 1 * Size.Z), new Vector3(0 * Size.X, 1 * Size.Y, 1 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 1 * Size.Z), VU, "N");

            DrawTexture3D(device, CContainer, TextureName, new Vector3(0 * Size.X, 0 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 0 * Size.Y, 0 * Size.Z), new Vector3(0 * Size.X, 1 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 0 * Size.Z), VU, "X");
        }
        #endregion
        #region DrawCube3D 3 texture
        /// <summary>
        /// Передаётся имя трёх текстур: бока, верх, низ
        /// </summary>
        /// <param name="device"></param>
        /// <param name="Size"></param>
        /// <param name="TextureName"></param>
        public void DrawCube3D(Device device, ContentContainer CContainer, Vector2 VU, String TextureName1, String TextureName2, String TextureName3)
        {
            Vector3 Size = new Vector3(1f, 1f, 1f);

            DrawTexture3D(device, CContainer, TextureName1, new Vector3(0 * Size.X, 1 * Size.Y, 1 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 1 * Size.Z), new Vector3(0 * Size.X, 1 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 0 * Size.Z), VU, "XY");
            DrawTexture3D(device, CContainer, TextureName1, new Vector3(0 * Size.X, 0 * Size.Y, 1 * Size.Z), new Vector3(1 * Size.X, 0 * Size.Y, 1 * Size.Z), new Vector3(0 * Size.X, 0 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 0 * Size.Y, 0 * Size.Z), VU, "Y");
            DrawTexture3D(device, CContainer, TextureName1, new Vector3(0 * Size.X, 1 * Size.Y, 0 * Size.Z), new Vector3(0 * Size.X, 0 * Size.Y, 0 * Size.Z), new Vector3(0 * Size.X, 1 * Size.Y, 1 * Size.Z), new Vector3(0 * Size.X, 0 * Size.Y, 1 * Size.Z), VU, "N");
            DrawTexture3D(device, CContainer, TextureName1, new Vector3(1 * Size.X, 0 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 0 * Size.Y, 1 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 1 * Size.Z), VU, "N");

            DrawTexture3D(device, CContainer, TextureName2, new Vector3(0 * Size.X, 0 * Size.Y, 1 * Size.Z), new Vector3(1 * Size.X, 0 * Size.Y, 1 * Size.Z), new Vector3(0 * Size.X, 1 * Size.Y, 1 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 1 * Size.Z), VU, "N");

            DrawTexture3D(device, CContainer, TextureName3, new Vector3(0 * Size.X, 0 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 0 * Size.Y, 0 * Size.Z), new Vector3(0 * Size.X, 1 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 0 * Size.Z), VU, "X");
        }
        #endregion
        #region DrawCube3D 6 texture
        /// <summary>
        /// Передаётся имя шести текстур: бок1, бок2, бок3, бок4, верх, низ
        /// </summary>
        /// <param name="device"></param>
        /// <param name="Size"></param>
        /// <param name="TextureName"></param>
        public void DrawCube3D(Device device, ContentContainer CContainer, Vector2 VU, String TextureName1, String TextureName2, String TextureName3, String TextureName4, String TextureName5, String TextureName6)
        {
            Vector3 Size = new Vector3(1f, 1f, 1f);

            if (TextureName1 != null) DrawTexture3D(device, CContainer, TextureName1, new Vector3(0 * Size.X, 1 * Size.Y, 1 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 1 * Size.Z), new Vector3(0 * Size.X, 1 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 0 * Size.Z), VU, "XY");
            if (TextureName2 != null) DrawTexture3D(device, CContainer, TextureName2, new Vector3(0 * Size.X, 0 * Size.Y, 1 * Size.Z), new Vector3(1 * Size.X, 0 * Size.Y, 1 * Size.Z), new Vector3(0 * Size.X, 0 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 0 * Size.Y, 0 * Size.Z), VU, "Y");
            if (TextureName3 != null) DrawTexture3D(device, CContainer, TextureName3, new Vector3(0 * Size.X, 1 * Size.Y, 0 * Size.Z), new Vector3(0 * Size.X, 0 * Size.Y, 0 * Size.Z), new Vector3(0 * Size.X, 1 * Size.Y, 1 * Size.Z), new Vector3(0 * Size.X, 0 * Size.Y, 1 * Size.Z), VU, "N");
            if (TextureName4 != null) DrawTexture3D(device, CContainer, TextureName4, new Vector3(1 * Size.X, 0 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 0 * Size.Y, 1 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 1 * Size.Z), VU, "N");

            if (TextureName5 != null) DrawTexture3D(device, CContainer, TextureName5, new Vector3(0 * Size.X, 0 * Size.Y, 1 * Size.Z), new Vector3(1 * Size.X, 0 * Size.Y, 1 * Size.Z), new Vector3(0 * Size.X, 1 * Size.Y, 1 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 1 * Size.Z), VU, "N");

            if (TextureName6 != null) DrawTexture3D(device, CContainer, TextureName6, new Vector3(0 * Size.X, 0 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 0 * Size.Y, 0 * Size.Z), new Vector3(0 * Size.X, 1 * Size.Y, 0 * Size.Z), new Vector3(1 * Size.X, 1 * Size.Y, 0 * Size.Z), VU, "X");
        }
        #endregion
        #endregion

        #region LocationSwitching
        public void LocationSwitching(String NowL, String NextL)
        {
            #region game
            if (NowL == "mainmenu" && NextL == "game")
            {
                //cubeController.Cubes.Clear();
            }
            #endregion
        }
        #endregion

        #region Update
        public void Update(Device device, Sprite sprite, SettingsContainer SContainer, ContentContainer CContainer) // обновляет информацию и управляет логикой игры
        {

            SContainer.MousePosition = new Vector2(Cursor.Position.X / SContainer.ScreenAsRa.X - SContainer.CameraPosition.X, Cursor.Position.Y / SContainer.ScreenAsRa.Y - SContainer.CameraPosition.Y); // вычисляем положение курсора относительно идеального экрана

            //if (Cursor.Position.X <= 0) { Cursor.Position = new Point(Screen.PrimaryScreen.Bounds.Width, Cursor.Position.Y); }
            //else if (Cursor.Position.X >= Screen.PrimaryScreen.Bounds.Width - 1) { Cursor.Position = new Point(0, Cursor.Position.Y); }

            CContainer.SpritesNextStep(SContainer); // переключаем спрайты
            SContainer.GameTime += 1f * SContainer.FPS_Stab;
            SContainer.FPS += 1f;
            picturePController.Next(SContainer);
            pointPController.Next(SContainer);

            #region Расчёт поворота камеры
            ////if (SContainer.DebugConsoleShow == true) { SetDebugConsole(SContainer, CContainer, SContainer.CameraRotate.X.ToString()); }
            //SContainer.CameraRotate.X += (SContainer.OldCursorPosition.X - SContainer.NowCursorPosition.X) / (101.8f * SContainer.Camera3DSensitivity);
            //SContainer.CameraRotate.Y = ((SContainer.MousePosition.Y - SContainer.IdealScreen.Y / 2f) / SContainer.IdealScreen.Y) * 4f + SContainer.Camera3DPosition.Z;
            //SContainer.Camera3DDirection = new Vector3((float)Math.Cos(SContainer.CameraRotate.X) + SContainer.Camera3DPosition.X, (float)Math.Sin(SContainer.CameraRotate.X) + SContainer.Camera3DPosition.Y, SContainer.CameraRotate.Y);
            //SContainer.OldCursorPosition = SContainer.NowCursorPosition;
            //SContainer.NowCursorPosition = new Vector2(Cursor.Position.X, Cursor.Position.Y);
            ////SContainer.Camera3DDirection = new Vector3(5, 5, 0);
            #endregion
        }
        #endregion
        #region Draw
        public void Draw(Device device, Sprite sprite, SettingsContainer SContainer, ContentContainer CContainer) // управляет отрисовкой
        {
            #region примеры использования некоторых функций
            //DrawLine(device, SContainer, new Vector2(10, 10), SContainer.MousePosition, Color.Black);
            //DrawCircle(device, SContainer, SContainer.MousePosition, new Vector2(100, 100), 30, Color.Red, 0f);

            //DrawLineP(SContainer, sprite, CContainer, new Vector2(200, 200), SContainer.MousePosition, Color.Black, 2f);
            //DrawCircleP(SContainer, sprite, CContainer, SContainer.MousePosition, new Vector2(100, 100), 30, SContainer.GameTime / 50f, Color.Black, 2f);
            //DrawRectangleP(SContainer, sprite, CContainer, new RectangleF(100, 100, SContainer.MousePosition.X - 100, SContainer.MousePosition.Y - 100), Color.Black, 2f);

            //EDraw(SContainer, sprite, CContainer.GetTextureByName("RMB"), new Rectangle(0, 0, 128, 128), new SizeF(128, 128), new PointF(0, 0), Color.White); // примеры использования отрисовки
            //DrawString(SContainer, sprite, CContainer, "heLLo", new Point(0, 0), Color.White, device, 10, 15);

            //SetDebugConsole(SContainer, CContainer, "message"); // вывод сообщения в консоль дебага

            //scriptController.DrawScene(this, SContainer, sprite, CContainer); // Скриптинг отключён в этой версии движка.
            #endregion

            #region Локации
            #region loading
            if (SContainer.LOCATION == "loading") // локация загрузки. В этой локации запрещено использовать ресурсы из ContentContainer, ибо они ещё загружаются. 
            {
                SContainer.ClearColor = Color.Black;
                String DotString = "";
                #region количество точек
                for (Int32 i = 0; i < (int)(Math.Abs(Math.Cos(SContainer.GameTime / 10) * 20)); i++)
                {
                    DotString += ".";
                }
                #endregion
                DrawString(SContainer, sprite, CContainer, "Now loading" + DotString, new Point((int)(SContainer.IdealScreen.X / 2 - ("Now loading").Length * 30 / 2) - 200, (int)(SContainer.IdealScreen.Y / 2 - 45 / 2)), Color.White, device, 30, 45, false); // использовать можно, т.к. не связано с ContantContainer
            }
            #endregion
            #region banner
            if (SContainer.LOCATION == "banner")
            {
                SContainer.ClearColor = Color.Black;

                EDraw(SContainer, sprite, CContainer.GetTextureByName("DX11"), new Rectangle(0, 0, 256, 256), new SizeF(128, 128), new PointF(0, 0), Color.White, false);
                EDraw(SContainer, sprite, CContainer.GetTextureByName("LiMiDe"), new Rectangle(0, 0, 512, 512), new SizeF(128, 128), new PointF(0, 128), Color.White, false);

                EDraw(SContainer, sprite, CContainer.GetTextureByName("EDEL"), new Rectangle(0, 0, 256, 256), new SizeF(256, 256), new PointF(0, SContainer.IdealScreen.Y - 256), Color.White, false);
                EDraw(SContainer, sprite, CContainer.GetTextureByName("EDEL_VER"), new Rectangle(0, 0, 256, 256), new SizeF(128, 128), new PointF(SContainer.IdealScreen.X - 128, SContainer.IdealScreen.Y - 128), Color.White, false);

                //if (SContainer.MouseLeft == true)
                //{
                //    SContainer.MouseLeft = false;
                //    NextLocation("mainmenu");
                //    //SContainer.LOCATION = "mainmenu";

                //    //picturePController.TextureToPart(CContainer.GetTextureByName("EDEL"), new Point(16, 16), new Vector2(SContainer.IdealScreen.X / 2 - 512 / 2, SContainer.IdealScreen.Y / 2 - 512 / 2), new Vector2(512, 512), true, Color.White, false);
                //    //picturePController.TextureToPart(CContainer.GetTextureByName("EDEL_VER"), new Point(4, 4), new Vector2(SContainer.IdealScreen.X / 2 - 128 / 2, SContainer.IdealScreen.Y / 2 - 128 / 2 + 100), new Vector2(128, 128), true, Color.White, false);
                //    //picturePController.TextureToPart(CContainer.GetTextureByName("DX11"), new Point(4, 4), new Vector2(0, 0), new Vector2(128, 128), true, Color.White, false);
                //    //picturePController.TextureToPart(CContainer.GetTextureByName("LiMiDe"), new Point(4, 4), new Vector2(0, 128), new Vector2(128, 128), true, Color.White, false);
                //    //picturePController.TextureToPart(CContainer.GetTextureByName("LMB"), new Point(4, 4), new Vector2(SContainer.IdealScreen.X / 2 - 128 / 2, SContainer.IdealScreen.Y - 128 - 15), new Vector2(128, 128), true, Color.White, false);
                //}

                if (GResource.banner_location_time >= 50f)
                {
                    NextLocation("game");
                }

                GResource.banner_location_time += 10f * SContainer.FPS_Stab;
            }
            #endregion

            #region game
            if (SContainer.LOCATION == "game") // локация загрузки. В этой локации запрещено использовать ресурсы из ContentContainer, ибо они ещё загружаются. 
            {
                SContainer.ClearColor = Color.Black;

                gameController.Update(SContainer, this); // передаём управление игрой контроллеру1
                gameController.Draw(this, SContainer, sprite, CContainer); // передаём отрисовку контроллеру

                if (SContainer.MouseLeft == true)
                {
                    SContainer.MouseLeft = false;
                    gameController.CreateGraviPoint(SContainer.MousePosition, 10000f, 5f, Color.White);
                }
                if (SContainer.MouseRight == true)
                {
                    SContainer.MouseRight = false;
                    gameController.CreateBall(SContainer.MousePosition, new Vector2(0, 0), 0.5f, 0.01f, 10f, Color.White);
                }
                if (SContainer.MouseMiddle == true)
                {
                    SContainer.MouseMiddle = false;
                    gameController.CreateBlock(new Vector2((int)(SContainer.MousePosition.X / 20) * 20 + 10, (int)(SContainer.MousePosition.Y / 20) * 20 + 10), new Vector2(10, 10), Color.White);
                }

                #region отрисовка вектора от курсора
                Ball B = new Ball(SContainer.MousePosition, new Vector2(0, 0), 0.5f, 0.01f, 10f, Color.White);
                for (Int32 i = 0; i < 100; i++)
                {
                    Vector2 OLDPOS = B.Position;
                    gameController.GetBallNextVector(SContainer, this, B, 1f);
                    DrawLineP(SContainer, sprite, CContainer, OLDPOS, B.Position, Color.FromArgb(50, 255, 0, 0), 2f, true);
                }
                #endregion

                if (SContainer.KEY_SPACE == true)
                {
                    SContainer.KEY_SPACE = false;
                    gameController.blocks.Clear();
                    gameController.balls.Clear();
                    gameController.graviPoints.Clear();
                }
            }
            #endregion
            #endregion

            #region системная отрисовка (желательно не трогать)
            picturePController.Draw(this, SContainer, sprite, false);
            pointPController.Draw(this, device, SContainer, false);
            #region Затемнение экрана при переключении между локациями
            if (_NLState != "none")
            {
                if (_NLState == "down")
                {
                    if (_NLStep < 255)
                    {
                        _NLStep += 10f * SContainer.FPS_Stab;
                    }
                    else
                    {
                        LocationSwitching(SContainer.LOCATION, _NLNLocation);
                        SContainer.LOCATION = _NLNLocation;
                        _NLState = "up";
                        _NLStep = 255f;
                    }
                }
                if (_NLState == "up")
                {
                    if (_NLStep > 0)
                    {
                        _NLStep -= 10f * SContainer.FPS_Stab;
                    }
                    else
                    {
                        _NLState = "none";
                        _NLStep = 0f;
                    }
                }
                Int32 Alpha = (int)_NLStep;
                if (_NLStep < 0) { Alpha = 0; }
                if (_NLStep > 255) { Alpha = 255; }
                EDraw(SContainer, sprite, CContainer.GetTextureByName("pixel"), new Rectangle(0, 0, 2, 2), new SizeF(SContainer.IdealScreen.X, SContainer.IdealScreen.Y), new PointF(0, 0), Color.FromArgb(Alpha, 0, 0, 0), false);
            }
            #endregion
            DrawString(SContainer, sprite, CContainer, SContainer.FPS_Now.ToString(), new Point(0, 0), Color.FromArgb(127, 255, 255, 255), device, 10, 15, false);
            DrawString(SContainer, sprite, CContainer, SContainer.FPS_Now.ToString(), new Point(1, 1), Color.FromArgb(127, 0, 0, 0), device, 10, 15, false);
            picturePController.Draw(this, SContainer, sprite, true);
            //pointPController.Draw(this, device, SContainer, true);
            #region Отрисовка Debug консоли
            if (SContainer.DebugConsoleShow == true)
            {
                while (CContainer.DebugText.Count > 10)
                {
                    CContainer.DebugText.RemoveAt(0);
                }
                Int32 DSize = CContainer.DebugText.Count;
                if (DSize > 10) { DSize = 10; }
                EDraw(SContainer, sprite, CContainer.GetTextureByName("pixel"), new Rectangle(0, 0, 2, 2), new SizeF(SContainer.IdealScreen.X, DSize * 15), new PointF(0, 0), Color.FromArgb(200, 0, 0, 0), false);
                for (Int32 i = 0; i < CContainer.DebugText.Count; i++)
                {
                    DrawString(SContainer, sprite, CContainer, CContainer.DebugText[i], new PointF(0, i * 15), Color.FromArgb(200, 255, 255, 255), CContainer.GetFontByName("f1015"), false);
                }
            }
            #endregion
            EDraw(SContainer, sprite, CContainer.GetTextureByName("cursor"), new Rectangle(0, 0, 128, 128), new SizeF(90, 90), new PointF(SContainer.MousePosition.X, SContainer.MousePosition.Y), Color.White, true); // отрисовка курсора
            #endregion
        }
        #endregion
        #region Draw3D
        public void Draw3D(Device device, Sprite sprite, SettingsContainer SContainer, ContentContainer CContainer)
        {
        }
        #endregion
    }
    #endregion
}
