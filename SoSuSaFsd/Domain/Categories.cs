using System;
using System.ComponentModel.DataAnnotations;

namespace SoSuSaFsd.Domain
{
    // We inherit from BaseDomainModel to get the audit fields (DateCreated, etc.)
    public class Categories : BaseDomainModel
    {
        // 1. Matches ERD: CategoryName
        public string? CategoryName { get; set; }

        // 2. Matches ERD: CategoryDescription (Renamed from 'Description')
        public string? CategoryDescription { get; set; }

        // 3. Matches ERD: CategoryIsRestricted (Renamed from 'IsRestricted')
        public bool CategoryIsRestricted { get; set; }
    }
}