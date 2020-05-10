namespace TinkoffRateBot.Services
{
    /// <summary>
    /// Minimal information about active credential.
    /// </summary>
    public class BasicAuthUser
    {
        /// <summary>
        /// User name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// User password.
        /// </summary>
        public string Password { get; set; }
    }
}