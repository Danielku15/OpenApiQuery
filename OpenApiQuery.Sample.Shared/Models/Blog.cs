using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenApiQuery.Sample.Models
{
    public sealed class Blog : IEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MinLength(ModelConstants.MinStringLength)]
        [MaxLength(ModelConstants.SmallStringLength)]
        public string Name { get; set; }

        [Required]
        [MinLength(ModelConstants.MinStringLength)]
        [MaxLength(ModelConstants.LongStringLength)]
        public string Description { get; set; }

        [ForeignKey(nameof(Owner))]
        public int OwnerId { get; set; }
        public User Owner { get; set; }

        [InverseProperty(nameof(BlogPost.Blog))]
        public ICollection<BlogPost> Posts { get; set; }
    }
}
