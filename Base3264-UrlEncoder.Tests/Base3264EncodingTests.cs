using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MhanoHarkness;
using Xunit;

namespace Base3264_UrlEncoder.Tests
{
    /// <summary>
    /// Some initial base32/64 unit tests including static values to ensure compatibility going forward.
    /// TODO: Anyone feels like it some more comprehensive testing of the crockford encoding would be helpful. Cheers, Mhano
    /// TODO: Tests evolved a bit over time, refactoring to organise might be needed if adding significant test cases.
    /// </summary>
    public class Base3264EncodingTests
    {
        private const string Chars = @"!""#$%&'()*+,-./:;<=>?@[\]^_`{|}~€‚ƒ„…†‡ˆ‰Š‹Œ¼½¾¿ÀÁÂÃÄÅÆÇÈÏÐÑÕ×ØÛÜÝÞßåæçéíðñõö÷øüýþÿabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ electricity 電	电	電 red 紅	红	紅";

        [Fact]
        public void TestEncodeDecode()
        {
            var bytes = Encoding.UTF8.GetBytes(Chars);

            foreach (EncodingType encType in Enum.GetValues(typeof(EncodingType)))
            {
                var enc = Base3264Encoding.Encode(encType, bytes);
                var dec = Base3264Encoding.Decode(encType, enc);

                Assert.NotEqual(Chars, enc);
                Assert.Equal(bytes, dec);
                // Console.WriteLine("Passed Binary: " + encType);

                var enc2 = Base3264Encoding.EncodeString(encType, Chars);
                var dec2 = Base3264Encoding.DecodeToString(encType, enc2);

                Assert.NotEqual(Chars, enc2);

                Assert.Equal(Chars, dec2);
                // Console.WriteLine("Passed Text: " + encType);
            }
        }

        [Fact]
        public void TestAssymetricAlphabet()
        {
            var b32 = new Base32Url(false, true, true, Base32Url.Base32CrockfordHumanFriendlyAlphabet);

            var s = "Hello World!";
            var b = Encoding.UTF8.GetBytes(s);
            var enc = b32.Encode(b);

            Assert.Equal("91JPRV3F41BPYWKCCGGG", enc);
            Assert.Equal(s, Encoding.UTF8.GetString(b32.Decode(enc)));

            // test decode replacing second character which is a 1 above with an I here
            Assert.Equal(s, Encoding.UTF8.GetString(b32.Decode("9IJPRV3F41BPYWKCCGGG")));
        }

        [Fact]
        public void TestBadAlphabetCI()
        {
            var ex = Assert.Throws<ArgumentException>(() => new Base32Url(false, true, true, "AACDEFGHIJKLMNOPQRSTUVWXYZ123456").Encode(Guid.NewGuid().ToByteArray()));
            Assert.Equal("Case sensitive alphabet contains duplicate encoding characters: A", ex.Message);
        }

        [Fact]
        public void TestBadAlphabetCI2()
        {
            var ex = Assert.Throws<ArgumentException>(() => new Base32Url(false, false, true, "AaCDEFGHIJKLMNOPQRSTUVWXYZ123456").Encode(Guid.NewGuid().ToByteArray()));
            Assert.Equal("Case insensitive alphabet contains duplicate encoding characters: A", ex.Message);
        }

        [Fact]
        public void TestBadAlphabetAllowedIfCS()
        {
            var g = Guid.NewGuid();
            var enc = new Base32Url(false, true, true, "AaCDEFGHIJKLMNOPQRSTUVWXYZ123456");
            var o = enc.Encode(g.ToByteArray());
            Assert.Equal(g, new Guid(enc.Decode(o)));
        }

