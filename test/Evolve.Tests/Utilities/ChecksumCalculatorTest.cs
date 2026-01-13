using EvolveDb.Configuration;
using EvolveDb.Utilities;
using Xunit;

namespace EvolveDb.Tests.Utilities
{
    public class ChecksumCalculatorTest
    {
        private const string TestContent = "SELECT 1;";

        [Fact]
        [Category(Test.Migration)]
        public void MD5_returns_32_character_hash()
        {
            var result = ChecksumCalculator.Calculate(TestContent, HashAlgorithmType.MD5);
            Assert.Equal(32, result.Length);
        }

        [Fact]
        [Category(Test.Migration)]
        public void SHA256_returns_64_character_hash()
        {
            var result = ChecksumCalculator.Calculate(TestContent, HashAlgorithmType.SHA256);
            Assert.Equal(64, result.Length);
        }

        [Fact]
        [Category(Test.Migration)]
        public void SHA384_returns_96_character_hash()
        {
            var result = ChecksumCalculator.Calculate(TestContent, HashAlgorithmType.SHA384);
            Assert.Equal(96, result.Length);
        }

        [Fact]
        [Category(Test.Migration)]
        public void SHA512_returns_128_character_hash()
        {
            var result = ChecksumCalculator.Calculate(TestContent, HashAlgorithmType.SHA512);
            Assert.Equal(128, result.Length);
        }

        [Theory]
        [Category(Test.Migration)]
        [InlineData(32, HashAlgorithmType.MD5)]
        [InlineData(64, HashAlgorithmType.SHA256)]
        [InlineData(96, HashAlgorithmType.SHA384)]
        [InlineData(128, HashAlgorithmType.SHA512)]
        public void DetectAlgorithm_correctly_identifies_by_length(int length, HashAlgorithmType expected)
        {
            var checksum = new string('A', length);
            Assert.Equal(expected, ChecksumCalculator.DetectAlgorithm(checksum));
        }

        [Fact]
        [Category(Test.Migration)]
        public void DetectAlgorithm_returns_null_for_unknown_length()
        {
            var checksum = new string('A', 50); // Invalid length
            Assert.Null(ChecksumCalculator.DetectAlgorithm(checksum));
        }

        [Fact]
        [Category(Test.Migration)]
        public void DetectAlgorithm_returns_null_for_null_or_empty()
        {
            Assert.Null(ChecksumCalculator.DetectAlgorithm(null));
            Assert.Null(ChecksumCalculator.DetectAlgorithm(""));
        }

        [Fact]
        [Category(Test.Migration)]
        public void Same_content_produces_same_hash()
        {
            var hash1 = ChecksumCalculator.Calculate(TestContent, HashAlgorithmType.SHA256);
            var hash2 = ChecksumCalculator.Calculate(TestContent, HashAlgorithmType.SHA256);
            Assert.Equal(hash1, hash2);
        }

        [Fact]
        [Category(Test.Migration)]
        public void Different_algorithms_produce_different_hashes()
        {
            var md5 = ChecksumCalculator.Calculate(TestContent, HashAlgorithmType.MD5);
            var sha256 = ChecksumCalculator.Calculate(TestContent, HashAlgorithmType.SHA256);
            Assert.NotEqual(md5, sha256);
        }

        [Fact]
        [Category(Test.Migration)]
        public void GetHashLength_returns_correct_values()
        {
            Assert.Equal(32, ChecksumCalculator.GetHashLength(HashAlgorithmType.MD5));
            Assert.Equal(64, ChecksumCalculator.GetHashLength(HashAlgorithmType.SHA256));
            Assert.Equal(96, ChecksumCalculator.GetHashLength(HashAlgorithmType.SHA384));
            Assert.Equal(128, ChecksumCalculator.GetHashLength(HashAlgorithmType.SHA512));
        }

        [Fact]
        [Category(Test.Migration)]
        public void Hash_output_is_uppercase_hex()
        {
            var hash = ChecksumCalculator.Calculate(TestContent, HashAlgorithmType.MD5);
            Assert.Matches("^[0-9A-F]+$", hash);
        }
    }
}
