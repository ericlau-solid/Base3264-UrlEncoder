using System;
using System.Text;

namespace MhanoHarkness
{
    /// <summary>
    /// Base3264Encoding is a standard base 32/64 encoder/decoder
    /// 
    /// The base 32 conversions are based on the standards except that padding is turned
    /// off and it is not case sensitive (by default).
    /// 
    /// ZBase32, Crockford and a low profanity base 32 alphabets are included to support
    /// various use cases.
    /// 
    /// Note that the crockford base32 encoding doesn't support the crockford checksum
    /// mechanism.
    /// 
    /// RFC: http://tools.ietf.org/html/rfc4648
    /// Base32: http://en.wikipedia.org/wiki/Base32
    /// Base64: http://en.wikipedia.org/wiki/Base64
    /// Crockford: http://www.crockford.com/wrmg/base32.html
    /// </summary>
    public static class Base3264Encoding
    {
        /// <summary>
        /// Encodes bytes (such as binary url tokens) into a string.
        /// </summary>
        /// <param name="type">Encoding type.</param>
        /// <param name="input">byte[] to be encoded</param>
        /// <returns>encoded string</returns>
        /// <exception cref="ArgumentException">If encoding type is None, UriData, UriPathData or Uri</exception>
        public static string Encode(EncodingType type, byte[] input)
        {
            switch (type)
            {
                case EncodingType.Base64:
                    return Convert.ToBase64String(input, Base64FormattingOptions.None);
                case EncodingType.Base64Url:
                    return Base64Url.ToBase64ForUrlString(input);
                case EncodingType.Base32Url:
                    return Base32Url.ToBase32String(input);
                case EncodingType.ZBase32:
                    return new Base32Url(false, false, true, Base32Url.ZBase32Alphabet).Encode(input);
                case EncodingType.Base32LowProfanity:
                    return new Base32Url(false, true, true, Base32Url.Base32LowProfanityAlphabet).Encode(input);
                case EncodingType.Base32Crockford:
                    return new Base32Url(false, true, true, Base32Url.Base32CrockfordHumanFriendlyAlphabet).Encode(input);
                default:
                    throw new NotImplementedException("Encoding type not implemented: " + type);
            }
        }

        /// <summary>
        /// Decodes an encoded string to the original binary.
        /// </summary>
        /// <param name="type">Encoding type</param>
        /// <param name="input">Encoded string</param>
        /// <returns>Original byte[]</returns>
        /// <exception cref="ArgumentException">If encoding type is None, UriData, UriPathData or Uri</exception>
        public static byte[] Decode(EncodingType type, string input)
        {
            switch (type)
            {
                case EncodingType.Base64:
                    return Convert.FromBase64String(input);
                case EncodingType.Base64Url:
                    return Base64Url.FromBase64ForUrlString(input);
                case EncodingType.Base32Url:
                    return Base32Url.FromBase32String(input);
                case EncodingType.ZBase32:
                    return new Base32Url(false, false, true, Base32Url.ZBase32Alphabet).Decode(input);
                case EncodingType.Base32LowProfanity:
                    return new Base32Url(false, true, true, Base32Url.Base32LowProfanityAlphabet).Decode(input);
                case EncodingType.Base32Crockford:
                    return new Base32Url(false, true, true, Base32Url.Base32CrockfordHumanFriendlyAlphabet).Decode(input);
                default:
                    throw new NotImplementedException("Encoding type not implemented: " + type);
            }
        }

        /// <summary>
        /// Encodes a string to specified format encoding (base 32/64/etc), strings are first converted to a bom-free utf8 byte[] then to relevant encoding.
        /// </summary>
        /// <param name="type">encoding type</param>
        /// <param name="input">string to be encoded</param>
        /// <returns>Encoded string</returns>
        public static string EncodeString(EncodingType type, string input)
        {
            var enc = new UTF8Encoding(false, true);

            switch (type)
            {
                case EncodingType.Base64:
                    return Convert.ToBase64String(enc.GetBytes(input), Base64FormattingOptions.None);
                case EncodingType.Base64Url:
                    return Base64Url.ToBase64ForUrlString(enc.GetBytes(input));
                case EncodingType.Base32Url:
                    return Base32Url.ToBase32String(enc.GetBytes(input));
                case EncodingType.ZBase32:
                    return new Base32Url(false, false, true, Base32Url.ZBase32Alphabet).Encode(enc.GetBytes(input));
                case EncodingType.Base32LowProfanity:
                    return new Base32Url(false, true, true, Base32Url.Base32LowProfanityAlphabet).Encode(enc.GetBytes(input));
                case EncodingType.Base32Crockford:
                    return new Base32Url(false, true, true, Base32Url.Base32CrockfordHumanFriendlyAlphabet).Encode(enc.GetBytes(input));
                default:
                    throw new NotImplementedException("Encoding type not implemented: " + type);
            }
        }

