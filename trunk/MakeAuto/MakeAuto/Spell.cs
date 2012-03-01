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
            // 遍历详细设计说明书
            foreach (Detail d in dls)
            {
                dic.Add(GetFirstPinyin(d.Name),;
            }
        }

        public void LoadPolyphone(string path)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(conf);

            XmlElement root = xmldoc.DocumentElement;

            // 读取多音字修正表
            XmlNode xn = root.SelectSingleNode("polyphone");
            XmlNodeList xnl = xn.ChildNodes;
            foreach (XmlNode x in xnl)
            {
                try
                {
                    polyphone.Add(x.Attributes["key"].Value, x.Attributes["value"].Value);
                }
                catch (ArgumentException)
                {
                    Debug.WriteLine("An element with Key = " + x.Attributes["key"].Value + " already exists.");
                }
                catch
                {
                    Debug.WriteLine("添加多音字异常！");
                }
            }
        }

        /// <summary> 
        /// 汉字转化为拼音首字母
        /// </summary> 
        /// <param name="str">汉字</param> 
        /// <returns>首字母</returns> 
        public string GetFirstPinyin(string str)
        {
            string r = string.Empty;
            string t;
            ChineseChar chinChar;
            foreach (char c in str)
            {
                // 数字和字母不需要处理
                if (Char.IsLetterOrDigit(c))
                {
                    r += c.ToString();
                    continue;
                }
                else  // 处理中文字符
                {
                    // 先检查多音字
                    if (polyphone.ContainsKey(c.ToString()))
                    {
                        r += polyphone[c.ToString()].Substring(0, 1);
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
        private Dictionary<string, string> polyphone = new Dictionary<string, string>();

        private Dictionary<string, string> dic = new Dictionary<string, string>();

        private readonly string conf = "polyphone.xml";

    }
}
