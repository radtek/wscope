using System;
using System.Text;
using System.Xml;
using System.Data.OleDb;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;
using System.Security;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Collections;

namespace OraZip
{
    public class DBUser
    {
        public DBUser(string name, string pass, string note)
        {
            this.name = name;
            this.pass = pass;
            this.note = note;
        }

        public static bool test(string pass)
        {
            return true;

        }

        private static byte[] key;
        private static byte[] iv;

        private static void InitKey()
        {
            if (key == null || iv == null)
            {
                byte[] key_t = Encoding.UTF8.GetBytes(G_KEY);
                // 做一下MD5哈希，作为 key，实际可以直接使用，HASH的值太小了
                SHA512Managed provider_SHA = new SHA512Managed();
                byte[] byte_pwdSHA = provider_SHA.ComputeHash(key_t);

                key = new byte[32];
                Array.Copy(byte_pwdSHA, key, 32);
	            iv = new byte[16];
                Array.Copy(Encoding.UTF8.GetBytes(G_IV), key, 16);
                myAes.Key = key;
                myAes.IV = iv;
            }
        }

        public static string DecPass(string encpass)
        {
            InitKey();
            string plainText = DecryptStringFromBytes_Aes(Convert.FromBase64String(encpass), myAes.Key, myAes.IV);
            return plainText;

        }

        public static string EncPass(string original)
        {
            InitKey();
            byte[] cipherText = EncryptStringToBytes_Aes(original, myAes.Key, myAes.IV);
            return Convert.ToBase64String(cipherText);
        }

        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }

        public string name { get; private set; }
        public string pass { get; private set; }
        public string note { get; private set; }
        public string file;

        public static AesManaged myAes = new AesManaged();
        const string G_KEY = "12345678xyze133ttyyfahdfajyeafjaldjzdvjldjfdlajafljdfnvladnnvnswdehhe329789oweuw";
        const string G_IV = "87654321xywer123488ewfjaldjf9u238r2fsjfalsfj;al;fjaoewurwhvfadlfhadfjalkfweourw";
    }

    class DBConf
    {
        public DBConf(string tnsname, string note)
        {
            this.tnsname = tnsname;
            this.note = note;
            Users = new List<DBUser>();
        }
        
        public string tnsname { get; set; }
        public string note;

        // 保存不同递交单的基础路径
        public List<DBUser> Users { get; private set; }
    }

    sealed class OraConf
    {
        private OraConf()
        {
            log = OperLog.instance;
            DBs = new List<DBConf>();
            // 加载配置文件
            LoadConf();
        }

        // 单例化 MAConf
        public static readonly OraConf instance = new OraConf();

        private void LoadConf()
        {
            log.WriteFileLog("加载配置文件");
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(conf);

            XmlElement root = xmldoc.DocumentElement;

            // 读取显示属性
            XmlNode xn;
            try
            {
                xn = root.SelectSingleNode("encry_str");
                keystr = xn.Attributes["key"].InnerText;
            }
            catch (Exception ex)
            {
                keystr = string.Empty;
            }

            // 读取Ssh连接配置
            xn = root.SelectSingleNode("DBBase");
            XmlNodeList xnl = xn.ChildNodes;
            XmlNodeList xll;
            foreach (XmlNode x in xnl)
            {
                // 跳过注释，否则格式不对，会报错
                if (x.NodeType == XmlNodeType.Comment)
                    continue;

                DBConf d = new DBConf(x.Attributes["dbtns"].InnerText,
                    x.Attributes["note"].InnerText);
                
                xll = x.ChildNodes;
                foreach (XmlNode xx in xll)
                {
                    DBUser u = new DBUser(xx.Attributes["name"].InnerText,
                        xx.Attributes["pass"].InnerText,
                        xx.Attributes["note"].InnerText);

                    u.file = xx.Attributes["file"].InnerText;
                    d.Users.Add(u);

                    
                }
                DBs.Add(d);
            }

            log.WriteFileLog("配置初始化完成");
        }

        // 先初始化日志
        private OperLog log;
        public List<DBConf> DBs;


        private string keystr;
        // 取配置文件名称
        private readonly string conf = "OraConf.xml";
    }
}