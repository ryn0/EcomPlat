﻿using EcomPlat.Data.Models;
namespace EcomPlat.Web.Models
{
    public class ProductReviewManageViewModel
    {
        required public List<ProductReview> Reviews { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}