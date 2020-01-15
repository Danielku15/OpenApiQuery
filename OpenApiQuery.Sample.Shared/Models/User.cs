using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenApiQuery.Sample.Models
{
    public sealed class User : IEntity
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MinLength(ModelConstants.MinStringLength)]
        [MaxLength(ModelConstants.SmallStringLength)]
        public string Username { get; set; }

        [Required]
        [MinLength(ModelConstants.MinStringLength)]
        [MaxLength(ModelConstants.SmallStringLength)]
        public string FirstName { get; set; }

        [Required]
        [MinLength(ModelConstants.MinStringLength)]
        [MaxLength(ModelConstants.SmallStringLength)]
        public string LastName { get; set; }

        [Required]
        [MinLength(ModelConstants.MinStringLength)]
        [MaxLength(ModelConstants.SmallStringLength)]
        public string EMail { get; set; }

        [InverseProperty(nameof(Blog.Owner))]
        public ICollection<Blog> Blogs { get; set; }
    }
}
