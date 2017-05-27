using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MhanoHarkness
{
    /// <summary>
    /// Base32Url is a standard base 32 encoder/decoder except that padding turned
    /// off and it is not case sensitive (by default).
    /// 
    /// If you turn padding and case sensitivity on it becomes a standard base32
    /// encoder/decoder giving you 8 character chunks right padded with equals symbols.
    /// 
    /// If you leave padding off and use Base32Url.ZBase32Alphabet you
    /// get a z-base-32 compatible encoder/decoder.
    /// 
    /// Note that the crockford base32 encoding doesn't support the crockford checksum
    /// mechanism.
    /// 
    /// See http://tools.ietf.org/html/rfc4648
    /// For more information see http://en.wikipedia.org/wiki/Base32
    /// </summary>
    public class Base32Url
    {
        public const char StandardPaddingChar = '=';
        public const string Base32StandardAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        public const string ZBase32Alphabet = "ybndrfg8ejkmcpqxot1uwisza345h769";
        public const string Base32LowProfanityAlphabet = "ybndrfg8NjkmGpq2HtPRYSszT3J5h769";

        public static readonly CharMap[] Base32CrockfordHumanFriendlyAlphabet =
        {
            new CharMap('0', "0Oo"), new CharMap('1', "1IiLl"), new CharMap('2', "2"), new CharMap('3', "3"), new CharMap('4', "4"),
            new CharMap('5', "5"), new CharMap('6', "6"), new CharMap('7', "7"), new CharMap('8', "8"), new CharMap('9', "9"), new CharMap('A', "Aa"),
            new CharMap('B', "Bb"), new CharMap('C', "Cc"), new CharMap('D', "Dd"), new CharMap('E', "Ee"), new CharMap('F', "Ff"), new CharMap('G', "Gg"),
            new CharMap('H', "Hh"), new CharMap('J', "Jj"), new CharMap('K', "Kk"), new CharMap('M', "Mm"), new CharMap('N', "Nn"), new CharMap('P', "Pp"),
            new CharMap('Q', "Qq"), new CharMap('R', "Rr"), new CharMap('S', "Ss"), new CharMap('T', "Tt"), new CharMap('V', "Vv"), new CharMap('W', "Ww"),
            new CharMap('X', "Xx"), new CharMap('Y', "Yy"), new CharMap('Z', "Zz"),
        };

        #region CharMap struct
        public struct CharMap
        {
            public CharMap(char encodeTo, IEnumerable<char> decodeFrom)
            {
                Encode = encodeTo.ToString(CultureInfo.InvariantCulture);

                if (decodeFrom == null)
                {
                    throw new ArgumentException("CharMap decodeFrom cannot be null, encodeTo was: " + Encode);
                }

                Decode = decodeFrom.Select(c => c.ToString(CultureInfo.InvariantCulture)).ToArray();

                if (Decode.Length == 0)
                {
                    throw new ArgumentException("CharMap decodeFrom cannot be empty, encodeTo was: " + Encode);
                }

                if (!Decode.Contains(Encode))
                {
                    throw new ArgumentException("CharMap decodeFrom must include encodeTo. encodeTo was: '" + Encode + "', decodeFrom was: '" + string.Join("", Decode) + "'");
                }
            }

            public readonly string Encode;
            public readonly string[] Decode;
        }
        #endregion CharMap struct

        public char PaddingChar;
        public bool UsePadding;
        public bool IsCaseSensitive;
        public bool IgnoreWhiteSpaceWhenDecoding;

        private readonly CharMap[] _alphabet;
        private Dictionary<string, uint> _index;

        // alphabets may be used with varying case sensitivity, thus index must not ignore case
        private static Dictionary<string, Dictionary<string, uint>> _indexes = new Dictionary<string, Dictionary<string, uint>>(2, StringComparer.InvariantCulture);

        /// <summary>
        /// Create case insensitive encoder/decoder using the standard base32 alphabet without padding.
        /// White space is not permitted when decoding (not ignored).
        /// </summary>
        public Base32Url() : this(false, false, false, Base32StandardAlphabet) { }

        /// <summary>
        /// Create case insensitive encoder/decoder using the standard base32 alphabet.
        /// White space is not permitted when decoding (not ignored).
        /// </summary>
        /// <param name="padding">Require/use padding characters?</param>
        public Base32Url(bool padding) : this(padding, false, false, Base32StandardAlphabet) { }

        /// <summary>
        /// Create encoder/decoder using the standard base32 alphabet.
        /// White space is not permitted when decoding (not ignored).
        /// </summary>
        /// <param name="padding">Require/use padding characters?</param>
        /// <param name="caseSensitive">Be case sensitive when decoding?</param>
        public Base32Url(bool padding, bool caseSensitive) : this(padding, caseSensitive, false, Base32StandardAlphabet) { }

        /// <summary>
        /// Create encoder/decoder using the standard base32 alphabet.
        /// </summary>
        /// <param name="padding">Require/use padding characters?</param>
        /// <param name="caseSensitive">Be case sensitive when decoding?</param>
        /// <param name="ignoreWhiteSpaceWhenDecoding">Ignore / allow white space when decoding?</param>
        public Base32Url(bool padding, bool caseSensitive, bool ignoreWhiteSpaceWhenDecoding) : this(padding, caseSensitive, ignoreWhiteSpaceWhenDecoding, Base32StandardAlphabet) { }

        /// <summary>
        /// Create case insensitive encoder/decoder with alternative alphabet and no padding.
        /// White space is not permitted when decoding (not ignored).
        /// </summary>
        /// <param name="alternateAlphabet">Alphabet to use (such as Base32Url.ZBase32Alphabet)</param>
        public Base32Url(string alternateAlphabet) : this(false, false, false, alternateAlphabet) { }

        /// <summary>
        /// Create the encoder/decoder specifying all options manually.
        /// </summary>
        /// <param name="padding">Require/use padding characters?</param>
        /// <param name="caseSensitive">Be case sensitive when decoding?</param>
        /// <param name="ignoreWhiteSpaceWhenDecoding">Ignore / allow white space when decoding?</param>
        /// <param name="alternateAlphabet">Alphabet to use (such as Base32Url.ZBase32Alphabet, Base32Url.Base32StandardAlphabet or your own custom 32 character alphabet string)</param>
        public Base32Url(bool padding, bool caseSensitive, bool ignoreWhiteSpaceWhenDecoding, string alternateAlphabet)
            : this(padding, caseSensitive, ignoreWhiteSpaceWhenDecoding, alternateAlphabet.Select(c => new CharMap(c, new[] { c })).ToArray())
        {
        }

        /// <summary>
        /// Create the encoder/decoder specifying all options manually.
        /// </summary>
        /// <param name="padding">Require/use padding characters?</param>
        /// <param name="caseSensitive">Be case sensitive when decoding?</param>
        /// <param name="ignoreWhiteSpaceWhenDecoding">Ignore / allow white space when decoding?</param>
        /// <param name="alphabet">
        ///     Alphabet to use (such as Base32Url.Base32CrockfordHumanFriendlyAlphabet) that decodes multiple characters with same meaning (i.e. 1,l,L etc.).
        ///     The array must have exactly 32 elements. EncodeTo in each CharMap is the character used for encoding.
        ///     DecodeFrom in each CharMap contains a list of characters that decode. This allows you to decode 1 L l i and I all as having the same meaning.
        ///     The position in the array of CharMaps is the binary "value" encoded, i.e. position 0 to 31 in the array map to binary nibble values of 0 to 31.
        ///     NOTE: an alphabet such as crockford may result in a case insensitive decoding of text, but case SENSITIVE must be specified as true in cases
        ///           like this to provide a unique mapping during decoding, thus to create a crockford style map you must always include the upper and lower
        ///           decode mappings of any case insensitive decode characters required.
        /// </param>
        public Base32Url(bool padding, bool caseSensitive, bool ignoreWhiteSpaceWhenDecoding, CharMap[] alphabet)
        {
            if (alphabet.Length != 32)
            {
                throw new ArgumentException("Alphabet must be exactly 32 characters long for base 32 encoding.");
            }
            if (alphabet.Any(t => t.Decode == null || t.Decode.Length == 0))
            {
                throw new ArgumentException("Alphabet must contain at least one decoding character for any given encoding chharacter.");
            }
            var equality = caseSensitive ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase;

            var encodingChars = alphabet.Select(t => t.Encode).GroupBy(k => k, equality).ToArray();
            if (encodingChars.Any(g => g.Count() > 1))
            {
                throw new ArgumentException("Case " + (caseSensitive ? "sensitive" : "insensitive") + " alphabet contains duplicate encoding characters: "
                    + string.Join(", ", encodingChars.Where(g => g.Count() > 1).Select(g => g.Key)));
            }

            var decodingChars = alphabet.SelectMany(t => t.Decode.Select(c => c)).GroupBy(k => k, equality).ToArray();
            if (decodingChars.Any(g => g.Count() > 1))
            {
                throw new ArgumentException("Case " + (caseSensitive ? "sensitive" : "insensitive") + " alphabet contains duplicate decoding characters: "
                    + string.Join(", ", decodingChars.Where(g => g.Count() > 1).Select(g => g.Key)));
            }


            PaddingChar = StandardPaddingChar;
            UsePadding = padding;
            IsCaseSensitive = caseSensitive;
            IgnoreWhiteSpaceWhenDecoding = ignoreWhiteSpaceWhenDecoding;

            _alphabet = alphabet;
        }

        /// <summary>
        /// Decode a base32 string to a byte[] using the default options
        /// (case insensitive without padding using the standard base32 alphabet from rfc4648).
        /// White space is not permitted (not ignored).
        /// Use alternative constructors for more options.
        /// </summary>
        public static byte[] FromBase32String(string input)
        {
            return new Base32Url().Decode(input);
        }

        /// <summary>
        /// Encode a base32 string from a byte[] using the default options
        /// (case insensitive without padding using the standard base32 alphabet from rfc4648).
        /// Use alternative constructors for more options.
        /// </summary>
        public static string ToBase32String(byte[] data)
        {
            return new Base32Url().Encode(data);
        }

        /// <summary>
        /// Converts a byte[] to a base32 string with the parameters provided in the constructor.
        /// </summary>
        /// <param name="data">bytes to encode</param>
        /// <returns>base 32 string</returns>
        public string Encode(byte[] data)
        {
            var result = new StringBuilder(Math.Max((int)Math.Ceiling(data.Length * 8 / 5.0), 1));

            var emptyBuff = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            var buff = new byte[8];

            // take input five bytes at a time to chunk it up for encoding
            for (int i = 0; i < data.Length; i += 5)
            {
                int bytes = Math.Min(data.Length - i, 5);

                // parse five bytes at a time using an 8 byte ulong
                Array.Copy(emptyBuff, buff, emptyBuff.Length);
                Array.Copy(data, i, buff, buff.Length - (bytes + 1), bytes);
                Array.Reverse(buff);
                ulong val = BitConverter.ToUInt64(buff, 0);

                for (int bitOffset = ((bytes + 1) * 8) - 5; bitOffset > 3; bitOffset -= 5)
                {
                    result.Append(_alphabet[(int)((val >> bitOffset) & 0x1f)].Encode);
                }
            }

            if (UsePadding)
            {
                result.Append(string.Empty.PadRight((result.Length % 8) == 0 ? 0 : (8 - (result.Length % 8)), PaddingChar));
            }

            return result.ToString();
        }

        /// <summary>
        /// Decodes a base32 string back to the original binary based on the constructor parameters.
        /// </summary>
        /// <param name="input">base32 string</param>
        /// <returns>byte[] of data originally encoded with Encode method</returns>
        /// <exception cref="ArgumentException">Thrown when string is invalid length if padding is expected or invalid (not in the base32 decoding set) characters are provided.</exception>
        public byte[] Decode(string input)
        {
            if (IgnoreWhiteSpaceWhenDecoding)
            {
                input = Regex.Replace(input, "\\s+", "");
            }

            if (UsePadding)
            {
                if (input.Length % 8 != 0)
                {
                    throw new ArgumentException("Invalid length for a base32 string with padding.");
                }

                input = input.TrimEnd(PaddingChar);
            }

            // index the alphabet for decoding only when needed
            EnsureAlphabetIndexed();

            var ms = new MemoryStream(Math.Max((int)Math.Ceiling(input.Length * 5 / 8.0), 1));

            // take input eight bytes at a time to chunk it up for encoding
            for (int i = 0; i < input.Length; i += 8)
            {
                int chars = Math.Min(input.Length - i, 8);

                ulong val = 0;

                int bytes = (int)Math.Floor(chars * (5 / 8.0));

                for (int charOffset = 0; charOffset < chars; charOffset++)
                {
                    uint cbyte;
                    if (!_index.TryGetValue(input.Substring(i + charOffset, 1), out cbyte))
                    {
                        throw new ArgumentException("Invalid character '" + input.Substring(i + charOffset, 1) + "' in base32 string, valid characters are: " + _alphabet);
                    }

                    val |= (((ulong)cbyte) << ((((bytes + 1) * 8) - (charOffset * 5)) - 5));
                }

                byte[] buff = BitConverter.GetBytes(val);
                Array.Reverse(buff);
                ms.Write(buff, buff.Length - (bytes + 1), bytes);
            }

            return ms.ToArray();
        }

        private void EnsureAlphabetIndexed()
        {
            if (_index != null) return;

            Dictionary<string, uint> cidx;

            var indexKey = (IsCaseSensitive ? "S" : "I") +
                string.Join("", _alphabet.Select(t => t.Encode)) +
                "_" + string.Join("", _alphabet.SelectMany(t => t.Decode).Select(c => c));

            if (!_indexes.TryGetValue(indexKey, out cidx))
            {
                lock (_indexes)
                {
                    if (!_indexes.TryGetValue(indexKey, out cidx))
                    {
                        var equality = IsCaseSensitive ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase;
                        cidx = new Dictionary<string, uint>(_alphabet.Length, equality);
                        for (int i = 0; i < _alphabet.Length; i++)
                        {
                            foreach (var c in _alphabet[i].Decode.Select(c => c))
                            {
                                cidx[c] = (uint)i;
                            }

                        }
                        _indexes.Add(indexKey, cidx);
                    }
                }
            }

            _index = cidx;
        }
    }
}
