using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using CmsShop.Models.Data;

namespace CmsShop.Models.ViewModels.Pages
{
    public class SidebarVM
    {
        public SidebarVM()
        {
        }

        public SidebarVM(SidebarDTO row)
        {
            Id = row.Id;
            Body = row.Body;
        }

        public int Id { get; set; }
        [Display(Name = "Zawartość paska bocznego")]
        [AllowHtml]
        public string Body { get; set; }
    }
}