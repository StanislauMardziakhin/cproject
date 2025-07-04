using System.Collections.Generic;

namespace CourseProject.ViewModels;

public class HomePageViewModel
{
    public List<TemplateViewModel> LatestTemplates { get; set; } = [];
    public List<TemplateViewModel> TopTemplates { get; set; } = [];
} 