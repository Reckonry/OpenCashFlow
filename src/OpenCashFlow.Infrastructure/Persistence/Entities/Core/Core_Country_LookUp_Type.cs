using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCashFlow.Infrastructure.Persistence.Entities.Core
{
    [Table(name: "Core_Countries")]
    public class Core_Country_LookUp_Type
    {
        [Required, NotNull, Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid CountryID { get; set; } = Guid.NewGuid();

        [Required, NotNull, Column(TypeName = "varchar(256)", Order = 1)]
        public required string CountryName { get; set; }

        [AllowNull, Column(TypeName = "varchar(256)", Order = 2)]
        public string? CountryFlag { get; set; }

        [Required, NotNull, Column(Order = 3)]
        public bool Visible { get; set; } = true;
    }
}
