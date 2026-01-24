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

namespace Shared.Models
{
    [Table(name: "Features")]
    [PrimaryKey(nameof(FeatureID))]
    public class Feature
    {
        [Required, NotNull, Key, Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), DefaultValue("gen_random_uuid()")]
        public Guid FeatureID { get; set; } = Guid.NewGuid();

        [Required, NotNull, Column(TypeName = "text", Order = 10), Display(Name = "Feature Name")]
        public string FeatureName { get; set; } = string.Empty;

        [Required, NotNull, Column(TypeName = "varchar(150)", Order = 11), Display(Name = "Feature Code")]
        /// <summary>
        /// Unique code for the feature (e.g., "PREMIUM_SUPPORT").
        /// </summary>
        public required string FeatureCode { get; set; }

        [AllowNull, Column(TypeName = "varchar(128)", Order = 12), Display(Name = "Feature Category")]
        /// <summary> Feature category (e.g., "Support", "Analytics"). </summary>
        public string? FeatureCategory { get; set; }

        [AllowNull, Column(TypeName = "varchar(128)", Order = 13), Display(Name = "Feature Icon")]
        /// <summary> Feature icon (e.g., "fa fa-download"). </summary>
        public string FeatureIcon { get; set; } = string.Empty;

        [AllowNull, Column(TypeName = "varchar(128)", Order = 14), Display(Name = "Feature Icon")]
        /// <summary> Feature icon (e.g., "fa fa-download"). </summary>
        public string? FeatureIconColor { get; set; }

        [AllowNull, Column(TypeName = "text", Order = 15), Display(Name = "Description")]
        /// <summary>
        /// Feature description, useful for documentation.
        /// </summary>
        public string? Description { get; set; }

        [Required, NotNull, Column(TypeName = "bigint", Order = 16), Display(Name = "Sort Order")]
        /// <summary> Sort order value for features within a plan. </summary>
        public int DisplayOrder { get; set; } = 0;

        [Required, NotNull, Column(TypeName = "boolean", Order = 17), Display(Name = "Is Enabled")]
        public bool IsEnabled { get; set; } = true;

        [Required, NotNull, Column(TypeName = "boolean", Order = 18), Display(Name = "Is Customizable")]
        /// <summary> Indicates whether the feature can be customized or disabled by the user. </summary>
        public bool IsCustomizable { get; set; } = false;

        

        [Required, NotNull, Column(TypeName = "boolean", Order = 19), Display(Name = "Is Visible")]
        public bool IsVisible { get; set; } = true; // Whether it is visible


        [Required, NotNull, Column(TypeName = "timestamp", Order = 30), DefaultValue("now()"), Display(Name = "Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Creation date


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