        [Fact]
        public void TestMethods()
        {
            var enc = Encoding.UTF8;
            var stringIn = "Hello World!";
            var bytes = enc.GetBytes(stringIn);

            Assert.Equal("SGVsbG8gV29ybGQh", Base3264Encoding.ToBase64(bytes));
            Assert.Equal("SGVsbG8gV29ybGQh", Base3264Encoding.ToBase64Url(bytes));
            Assert.Equal("JBSWY3DPEBLW64TMMQQQ", Base3264Encoding.ToBase32Url(bytes));
            Assert.Equal("jb1sa5dxrbms6huccooo", Base3264Encoding.ToZBase32(bytes));
            Assert.Equal("jbPsT5d2rbms6hRGGHHH", Base3264Encoding.ToBase32LowProfanity(bytes));
            Assert.Equal("SGVsbG8gV29ybGQh", Base3264Encoding.ToBase64(stringIn));
            Assert.Equal("SGVsbG8gV29ybGQh", Base3264Encoding.ToBase64Url(stringIn));
            Assert.Equal("JBSWY3DPEBLW64TMMQQQ", Base3264Encoding.ToBase32Url(stringIn));
            Assert.Equal("jb1sa5dxrbms6huccooo", Base3264Encoding.ToZBase32(stringIn));
            Assert.Equal("jbPsT5d2rbms6hRGGHHH", Base3264Encoding.ToBase32LowProfanity(stringIn));
            Assert.Equal("91JPRV3F41BPYWKCCGGG", Base3264Encoding.ToBase32Crockford(stringIn));

            Assert.Equal(stringIn, enc.GetString(Base3264Encoding.FromBase64(Base3264Encoding.ToBase64(stringIn))));
            Assert.Equal(stringIn, enc.GetString(Base3264Encoding.FromBase64Url(Base3264Encoding.ToBase64Url(stringIn))));
            Assert.Equal(stringIn, enc.GetString(Base3264Encoding.FromBase32Url(Base3264Encoding.ToBase32Url(stringIn))));
            Assert.Equal(stringIn, enc.GetString(Base3264Encoding.FromZBase32(Base3264Encoding.ToZBase32(stringIn))));
            Assert.Equal(stringIn, enc.GetString(Base3264Encoding.FromBase32LowProfanity(Base3264Encoding.ToBase32LowProfanity(stringIn))));
            Assert.Equal(stringIn, enc.GetString(Base3264Encoding.FromBase32Crockford(Base3264Encoding.ToBase32Crockford(stringIn))));
            Assert.Equal(stringIn, Base3264Encoding.FromBase64ToString(Base3264Encoding.ToBase64(stringIn)));

            Assert.Equal(stringIn, Base3264Encoding.FromBase64UrlToString(Base3264Encoding.ToBase64Url(stringIn)));
            Assert.Equal(stringIn, Base3264Encoding.FromBase32UrlToString(Base3264Encoding.ToBase32Url(stringIn)));
            Assert.Equal(stringIn, Base3264Encoding.FromZBase32ToString(Base3264Encoding.ToZBase32(stringIn)));
            Assert.Equal(stringIn, Base3264Encoding.FromBase32LowProfanityToString(Base3264Encoding.ToBase32LowProfanity(stringIn)));
            Assert.Equal(stringIn, Base3264Encoding.FromBase32CrockfordToString(Base3264Encoding.ToBase32Crockford(stringIn)));
        }

