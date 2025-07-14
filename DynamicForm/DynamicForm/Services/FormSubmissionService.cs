using DynamicForm.Models;

namespace DynamicForm.Services;

public class FormSubmissionService
{
    // Static list to hold all submissions
    private static readonly List<FormSubmission> _submissions = new();

    // Add a new submission
    public void AddSubmission(FormSubmission submission)
    {
        _submissions.Add(submission);
    }

    // Get all submissions
    public IReadOnlyList<FormSubmission> GetAllSubmissions()
    {
        return _submissions.AsReadOnly();
    }

    // Get submissions for a specific form
    public IReadOnlyList<FormSubmission> GetSubmissionsByFormId(string formId)
    {
        return _submissions.Where(s => s.FormDefinitionId == formId).ToList().AsReadOnly();
    }
    public void UpdateSubmission(FormSubmission updated)
    {
        var index = _submissions.FindIndex(s => s.Id == updated.Id);
        if (index >= 0)
            _submissions[index] = updated;
    }
}
