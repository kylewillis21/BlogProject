﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace AuthSystem.Areas.Identity.Data;

// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{ 
    [PersonalData]
    [Column(TypeName  = "nvarchar(100)")]
    public string FirstName { get; set; }

    [PersonalData]
    [Column(TypeName = "nvarchar(100)")]
    public string LastName { get; set; }

    [PersonalData]
    [Column(TypeName = "nvarchar(100)")]
    public string Nickname { get; set; }

    [PersonalData]
    [Column(TypeName = "nvarchar(250)")]
    public string? ProfilePicturePath { get; set; }

    public ICollection<BlogPost> Posts { get; set; }

    public ICollection<Comment> Comments { get; set; } 


}

