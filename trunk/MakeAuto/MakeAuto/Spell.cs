using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.International.Converters.PinYinConverter;
using System.Collections;
using System.Diagnostics;
using System.Xml;

namespace MakeAuto
{
    class Spell
    {

        public Spell()
        { }

        public Spell(Details dls)
        {
            polyphone = new Dictionary<char, char>();
            Detaildic = new Dictionary<string, string>();

            LoadPolyphone(conf);

            // 遍历详细设计说明书
            foreach (Detail d in dls)
            {
                Detaildic.Add(d.Name, GetFirstPinyin(d.Name));
                Debug.WriteLine(d.Name, Detaildic[d.Name]);
            }
        }

        private void LoadPolyphone(string path)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(path);

            XmlElement root = xmldoc.DocumentElement;

            // 读取多音字修正表
            XmlNode xn = root.SelectSingleNode("secu");
            XmlNodeList xnl = xn.ChildNodes;
            foreach (XmlNode x in xnl)
            {
                try
                {
                    polyphone.Add(x.Attributes["key"].Value[0], x.Attributes["value"].Value[0]);
                }
                catch (ArgumentException)
                {
                    log.WriteLog("An element with Key = " + x.Attributes["key"].Value[0] + " already exists.", LogLevel.Error);
                }
                catch
                {
                    log.WriteLog("添加多音字异常！" + x.Attributes["key"].Value, LogLevel.Error);
                }
            }
        }

        /// <summary> 
        /// 汉字转化为拼音首字母
        /// </summary> 
        /// <param name="str">汉字</param> 
        /// <returns>首字母</returns> 
        private string GetFirstPinyin(string str)
        {
            string r = string.Empty;
            string t;
            ChineseChar chinChar;
            char val = ' ';
            foreach (char c in str)
            {
                // 数字和字母不需要处理
                if (Char.IsDigit(c) || Char.IsUpper(c) || Char.IsLower(c))
                {
                    r += c.ToString();
                    continue;
                }
                else  // 处理中文字符
                {
                    // 先检查多音字
                    if (polyphone.TryGetValue(c, out val))
                    {
                        r += val;
                    }
                    else  // 正常处理掉
                    {
                        try
                        {
                            chinChar = new ChineseChar(c);
                            t = chinChar.Pinyins[0].ToString();
                            r += t.Substring(0, 1);
                        }
                        catch (NotSupportedException)
                        {
                            Debug.WriteLine("不支持的字符集：" + c.ToString());
                        }
                        catch(Exception e)
                        {
                            Debug.WriteLine("异常: " + c.ToString() + "  " +e.ToString());
                        }
                    }
                }
            }
            return r;
        }

        // 多音字
        private Dictionary<char, char> polyphone;

        public Dictionary<string, string> Detaildic;

        private readonly string conf = "polyphone.xml";

        private OperLog log = OperLog.instance;

    }
}
