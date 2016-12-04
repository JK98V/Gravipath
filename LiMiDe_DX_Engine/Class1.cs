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
using Microsoft.DirectX.AudioVideoPlayback;

using System.IO;

#region Инструкция (!!!)
/* ИНСТРУКЦИЯ
 * 
 * Что бы получить текстуру/анимацию:
 * CContainer.GetTextureByName("LMB") - вернёт текстуру или текущий кадр анимации под именем LMB
 * 
 * Что бы получить текст из языкового пакета:
 * CContainer.GetTextByName("нажмите лкм для продолжения"); - вернёт текст из загруженного языкового пакета
 * 
 * Что бы отрисовать текстуру с учётом идеального экрана
 * EDraw(SContainer, sprite, CContainer.GetTextureByName("LMB"), new Rectangle(0, 0, 128, 128), new SizeF(128, 128), new PointF(0, 0), Color.White);
 * - Передаётся класс SettingsContainer, обычно названный как SContainer
 * - Передаётся класс Sprite, обычно названный как sprite
 * - Передаётся текстура
 * - Передаётся прямоугольник, который будет вырезан и отрисован с переданной текстуры. Обычно прямоугольник охватывает всю текстуруъ
 * - Передаётся размер текстуры, которая будет отрисовываться
 * - Передаются координаты верхнего левого угла текстуры
 * - Передаётся цветовой фильтр применяемый к текстуре. Что бы не использовать фильтр укажите Color.White
 * 
 * В классе SContainer есть переменная LOCATION, она отвечает за ту локацию, которая сейчас отрисовывается.
 * Что бы красиво переключаться между локациями, следует вызвать функцию NextLocation(String location) класса GameManager
 * В функцию передаётся название следующей локации
 * 
 * Для отрисовки текста:
 * Для отрисовки стандартного текста (желательно)
 * DrawString(SContainer, sprite, CContainer, "heLLo", new Point(0, 0), Color.White);
 * - SettingsContainer
 * - Sprite
 * - ContentContainer
 * - Сам текст
 * - Координаты (относительно идеального экрана)
 * - цвет
 * 
 * Для отрисовки настраиваемого текста (не желательно, нагрузка на ЦП больше)
 * DrawString(SContainer, sprite, CContainer, "heLLo", new Point(0, 0), Color.White, device, 10, 15);
 * - SettingsContainer
 * - Sprite
 * - ContentContainer
 * - Сам текст
 * - Координаты (относительно идеального экрана)
 * - цвет
 * - Device
 * - Ширина одной буквы
 * - Высота одной буквы
 * 
 * ---===[загрузка текстур]===---
 * 
 * Добавление новой текстуры/анимации
 * В папку Textures необходимо кинуть картинку !(Высота и ширина картинки должны быть равны степени двойки, к примеру: 16х16, 32х32, 16х64 итд, некоторые видео карты не поддерживают возможность того, что бы ширина и высота были разными, желательно что бы картинка была квадратная).
 * Картинка должна быть в формате .png
 * После того как картинка добавлена (допустим мы кинули картинку img.png) необходимо в файл sprites.txt на новой строке внести название этой картинки, название пишется без расширения, т.е. просто img. После этого картинка будет загруженна в игру.
 * Если необходимо добавить анимацию, то в папку Textures просто кидаем картинки с именами img_0.png, img_1.png, img_2.png итд. После этого в файле sprites.txt опять указываем только имя, без расширения и БЕЗ _0, _1, _2... , т.е. просто на новой строке пишем img
 * Можно установить скорость анимации, для этого в папке Textures должен находиться файл, который будет называться так же как и картинка, но с расширением .txt в этом файле должно быть указанно дробное число, которое будет прибавляться к шагу. Если такой файл отсутствует, то это число по умолчанию будет равно 0,1
 * В папке Textures можно создать подпапку, тогда что бы загрузить текстуру в подпапке, необходимо в sprites.txt указать [имя_подпапки]/[имя_текстуры], пример: animation/an
 * В программном коде, обращаться к этой текстуре следует так же по указанному имени, т.е. animation/an
*/
#endregion

namespace LiMiDe_DX_Engine
{
    #region SettingsContainer, ContentContainer, SpriteController ...
    #region SettingsContainer
    public class SettingsContainer // отвечает за хранение настроек и параметров игры
    {
        #region KEY
        public Boolean KEY_W = false;
        public Boolean KEY_S = false;
        public Boolean KEY_A = false;
        public Boolean KEY_D = false;
        public Boolean KEY_SHIFT = false;
        public Boolean KEY_CTRL = false;
        public Boolean KEY_SPACE = false;
        #endregion

        public String LOCATION = "loading"; // текущая локация в игре
        public String PathToContent = "Content"; // путь к папке с ресурсками
        public String LanguagePackage = "ru"; // текущий языковой пакет
        public String EDEL_Version = "EDEL AVNB3D 4.1"; // версия движка

        public PointF IdealScreen = new PointF(1200f, 800f); // идеальный размер экрана, под него будет подстраиваться игра. Желательно не трогать эти цифры!
        public PointF ScreenAsRa = new PointF(0f, 0f); // коэфицент разницы между идеальным экраном и существующим
        public PointF RealScreen = new PointF(0, 0);

        public Vector2 MousePosition = new Vector2(0f, 0f);
        public Vector2 OldCursorPosition = new Vector2(0f, 0f); // прошлая позиция реального курсора
        public Vector2 NowCursorPosition = new Vector2(0f, 0f); // текущая позиция курсора
        public Boolean MouseLeft = false, MouseRight = false, MouseMiddle = false;

        public float GameTime = 1f; // ведёт отсчёт тиков с момента начала игры

        public float FPS = 0f; // текущее количество кадров
        public float FPS_Now = 60f; // сколько FPS с последнего замера
        public float FPS_Default = 60f; // стандартный FPS, необходим для расчёта коэфицента стабилизации
        public float FPS_Stab = 1f; // все движущиеся объекты слдеует умножать на этот коэфицент, это предотвратит подтормаживание игры в случае низкого или чересчур быструю игру в случае высокого FPS

