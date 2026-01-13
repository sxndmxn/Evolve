using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using EvolveDb.Configuration;

namespace EvolveDb.Utilities
{
    /// <summary>
    ///     Utility class for calculating migration checksums with configurable algorithms.
    /// </summary>
    internal static class ChecksumCalculator
    {
        /// <summary>
        ///     Returns the expected hex string length for each algorithm.
        /// </summary>
        /// <param name="algorithm"> The hash algorithm type. </param>
        /// <returns> The hex string length (32 for MD5, 64 for SHA256, 96 for SHA384, 128 for SHA512). </returns>
        public static int GetHashLength(HashAlgorithmType algorithm) => algorithm switch
        {
            HashAlgorithmType.MD5 => 32,
            HashAlgorithmType.SHA256 => 64,
            HashAlgorithmType.SHA384 => 96,
            HashAlgorithmType.SHA512 => 128,
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm))
        };

        /// <summary>
        ///     Detects the hash algorithm based on checksum string length.
        /// </summary>
        /// <param name="checksum"> The checksum string to analyze. </param>
        /// <returns> The detected algorithm type, or null if length doesn't match any known algorithm. </returns>
        public static HashAlgorithmType? DetectAlgorithm(string? checksum)
        {
            if (string.IsNullOrEmpty(checksum))
            {
                return null;
            }

            return checksum.Length switch
            {
                32 => HashAlgorithmType.MD5,
                64 => HashAlgorithmType.SHA256,
                96 => HashAlgorithmType.SHA384,
                128 => HashAlgorithmType.SHA512,
                _ => null
            };
        }

        /// <summary>
        ///     Calculates checksum of string content using the specified algorithm.
        /// </summary>
        /// <param name="content"> The string content to hash. </param>
        /// <param name="algorithm"> The hash algorithm to use. </param>
        /// <returns> The uppercase hex string representation of the hash. </returns>
        public static string Calculate(string content, HashAlgorithmType algorithm)
        {
            using var hashAlgorithm = CreateHashAlgorithm(algorithm);
            byte[] hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(content));
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        /// <summary>
        ///     Calculates checksum from a stream using the specified algorithm.
        /// </summary>
        /// <param name="stream"> The stream to hash. </param>
        /// <param name="algorithm"> The hash algorithm to use. </param>
        /// <returns> The uppercase hex string representation of the hash. </returns>
        public static string Calculate(Stream stream, HashAlgorithmType algorithm)
        {
            using var hashAlgorithm = CreateHashAlgorithm(algorithm);
            byte[] hash = hashAlgorithm.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }

        /// <summary>
        ///     Creates a HashAlgorithm instance for the specified algorithm type.
        /// </summary>
        /// <param name="algorithm"> The hash algorithm type. </param>
        /// <returns> A new HashAlgorithm instance. </returns>
        private static HashAlgorithm CreateHashAlgorithm(HashAlgorithmType algorithm) => algorithm switch
        {
            HashAlgorithmType.MD5 => MD5.Create(),
            HashAlgorithmType.SHA256 => SHA256.Create(),
            HashAlgorithmType.SHA384 => SHA384.Create(),
            HashAlgorithmType.SHA512 => SHA512.Create(),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm))
        };
    }
}
