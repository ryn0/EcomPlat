using System.ComponentModel.DataAnnotations;
using EcomPlat.Data.Models.BaseModels;

namespace EcomPlat.Data.Models
{
    public class ApplicationUser : ApplicationUserStateInfo
    {
        public ApplicationUser()
        {
        }

        [StringLength(36)]
        public override string Id
        {
            get
            {
                return base.Id;
            }

            set
            {
                base.Id = value;
            }
        }
    }
}