﻿using System.ComponentModel.DataAnnotations;
using CmsShop.Models.Data;

namespace CmsShop.Models.ViewModels.Shop
{
    public class CategoryVM
    {
        public CategoryVM()
        {
            
        }

        public CategoryVM(CategoryDto row)
        {
            Id = row.Id;
            Name = row.Name;
            Slug = row.Slug;
            Sorting = row.Sorting;
        }

        public int Id { get; set; }
        [Display(Name = "Nazwa kategorii")]
        public string Name { get; set; }
        public string Slug { get; set; }
        public int Sorting { get; set; }
    }
}