        public Color ClearColor = Color.FromArgb(50, 50, 50); // цвет, которым очищается экран

        public Boolean DebugConsoleShow = false; // отображать ли дебаговую консоль

        public Vector2 CameraPosition = new Vector2(0f, 0f); // текущая позиция камеры (2D)
        
        public Vector3 Camera3DPosition = new Vector3(0f, 0f, 0f); // текущая позиция камеры 3D
        public Vector3 Camera3DDirection = new Vector3(0f, 0f, 0f); // точка на которую смотрит 3D камера

        public Vector2 CameraRotate = new Vector2(0, 0); // угол поворота камеры обзора

        public float Camera3DSensitivity = 2f; // чувствительность мыши. (0.5 1 2) - допустимые значения
    }
    #endregion
    #region ContentContainer
    public class ContentContainer // отвечает за хранение контента
    {
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
        #region GetTimeNow
        public String GetTimeNow()
        {
            return "[" + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString() + ":" + DateTime.Now.Millisecond.ToString() + "]";
        }
        #endregion

        public Boolean ContentLoader(Device device, SettingsContainer SContainer) // загрузка контента
        {
            try
            {
                Sprites = File.ReadAllLines(SContainer.PathToContent + "/sprites.txt", Encoding.Default); // загружаем список текстур которые необходимо загрузить
                Audios = File.ReadAllLines(SContainer.PathToContent + "/audios.txt", Encoding.Default); // загружаем список аудио которые необходимо загрузить
                AllLanguages = Directory.GetFiles(SContainer.PathToContent + "/Languages/", "*.txt", SearchOption.AllDirectories);
                for (Int32 i = 0; i < AllLanguages.Length; i++)
                {
                    AllLanguages[i] = AllLanguages[i].Substring(AllLanguages[i].LastIndexOf("/") + 1, AllLanguages[i].LastIndexOf(".") - 1 - AllLanguages[i].LastIndexOf("/"));
                }
                
                Microsoft.DirectX.Direct3D.FontDescription FD = new FontDescription();
                FD.Quality = FontQuality.ClearType;

                FD.Width = 20;
                FD.Height = 30;
                DXFont = new Microsoft.DirectX.Direct3D.Font(device, FD); // инициализируем шрифт
                FD.Width = 10;
                FD.Height = 15;
                DXFont10_15 = new Microsoft.DirectX.Direct3D.Font(device, FD); // инициализируем шрифт

                #region производим загрузку текстур
                SCList.Clear(); // истим коллекцию на случай перезагрузки текстур
                for (Int32 i = 0; i < Sprites.Length; i++)
                {
                        SCList.Add(new SpriteController());
                        SCList[SCList.Count - 1].SpriteLoader(device, SContainer, Sprites[i]); // вызываем загрузку текстуры у контроллера
                        SetDebugConsole(SContainer, this, "Загрузка текстуры: " + Sprites[i] + " успешно.");
                }
                #endregion
                #region Производим загрузку аудио
                ACList.Clear();
                for (Int32 i = 0; i < Audios.Length; i++)
                {
                    ACList.Add(new AudioController());
                    ACList[ACList.Count - 1].audio = new Audio(SContainer.PathToContent + "/Audios/" + Audios[i]);
                    ACList[ACList.Count - 1].name = Audios[i];
                }
                #endregion

                #region Создание стандартных шрифтов
                FontCreate(device, 10, 15, false, "f1015");
                FontCreate(device, 10, 20, false, "f1020");
                FontCreate(device, 20, 30, false, "f2030");
                FontCreate(device, 10, 15, true, "f1015i");
                FontCreate(device, 10, 20, true, "f1020i");
                FontCreate(device, 20, 30, true, "f2030i");
                #endregion

                return true;
            }
            catch
            {
                return false;
            }
        }
        #region SpritesNextStep / GetAudioNowPosition / GetAudioStopPosition
        public void SpritesNextStep(SettingsContainer SContainer)
        {
            for (Int32 i = 0; i < SCList.Count; i++)
            {
                SCList[i].NextStep(SContainer);
            }
        }
        public Double GetAudioNowPosition(Audio A)
        {
            return Convert.ToDouble(Convert.ToInt32(A.CurrentPosition));
        }
        public Double GetAudioStopPosition(Audio A)
        {
            Int32 CONTROLNUM = 3;
            Double RET;
            if (A.StopPosition.ToString().Replace(",", "").Length >= CONTROLNUM)
            {
                RET = Convert.ToDouble(A.StopPosition.ToString().Replace(",", "").Substring(0, CONTROLNUM));
            }
            else
            {
                RET = Convert.ToDouble(A.StopPosition.ToString().Replace(",", "").Substring(0));
            }
            Int32 NULPLUS = CONTROLNUM - A.StopPosition.ToString().Replace(",", "").ToString().Length;
            String NPS = RET.ToString();
            for (Int32 i = 0; i < NULPLUS; i++)
            {
                NPS += "0";
            }
            RET = Convert.ToDouble(NPS);
            return RET;
        }
        #endregion
        #region GetTextureByName
        public Texture GetTextureByName(String Name) // возвращает спрайт текстуры по имени
        {
            try
            {
                for (Int32 i = 0; i < Sprites.Length; i++)
                {
                    if (Sprites[i] == Name)
                    {
                        return SCList[i].GetSprite();
                    }
                }
                return null; // если текстура не найдена
            }
            catch
            {
                return null;
            }
        }
        #endregion
        public Microsoft.DirectX.Direct3D.Font DXFont; // основной шрифт
        public Microsoft.DirectX.Direct3D.Font DXFont10_15;
        public List<SpriteController> SCList = new List<SpriteController>(); // коллекция спрайтов
        public List<AudioController> ACList = new List<AudioController>(); // коллекция аудио
        public String[] Sprites;
        public String[] Audios;

        public List<String> DebugText = new List<string>();
        public List<String> DebugTextFull = new List<string>();

