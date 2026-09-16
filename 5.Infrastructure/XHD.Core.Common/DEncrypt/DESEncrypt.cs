using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace XHD.Core.Common.DEncrypt
{
    /// <summary>
    ///     DES加密/解密类。
    ///     Copyright (C) XHD
    /// </summary>
    public static class DESEncrypt
    {
        #region ========加密======== 

        /// <summary>
        ///     加密
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string Encrypt(string Text)
        {
            return Encrypt(Text, "XHD");
        }

        /// <summary>
        ///     加密数据
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public static string Encrypt(string Text, string sKey)
        {
            var des = new DESCryptoServiceProvider();
            byte[] inputByteArray;
            inputByteArray = Encoding.Default.GetBytes(Text);

            string md5SKey = MD5Comm.Get32MD5One(sKey).Substring(0, 8);

            des.Key =Encoding.ASCII.GetBytes(md5SKey);
            des.IV =Encoding.ASCII.GetBytes(md5SKey);
            var ms = new MemoryStream();
            var cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            var ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        #endregion

        #region ========解密======== 

        /// <summary>
        ///     解密
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string Decrypt(string Text)
        {
            return Decrypt(Text, "XHD");
        }

        /// <summary>
        ///     解密数据
        /// </summary>
        /// <param name="Text"></param>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public static string Decrypt(string Text, string sKey)
        {
            var des = new DESCryptoServiceProvider();
            int len;
            len = Text.Length/2;
            var inputByteArray = new byte[len];
            int x, i;
            for (x = 0; x < len; x++)
            {
                i = Convert.ToInt32(Text.Substring(x*2, 2), 16);
                inputByteArray[x] = (byte) i;
            }

            string md5SKey = MD5Comm.Get32MD5One(sKey).Substring(0, 8);

            des.Key =Encoding.ASCII.GetBytes(md5SKey);
            des.IV =Encoding.ASCII.GetBytes(md5SKey);
            var ms = new MemoryStream();
            var cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.Default.GetString(ms.ToArray());
        }

        #endregion
    }
}