namespace EvolveDb.Configuration
{
    /// <summary>
    ///     Hash algorithms supported for migration checksum calculation.
    /// </summary>
    public enum HashAlgorithmType
    {
        /// <summary>
        ///     MD5 hash algorithm (32 character hex output).
        ///     Not FIPS-compliant.
        /// </summary>
        MD5,

        /// <summary>
        ///     SHA-256 hash algorithm (64 character hex output).
        ///     FIPS-compliant.
        /// </summary>
        SHA256,

        /// <summary>
        ///     SHA-384 hash algorithm (96 character hex output).
        ///     FIPS-compliant.
        /// </summary>
        SHA384,

        /// <summary>
        ///     SHA-512 hash algorithm (128 character hex output).
        ///     FIPS-compliant.
        /// </summary>
        SHA512
    }
}
