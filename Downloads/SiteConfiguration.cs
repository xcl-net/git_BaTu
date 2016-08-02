using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Downloads
{
    /// <summary>
    ///     网站配置的xml文件读取类
    /// </summary>
    public class SiteConfiguration
    {
        /// <summary>
        ///     其他字典类型配置
        /// </summary>
        public Dictionary<string, KeyValueElementCollection> Collections;

        /// <summary>
        ///     图片切割配置
        /// </summary>
        public Dictionary<string, List<Size>> Thumbnails;

        public SiteConfiguration()
        {
            Collections = new Dictionary<string, KeyValueElementCollection>();
            Thumbnails = new Dictionary<string, List<Size>>();

        }

    }
}