        #region шрифты
        public void FontCreate(Device device, Int32 W, Int32 H, Boolean IsItalic, String Name)
        {
            Microsoft.DirectX.Direct3D.FontDescription FD;
            FD.Width = W;
            FD.Height = H;
            FD.IsItalic = IsItalic;
            FD.Quality = FontQuality.ClearType;
            Microsoft.DirectX.Direct3D.Font DXF = new Microsoft.DirectX.Direct3D.Font(device, FD);
            DXFonts.Add(new DXCustomFont());
            DXFonts[DXFonts.Count - 1].name = Name;
            DXFonts[DXFonts.Count - 1].DXFont = DXF;
        }
        public Microsoft.DirectX.Direct3D.Font GetFontByName(String Name)
        {
            for (Int32 i = 0; i < DXFonts.Count; i++)
            {
                if (Name == DXFonts[i].name)
                {
                    return DXFonts[i].DXFont;
                }
            }
            return DXFont10_15;
        }
        public List<DXCustomFont> DXFonts = new List<DXCustomFont>();
        #endregion

        #region LanguageController / GetTextByName
        public List<LanguageController> LANList = new List<LanguageController>();
        public String[] AllLanguages;
        public void LanguageLoader(SettingsContainer SContainer)
        {
            try
            {
                LANList.Clear(); // в случае загрузки нового языка
                String[] LANGUAGE = File.ReadAllLines(SContainer.PathToContent + "/Languages/" + SContainer.LanguagePackage + ".txt", Encoding.Default);
                for (Int32 i = 0; i < LANGUAGE.Length; i++)
                {
                    String Name = LANGUAGE[i].Substring(0, LANGUAGE[i].IndexOf("="));
                    String Text = LANGUAGE[i].Substring(LANGUAGE[i].IndexOf("=") + 1);
                    LANList.Add(new LanguageController());
                    LANList[LANList.Count - 1].name = Name;
                    LANList[LANList.Count - 1].text = Text;
                }
            }
            catch
            {
            }
        }
        public String GetTextByName(String Name)
        {
            for (Int32 i = 0; i < LANList.Count; i++)
            {
                if (LANList[i].name == Name)
                {
                    return LANList[i].text;
                }
            }
            return Name;
        }
        #endregion
    }
    #endregion
    #region DXCustomFont
    public class DXCustomFont
    {
        public Microsoft.DirectX.Direct3D.Font DXFont;
        public String name = "none";
    }
    #endregion

