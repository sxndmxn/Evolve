using System;
using System.IO;
using System.Security.Cryptography;
using EvolveDb.Metadata;
using EvolveDb.Migration;
using EvolveDb.Utilities;
using Xunit;
using static EvolveDb.Tests.TestContext;
using static EvolveDb.Tests.TestUtil;

namespace EvolveDb.Tests.Migration
{
    public class FileMigrationScriptTest
    {
        [Fact]
        [Category(Test.Migration)]
        public void Should_throw_NotSupportedException_when_type_is_not_migration_or_repeatable_migration()
        {
            Assert.Throws<NotSupportedException>(() => new FileMigrationScript(CrLfScriptPath, "2.3.1", "desc", MetadataType.EmptySchema));
        }

        [Fact]
        [Category(Test.Migration)]
        public void CalculateChecksum_should_not_return_null()
        {
            string checksum = BuildFileMigrationScript(CrLfScriptPath, "2.3.1").CalculateChecksum();
            Assert.False(string.IsNullOrEmpty(checksum));
        }

        [Fact]
        [Category(Test.Migration)]
        public void ValidateChecksum_should_work_with_both_crlf_and_lf_versions()
        {
            // Arrange
            var crlfScript = BuildFileMigrationScript(CrLfScriptPath, "2.3.1");
            string lfCheckSum = BuildFileMigrationScript(LfScriptPath, "2.3.2").CalculateChecksum();

            // Assert
            crlfScript.ValidateChecksum(lfCheckSum);
            Assert.NotEqual(File.ReadAllText(CrLfScriptPath), File.ReadAllText(LfScriptPath));
        }

        [Fact]
        [Category(Test.Migration)]
        public void ValidateChecksum_should_work_with_old_checksum_version()
        {
            // Arrange
            var loader = new FileMigrationLoader(new EvolveConfiguration { Locations = new[] { "Resources/LF_CRLF" } });

            // Assert
            foreach (FileMigrationScript script in loader.GetMigrations())
            {
                script.ValidateChecksum(FallbackCheck(script.Path));
            }
        }

        [Fact]
        [Category(Test.Migration)]
        public void ValidateChecksum_throws_EvolveValidationException_when_checksums_mismatch()
        {
            // Arrange
            var crlfScript = BuildFileMigrationScript(CrLfScriptPath, "2.3.1");

            // Assert
            Assert.Throws<EvolveValidationException>(() => crlfScript.ValidateChecksum("checksums mismatch"));
        }

        [Fact]
        [Category(Test.Migration)]
        public void CalculateChecksum_should_be_the_same_with_both_crlf_and_lf_versions()
        {
            // Arrange
            string crlfCheckSum = BuildFileMigrationScript(CrLfScriptPath, "2.3.1").CalculateChecksum();
            string lfCheckSum = BuildFileMigrationScript(LfScriptPath, "2.3.2").CalculateChecksum();

            // Assert
            Assert.Equal(crlfCheckSum, lfCheckSum);
            Assert.NotEqual(File.ReadAllText(CrLfScriptPath), File.ReadAllText(LfScriptPath));
        }

        [Fact]
        [Category(Test.Migration)]
        public void IsTransactionEnabled_should_be_false_when_tx_off_is_found_in_the_script()
        {
            Assert.False(BuildFileMigrationScript(PostgreSQL.VacuumScriptPath).IsTransactionEnabled);
        }

        [Fact]
        [Category(Test.Migration)]
        public void IsTransactionEnabled_should_be_true_when_tx_off_is_not_found_in_the_script()
        {
            Assert.True(BuildFileMigrationScript(SQLite.ChinookScriptPath).IsTransactionEnabled);
        }

        [Fact]
        [Category(Test.Migration)]
        public void MustRepeatAlways_should_be_true_when_repeat_always_is_found_in_the_script()
        {
            Assert.True(BuildRepeatableFileMigrationScript(SQLite.ChinookScriptPath).MustRepeatAlways);
        }

        [Fact]
        [Category(Test.Migration)]
        public void MustRepeatAlways_should_be_false_when_repeat_always_is_found_in_the_script()
        {
            Assert.False(BuildRepeatableFileMigrationScript(PostgreSQL.VacuumScriptPath).MustRepeatAlways);
        }

        /// <summary>
        ///     Calculate the checksum with the pre v1.8.0 version.
        /// </summary>
        private static string FallbackCheck(string path)
        {
            using var md5 = MD5.Create();
            using FileStream stream = File.OpenRead(path);
            byte[] checksum = md5.ComputeHash(stream);
            return BitConverter.ToString(checksum).Replace("-", string.Empty);
        }

        [Fact]
        [Category(Test.Migration)]
        public void ValidateChecksum_accepts_MD5_when_configured_for_SHA256()
        {
            // Arrange
            var script = BuildFileMigrationScript(CrLfScriptPath, "2.3.1");
            string md5Checksum = script.CalculateChecksum(); // Default is MD5

            // Switch to SHA-256
            script.HashAlgorithm = HashAlgorithmName.SHA256;

            // Act & Assert - should not throw due to dual validation
            script.ValidateChecksum(md5Checksum);
        }

        [Fact]
        [Category(Test.Migration)]
        public void ValidateChecksum_accepts_SHA256_when_configured_for_SHA256()
        {
            // Arrange
            var script = BuildFileMigrationScript(CrLfScriptPath, "2.3.1");
            script.HashAlgorithm = HashAlgorithmName.SHA256;
            string sha256Checksum = script.CalculateChecksum();

            // Act & Assert - should not throw
            script.ValidateChecksum(sha256Checksum);
        }

        [Fact]
        [Category(Test.Migration)]
        public void CalculateChecksum_returns_different_lengths_for_different_algorithms()
        {
            // Arrange
            var script = BuildFileMigrationScript(CrLfScriptPath, "2.3.1");

            // Act
            script.HashAlgorithm = HashAlgorithmName.MD5;
            string md5 = script.CalculateChecksum();

            script.HashAlgorithm = HashAlgorithmName.SHA256;
            string sha256 = script.CalculateChecksum();

            script.HashAlgorithm = HashAlgorithmName.SHA512;
            string sha512 = script.CalculateChecksum();

            // Assert
            Assert.Equal(32, md5.Length);
            Assert.Equal(64, sha256.Length);
            Assert.Equal(128, sha512.Length);
        }

        [Fact]
        [Category(Test.Migration)]
        public void ValidateChecksum_detects_algorithm_from_stored_checksum_length()
        {
            // Arrange - script configured for SHA-512
            var script = BuildFileMigrationScript(CrLfScriptPath, "2.3.1");
            script.HashAlgorithm = HashAlgorithmName.SHA512;

            // Calculate SHA-256 checksum separately
            var tempScript = BuildFileMigrationScript(CrLfScriptPath, "2.3.1");
            tempScript.HashAlgorithm = HashAlgorithmName.SHA256;
            string sha256Checksum = tempScript.CalculateChecksum();

            // Act & Assert - should detect SHA-256 from length and validate
            script.ValidateChecksum(sha256Checksum);
        }
    }
}
