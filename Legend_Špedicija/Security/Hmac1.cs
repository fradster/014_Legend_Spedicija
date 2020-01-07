using System;
using System.Security.Cryptography;
using System.Text;

namespace Legend_Špedicija.Security {
	/*usage:
	byte[] salt1 = Hmac1.GenerateSalt();//generate salt
	string hmac1 = GetHashString(strToHash, salt1);//get hashed string
	*/
	public static class Hmac1 {
		private const int SaltSize = 32;

		//------------------------------------------------------
		//GenerateSalt
		public static byte[] GenerateSalt() {
			using (var rng = new RNGCryptoServiceProvider()) {
				byte[] randomNumber = new byte[SaltSize];
				rng.GetBytes(randomNumber);

				return randomNumber;
			}
		}

		//------------------------------------------------------
		//get Hash from arrays of bytes, and input salt, returns arrays of bytes
		public static byte[] ComputeHMAC_SHA256(byte[] data, byte[] salt) {
			using (var hmac = new HMACSHA256(salt)) {
				return hmac.ComputeHash(data);
			}
		}

		//------------------------------------------------------
		//get Hash from string and salt, return hashed string
		public static string GetHashString (string strToHash, byte[] salt1) {
			byte[] hmac2 = Hmac1.ComputeHMAC_SHA256(Encoding .UTF8 .GetBytes (strToHash), salt1);
			string str2 = Convert.ToBase64String (hmac2);
			return str2;
		}
	}
}