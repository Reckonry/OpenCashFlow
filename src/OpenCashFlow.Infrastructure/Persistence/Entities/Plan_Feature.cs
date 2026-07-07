using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OpenCashFlow.Infrastructure.Persistence.Entities
{
    [Table(name: "Plans_Features")]
    [PrimaryKey(nameof(PlanID), nameof(FeatureID))]
    public class Plan_Feature
    {
        [Required, NotNull, Column(Order = 0)]
        public Guid PlanID { get; set; }

        [Required, NotNull, Key, Column(Order = 1)]
        public Guid FeatureID { get; set; }

        [ForeignKey(nameof(PlanID))]
        public virtual Plan? Plan { get; set; }

        [ForeignKey(nameof(FeatureID))]
        public virtual Feature? Features { get; set; }

        [Required, NotNull, Column(TypeName = "timestamp", Order = 30), DefaultValue("now()"), Display(Name = "Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Data creazione


    }
}

/*
 FeatureID	PlanID	FeatureName	IsEnabled
1	1	Download	FALSE
2	1	Customer Support	FALSE
3	2	Download	TRUE
4	2	Customer Support	TRUE
5	3	Download	TRUE
6	3	Customer Support	TRUE
 */