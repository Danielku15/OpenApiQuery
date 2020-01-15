using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenApiQuery.Sample.Models
{
    /// <summary>
    /// Represents a static page on the website
    /// </summary>
    public abstract class StaticPage : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MinLength(ModelConstants.MinStringLength)]
        [MaxLength(ModelConstants.SmallStringLength)]
        public string Title { get; set; }

        [Required]
        [MinLength(ModelConstants.MinStringLength)]
        [MaxLength(ModelConstants.SmallStringLength)]
        public string Slug { get; set; }
    }
}
