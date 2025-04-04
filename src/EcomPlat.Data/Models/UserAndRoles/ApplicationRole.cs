﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace EcomPlat.Data.Models
{
    public class ApplicationUserRole : IdentityUserRole<string>
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [StringLength(36)]
        public override string UserId
        {
            get
            {
                return base.UserId;
            }

            set
            {
                base.UserId = value;
            }
        }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override string RoleId
        {
            get
            {
                return base.RoleId;
            }

            set
            {
                base.RoleId = value;
            }
        }
    }
}