using DynamicForm.Models;

namespace DynamicForm.Services;

public interface IFormService
{
    Task<List<FormDefinition>> GetAllFormsAsync();
    Task<FormDefinition> GetFormAsync(string id);
    Task SaveFormAsync(FormDefinition form);
    Task DeleteFormAsync(string id);
}
