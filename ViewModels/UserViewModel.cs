﻿namespace CourseProject.ViewModels;

public class UserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
    public bool IsAdmin { get; set; }
}