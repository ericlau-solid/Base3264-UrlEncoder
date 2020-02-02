using System;
using System.Text;
using Xunit;
using MhanoHarkness;

namespace Base3264_UrlEncoder.Tests
{
	/// <summary>
	/// Basic unit tests covering key functional variants of base 32 encoding / decoding.
	/// TODO: Anyone feels like it some more comprehensive testing of the crockford encoding would be helpful. Cheers, Mhano
	/// TODO: Tests evolved a bit over time, refactoring to organise might be needed if adding significant test cases.
	/// </summary>
	public class Base32UrlTests
	{
		private static string[][] rfc4684TestVectors = new[]{
																new []{"f", "MY======"},
																new []{"fo", "MZXQ===="},
																new []{"foo", "MZXW6==="},
																new []{"foob", "MZXW6YQ="},
																new []{"fooba", "MZXW6YTB"},
																new []{"foobar", "MZXW6YTBOI======"},
																new []{"", ""},
															};

		[Fact]
		public void Rfc4648TestVectorsEncodeDecode()
		{
			var enc = new Base32Url(true, true, false);

			foreach (string[] s in rfc4684TestVectors)
			{
				Assert.Equal(enc.Encode(Encoding.ASCII.GetBytes(s[0])), s[1]);
				Assert.Equal(Encoding.ASCII.GetString(enc.Decode(s[1])), s[0]);
			}
		}

		[Fact]
		public void IgnoreWs()
		{
			Assert.Equal(Encoding.ASCII.GetString(new Base32Url(false, true, true).Decode("JBS\t\r\n WY3D\tPE BLW64\n TMM\r QQQ")), "Hello World!");
		}

		[Fact]
		public void NotIgnoreWs()
		{
			Assert.Throws<ArgumentException>(() =>
				Assert.Equal(Encoding.ASCII.GetString(new Base32Url(false, true, false).Decode("JBS\t\r\n WY3D\tPE BLW64\n TMM\r QQQ")), "Hello World!")
			);
		}

		[Fact]
		public void NoPaddingEncodeDecode()
		{
			var enc = new Base32Url(false, true, false);

			foreach (string[] s in rfc4684TestVectors)
			{
				Assert.Equal(Encoding.ASCII.GetString(enc.Decode(s[1].TrimEnd('='))), s[0]);
				Assert.Equal(enc.Encode(Encoding.ASCII.GetBytes(s[0])), s[1].TrimEnd('='));
			}
		}


		[Fact]
		public void CaseInsensitiveDecode()
		{
			var enc = new Base32Url(true, false, false);

			foreach (string[] s in rfc4684TestVectors)
			{
				string encodedS1 = s[1].ToLowerInvariant();
				Assert.Equal(Encoding.ASCII.GetString(enc.Decode(encodedS1)), s[0]);
			}
		}


		public void CaseSensitiveDecodeError()
		{
			var enc = new Base32Url(true, true, false);

			foreach (string[] s in rfc4684TestVectors)
			{
				string encodedS1 = s[1].ToLowerInvariant();
				Assert.Equal(Encoding.ASCII.GetString(enc.Decode(encodedS1)), s[0]);
			}
		}

		[Fact]
		public void NoPaddingCaseSensitiveCustomAlphabetsEncodeDecode()
		{
			// no vowels (prevents accidental profanity)
			// no numbers or letters easily mistakable

			foreach (string alphabet in new[] { "BCDFGHKMNPQRSTVWXYZ23456789bcdfg", "BCDFGHKMNPQRSTVWXYZbcdfghkmnpqrs" })
			{
				var enc = new Base32Url(true, true, false, alphabet);

				foreach (string[] s in rfc4684TestVectors)
				{
					Assert.Equal(Encoding.ASCII.GetString(enc.Decode(enc.Encode(Encoding.ASCII.GetBytes(s[0])))), s[0]);
				}
			}
		}

		[Fact]
		public void NoPaddingCaseInSensitiveCustomAlphabetsEncodeDecode()
		{
			// no vowels (prevents accidental profanity)
			// no numbers or letters easily mistakable

			foreach (string alphabet in new[] { "BCDFGHKMNPQRSTVWXYZ23456789~!@#$" })
			{
				var enc1 = new Base32Url(true, false, false, alphabet);
				var enc2 = new Base32Url(true, false, false, alphabet.ToLower());

				foreach (string[] s in rfc4684TestVectors)
				{
					Assert.Equal(Encoding.ASCII.GetString(enc2.Decode(enc1.Encode(Encoding.ASCII.GetBytes(s[0])))), s[0]);
					Assert.Equal(Encoding.ASCII.GetString(enc1.Decode(enc2.Encode(Encoding.ASCII.GetBytes(s[0])))), s[0]);
				}
			}
		}

		[Fact]
		public void SequentialValuesSequentialLengths()
		{
			var bcv = new Base32Url(false, false, false);

			for (int j = 1; j < 11; j++)
			{
				byte[] d = new byte[j];

				for (byte b = 0; b < 255; b++)
				{
					for (int i = 0; i < d.Length; i++)
					{
						d[i] = (byte)((b + i) % 255);
					}

					byte[] r = bcv.Decode(bcv.Encode(d));
					int tcnt = -1;

					bool result = Array.TrueForAll(d, b1 => { tcnt++; return r[tcnt] == d[tcnt]; });

					Assert.True(result);
				}
			}
		}
	}
}