        [Fact]
        public void TestKnownValues()
        {
            // it is important to lock in fixed values, and not just test round trip encoding/decoding,
            // this protects us from future breaking encoding changes.

            var knownValues = new Dictionary<string, Dictionary<EncodingType, string>>
            {
                {"Hello World!",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "SGVsbG8gV29ybGQh"},
                    {EncodingType.Base64Url, "SGVsbG8gV29ybGQh"},
                    {EncodingType.Base32Url, "JBSWY3DPEBLW64TMMQQQ"},
                    {EncodingType.ZBase32, "jb1sa5dxrbms6huccooo"},
                    {EncodingType.Base32LowProfanity, "jbPsT5d2rbms6hRGGHHH"},
                    {EncodingType.Base32Crockford, "91JPRV3F41BPYWKCCGGG"},
                }},
                {"Lorem",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0="},
                    {EncodingType.Base64Url, "TG9yZW0"},
                    {EncodingType.Base32Url, "JRXXEZLN"},
                    {EncodingType.ZBase32, "jtzzr3mp"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mp"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD"},
                }},
                {"Lorem ipsum",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0="},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNU"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipw"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpY"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDM"},
                }},
                {"Lorem ipsum dolor",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0gZG9sb3I="},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0gZG9sb3I"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNUQGI33MN5ZA"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipwoge55cp73y"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpYHgN55Gp73y"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDMG68VVCDXS0"},
                }},
                {"Lorem ipsum dolor sit",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0"},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNUQGI33MN5ZCA43JOQ"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipwoge55cp73nyh5jqo"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpYHgN55Gp73nyh5jqH"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDMG68VVCDXS20WV9EG"},
                }},
                {"Lorem ipsum dolor sit amet,",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQs"},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQs"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNUQGI33MN5ZCA43JOQQGC3LFOQWA"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipwoge55cp73nyh5jqoogn5mfqosy"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpYHgN55Gp73nyh5jqHHgn5mfqHsy"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDMG68VVCDXS20WV9EGG62VB5EGP0"},
                }},
                {"Lorem ipsum dolor sit amet, consectetur",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVy"},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVy"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNUQGI33MN5ZCA43JOQQGC3LFOQWCAY3PNZZWKY3UMV2HK4Q"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipwoge55cp73nyh5jqoogn5mfqosnya5xp33ska5wci48kho"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpYHgN55Gp73nyh5jqHHgn5mfqHsnyT52p33skT5YGSJ8khH"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDMG68VVCDXS20WV9EGG62VB5EGP20RVFDSSPARVMCNT7AWG"},
                }},
                {"Lorem ipsum dolor sit amet, consectetur adipiscing",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2Npbmc="},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2Npbmc"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNUQGI33MN5ZCA43JOQQGC3LFOQWCAY3PNZZWKY3UMV2HK4RAMFSGS4DJONRWS3TH"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipwoge55cp73nyh5jqoogn5mfqosnya5xp33ska5wci48khtycf1g1hdjqpts15u8"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpYHgN55Gp73nyh5jqHHgn5mfqHsnyT52p33skT5YGSJ8khtyGfPgPhdjqptsP5R8"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDMG68VVCDXS20WV9EGG62VB5EGP20RVFDSSPARVMCNT7AWH0C5J6JW39EDHPJVK7"},
                }},
                {"Lorem ipsum dolor sit amet, consectetur adipiscing elit,",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCw="},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCw"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNUQGI33MN5ZCA43JOQQGC3LFOQWCAY3PNZZWKY3UMV2HK4RAMFSGS4DJONRWS3THEBSWY2LUFQ"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipwoge55cp73nyh5jqoogn5mfqosnya5xp33ska5wci48khtycf1g1hdjqpts15u8rb1sa4mwfo"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpYHgN55Gp73nyh5jqHHgn5mfqHsnyT52p33skT5YGSJ8khtyGfPgPhdjqptsP5R8rbPsTJmYfH"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDMG68VVCDXS20WV9EGG62VB5EGP20RVFDSSPARVMCNT7AWH0C5J6JW39EDHPJVK741JPRTBM5G"},
                }},
                {"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2Vk"},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2Vk"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNUQGI33MN5ZCA43JOQQGC3LFOQWCAY3PNZZWKY3UMV2HK4RAMFSGS4DJONRWS3THEBSWY2LUFQQHGZLE"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipwoge55cp73nyh5jqoogn5mfqosnya5xp33ska5wci48khtycf1g1hdjqpts15u8rb1sa4mwfoo8g3mr"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpYHgN55Gp73nyh5jqHHgn5mfqHsnyT52p33skT5YGSJ8khtyGfPgPhdjqptsP5R8rbPsTJmYfHH8g3mr"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDMG68VVCDXS20WV9EGG62VB5EGP20RVFDSSPARVMCNT7AWH0C5J6JW39EDHPJVK741JPRTBM5GG76SB4"},
                }},
                {"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRv"},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRv"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNUQGI33MN5ZCA43JOQQGC3LFOQWCAY3PNZZWKY3UMV2HK4RAMFSGS4DJONRWS3THEBSWY2LUFQQHGZLEEBSG6"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipwoge55cp73nyh5jqoogn5mfqosnya5xp33ska5wci48khtycf1g1hdjqpts15u8rb1sa4mwfoo8g3mrrb1g6"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpYHgN55Gp73nyh5jqHHgn5mfqHsnyT52p33skT5YGSJ8khtyGfPgPhdjqptsP5R8rbPsTJmYfHH8g3mrrbPg6"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDMG68VVCDXS20WV9EGG62VB5EGP20RVFDSSPARVMCNT7AWH0C5J6JW39EDHPJVK741JPRTBM5GG76SB441J6Y"},
                }},
                {"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2Q="},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2Q"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNUQGI33MN5ZCA43JOQQGC3LFOQWCAY3PNZZWKY3UMV2HK4RAMFSGS4DJONRWS3THEBSWY2LUFQQHGZLEEBSG6IDFNF2XG3LPMQ"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipwoge55cp73nyh5jqoogn5mfqosnya5xp33ska5wci48khtycf1g1hdjqpts15u8rb1sa4mwfoo8g3mrrb1g6edfpf4zg5mxco"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpYHgN55Gp73nyh5jqHHgn5mfqHsnyT52p33skT5YGSJ8khtyGfPgPhdjqptsP5R8rbPsTJmYfHH8g3mrrbPg6NdfpfJzg5m2GH"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDMG68VVCDXS20WV9EGG62VB5EGP20RVFDSSPARVMCNT7AWH0C5J6JW39EDHPJVK741JPRTBM5GG76SB441J6Y835D5TQ6VBFCG"},
                }},
                {"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2QgdGVtcG9y"},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2QgdGVtcG9y"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNUQGI33MN5ZCA43JOQQGC3LFOQWCAY3PNZZWKY3UMV2HK4RAMFSGS4DJONRWS3THEBSWY2LUFQQHGZLEEBSG6IDFNF2XG3LPMQQHIZLNOBXXE"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipwoge55cp73nyh5jqoogn5mfqosnya5xp33ska5wci48khtycf1g1hdjqpts15u8rb1sa4mwfoo8g3mrrb1g6edfpf4zg5mxcoo8e3mpqbzzr"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpYHgN55Gp73nyh5jqHHgn5mfqHsnyT52p33skT5YGSJ8khtyGfPgPhdjqptsP5R8rbPsTJmYfHH8g3mrrbPg6NdfpfJzg5m2GHH8N3mpqbzzr"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDMG68VVCDXS20WV9EGG62VB5EGP20RVFDSSPARVMCNT7AWH0C5J6JW39EDHPJVK741JPRTBM5GG76SB441J6Y835D5TQ6VBFCGG78SBDE1QQ4"},
                }},
                {"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2QgdGVtcG9yIGluY2lkaWR1bnQ="},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2QgdGVtcG9yIGluY2lkaWR1bnQ"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNUQGI33MN5ZCA43JOQQGC3LFOQWCAY3PNZZWKY3UMV2HK4RAMFSGS4DJONRWS3THEBSWY2LUFQQHGZLEEBSG6IDFNF2XG3LPMQQHIZLNOBXXEIDJNZRWSZDJMR2W45A"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipwoge55cp73nyh5jqoogn5mfqosnya5xp33ska5wci48khtycf1g1hdjqpts15u8rb1sa4mwfoo8g3mrrb1g6edfpf4zg5mxcoo8e3mpqbzzredjp3ts13djct4sh7y"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpYHgN55Gp73nyh5jqHHgn5mfqHsnyT52p33skT5YGSJ8khtyGfPgPhdjqptsP5R8rbPsTJmYfHH8g3mrrbPg6NdfpfJzg5m2GHH8N3mpqbzzrNdjp3tsP3djGtJsh7y"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDMG68VVCDXS20WV9EGG62VB5EGP20RVFDSSPARVMCNT7AWH0C5J6JW39EDHPJVK741JPRTBM5GG76SB441J6Y835D5TQ6VBFCGG78SBDE1QQ4839DSHPJS39CHTPWX0"},
                }},
                {"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2QgdGVtcG9yIGluY2lkaWR1bnQgdXQ="},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2QgdGVtcG9yIGluY2lkaWR1bnQgdXQ"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNUQGI33MN5ZCA43JOQQGC3LFOQWCAY3PNZZWKY3UMV2HK4RAMFSGS4DJONRWS3THEBSWY2LUFQQHGZLEEBSG6IDFNF2XG3LPMQQHIZLNOBXXEIDJNZRWSZDJMR2W45BAOV2A"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipwoge55cp73nyh5jqoogn5mfqosnya5xp33ska5wci48khtycf1g1hdjqpts15u8rb1sa4mwfoo8g3mrrb1g6edfpf4zg5mxcoo8e3mpqbzzredjp3ts13djct4sh7byqi4y"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpYHgN55Gp73nyh5jqHHgn5mfqHsnyT52p33skT5YGSJ8khtyGfPgPhdjqptsP5R8rbPsTJmYfHH8g3mrrbPg6NdfpfJzg5m2GHH8N3mpqbzzrNdjp3tsP3djGtJsh7byqSJy"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDMG68VVCDXS20WV9EGG62VB5EGP20RVFDSSPARVMCNT7AWH0C5J6JW39EDHPJVK741JPRTBM5GG76SB441J6Y835D5TQ6VBFCGG78SBDE1QQ4839DSHPJS39CHTPWX10ENT0"},
                }},
                {"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2QgdGVtcG9yIGluY2lkaWR1bnQgdXQgbGFib3Jl"},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2QgdGVtcG9yIGluY2lkaWR1bnQgdXQgbGFib3Jl"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNUQGI33MN5ZCA43JOQQGC3LFOQWCAY3PNZZWKY3UMV2HK4RAMFSGS4DJONRWS3THEBSWY2LUFQQHGZLEEBSG6IDFNF2XG3LPMQQHIZLNOBXXEIDJNZRWSZDJMR2W45BAOV2CA3DBMJXXEZI"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipwoge55cp73nyh5jqoogn5mfqosnya5xp33ska5wci48khtycf1g1hdjqpts15u8rb1sa4mwfoo8g3mrrb1g6edfpf4zg5mxcoo8e3mpqbzzredjp3ts13djct4sh7byqi4ny5dbcjzzr3e"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpYHgN55Gp73nyh5jqHHgn5mfqHsnyT52p33skT5YGSJ8khtyGfPgPhdjqptsP5R8rbPsTJmYfHH8g3mrrbPg6NdfpfJzg5m2GHH8N3mpqbzzrNdjp3tsP3djGtJsh7byqSJny5dbGjzzr3N"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDMG68VVCDXS20WV9EGG62VB5EGP20RVFDSSPARVMCNT7AWH0C5J6JW39EDHPJVK741JPRTBM5GG76SB441J6Y835D5TQ6VBFCGG78SBDE1QQ4839DSHPJS39CHTPWX10ENT20V31C9QQ4S8"},
                }},
                {"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2QgdGVtcG9yIGluY2lkaWR1bnQgdXQgbGFib3JlIGV0"},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2QgdGVtcG9yIGluY2lkaWR1bnQgdXQgbGFib3JlIGV0"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNUQGI33MN5ZCA43JOQQGC3LFOQWCAY3PNZZWKY3UMV2HK4RAMFSGS4DJONRWS3THEBSWY2LUFQQHGZLEEBSG6IDFNF2XG3LPMQQHIZLNOBXXEIDJNZRWSZDJMR2W45BAOV2CA3DBMJXXEZJAMV2A"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipwoge55cp73nyh5jqoogn5mfqosnya5xp33ska5wci48khtycf1g1hdjqpts15u8rb1sa4mwfoo8g3mrrb1g6edfpf4zg5mxcoo8e3mpqbzzredjp3ts13djct4sh7byqi4ny5dbcjzzr3jyci4y"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpYHgN55Gp73nyh5jqHHgn5mfqHsnyT52p33skT5YGSJ8khtyGfPgPhdjqptsP5R8rbPsTJmYfHH8g3mrrbPg6NdfpfJzg5m2GHH8N3mpqbzzrNdjp3tsP3djGtJsh7byqSJny5dbGjzzr3jyGSJy"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDMG68VVCDXS20WV9EGG62VB5EGP20RVFDSSPARVMCNT7AWH0C5J6JW39EDHPJVK741JPRTBM5GG76SB441J6Y835D5TQ6VBFCGG78SBDE1QQ4839DSHPJS39CHTPWX10ENT20V31C9QQ4S90CNT0"},
                }},
                {"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2QgdGVtcG9yIGluY2lkaWR1bnQgdXQgbGFib3JlIGV0IGRvbG9yZQ=="},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2QgdGVtcG9yIGluY2lkaWR1bnQgdXQgbGFib3JlIGV0IGRvbG9yZQ"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNUQGI33MN5ZCA43JOQQGC3LFOQWCAY3PNZZWKY3UMV2HK4RAMFSGS4DJONRWS3THEBSWY2LUFQQHGZLEEBSG6IDFNF2XG3LPMQQHIZLNOBXXEIDJNZRWSZDJMR2W45BAOV2CA3DBMJXXEZJAMV2CAZDPNRXXEZI"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipwoge55cp73nyh5jqoogn5mfqosnya5xp33ska5wci48khtycf1g1hdjqpts15u8rb1sa4mwfoo8g3mrrb1g6edfpf4zg5mxcoo8e3mpqbzzredjp3ts13djct4sh7byqi4ny5dbcjzzr3jyci4ny3dxptzzr3e"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpYHgN55Gp73nyh5jqHHgn5mfqHsnyT52p33skT5YGSJ8khtyGfPgPhdjqptsP5R8rbPsTJmYfHH8g3mrrbPg6NdfpfJzg5m2GHH8N3mpqbzzrNdjp3tsP3djGtJsh7byqSJny5dbGjzzr3jyGSJny3d2ptzzr3N"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDMG68VVCDXS20WV9EGG62VB5EGP20RVFDSSPARVMCNT7AWH0C5J6JW39EDHPJVK741JPRTBM5GG76SB441J6Y835D5TQ6VBFCGG78SBDE1QQ4839DSHPJS39CHTPWX10ENT20V31C9QQ4S90CNT20S3FDHQQ4S8"},
                }},
                {"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2QgdGVtcG9yIGluY2lkaWR1bnQgdXQgbGFib3JlIGV0IGRvbG9yZSBtYWduYQ=="},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2QgdGVtcG9yIGluY2lkaWR1bnQgdXQgbGFib3JlIGV0IGRvbG9yZSBtYWduYQ"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNUQGI33MN5ZCA43JOQQGC3LFOQWCAY3PNZZWKY3UMV2HK4RAMFSGS4DJONRWS3THEBSWY2LUFQQHGZLEEBSG6IDFNF2XG3LPMQQHIZLNOBXXEIDJNZRWSZDJMR2W45BAOV2CA3DBMJXXEZJAMV2CAZDPNRXXEZJANVQWO3TB"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipwoge55cp73nyh5jqoogn5mfqosnya5xp33ska5wci48khtycf1g1hdjqpts15u8rb1sa4mwfoo8g3mrrb1g6edfpf4zg5mxcoo8e3mpqbzzredjp3ts13djct4sh7byqi4ny5dbcjzzr3jyci4ny3dxptzzr3jypiosq5ub"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpYHgN55Gp73nyh5jqHHgn5mfqHsnyT52p33skT5YGSJ8khtyGfPgPhdjqptsP5R8rbPsTJmYfHH8g3mrrbPg6NdfpfJzg5m2GHH8N3mpqbzzrNdjp3tsP3djGtJsh7byqSJny5dbGjzzr3jyGSJny3d2ptzzr3jypSHsq5Rb"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDMG68VVCDXS20WV9EGG62VB5EGP20RVFDSSPARVMCNT7AWH0C5J6JW39EDHPJVK741JPRTBM5GG76SB441J6Y835D5TQ6VBFCGG78SBDE1QQ4839DSHPJS39CHTPWX10ENT20V31C9QQ4S90CNT20S3FDHQQ4S90DNGPEVK1"},
                }},
                {"Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",  new Dictionary<EncodingType, string> {
                    {EncodingType.Base64, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2QgdGVtcG9yIGluY2lkaWR1bnQgdXQgbGFib3JlIGV0IGRvbG9yZSBtYWduYSBhbGlxdWEu"},
                    {EncodingType.Base64Url, "TG9yZW0gaXBzdW0gZG9sb3Igc2l0IGFtZXQsIGNvbnNlY3RldHVyIGFkaXBpc2NpbmcgZWxpdCwgc2VkIGRvIGVpdXNtb2QgdGVtcG9yIGluY2lkaWR1bnQgdXQgbGFib3JlIGV0IGRvbG9yZSBtYWduYSBhbGlxdWEu"},
                    {EncodingType.Base32Url, "JRXXEZLNEBUXA43VNUQGI33MN5ZCA43JOQQGC3LFOQWCAY3PNZZWKY3UMV2HK4RAMFSGS4DJONRWS3THEBSWY2LUFQQHGZLEEBSG6IDFNF2XG3LPMQQHIZLNOBXXEIDJNZRWSZDJMR2W45BAOV2CA3DBMJXXEZJAMV2CAZDPNRXXEZJANVQWO3TBEBQWY2LROVQS4"},
                    {EncodingType.ZBase32, "jtzzr3mprbwzyh5ipwoge55cp73nyh5jqoogn5mfqosnya5xp33ska5wci48khtycf1g1hdjqpts15u8rb1sa4mwfoo8g3mrrb1g6edfpf4zg5mxcoo8e3mpqbzzredjp3ts13djct4sh7byqi4ny5dbcjzzr3jyci4ny3dxptzzr3jypiosq5ubrbosa4mtqio1h"},
                    {EncodingType.Base32LowProfanity, "jtzzr3mprbYzyh5SpYHgN55Gp73nyh5jqHHgn5mfqHsnyT52p33skT5YGSJ8khtyGfPgPhdjqptsP5R8rbPsTJmYfHH8g3mrrbPg6NdfpfJzg5m2GHH8N3mpqbzzrNdjp3tsP3djGtJsh7byqSJny5dbGjzzr3jyGSJny3d2ptzzr3jypSHsq5RbrbHsTJmtqSHPh"},
                    {EncodingType.Base32Crockford, "9HQQ4SBD41MQ0WVNDMG68VVCDXS20WV9EGG62VB5EGP20RVFDSSPARVMCNT7AWH0C5J6JW39EDHPJVK741JPRTBM5GG76SB441J6Y835D5TQ6VBFCGG78SBDE1QQ4839DSHPJS39CHTPWX10ENT20V31C9QQ4S90CNT20S3FDHQQ4S90DNGPEVK141GPRTBHENGJW"},
                }},
            };

            foreach (var de in knownValues)
            {
                foreach (var di in de.Value)
                {
                    // Console.WriteLine("Testing Text: {0}: {1}", di.Key, de.Key.Length > 20 ? (de.Key.Substring(0, 20) + "... Len=" + de.Key.Length) : de.Key);
                    var enc2 = Base3264Encoding.EncodeString(di.Key, de.Key);
                    var dec2 = Base3264Encoding.DecodeToString(di.Key, enc2);
                    Assert.Equal(di.Value, enc2);
                    Assert.Equal(de.Key, dec2);
                }
            }

            // The following can be used to generate above test cases
            /* var kval = "Hello World!";
             Console.WriteLine("{{\"{0}\",  new Dictionary<EncodingType, string> {{", kval);
             foreach (EncodingType encType in Enum.GetValues(typeof(EncodingType)))
             {
                 var enc2 = Base3264Encoding.Encode(encType, kval);
                 Console.WriteLine("\t{{EncodingType.{0}, \"{1}\"}},", encType, enc2);
             }
             Console.WriteLine("}},");

             var lipsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
             var lsplit = lipsum.Split(' ');
             for (int i = 1; i < 20; i++)
             {
                 kval = string.Join(" ", lsplit.Take(i));
                 Console.WriteLine("{{\"{0}\",  new Dictionary<EncodingType, string> {{", kval);
                 foreach (EncodingType encType in Enum.GetValues(typeof (EncodingType)))
                 {
                     var enc2 = Base3264Encoding.Encode(encType, kval);
                     Console.WriteLine("\t{{EncodingType.{0}, \"{1}\"}},", encType, enc2);
                 }
                 Console.WriteLine("}},");
             }*/
        }
    }
}
