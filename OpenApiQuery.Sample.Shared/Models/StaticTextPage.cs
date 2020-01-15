using System.ComponentModel.DataAnnotations;

namespace OpenApiQuery.Sample.Models
{
    /// <summary>
    /// A static page with text content
    /// </summary>
    public class StaticTextPage : StaticPage
    {
        [Required]
        [MinLength(ModelConstants.MinStringLength)]
        [MaxLength(ModelConstants.LongStringLength)]
        public string Content { get; set; }
    }
}
