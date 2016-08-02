using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CsQuery;
using System.Web;
using System.Web.UI;
using Newtonsoft.Json;

namespace Downloads
{
    /// <summary>
    /// 从食材网下载食材图片
    /// </summary>
    class Program
    {
        /// <summary>
        /// 食材模型集合
        /// </summary>
        public static List<YuanLiao> YuanLiaos = new List<YuanLiao>();

        public static string ROOT = AppDomain.CurrentDomain.BaseDirectory;
        static void Main(string[] args)
        {
            #region 第一步：从食材网下载食材图片
            //CQ query = Html("http://www.meishichina.com/YuanLiao/");
            //// 获取dom中的a标签
            //List<IDomObject> list = query["a"].ToList();
            //foreach (var item in list)
            //{
            //    //获取a标签“href”属性的值 ，无则为返回空
            //    string url = item.GetAttribute("href") ?? "";
            //    //获取a标签中包含这个地址的a标签
            //    if (url.Contains("http://www.meishichina.com/YuanLiao/"))
            //    {

            //        CQ html = Html(url);
            //        CQ ui = html.Select("#category_pic");
            //        YuanLiao model = new YuanLiao();
            //        IDomObject obj = ui.Selection.FirstOrDefault();
            //        if (obj != null)
            //        {
            //            model.Image = obj.GetAttribute("data-src");
            //            // HttpUtility.HtmlDecode完成对汉字的解码
            //            model.Name = HttpUtility.HtmlDecode(html.Select("#category_title").Selection.First().InnerText);
            //            model.Content = HttpUtility.HtmlDecode(html.Select("#category_txt1").Selection.First().InnerText);
            //            YuanLiaos.Add(model);
            //            Download(model.Image);
            //            Console.WriteLine(model.Name);
            //        }
            //    }
            //}
            ////序列化YuanLiaos模型为json
            //string str = JsonConvert.SerializeObject(YuanLiaos);
            //File.WriteAllText("list.txt", str);
            //Console.ReadKey();
            #endregion

            #region 第二步：从序列化的文件中转化为需要的模型
            string txt = File.ReadAllText("list.txt");
            YuanLiaos = JsonConvert.DeserializeObject<List<YuanLiao>>(txt);

            foreach (var item in YuanLiaos)
            {
                try
                {
                    Console.WriteLine("开始下载：" + item.Image);
                    string file = Download(item.Image);
                    //将图片的转化为文件流
                    Console.WriteLine("开始下载完成!");

                    string accessKey = Guid.NewGuid().ToString("N");
                    Console.WriteLine("开始生成缩略图!");
                    using (Stream fileStream = new FileStream(file, FileMode.Open))
                    {
                        GeneralMake(fileStream, accessKey, new[] { new Size(60, 60), new Size(120, 120), new Size(240, 240) }, true);
                    }
                    Console.WriteLine("生成缩略图完成!");
                    //压缩处理图片到临时目录中
                    //保存到到食材目录中，并返回保存的路径
                    string path = Storage(accessKey, UploadType.Material);
                    string sql = "insert into CookMaterial( Name, Price, ImgAccessKey, Category, Unit, MaketPrice, SN) values ('" + item.Name + "',4,'" + path + "',3,5,7.000,'asldfjadsaaa')";
                    File.AppendAllText("sql.sql", sql + "\r\n");
                    Console.WriteLine("保存SQL!");
                }
                catch (Exception exception)
                {
                    Console.WriteLine(item.Image + ":" + exception.Message);

                }

            }
            Console.ReadKey();
            #endregion
        }
        /// <summary>
        /// 返回网页 html内容
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string Html(string url)
        {
            try
            {
                //1. 实现了IDisposable ，需要using 
                using (HttpClient client = new HttpClient())
                {
                    //2. 异步获取url页面
                    return client.GetStringAsync(url).Result;
                }
            }
            catch (Exception)
            {
                //3. 请求不到到信息，页面404，则返回 “” ,空字符串
                return "";
            }

        }
        /// <summary>
        /// 下载指定的URL资源到本地
        /// </summary>
        /// <param name="httpfile"></param>
        /// <returns></returns>
        public static string Download(string httpfile)
        {
            try
            {
                // 提供用于将数据发送到由 URI 标识的资源及从这样的资源接收数据的常用方法。
                using (WebClient client = new WebClient())
                {
                    //1. 设置一个下载文件存放的目录
                    string fileName = ROOT + "image\\";//+Path.GetFileName(httpfile);
                    //2. 目录不存在，则创建
                    if (!Directory.Exists(fileName))
                        Directory.CreateDirectory(fileName);
                    //3. 下载文件的URL资源地址 ；下载到本地的地址
                    client.DownloadFile(new Uri(httpfile), fileName + Path.GetFileName(httpfile));
                    return fileName + Path.GetFileName(httpfile);
                }
            }
            catch (Exception)
            {
                //下载资源文件不成功返回空字符串
                return "";
            }
        }

