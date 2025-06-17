using System.ComponentModel.DataAnnotations;

namespace CourseProject.Models;

public enum ActionType
{
    [Display(Name = "Block Users")] Block,

    [Display(Name = "Unblock Users")] Unblock,

    [Display(Name = "Assign Admin Role")] AssignAdmin,

    [Display(Name = "Remove Admin Role")] RemoveAdmin,

    [Display(Name = "Delete Users")] Delete
}