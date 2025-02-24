using System.ComponentModel.DataAnnotations.Schema;

namespace EcomPlat.Data.Models.BaseModels
{
    public class StateInfo : CreatedStateInfo
    {
        [Column(TypeName = "datetime2")]
        public DateTime? UpdateDate { get; set; }
    }
}
