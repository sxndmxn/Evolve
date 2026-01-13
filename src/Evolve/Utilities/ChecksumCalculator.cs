using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EvolveDb.Utilities
{
    /// <summary>
    ///     Utility class for calculating migration checksums with configurable algorithms.
    /// </summary>
    internal static class ChecksumCalculator
    {
        /// <summary>
        ///     Known algorithm lengths for detection.
        /// </summary>
        private static readonly Dictionary<int, HashAlgorithmName> LengthToAlgorithm = new()
        {
            { 32, HashAlgorithmName.MD5 },
            { 64, HashAlgorithmName.SHA256 },
            { 96, HashAlgorithmName.SHA384 },
            { 128, HashAlgorithmName.SHA512 }
        };

        /// <summary>
        ///     Returns the expected hex string length for each algorithm.
        /// </summary>
        /// <param name="algorithm"> The hash algorithm name. </param>
        /// <returns> The hex string length (32 for MD5, 64 for SHA256, 96 for SHA384, 128 for SHA512). </returns>
        public static int GetHashLength(HashAlgorithmName algorithm)
        {
            if (algorithm == HashAlgorithmName.MD5) return 32;
            if (algorithm == HashAlgorithmName.SHA256) return 64;
            if (algorithm == HashAlgorithmName.SHA384) return 96;
            if (algorithm == HashAlgorithmName.SHA512) return 128;
            // For unknown algorithms, compute a sample hash to determine length
            return CryptographicOperations.HashData(algorithm, Array.Empty<byte>()).Length * 2;
        }

        /// <summary>
        ///     Detects the hash algorithm based on checksum string length.
        /// </summary>
        /// <param name="checksum"> The checksum string to analyze. </param>
        /// <returns> The detected algorithm name, or null if length doesn't match any known algorithm. </returns>
        public static HashAlgorithmName? DetectAlgorithm(string? checksum)
        {
            if (string.IsNullOrEmpty(checksum))
            {
                return null;
            }

            return LengthToAlgorithm.TryGetValue(checksum.Length, out var algorithm) ? algorithm : null;
        }

        /// <summary>
        ///     Calculates checksum of string content using the specified algorithm.
        /// </summary>
        /// <param name="content"> The string content to hash. </param>
        /// <param name="algorithm"> The hash algorithm to use. </param>
        /// <returns> The uppercase hex string representation of the hash. </returns>
        public static string Calculate(string content, HashAlgorithmName algorithm)
        {
            byte[] data = Encoding.UTF8.GetBytes(content);
            byte[] hash = CryptographicOperations.HashData(algorithm, data);
            return Convert.ToHexString(hash);
        }

        /// <summary>
        ///     Calculates checksum from a stream using the specified algorithm.
        /// </summary>
        /// <param name="stream"> The stream to hash. </param>
        /// <param name="algorithm"> The hash algorithm to use. </param>
        /// <returns> The uppercase hex string representation of the hash. </returns>
        public static string Calculate(Stream stream, HashAlgorithmName algorithm)
        {
            byte[] hash = CryptographicOperations.HashData(algorithm, stream);
            return Convert.ToHexString(hash);
        }
    }
}