        /// <summary>
        /// Decodes an encoded string to its original string value (before it was encoded with the EncodeString method)
        /// </summary>
        /// <param name="type">Decoding type</param>
        /// <param name="input">Encoded string</param>
        /// <returns>Original string value</returns>
        public static string DecodeToString(EncodingType type, string input)
        {
            var enc = new UTF8Encoding(false, true);

            switch (type)
            {
                case EncodingType.Base64:
                    return enc.GetString(Convert.FromBase64String(input));
                case EncodingType.Base64Url:
                    return enc.GetString(Base64Url.FromBase64ForUrlString(input));
                case EncodingType.Base32Url:
                    return enc.GetString(Base32Url.FromBase32String(input));
                case EncodingType.ZBase32:
                    return enc.GetString(new Base32Url(false, false, true, Base32Url.ZBase32Alphabet).Decode(input));
                case EncodingType.Base32LowProfanity:
                    return enc.GetString(new Base32Url(false, true, true, Base32Url.Base32LowProfanityAlphabet).Decode(input));
                case EncodingType.Base32Crockford:
                    return enc.GetString(new Base32Url(false, true, true, Base32Url.Base32CrockfordHumanFriendlyAlphabet).Decode(input));
                default:
                    throw new NotImplementedException("Encoding type not implemented: " + type);
            }
        }

        /// <summary>
        /// Binary to standard base 64 (not url/uri safe)
        /// </summary>
        public static string ToBase64(byte[] input)
        {
            return Encode(EncodingType.Base64, input);
        }

        /// <summary>
        /// String to standard base 64 (not url/uri safe) - uses bom-free utf8 for string to binary encoding
        /// </summary>
        public static string ToBase64(string input)
        {
            return EncodeString(EncodingType.Base64, input);
        }

        /// <summary>
        /// Binary to "base64-url" per rfc standard (url safe 63rd/64th characters and no padding)
        /// </summary>
        public static string ToBase64Url(byte[] input)
        {
            return Encode(EncodingType.Base64Url, input);
        }
        /// <summary>
        /// String to "base64-url" per rfc standard (url safe 63rd/64th characters and no padding) - uses bom-free utf8 for string to binary encoding
        /// </summary>
        public static string ToBase64Url(string input)
        {
            return EncodeString(EncodingType.Base64Url, input);
        }

        /// <summary>
        /// Binary to "base32 url" per rfc (standard base 32 without padding, no padding, case insensitive)
        /// </summary>
        public static string ToBase32Url(byte[] input)
        {
            return Encode(EncodingType.Base32Url, input);
        }

        /// <summary>
        /// String to "base32 url" per rfc (standard base 32 without padding, no padding, case insensitive) - uses bom-free utf8 for string to binary encoding
        /// </summary>
        public static string ToBase32Url(string input)
        {
            return EncodeString(EncodingType.Base32Url, input);
        }

        /// <summary>
        /// Binary to zbase32 (no padding, case insensitive)
        /// </summary>
        public static string ToZBase32(byte[] input)
        {
            return Encode(EncodingType.ZBase32, input);
        }

        /// <summary>
        /// String to zbase32 (no padding, case insensitive) - uses bom-free utf8 for string to binary encoding
        /// </summary>
        public static string ToZBase32(string input)
        {
            return EncodeString(EncodingType.ZBase32, input);
        }

        /// <summary>
        /// Binary to base32 encoding with alphabet designed to reduce accidental profanity in output (no padding, case SENSITIVE)
        /// </summary>
        public static string ToBase32LowProfanity(byte[] input)
        {
            return Encode(EncodingType.Base32LowProfanity, input);
        }

        /// <summary>
        /// String to base32 encoding with alphabet designed to reduce accidental profanity in output (no padding, case SENSITIVE) - uses bom-free utf8 for string to binary encoding
        /// </summary>
        public static string ToBase32LowProfanity(string input)
        {
            return EncodeString(EncodingType.Base32LowProfanity, input);
        }

