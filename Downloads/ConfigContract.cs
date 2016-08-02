using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;

namespace Downloads
{
   public static class ConfigContract
    {
        private static readonly SiteConfiguration CONFS;
        public static readonly KeyValueElementCollection SITE;
        //访问Key
        public static readonly string ACCESS_TOKEN_NAME;
        public static readonly string VESION_FOR_APP;
        //文件存储基础地址
        public static readonly string ROOT_DATA_DIR;
        //普通缩略图
        public static readonly Size[] GENERAL_THUMB_SIZE;

        public static readonly KeyValueElementCollection KEY_PREFIXS;
        //相对存储路径
        public static readonly Dictionary<UploadType, string> STOREAGE;

        static ConfigContract()
        {
            CONFS = ConfigurationManager.GetSection("siteConfig") as SiteConfiguration; //  检索当前应用程序默认配置的指定配置节。
            if (CONFS == null)
                throw new ConfigurationErrorsException("配置文件错误 siteConfig");
            SITE = GetDic("constKeyValue");
            KEY_PREFIXS = GetDic("keyPrefixs");
            ACCESS_TOKEN_NAME = SITE["access_token_name"];
            VESION_FOR_APP = SITE["vesion_for_app"];
            ROOT_DATA_DIR = SITE["storage"];

            #region 初始化图片切割大小

            GENERAL_THUMB_SIZE = CONFS.Thumbnails["default"].ToArray(); //默认类型初始化切割图片三种类型，使用了字典读取的方式 

            #endregion

            STOREAGE = new Dictionary<UploadType, string>();
            foreach (var item in Enum.GetValues(typeof(UploadType))) //从已经枚举的文件名称中，读取要上传的文件目录名称
            {
                STOREAGE.Add((UploadType)item, $"\\{item}"); //添加到相对存储路径字典中
            }
        }

        /// <summary>
        ///     读取xml文件中指定键，对应的值
        /// </summary>
        /// <param name="key">要读取的key</param>
        /// <returns></returns>
        public static KeyValueElementCollection GetDic(string key)
        {
            return CONFS.Collections.FirstOrDefault(x => x.Key == key).Value;
        }
    }
}
