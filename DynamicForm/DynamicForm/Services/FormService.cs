using DynamicForm.Models;

namespace DynamicForm.Services;

public class FormService : IFormService
{
    private readonly List<FormDefinition> _forms = new();

    public FormService()
    {
        // Initialize with some test forms
        _forms.Add(new FormDefinition
        {
            Id = "1",
            Name = "User Registration",
            Fields = new List<FormField>
            {
                new FormField { Name = "FirstName", Label = "First Name", Type = FieldType.Text },
                new FormField { Name = "LastName", Label = "Last Name", Type = FieldType.Text },
                new FormField { Name = "Email", Label = "Email", Type = FieldType.Text }
            }
        });

        _forms.Add(new FormDefinition
        {
            Id = "2",
            Name = "Survey",
            Fields = new List<FormField>
            {
                new FormField
                {
                    Name = "Rating",
                    Label = "Rating",
                    Type = FieldType.DropDown,
                    Data = new List<SelectListItem>
                    {
                        new SelectListItem { Value = "1", Text = "Poor" },
                        new SelectListItem { Value = "2", Text = "Fair" },
                        new SelectListItem { Value = "3", Text = "Good" },
                        new SelectListItem { Value = "4", Text = "Very Good" },
                        new SelectListItem { Value = "5", Text = "Excellent" }
                    }
                }
            }
        });
    }
    public Task<List<FormDefinition>> GetAllFormsAsync()
    {
        return Task.FromResult(_forms.ToList());
    }

    public Task<FormDefinition> GetFormAsync(string id)
    {
        return Task.FromResult(_forms.FirstOrDefault(f => f.Id == id));
    }

    public Task SaveFormAsync(FormDefinition form)
    {
        if (string.IsNullOrEmpty(form.Id))
        {
            form.Id = Guid.NewGuid().ToString();
        }

        var existing = _forms.FirstOrDefault(f => f.Id == form.Id);
        if (existing != null)
        {
            _forms.Remove(existing);
        }

        _forms.Add(form);
        return Task.CompletedTask;
    }

    public Task DeleteFormAsync(string id)
    {
        var form = _forms.FirstOrDefault(f => f.Id == id);
        if (form != null)
        {
            _forms.Remove(form);
        }
        return Task.CompletedTask;
    }
}