        /// <summary>
        ///     生成普通图片
        /// </summary>
        /// <param name="fileStream">上传的图片流</param>
        /// <param name="accessKey">为图片赋值的Guid值</param>
        /// <param name="sizes">生成图片的几种大小规格集合</param>
        /// <param name="isChangeFormat">是图片的原来后缀名</param>
        /// <returns></returns>
        private static void GeneralMake(Stream fileStream, string accessKey, Size[] sizes, bool isChangeFormat)
        {

            using (var objImage = Image.FromStream(fileStream))
            {

                ImageFormat format = isChangeFormat ? ImageFormat.Jpeg : objImage.RawFormat;
                string extension = "jpg";
                var accessDir = $@"{MakeTypeDir(UploadType.Temp)}\{accessKey}"; //生成保存到临时目录绝对路径地址
                string org = accessDir + "." + extension;
                if (!File.Exists(org))//保存原始图片
                    objImage.Save(org, format);
                int min;
                var w = objImage.Width;
                var h = objImage.Height;
                Rectangle srcRect;
                if (w > h)
                {
                    min = h;
                    srcRect = new Rectangle((w - h) / 2, 0, min, min);
                }
                else
                {
                    min = w;
                    srcRect = new Rectangle(0, (h - w) / 2, min, min);
                }


                using (Image newimage = new Bitmap(min, min, PixelFormat.Format32bppPArgb))
                {
                    using (var g = Graphics.FromImage(newimage))
                    {
                        g.InterpolationMode = InterpolationMode.High;
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        var destRect = new Rectangle(0, 0, min, min);
                        var units = GraphicsUnit.Pixel;
                        g.DrawImage(objImage, destRect, srcRect, units);
                    }
                    foreach (var t in sizes)
                    {
                        using (var thumb = newimage.GetThumbnailImage(t.Width, t.Height, null, IntPtr.Zero))
                        {
                            var savePath = $@"{accessDir}_{t.Width}x{t.Height}." + extension;
                            thumb.Save(savePath, format); //每次循环保存一个，压缩图到临时目录中，
                        }
                    }
                }
            }
        }
        /// <summary>
        ///     如果上传临时目录不存在，则创建Temp临时目录
        /// </summary>
        /// <param name="type">上传文件目录名称</param>
        /// <returns>返回临时目录的路径</returns>
        private static string MakeTypeDir(UploadType type)
        {
            var mainDir = $@"{ROOT}\" + UploadType.Temp; //从siteConfig.xml中读取文件前缀 + 从枚举中读取后缀
            if (!Directory.Exists(mainDir)) Directory.CreateDirectory(mainDir);
            return mainDir;
        }


        /// <summary>
        ///     获取访问路径：Http访问类型地址,例如：Banner/2015/12/11/lkajlkdlaasd
        /// </summary>
        /// <param name="accessKey">访问Key</param>
        /// <param name="type">图片类型</param>
        /// <returns></returns>
        public static string Storage(string accessKey, UploadType type)
        {
            //相对地址
            var vPath = $@"\{type}\{DateTime.Now.Year}\{DateTime.Now.Month:D2}\{DateTime.Now.Day:D2}\";
            //绝对地址
            var rPath = ROOT + vPath;
            //临时文件绝对地址
            var rTemp = ROOT + @"\" + UploadType.Temp;
            if (!File.Exists(rPath)) Directory.CreateDirectory(rPath);
            foreach (
                var item in Directory.GetFiles(rTemp, $"{accessKey}*", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileName(item);
                File.Move(item, rPath + name);
            }
            //返回Http访问类型地址
            return (vPath + accessKey).Replace("\\", "/");
        }
    }

    public class YuanLiao
    {
        public string Name { get; set; }

        public string Image { get; set; }

        public string Content { get; set; }
    }
    public enum UploadType
    {

        //缓存图片文件夹
        Temp,
        //食材图片
        Material

    }
}