        /// <summary>
        /// Binary to base32 encoding designed for human readability or OCR / hand writing recognition situations (non symmetric conversion - i.e. 1IiLl all mean the same thing). No padding, case SENSITIVE.
        /// </summary>
        public static string ToBase32Crockford(byte[] input)
        {
            return Encode(EncodingType.Base32Crockford, input);
        }

        /// <summary>
        /// String to base32 encoding designed for human readability or OCR / hand writing recognition situations (non symmetric conversion - i.e. 1IiLl all mean the same thing). No padding, case SENSITIVE.
        /// Uses bom-free utf8 for string to binary encoding.
        /// </summary>
        public static string ToBase32Crockford(string input)
        {
            return EncodeString(EncodingType.Base32Crockford, input);
        }

        /// <summary>
        /// To binary from standard base 64 (not url/uri safe)
        /// </summary>
        public static byte[] FromBase64(string input)
        {
            return Decode(EncodingType.Base64, input);
        }

        /// <summary>
        /// To string from base 64 (not url/uri safe) - uses bom-free utf8 for string to binary encoding
        /// </summary>
        public static string FromBase64ToString(string input)
        {
            return DecodeToString(EncodingType.Base64, input);
        }

        /// <summary>
        /// To binary from "base64-url" per rfc standard (url safe 63rd/64th characters and no padding)
        /// </summary>
        public static byte[] FromBase64Url(string input)
        {
            return Decode(EncodingType.Base64Url, input);
        }

        /// <summary>
        /// To string from "base64-url" per rfc standard (url safe 63rd/64th characters and no padding) - uses bom-free utf8 for string to binary encoding
        /// </summary>
        public static string FromBase64UrlToString(string input)
        {
            return DecodeToString(EncodingType.Base64Url, input);
        }

        /// <summary>
        /// To binary from "base32 url" per rfc (standard base 32 without padding, no padding, case insensitive)
        /// </summary>
        public static byte[] FromBase32Url(string input)
        {
            return Decode(EncodingType.Base32Url, input);
        }

        /// <summary>
        /// To string from "base32 url" per rfc (standard base 32 without padding, no padding, case insensitive) - uses bom-free utf8 for string to binary encoding
        /// </summary>
        public static string FromBase32UrlToString(string input)
        {
            return DecodeToString(EncodingType.Base32Url, input);
        }

        /// <summary>
        /// To binary from zbase32 (no padding, case insensitive) - uses bom-free utf8 for string to binary encoding
        /// </summary>
        public static byte[] FromZBase32(string input)
        {
            return Decode(EncodingType.ZBase32, input);
        }

        /// <summary>
        /// to string from zbase32 (no padding, case insensitive) - uses bom-free utf8 for string to binary encoding
        /// </summary>
        public static string FromZBase32ToString(string input)
        {
            return DecodeToString(EncodingType.ZBase32, input);
        }

        /// <summary>
        /// To binary from base32 encoding with alphabet designed to reduce accidental profanity in output (no padding, case SENSITIVE)
        /// </summary>
        public static byte[] FromBase32LowProfanity(string input)
        {
            return Decode(EncodingType.Base32LowProfanity, input);
        }

        /// <summary>
        /// To string from base32 encoding with alphabet designed to reduce accidental profanity in output (no padding, case SENSITIVE) - uses bom-free utf8 for string to binary encoding
        /// </summary>
        public static string FromBase32LowProfanityToString(string input)
        {
            return DecodeToString(EncodingType.Base32LowProfanity, input);
        }

        /// <summary>
        /// To binary from base32 encoding designed for human readability or OCR / hand writing recognition situations (non symmetric conversion - i.e. 1IiLl all mean the same thing). No padding, case SENSITIVE.
        /// </summary>
        public static byte[] FromBase32Crockford(string input)
        {
            return Decode(EncodingType.Base32Crockford, input);
        }

        /// <summary>
        /// To string from base32 encoding designed for human readability or OCR / hand writing recognition situations (non symmetric conversion - i.e. 1IiLl all mean the same thing). No padding, case SENSITIVE.
        /// Uses bom-free utf8 for string to binary encoding.
        /// </summary>
        public static string FromBase32CrockfordToString(string input)
        {
            return DecodeToString(EncodingType.Base32Crockford, input);
        }
    }
}
