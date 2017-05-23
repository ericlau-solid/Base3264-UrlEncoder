Base3264-UrlEncoder
===================
This is a direct port of mhano's wonderful base32 / base64 url encoder / decoder. Supports z-base-32 encoding, which I am a big fan of. Helping out the community by promoting this to GitHub, and Nuget.

Original code project article located here: http://www.codeproject.com/Tips/76650/Base-base-url-base-url-and-z-base-encoding

Contributor: Eric Lau


Introduction / Usage
====================

Standards based implementations of various Base32 and Base64 encoding/decoding methods. These are designed to encode binary data to plain text, and decode the resulting text back to the original binary. This is useful when you need to transfer binary data through technologies that only support text (such as including binary security tokens in URLs).

Base32Url encodes with only the characters A to Z and 2 to 7. No hyphens, underscores, pluses, slashes or equals are used, making it usable as a URL token in almost all circumstances. Base32Url also supports custom alphabets. A custom case sensitive alphabet with only consonant (non vowel) characters can be used to ensure your tokens do not contain accidental profanities. The following is an example that avoids vowels, the letter L and has no numeric characters: BCDFGHKMNPQRSTVWXYZbcdfghkmnpqrs.

Base64Url is more compact than Base32Url and it is almost always usable as a URL token or file-name. The only non alpha-numeric characters Base64Url contains are the hyphen (-) and underscore (_) characters, neither of these need further encoding for use in URLs or file-names.


Base32Url (Encoder / Decoder)
=============================

The default mode for the Base32 encoder/decoder is Base32Url. This uses the standard Base32 alphabet encoding but omits padding characters and is case insensitive.

* Supports standard Base32 with padding characters (=) per Base32 from RFC 4648.
* Supports the Base32 extension / alternate alphabet z-base-32 and the asymmetric crockford encoding.


Base64Url (Encoder / Decoder)
=============================

* Based on the standard .NET Base64 encoder
* Uses the URL-Safe alternative Base64 alphabet from RFC 4648
* This is not the same as Microsoft’s HttpServerUtility.UrlTokenEncode.

Further Information and Usage:

There are other implementations of base32 encoding out there but I feel the code of this base32 implementation is much simpler (far less code involved in the bit shifting calculations).

The base64 implementation I have here is a little hackish, but a far better option than the one you get from Microsoft.

The result you get from HttpServerUtility.UrlTokenEncode is essentially base64url, but instead of truncating the padding, they append a digit (of 0, 1 or 2) indicating the number of padding characters removed.


Usage / Examples
===================
 
byte[] myByteArray = Encoding.ASCII.GetBytes("Hello World!");

Base32Url.ToBase32String(myByteArray);
 
JBSWY3DPEBLW64TMMQQQ
 
var b32 = new Base32Url(true); // Base32Url(bool usePadding)
b32.Encode(myByteArray);
 
JBSWY3DPEBLW64TMMQQQ====
For more information about the standards involved, please see RFC 4648 http://tools.ietf.org/html/rfc4648

Base3264Encoding Convienience Methods
=====================================

string str1 = Base3264Encoding.ToZBase32(myByteArray);

byte[] decoded = Base3264Encoding.FromZBase32(str1);

Other Helper / Convienience Methods from Base3264Encoding:

* Encodings: Base32Url, ZBase32, Base32LowProfanity, Base32Crockford, Base64Url
* Methods to encode from binary (and unicode string) to encoded string
* Methods to decode from encoded string back to binary (or original unicode string)

References / Standards
======================

* http://en.wikipedia.org/wiki/Base32
* http://en.wikipedia.org/wiki/Base64
* http://tools.ietf.org/html/rfc4648
* http://www.crockford.com/wrmg/base32.html
