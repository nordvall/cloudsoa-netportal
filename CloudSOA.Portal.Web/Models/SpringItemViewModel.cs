using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudSOA.Portal.Web.Models
{
    public class SpringListViewModel
    {
        public List<SpringItemViewModel> items { get; set; }
    }

    public class SpringItemViewModel
    {
        public int id { get; set; }

        [Required]
        [Display(Name = "Text")]
        public string text { get; set; }

        public string modified_by { get; set; }

        public DateTimeOffset modified_at { get; set; }
    }
}
