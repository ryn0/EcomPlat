using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EcomPlat.Data.Models.BaseModels;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EcomPlat.Data.Models
{
    /// <summary>
    /// Represents a company that serves as the source or brand for products.
    /// </summary>
    public class Company : UserStateInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier for the company.
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Gets or sets the name of the company.
        /// </summary>
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the company.
        /// </summary>
        [MaxLength(2000)]
        public string Description { get; set; }

        /// <summary>
        /// Navigation property for the products associated with the company.
        /// </summary>
        [ValidateNever]
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}