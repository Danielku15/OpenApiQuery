using System.ComponentModel.DataAnnotations;

namespace OpenApiQuery.Sample.Models
{
    /// <summary>
    /// A page that opens an external link
    /// </summary>
    public class ExternalPage : StaticPage
    {
        [Required]
        [MinLength(ModelConstants.MinStringLength)]
        [MaxLength(ModelConstants.SmallStringLength)]
        public string ExternalUrl { get; set; }
    }
}
