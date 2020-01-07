using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Legend_Špedicija.Security
{
	public class Crypt_MD5 {
		public static string encryptPass(string str1) {
			string EncrptKey = "2013;[pnuLIT)WebCodeExpert";
			byte[] byKey = { };
			byte[] IV = { 18, 52, 86, 120, 144, 171, 205, 239 };
			byKey = System.Text.Encoding.UTF8.GetBytes(EncrptKey.Substring(0, 8));
			DESCryptoServiceProvider des = new DESCryptoServiceProvider();
			byte[] inputByteArray = Encoding.UTF8.GetBytes(str1);
			MemoryStream ms = new MemoryStream();
			CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
			cs.Write(inputByteArray, 0, inputByteArray.Length);
			cs.FlushFinalBlock();
			return Convert.ToBase64String(ms.ToArray());
		}

		public static string decryptPass(string str1) {
			str1 = str1.Replace(" ", "+");
			string DecryptKey = "2013;[pnuLIT)WebCodeExpert";
			byte[] byKey = { };
			byte[] IV = { 18, 52, 86, 120, 144, 171, 205, 239 };
			byte[] inputByteArray = new byte[ str1.Length ];

			byKey = System.Text.Encoding.UTF8.GetBytes(DecryptKey.Substring(0, 8));
			DESCryptoServiceProvider des = new DESCryptoServiceProvider();
			inputByteArray = Convert.FromBase64String(str1.Replace(" ", "+"));
			MemoryStream ms = new MemoryStream();
			CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);
			cs.Write(inputByteArray, 0, inputByteArray.Length);
			cs.FlushFinalBlock();
			System.Text.Encoding encoding = System.Text.Encoding.UTF8;
			return encoding.GetString(ms.ToArray());
		}
	}
}