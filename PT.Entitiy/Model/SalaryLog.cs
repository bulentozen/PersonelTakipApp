using PT.Entitiy.IdentityModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.Entitiy.Model
{
    public class SalaryLog:BaseModel
    {
        public decimal Salary { get; set; }
        public string UserID { get; set; }
        [ForeignKey("UserID")]
        public virtual ApplicationUser User { get; set; }
    }
}
