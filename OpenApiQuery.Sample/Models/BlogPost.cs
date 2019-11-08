using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenApiQuery.Sample.Models
{
    public class BlogPost
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
        [MaxLength(ModelConstants.LongStringLength)]
        public string Text { get; set; }

        [ForeignKey(nameof(Blog))] public int BlogId { get; set; }

        public Blog Blog { get; set; }
    }
}