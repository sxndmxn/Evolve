using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using EvolveDb.Metadata;
using EvolveDb.Utilities;

namespace EvolveDb.Migration
{
    /// <summary>
    ///     Provide a common abstraction for versioned and repeatable migrations.
    /// </summary>
    public abstract class MigrationScript : MigrationBase
    {
        private const string IncorrectMigrationChecksum = "Validate failed: invalid checksum for migration: {0}.";

        protected MigrationScript(string? version, string description, string name, string content, MetadataType type)
            : base(version, description, name, type)
        {
            Content = Check.NotNull(content, nameof(content));
        }

        /// <summary>
        ///     Gets the raw content of a migration script.
        /// </summary>
        public string Content { get; }

        /// <summary>
        ///     Gets or sets the hash algorithm used for checksum calculation.
        /// </summary>
        internal HashAlgorithmName HashAlgorithm { get; set; } = HashAlgorithmName.MD5;

        /// <summary>
        ///     Returns false if the special comment "evolve-tx-off" is found in the first line of the script, true otherwise.
        /// </summary>
        public virtual bool IsTransactionEnabled => !OptionExists("evolve-tx-off");

        /// <summary>
        ///     Returns true if the special comment "evolve-repeat-always" is found in the first line of the script, false otherwise.
        /// </summary>
        public virtual bool MustRepeatAlways => Type == MetadataType.RepeatableMigration && OptionExists("evolve-repeat-always");

        /// <summary>
        ///     Validates the <paramref name="checksum"/> against the actual migration one.
        ///     Supports dual validation: accepts both the configured algorithm and MD5 for backward compatibility.
        /// </summary>
        /// <param name="checksum"> The applied migration checksum. </param>
        /// <exception cref="EvolveValidationException"></exception>
        public virtual void ValidateChecksum(string? checksum)
        {
            Check.NotNull(checksum, nameof(checksum));

            string currentChecksum = CalculateChecksum();

            // Direct match with configured algorithm
            if (checksum == currentChecksum)
            {
                return;
            }

            // Dual validation: try MD5 for backward compatibility if using SHA-2
            if (HashAlgorithm != HashAlgorithmName.MD5)
            {
                string md5Checksum = CalculateChecksumWithAlgorithm(HashAlgorithmName.MD5);
                if (checksum == md5Checksum)
                {
                    return;
                }
            }

            // Detect stored algorithm and recalculate if different
            var detectedAlgorithm = ChecksumCalculator.DetectAlgorithm(checksum);
            if (detectedAlgorithm.HasValue && detectedAlgorithm.Value != HashAlgorithm)
            {
                string recalculated = CalculateChecksumWithAlgorithm(detectedAlgorithm.Value);
                if (checksum == recalculated)
                {
                    return;
                }
            }

            throw new EvolveValidationException(string.Format(IncorrectMigrationChecksum, Name));
        }

        /// <summary>
        ///     Returns the checksum where <code>crlf</code> and <code>lf</code> line endings have been previously normalized to <code>lf</code>.
        /// </summary>
        /// <returns> The checksum calculated with the configured hash algorithm. </returns>
        public virtual string CalculateChecksum()
        {
            return CalculateChecksumWithAlgorithm(HashAlgorithm);
        }

        /// <summary>
        ///     Calculates checksum using a specific algorithm.
        /// </summary>
        /// <param name="algorithm"> The hash algorithm to use. </param>
        /// <returns> The checksum calculated with the specified algorithm. </returns>
        protected string CalculateChecksumWithAlgorithm(HashAlgorithmName algorithm)
        {
            string normalizedContent = NormalizeLineEndings(Content);
            return ChecksumCalculator.Calculate(normalizedContent, algorithm);
        }

        /// <summary>
        ///     <code>crlf</code> and <code>lf</code> line endings will be normalized to <code>lf</code>
        /// </summary>
        protected static string NormalizeLineEndings(string str) => str.Replace("\r\n", "\n");

        private bool OptionExists(string option)
        {
            if (Content.IsNullOrWhiteSpace())
            {
                return false;
            }

            using var file = new StringReader(Content);
            string? firstLine = file.ReadLine();
            return !(firstLine is null || firstLine.IndexOf(option, StringComparison.OrdinalIgnoreCase) == -1);
        }
    }
}