    #region SpriteController
    public class SpriteController // управление и хранение анимация и текстур
    {
        public void SpriteLoader(Device device, SettingsContainer SContainer, String TextureName) // производит загрузку текстур
        {
            if (File.Exists(SContainer.PathToContent + "/Textures/" + TextureName + "_0.png") == true)
            {
                if (File.Exists(SContainer.PathToContent + "/Textures/" + TextureName + ".txt") == true)
                {
                    Step = (float)Convert.ToDouble(File.ReadAllText(SContainer.PathToContent + "/Textures/" + TextureName + ".txt", Encoding.Default));
                }
                for (Int32 i = 0; i < 1000000; i++) // поддерживает загрузку до 1000000 текстур для анимации
                {
                    if (File.Exists(SContainer.PathToContent + "/Textures/" + TextureName + "_" + i.ToString() + ".png") == true)
                    {
                        TList.Add(TextureLoader.FromFile(device, SContainer.PathToContent + "/Textures/" + TextureName + "_" + i.ToString() + ".png"));
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                TList.Add(TextureLoader.FromFile(device, SContainer.PathToContent + "/Textures/" + TextureName + ".png"));
            }
        }
        public Texture GetSprite() // возвращает текстуру 
        {
            return TList[(int)StepNow];
        }
        public void NextStep(SettingsContainer SContainer) // следующий шаг
        {
            StepNow += Step * SContainer.FPS_Stab;
            if ((int)StepNow >= TList.Count)
            {
                StepNow = 0f;
            }
        }

        public List<Texture> TList = new List<Texture>(); // коллекция текстур
        public float StepNow = 0f; // текущий спрайт
        public float Step = 0.1f; // шаг до следующего спрайта
    }
    #endregion
    #region LanguageController
    public class LanguageController // часть языкового пакета
    {
        public String text = "none";
        public String name = "none";
    }
    #endregion

    #region PicturePart
    public class PicturePart
    {
        public Vector2 position;
        public Vector2 vector;
        public Color color;

        public Texture texture;
        public Rectangle rect;

        public Vector2 sizecoi;

        public float LifeTime = 0f;

        public float rotation = 0f;
        public PointF rotationcenter = new PointF(0, 0);

        public Boolean priority = false;

        public float G = 0.2f; // 0.2f

        public Boolean EffectOfCamera = true;
    }
    #endregion
    #region PicturePartsController
    public class PicturePartsController
    {
        public List<PicturePart> PPList = new List<PicturePart>();

        public void PartCreate(Vector2 position, Vector2 vector, Color color, Texture texture, Rectangle rect, Vector2 sizecoi, Boolean priority, float G, Boolean EffectOfCamera)
        {
            PPList.Add(new PicturePart());
            PPList[PPList.Count - 1].position = position;
            PPList[PPList.Count - 1].vector = vector;
            PPList[PPList.Count - 1].color = color;
            PPList[PPList.Count - 1].texture = texture;
            PPList[PPList.Count - 1].rect = rect;
            PPList[PPList.Count - 1].sizecoi = sizecoi;
            PPList[PPList.Count - 1].rotationcenter = new PointF(rect.Width / 2, rect.Height / 2);
            PPList[PPList.Count - 1].priority = priority;
            PPList[PPList.Count - 1].G = G;
            PPList[PPList.Count - 1].EffectOfCamera = EffectOfCamera;
        }

        public void TextureToPart(Texture texture, Point parts, Vector2 position, Vector2 size, Boolean priority, Color color, Boolean EffectOfCamera)
        {
            Random RAN = new Random();

            SurfaceDescription SD = texture.GetLevelDescription(0);
            Point partsize = new Point((int)(SD.Width / parts.X), (int)(SD.Height / parts.Y)); // получаем ширину и высоту каждого кусочка

            Vector2 sizecoi = new Vector2(size.X / SD.Width, size.Y / SD.Height);

            for (Int32 iy = 0; iy < parts.Y; iy++)
            {
                for (Int32 ix = 0; ix < parts.X; ix++)
                {
                    PartCreate(new Vector2(position.X + ix * partsize.X * sizecoi.X, position.Y + iy * partsize.Y * sizecoi.Y), new Vector2(RAN.Next(-1, 2) + 0.1f * (float)RAN.NextDouble() * 2f, -20 * (float)RAN.NextDouble()), color, texture, new Rectangle((int)(ix * partsize.X), (int)(iy * partsize.Y), partsize.X, partsize.Y), sizecoi, priority, 0.2f, EffectOfCamera);
                }
            }
        }

        public void Draw(GameManager GManager, SettingsContainer SContainer, Sprite sprite, Boolean priority)
        {
            for (Int32 i = 0; i < PPList.Count; i++)
            {
                if (PPList[i].priority == priority)
                {
                    GManager.EDraw(SContainer, sprite, PPList[i].texture, PPList[i].rect, new SizeF(PPList[i].rect.Width * PPList[i].sizecoi.X, PPList[i].rect.Height * PPList[i].sizecoi.Y), new PointF(PPList[i].position.X, PPList[i].position.Y), PPList[i].color, PPList[i].EffectOfCamera);
                }
            }
        }
        public void Next(SettingsContainer SContainer)
        {
            for (Int32 i = 0; i < PPList.Count; i++)
            {
                PPList[i].position += PPList[i].vector * SContainer.FPS_Stab;
                PPList[i].vector.Y += PPList[i].G * SContainer.FPS_Stab;
                PPList[i].LifeTime += 1f * SContainer.FPS_Stab;
                //PPList[i].rotation += 0.01f;
                Int32 ALPHA = 255 - (int)PPList[i].LifeTime;
                if (ALPHA < 0) { ALPHA = 0; }
                PPList[i].color = Color.FromArgb(ALPHA, PPList[i].color.R, PPList[i].color.G, PPList[i].color.B);
                if (PPList[i].LifeTime > 255)
                {
                    PPList.RemoveAt(i);
                }
            }
        }
    }
    #endregion

    #region PointPart
    public class PointPart
    {
        public Vector2 position;
        public Vector2 vector;
        public Color color;
        public float LifeTime = 0f;
        public float G = 0.2f;
        public Boolean priority = false;
    }
    #endregion 
    #region PointPartsController
    public class PointPartsController
    {
        public List<PointPart> PPList = new List<PointPart>();

        public void PartCreate(SettingsContainer SContainer, Vector2 position, Vector2 vector, Color color, float G)
        {
            PPList.Add(new PointPart());
            //PPList[PPList.Count - 1].position = new Vector2(position.X * SContainer.ScreenAsRa.X, position.Y * SContainer.ScreenAsRa.Y);
            PPList[PPList.Count - 1].position = position;
            PPList[PPList.Count - 1].vector = vector;
            PPList[PPList.Count - 1].color = color;
            PPList[PPList.Count - 1].G = G;
            //PPList[PPList.Count - 1].priority = priority;
        }

        public void Draw(GameManager GManager, Device device, SettingsContainer SContainer, Boolean priority)
        {
            if (PPList.Count > 0)
            {
                CustomVertex.TransformedColored[] verts = new CustomVertex.TransformedColored[PPList.Count];
                for (Int32 i = 0; i < verts.Length; i++)
                {
                    verts[i].Color = PPList[i].color.ToArgb();
                    verts[i].Position = new Vector4(PPList[i].position.X * SContainer.ScreenAsRa.X, PPList[i].position.Y * SContainer.ScreenAsRa.Y, 0f, 1f);
                }
                device.DrawUserPrimitives(PrimitiveType.TriangleList, verts.Length / 3, verts);
            }
                    //GManager.DrawPoint(device, SContainer, PPList[i].position, PPList[i].color);
        }
        public void Next(SettingsContainer SContainer)
        {
            for (Int32 i = 0; i < PPList.Count; i++)
            {
                PPList[i].position += PPList[i].vector * SContainer.FPS_Stab;
                PPList[i].vector.Y += PPList[i].G * SContainer.FPS_Stab;
                PPList[i].LifeTime += 1f * SContainer.FPS_Stab;
                Int32 ALPHA = 255 - (int)PPList[i].LifeTime;
                if (ALPHA > 255) { ALPHA = 255; }
                if (ALPHA < 0) { ALPHA = 0; }
                PPList[i].color = Color.FromArgb(ALPHA, PPList[i].color.R, PPList[i].color.G, PPList[i].color.B);
                if (PPList[i].LifeTime >= 255)
                {
                    PPList.RemoveAt(i);
                }
            }
        }
    }
    #endregion

    #region AudioController
    public class AudioController // Управление аудио
    {
        public Audio audio;
        public String name = "default";
        public String artist = "default";
        public Boolean playing = false;
        public Boolean loop = false;
    }
    #endregion

    #region Script // Отключен в этой версии движка. РАДИ БОГА, НИЧЕГО ЗДЕСЬ НЕ ТРОГАТЬ, НЕДЕЛЮ ПИСАЛ!
    #region ScriptIamage
    public class ScriptImage
    {
        public String Texture = "";
        public String Name = "";
        public RectangleF TexturePosition = new RectangleF(0, 0, 0, 0);
        public RectangleF TextureCutPosition = new RectangleF(0, 0, 0, 0);
        public Color ColorFilter = Color.White;
    }
    #endregion
    #region ScriptText
    public class ScriptText
    {
        public String text = "";
        public String name = "";
        public Microsoft.DirectX.Direct3D.Font DXFont;
        public PointF position = new PointF(0, 0);
        public Color color = Color.White;
    }
    #endregion
    #region ScriptValue
    public class ScriptValue
    {
        public String value = "0";
        public String name = "default";
    }
    #endregion
    public class ScriptController
    {
        #region Элементы скрипта
        public List<ScriptImage> SImageList = new List<ScriptImage>();
        public List<ScriptText> STextList = new List<ScriptText>();
        public List<ScriptValue> SValueList = new List<ScriptValue>();
        #endregion

        #region Вспомогательные функции
        public Int32 GetIDByNameForValue(String name)
        {
            for (Int32 i = 0; i < SValueList.Count; i++)
            {
                if (SValueList[i].name == name)
                {
                    return i;
                }
            }
            return -1;
        }
        public void SetToValue(String name, String value)
        {
            Int32 SVLNUM = GetIDByNameForValue(name);
            if (SVLNUM >= 0)
            {
                SValueList[SVLNUM].value = value;
            }
            else
            {
                SValueList.Add(new ScriptValue());
                SValueList[SValueList.Count - 1].name = name;
                SValueList[SValueList.Count - 1].value = value;
            }
        }
        public String GetTimeNow()
        {
            return "[" + DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString() + ":" + DateTime.Now.Second.ToString() + ":" + DateTime.Now.Millisecond.ToString() + "]";
        }
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

        #region LoadScript
        public String LoadScript(SettingsContainer SContainer, ContentContainer CContainer, Device device, String ScriptName)
        {
            try
            {
                //String SCRIPT = File.ReadAllText(SContainer.PathToContent + "/Scripts/" + ScriptName + ".txt", Encoding.Default);
                //String[] SCRIPTLINES = SCRIPT.Split(new String[] {";"}, 1000000, StringSplitOptions.RemoveEmptyEntries);         
                String[] SCRIPTLINES = { "", };
                SCRIPTLINES = File.ReadAllLines(SContainer.PathToContent + "/Scripts/" + ScriptName + ".txt", Encoding.Default);
                for (Int32 i = 0; i < SCRIPTLINES.Length; i++)
                {
                    SCRIPTLINES[i] = SCRIPTLINES[i].Replace("\r", "");
                    SCRIPTLINES[i] = SCRIPTLINES[i].Replace("\n", "");
                    SCRIPTLINES[i] = SCRIPTLINES[i].Replace(";", "");
                }

                #region Переменные для скрипта
                Color TextureColorFilter = Color.White;
                RectangleF TextureCutRectangle = new RectangleF(0, 0, 0, 0);
                String TextureName = "default";
                Boolean AudioLoop = true;
                Int32 AudioVolume = 0;
                String WaitUserClick = "none";
                String TextName = "default";
                Color TextColor = Color.White;
                #endregion

                #region Выполнение скрипта
                for (Int32 i = 0; i < SCRIPTLINES.Length; i++)
                {
                    try
                    {
                    #region ожидание реакции пользователя
                    if (WaitUserClick == "none")
                    {
                    #endregion

                        String SL = SCRIPTLINES[i];

                        #region Зарезервированные значения
                        #region _GameTime
                        SL = SL.Replace("_GameTime", SContainer.GameTime.ToString());
                        #endregion
                        #region _LangPack
                        SL = SL.Replace("_LangPack", SContainer.LanguagePackage.ToString());
                        #endregion
                        #region _Location
                        SL = SL.Replace("_Location", SContainer.LOCATION);
                        #endregion
                        #region _MPosX
                        SL = SL.Replace("_MPosX", SContainer.MousePosition.X.ToString());
                        #endregion
                        #region _MPosY
                        SL = SL.Replace("_MPosY", SContainer.MousePosition.X.ToString());
                        #endregion
                        #region _MLeft
                        if (SContainer.MouseLeft == true)
                        {
                            SL = SL.Replace("_MLeft", "1");
                        }
                        else
                        {
                            SL = SL.Replace("_MLeft", "0");
                        }
                        #endregion
                        #region _MRight
                        if (SContainer.MouseRight == true)
                        {
                            SL = SL.Replace("_MRight", "1");
                        }
                        else
                        {
                            SL = SL.Replace("_MRight", "0");
                        }
                        #endregion
                        #region _MMiddle
                        if (SContainer.MouseMiddle == true)
                        {
                            SL = SL.Replace("_MMiddle", "1");
                        }
                        else
                        {
                            SL = SL.Replace("_MMiddle", "0");
                        }
                        #endregion
                        #region _TextureCount
                        SL = SL.Replace("_TextureCount", SImageList.Count.ToString());
                        #endregion
                        #region _TextCount
                        SL = SL.Replace("_TextCount", STextList.Count.ToString());
                        #endregion
                        #endregion
                        #region V["name"]
                        for (Int32 j = 0; j < SValueList.Count; j++)
                        {
                            SL = SL.Replace("V[\"" + SValueList[j].name + "\"]", SValueList[j].value);
                        }
                        #endregion
                        #region V:E["name"]
                        // V:E["name"];
                        if (SL.IndexOf("V:E[") == 0)
                        {
                            String[] PODSL = SL.Replace("V:E[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            if (GetIDByNameForValue(PODSL[0]) >= 0)
                            {
                                SL = SL.Replace("V:E[\"" + PODSL[0] + "\"]", "1");
                            }
                            else
                            {
                                SL = SL.Replace("V:E[\"" + PODSL[0] + "\"]", "0");
                            }
                        }
                        #endregion

                        #region IdealScreen:Size[W, H]
                        // IdealScreen:Size[W, H];
                        if (SL.IndexOf("IdealScreen:Size[") == 0)
                        {
                            String[] PODSL = SL.Replace("IdealScreen:Size[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);
                            SContainer.IdealScreen.X = (float)Convert.ToDouble(PODSL[0]);
                            SContainer.IdealScreen.Y = (float)Convert.ToDouble(PODSL[1]);
                            SContainer.ScreenAsRa = new PointF(SContainer.RealScreen.X / SContainer.IdealScreen.X, SContainer.RealScreen.Y / SContainer.IdealScreen.Y); // вычисляем соотношение сторон
                        }
                        #endregion
                        #region IdealScreen:Reset[]
                        // DrawTexture:Reset[];
                        if (SL.IndexOf("IdealScreen:Reset[]") == 0)
                        {
                            SContainer.IdealScreen.X = 1200;
                            SContainer.IdealScreen.Y = 800;
                            SContainer.ScreenAsRa = new PointF(SContainer.RealScreen.X / SContainer.IdealScreen.X, SContainer.RealScreen.Y / SContainer.IdealScreen.Y); // вычисляем соотношение сторон
                        }
                        #endregion

                        #region Texture:Draw["name", X, Y, W, H]
                        // Texture:Draw["texture_name", X, Y, W, H];
                        if (SL.IndexOf("Texture:Draw[") == 0)
                        {
                            String[] PODSL = SL.Replace("Texture:Draw[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            #region Создание объекта
                            SImageList.Add(new ScriptImage());
                            SImageList[SImageList.Count - 1].Texture = PODSL[0];
                            SImageList[SImageList.Count - 1].TexturePosition.X = (float)Convert.ToDouble(PODSL[1]);
                            SImageList[SImageList.Count - 1].TexturePosition.Y = (float)Convert.ToDouble(PODSL[2]);
                            SImageList[SImageList.Count - 1].TexturePosition.Width = (float)Convert.ToDouble(PODSL[3]);
                            SImageList[SImageList.Count - 1].TexturePosition.Height = (float)Convert.ToDouble(PODSL[4]);
                            SImageList[SImageList.Count - 1].ColorFilter = TextureColorFilter;
                            SImageList[SImageList.Count - 1].TextureCutPosition = TextureCutRectangle;
                            SImageList[SImageList.Count - 1].Name = TextureName;
                            #endregion
                        }
                        #endregion
                        #region Texture:ColorFilter[A, R, G, B]
                        // Texture:ColorFilter[A, R, G, B];
                        if (SL.IndexOf("Texture:ColorFilter[") == 0)
                        {
                            String[] PODSL = SL.Replace("Texture:ColorFilter[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            TextureColorFilter = Color.FromArgb(Convert.ToInt32(PODSL[0]), Convert.ToInt32(PODSL[1]), Convert.ToInt32(PODSL[2]), Convert.ToInt32(PODSL[3]));
                        }
                        #endregion
                        #region Texture:Cut[X, Y, W, H]
                        // Texture:Cut[X, Y, W, H];
                        if (SL.IndexOf("Texture:Cut[") == 0)
                        {
                            String[] PODSL = SL.Replace("Texture:Cut[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            TextureCutRectangle = new RectangleF((float)Convert.ToDouble(PODSL[0]), (float)Convert.ToDouble(PODSL[1]), (float)Convert.ToDouble(PODSL[2]), (float)Convert.ToDouble(PODSL[3]));
                        }
                        #endregion
                        #region Texture:Name["name"]
                        // Texture:Name["name"];
                        if (SL.IndexOf("Texture:Name[") == 0)
                        {
                            String[] PODSL = SL.Replace("Texture:Name[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            TextureName = PODSL[0];
                        }
                        #endregion
                        #region Texture:Reset[]
                        // Texture:Reset[];
                        if (SL.IndexOf("Texture:Reset[]") == 0)
                        {
                            TextureColorFilter = Color.White;
                            TextureCutRectangle = new RectangleF(0, 0, 0, 0);
                            TextureName = "default";
                        }
                        #endregion
                        #region Texture:Del["name"]
                        // Texture:Del["name"];
                        if (SL.IndexOf("Texture:Del[") == 0)
                        {
                            String[] PODSL = SL.Replace("Texture:Del[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            for (Int32 j = 0; j < SImageList.Count; j++)
                            {
                                if (SImageList[j].Name == PODSL[0])
                                {
                                    SImageList.RemoveAt(j);
                                    i--;
                                }
                            }
                        }
                        #endregion
                        #region Texture:Clear[]
                        // Texture:Clear[];
                        if (SL.IndexOf("Texture:Clear[") == 0)
                        {
                            String[] PODSL = SL.Replace("Texture:Clear[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            SImageList.Clear();
                        }
                        #endregion

                        #region Background:Color[R, G, B]
                        // Background:Color[R, G, B];
                        if (SL.IndexOf("Background:Color[") == 0)
                        {
                            String[] PODSL = SL.Replace("Background:Color[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            SContainer.ClearColor = Color.FromArgb(Convert.ToInt32(PODSL[0]), Convert.ToInt32(PODSL[1]), Convert.ToInt32(PODSL[2]));
                        }
                        #endregion
                        #region Background:Reset[]
                        // Background:Reset[];
                        if (SL.IndexOf("Background:Reset[]") == 0)
                        {
                            SContainer.ClearColor = Color.FromArgb(50, 50, 50);
                        }
                        #endregion

                        #region Audio:Play["name"]
                        // Audio:Play["name"];
                        if (SL.IndexOf("Audio:Play[") == 0)
                        {
                            String[] PODSL = SL.Replace("Audio:Play[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            for (Int32 j = 0; j < CContainer.ACList.Count; j++)
                            {
                                if (CContainer.ACList[j].name == PODSL[0])
                                {
                                    CContainer.ACList[j].audio.Play();
                                    CContainer.ACList[j].audio.Volume = AudioVolume;
                                    CContainer.ACList[j].loop = AudioLoop;
                                    CContainer.ACList[j].playing = true;
                                }
                            }
                        }
                        #endregion
                        #region Audio:Loop["name"]
                        // Audio:Loop["name"];
                        if (SL.IndexOf("Audio:Loop[") == 0)
                        {
                            String[] PODSL = SL.Replace("Audio:Loop[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            if (PODSL[0] == "false")
                            {
                                AudioLoop = false;
                            }
                            if (PODSL[0] == "true")
                            {
                                AudioLoop = true;
                            }
                        }
                        #endregion
                        #region Audio:Stop["name"]
                        // Audio:Stop["name"];
                        if (SL.IndexOf("Audio:Stop[") == 0)
                        {
                            String[] PODSL = SL.Replace("Audio:Stop[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            for (Int32 j = 0; j < CContainer.ACList.Count; j++)
                            {
                                if (CContainer.ACList[j].name == PODSL[0])
                                {
                                    CContainer.ACList[j].playing = false;
                                    CContainer.ACList[j].loop = false;
                                    CContainer.ACList[j].audio.Stop();
                                }
                            }
                        }
                        #endregion
                        #region Audio:StopAll[]
                        // Audio:StopAll[];
                        if (SL.IndexOf("Audio:StopAll[") == 0)
                        {
                            String[] PODSL = SL.Replace("Audio:StopAll[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            for (Int32 j = 0; j < CContainer.ACList.Count; j++)
                            {
                                CContainer.ACList[j].playing = false;
                                CContainer.ACList[j].loop = false;
                                CContainer.ACList[j].audio.Stop();
                            }
                        }
                        #endregion
                        #region Audio:Volume[v]
                        // Audio:Volume[v];
                        if (SL.IndexOf("Audio:Volume[") == 0)
                        {
                            String[] PODSL = SL.Replace("Audio:Volume[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            AudioVolume = Convert.ToInt32(PODSL[0]);
                        }
                        #endregion

                        #region System:Delay[ms]
                        // System:Delay[ms];
                        if (SL.IndexOf("System:Delay[") == 0)
                        {
                            String[] PODSL = SL.Replace("System:Delay[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            System.Threading.Thread.Sleep(Convert.ToInt32(PODSL[0]));
                        }
                        #endregion
                        #region System:WaitUserClick[left/right/middle]
                        // Control:WaitUserClick[left/right/middle];
                        if (SL.IndexOf("System:WaitUserClick[") == 0)
                        {
                            String[] PODSL = SL.Replace("System:WaitUserClick[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            WaitUserClick = PODSL[0];
                        }
                        #endregion
                        #region System:NextScript["name"]
                        // System:NextScript["name"];
                        if (SL.IndexOf("System:NextScript[") == 0)
                        {
                            String[] PODSL = SL.Replace("System:NextScript[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            LoadScript(SContainer, CContainer, device, PODSL[0]);

                            //return "nextscript:" + PODSL[0];
                        }
                        #endregion
                        #region System:Stop[]
                        // System:Stop[];
                        if (SL.IndexOf("System:Stop[") == 0)
                        {
                            String[] PODSL = SL.Replace("System:Stop[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            SetDebugConsole(SContainer, CContainer, "Stop Script >" + ScriptName);
                            return "stopbycommand";
                        }
                        #endregion

                        #region Text:Draw["name", X, Y, "font_name"]
                        // Text:Draw["name", X, Y, "font_name"];
                        if (SL.IndexOf("Text:Draw[") == 0)
                        {
                            String[] PODSL = SL.Replace("Text:Draw[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            #region Создание объекта
                            STextList.Add(new ScriptText());
                            STextList[STextList.Count - 1].text = PODSL[0];
                            STextList[STextList.Count - 1].position = new PointF((float)Convert.ToDouble(PODSL[1]), (float)Convert.ToDouble(PODSL[2]));
                            STextList[STextList.Count - 1].DXFont = CContainer.GetFontByName(PODSL[3]);
                            STextList[STextList.Count - 1].color = TextColor;
                            STextList[STextList.Count - 1].name = TextName;
                            #endregion
                        }
                        #endregion
                        #region Text:Name["name"]
                        // Text:Name["name"];
                        if (SL.IndexOf("Text:Name[") == 0)
                        {
                            String[] PODSL = SL.Replace("Text:Name[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            TextName = PODSL[0];
                        }
                        #endregion
                        #region Text:Color[A, R, G, B]
                        // Text:Color[A, R, G, B];
                        if (SL.IndexOf("Text:Color[") == 0)
                        {
                            String[] PODSL = SL.Replace("Text:Color[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            TextColor = Color.FromArgb(Convert.ToInt32(PODSL[0]), Convert.ToInt32(PODSL[1]), Convert.ToInt32(PODSL[2]), Convert.ToInt32(PODSL[3]));
                        }
                        #endregion
                        #region Text:Del["name"]
                        // Text:Del["name"];
                        if (SL.IndexOf("Text:Del[") == 0)
                        {
                            String[] PODSL = SL.Replace("Text:Del[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            for (Int32 j = 0; j < STextList.Count; j++)
                            {
                                if (STextList[j].name == PODSL[0])
                                {
                                    STextList.RemoveAt(j);
                                    i--;
                                }
                            }
                        }
                        #endregion
                        #region Text:Reset[]
                        // Text:Reset[];
                        if (SL.IndexOf("Text:Reset[]") == 0)
                        {
                            TextName = "default";
                            TextColor = Color.White;
                        }
                        #endregion
                        #region Text:CreateFont["name", X, Y, "font_name"]
                        // Text:CreateFont["name", X, Y, "font_name"];
                        if (SL.IndexOf("Text:CreateFont[") == 0)
                        {
                            String[] PODSL = SL.Replace("Text:CreateFont[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            #region Создание объекта
                            CContainer.FontCreate(device, Convert.ToInt32(PODSL[0]), Convert.ToInt32(PODSL[1]), Convert.ToBoolean(PODSL[2]), PODSL[3]);
                            #endregion
                        }
                        #endregion
                        #region Text:Clear[]
                        // Text:Clear[];
                        if (SL.IndexOf("Text:Clear[") == 0)
                        {
                            String[] PODSL = SL.Replace("Text:Clear[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            for (Int32 j = 0; j < STextList.Count; j++)
                            {
                                STextList.RemoveAt(j);
                                i--;
                            }
                        }
                        #endregion

                        #region Debug:Write["text"]
                        // Debug:Write["text"];
                        if (SL.IndexOf("Debug:Write[") == 0)
                        {
                            String[] PODSL = SL.Replace("Debug:Write[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            SetDebugConsole(SContainer, CContainer, PODSL[0]);
                        }
                        #endregion
                        #region Debug:Clear[]
                        // Debug:Clear[];
                        if (SL.IndexOf("Debug:Clear[") == 0)
                        {
                            String[] PODSL = SL.Replace("Debug:Clear[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            CContainer.DebugText.Clear();
                        }
                        #endregion

                        #region V:["name", "value"]
                        // V:["name", "value"];
                        if (SL.IndexOf("V:[") == 0)
                        {
                            String[] PODSL = SL.Replace("V:[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            SetToValue(PODSL[0], PODSL[1]);
                        }
                        #endregion

                        #region Math:Calc[name ,value, sign, value]
                        // Math:Calc[value, sign, value];
                        if (SL.IndexOf("Math:Calc[") == 0)
                        {
                            String[] PODSL = SL.Replace("Math:Calc[", "").Replace("]", "").Replace("\"", "").Split(new String[] { "," }, 100, StringSplitOptions.RemoveEmptyEntries);

                            if (PODSL[2].IndexOf("+") >= 0)
                            {
                                Double RES = Convert.ToDouble(PODSL[1]) + Convert.ToDouble(PODSL[3]);
                                SetToValue(PODSL[0], RES.ToString());
                            }
                            else if (PODSL[2].IndexOf("-") >= 0)
                            {
                                Double RES = Convert.ToDouble(PODSL[1]) - Convert.ToDouble(PODSL[3]);
                                SetToValue(PODSL[0], RES.ToString());
                            }
                            else if (PODSL[2].IndexOf("*") >= 0)
                            {
                                Double RES = Convert.ToDouble(PODSL[1]) * Convert.ToDouble(PODSL[3]);
                                SetToValue(PODSL[0], RES.ToString());
                            }
                            else if (PODSL[2].IndexOf("/") >= 0)
                            {
                                Double RES = Convert.ToDouble(PODSL[1]) / Convert.ToDouble(PODSL[3]);
                                SetToValue(PODSL[0], RES.ToString());
                            }
                            else if (PODSL[2].IndexOf("^") >= 0)
                            {
                                Double RES = Math.Pow(Convert.ToDouble(PODSL[1]), Convert.ToDouble(PODSL[3]));
                                SetToValue(PODSL[0], RES.ToString());
                            }
                            else if (PODSL[2].IndexOf("%") >= 0)
                            {
                                Double RES = Convert.ToDouble(PODSL[1]) % Convert.ToDouble(PODSL[3]);
                                SetToValue(PODSL[0], RES.ToString());
                            }
                        }
                        #endregion

                        #region ожидание реакции пользователя
                    }
                    else
                    {
                        i--;
                        if (WaitUserClick == "left" && SContainer.MouseLeft == true)
                        {
                            SContainer.MouseLeft = false;
                            WaitUserClick = "none";
                        }
                        if (WaitUserClick == "right" && SContainer.MouseRight == true)
                        {
                            SContainer.MouseRight = false;
                            WaitUserClick = "none";
                        }
                        if (WaitUserClick == "middle" && SContainer.MouseMiddle == true)
                        {
                            SContainer.MouseMiddle = false;
                            WaitUserClick = "none";
                        }
                    }
                        #endregion
                    }
                    catch (Exception ex)
                    {
                        SetDebugConsole(SContainer, CContainer, "ERROR > " + ScriptName + " > " + i.ToString() + " > " + ex.Message);
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                SetDebugConsole(SContainer, CContainer, "ERROR > " + ScriptName + " > " + "ALL" + " > " + ex.Message);
                return "error";
            }
            SetDebugConsole(SContainer, CContainer, "End Script > " + ScriptName);
            return "endscript";
        }
        #endregion

        #region отрисока сцены
        public void DrawScene(GameManager GManager, SettingsContainer SContainer, Sprite sprite, ContentContainer CContainer)
        {
            #region отрисовка текстур
            for (Int32 i = 0; i < SImageList.Count; i++)
            {
                Texture DrawTexture = CContainer.GetTextureByName(SImageList[i].Texture);
                if (SImageList[i].TextureCutPosition != new RectangleF(0, 0, 0, 0))
                {
                    GManager.EDraw(SContainer, sprite, DrawTexture, new Rectangle((int)SImageList[i].TextureCutPosition.X, (int)SImageList[i].TextureCutPosition.Y, (int)SImageList[i].TextureCutPosition.Width, (int)SImageList[i].TextureCutPosition.Height), new SizeF(SImageList[i].TexturePosition.Width, SImageList[i].TexturePosition.Height), new PointF(SImageList[i].TexturePosition.X, SImageList[i].TexturePosition.Y), SImageList[i].ColorFilter, true);
                }
                else
                {
                    GManager.EDraw(SContainer, sprite, DrawTexture, new Rectangle(0, 0, DrawTexture.GetLevelDescription(0).Width, DrawTexture.GetLevelDescription(0).Height), new SizeF(SImageList[i].TexturePosition.Width, SImageList[i].TexturePosition.Height), new PointF(SImageList[i].TexturePosition.X, SImageList[i].TexturePosition.Y), SImageList[i].ColorFilter, true);
                }
            }
            #endregion
            #region отрисовка текста
            for (Int32 i = 0; i < STextList.Count; i++)
            {
                GManager.DrawString(SContainer, sprite, CContainer, STextList[i].text, STextList[i].position, STextList[i].color, STextList[i].DXFont, true);
            }
            #endregion
            #region Обновление информации
            #region Audio
            for (Int32 i = 0; i < CContainer.ACList.Count; i++)
            {
                if (CContainer.ACList[i].audio.Playing == false)
                {
                    if (CContainer.ACList[i].loop == true)
                    {
                        CContainer.ACList[i].audio.Play();
                    }
                    else
                    {
                        CContainer.ACList[i].playing = false;
                    }
                }
            }
            #endregion
            #endregion
        }
        #endregion
    }
    #endregion
    #endregion
}
