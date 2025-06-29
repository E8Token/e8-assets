using Energy8.Contracts.Dto.Common;

namespace Energy8.Contracts.Dto.Errors
{
    /// <summary>
    /// Represents an error with detailed information and handling options.
    /// </summary>
    /// <remarks>
    /// Used for transmitting error details across the API boundary.
    /// Contains both a brief header and optional detailed description.
    /// </remarks>
    [System.Serializable]
    public class ErrorDto : DtoBase
    {
        /// <summary>
        /// Gets or sets the error header/title.
        /// </summary>
        public string Header { get; set; }

        /// <summary>
        /// Gets or sets the detailed error description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Default constructor for Unity serialization
        /// </summary>
        public ErrorDto()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorDto"/> class.
        /// </summary>
        /// <param name="header">The main error message or title.</param>
        /// <param name="description">Optional detailed description of the error.</param>
        public ErrorDto(string header, string description = null)
        {
            Header = header;
            Description = description;
        }
    }
}