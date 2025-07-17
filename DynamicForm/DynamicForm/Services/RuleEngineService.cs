using DynamicForm.Models;
using System.Text.Json;

namespace DynamicForm.Services;

public class RuleEngineService
{
    private readonly HttpClient _httpClient;
    private List<RuleEvaluationResult> _evaluationResults = new();

    public RuleEngineService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public IReadOnlyList<RuleEvaluationResult> EvaluationResults => _evaluationResults.AsReadOnly();

    public async Task EvaluateFormRules(FormDefinition formDefinition,
                                      Dictionary<string, object> formValues,
                                      string changedFieldId = null)
    {
        _evaluationResults.Clear();
        var evaluatedFields = new HashSet<string>();

        // If a specific field changed, start with its rules
        if (!string.IsNullOrEmpty(changedFieldId))
        {
            await EvaluateFieldRules(formDefinition, formValues, changedFieldId, evaluatedFields);
        }

        // Also evaluate all fields that might have conditions depending on other fields
        foreach (var field in formDefinition.Fields.Where(f => f.Rules.Any()))
        {
            if (!evaluatedFields.Contains(field.Id))
            {
                await EvaluateFieldRules(formDefinition, formValues, field.Id, evaluatedFields);
            }
        }
    }

    private async Task EvaluateFieldRules(FormDefinition formDefinition,
                                        Dictionary<string, object> formValues,
                                        string fieldId,
                                        HashSet<string> evaluatedFields)
    {
        if (evaluatedFields.Contains(fieldId)) return;
        evaluatedFields.Add(fieldId);

        var field = formDefinition.Fields.FirstOrDefault(f => f.Id == fieldId);
        if (field == null) return;

        // Process rules in priority order (higher first)
        foreach (var rule in field.Rules.OrderByDescending(r => r.Priority))
        {
            if (EvaluateConditionGroups(rule.ConditionGroups, formDefinition, formValues))
            {
                foreach (var action in rule.Actions)
                {
                    await ApplyAction(action, formDefinition, formValues, field, rule);

                    // Add evaluation result for UI feedback
                    if (!string.IsNullOrEmpty(action.Message))
                    {
                        _evaluationResults.Add(new RuleEvaluationResult
                        {
                            FieldId = field.Id,
                            Message = action.Message
                        });
                    }
                }
            }
            else
            {
                RevertToOriginalState(field);
            }
        }
    }

    public bool EvaluateConditionGroups(List<ConditionGroup> conditionGroups,
                                       FormDefinition formDefinition,
                                       Dictionary<string, object> formValues)
    {
        if (conditionGroups == null || !conditionGroups.Any()) return true;

        foreach (var group in conditionGroups)
        {
            bool groupResult = group.RequireAll; // true for AND, false for OR

            foreach (var condition in group.Conditions)
            {
                var conditionField = formDefinition.Fields.FirstOrDefault(f => f.Id == condition.FieldId);
                if (conditionField == null) continue;

                var conditionValue = formValues.TryGetValue(conditionField.Name, out var val) ? val : null;
                object compareValue = condition.Value;

                // If ValueFieldId is set, compare to another field's value
                if (!string.IsNullOrEmpty(condition.ValueFieldId))
                {
                    var compareField = formDefinition.Fields.FirstOrDefault(f => f.Id == condition.ValueFieldId);
                    if (compareField != null)
                    {
                        compareValue = formValues.TryGetValue(compareField.Name, out var cv) ? cv : null;
                    }
                }

                bool conditionResult = EvaluateSingleCondition(conditionField, conditionValue,
                    condition.Operator, compareValue);

                if (group.RequireAll)
                {
                    groupResult = groupResult && conditionResult;
                    if (!groupResult) break; // Short-circuit AND
                }
                else
                {
                    groupResult = groupResult || conditionResult;
                    if (groupResult) break; // Short-circuit OR
                }
            }

            if (groupResult) return true;
        }

        return false;
    }

    private bool EvaluateSingleCondition(FormField field, object fieldValue, string op, object compareValue)
    {
        try
        {
            // Handle null comparisons
            if (fieldValue == null || compareValue == null)
            {
                return op switch
                {
                    "==" => fieldValue == compareValue,
                    "!=" => fieldValue != compareValue,
                    _ => false
                };
            }

            // Type-specific comparisons
            switch (field.Type)
            {
                case FieldType.Number:
                    var num1 = Convert.ToDecimal(fieldValue);
                    var num2 = Convert.ToDecimal(compareValue);
                    return op switch
                    {
                        "==" => num1 == num2,
                        "!=" => num1 != num2,
                        ">" => num1 > num2,
                        "<" => num1 < num2,
                        ">=" => num1 >= num2,
                        "<=" => num1 <= num2,
                        _ => false
                    };

                case FieldType.Checkbox:
                    var bool1 = Convert.ToBoolean(fieldValue);
                    var bool2 = Convert.ToBoolean(compareValue);
                    return op switch
                    {
                        "==" => bool1 == bool2,
                        "!=" => bool1 != bool2,
                        _ => false
                    };

                case FieldType.Date:
                    var date1 = Convert.ToDateTime(fieldValue);
                    var date2 = Convert.ToDateTime(compareValue);
                    return op switch
                    {
                        "==" => date1 == date2,
                        "!=" => date1 != date2,
                        ">" => date1 > date2,
                        "<" => date1 < date2,
                        ">=" => date1 >= date2,
                        "<=" => date1 <= date2,
                        _ => false
                    };

                default: // Text, DropDown, etc.
                    var str1 = fieldValue.ToString();
                    var str2 = compareValue.ToString();
                    return op switch
                    {
                        "==" => str1.Equals(str2, StringComparison.OrdinalIgnoreCase),
                        "!=" => !str1.Equals(str2, StringComparison.OrdinalIgnoreCase),
                        "contains" => str1.Contains(str2, StringComparison.OrdinalIgnoreCase),
                        "startsWith" => str1.StartsWith(str2, StringComparison.OrdinalIgnoreCase),
                        "endsWith" => str1.EndsWith(str2, StringComparison.OrdinalIgnoreCase),
                        _ => false
                    };
            }
        }
        catch
        {
            return false;
        }
    }

    private async Task ApplyAction(FieldAction action,
                                 FormDefinition formDefinition,
                                 Dictionary<string, object> formValues,
                                 FormField targetField,
                                 FieldRule rule)
    {
        switch (action.Type.ToLower())
        {
            case "show":
                targetField.IsHidden = false;
                break;

            case "hide":
                targetField.IsHidden = true;
                break;

            case "enable":
                targetField.IsReadonly = false;
                break;

            case "disable":
                targetField.IsReadonly = true;
                break;

            case "setvalue":
                formValues[targetField.Name] = action.Value;
                targetField.IsSystemControlled = true;
                targetField.SystemSetReason = rule.Description;
                break;

            case "setrequired":
                targetField.IsRequired = Convert.ToBoolean(action.Value);
                break;

            case "loadoptions":
                if (action.DataSource != null)
                {
                    await LoadDynamicOptions(targetField, action.DataSource, formValues);
                }
                break;

            case "addvalidation":
                targetField.CustomValidations.Add(new ValidationRule
                {
                    ErrorMessage = action.Message,
                    Condition = action.Value?.ToString()
                });
                break;

            case "removefield":
                formDefinition.Fields.Remove(targetField);
                break;

            case "addfield":
                if (action.Value is FormField newField)
                {
                    formDefinition.Fields.Add(newField);
                }
                break;
        }
    }

    private async Task LoadDynamicOptions(FormField field,
                                        DataSourceConfig dataSource,
                                        Dictionary<string, object> formValues)
    {
        try
        {
            // Build URL with parameters
            var url = dataSource.Url;
            foreach (var param in dataSource.Parameters))
            {
                var paramValue = formValues.TryGetValue(param.Value, out var val)
                    ? val?.ToString()
                    : string.Empty;
                url = url.Replace($"{{{param.Key}}}", paramValue);
            }

            // Fetch data
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var items = await response.Content.ReadFromJsonAsync<List<JsonElement>>();

            // Map to options
            field.Data = items.Select(item => new SelectListItem
            {
                Value = item.GetProperty(dataSource.ValueField).GetString(),
                Text = item.GetProperty(dataSource.TextField).GetString()
            }).ToList();

            // Reset current value
            if (formValues.ContainsKey(field.Name))
            {
                formValues[field.Name] = null;
            }
        }
        catch (Exception ex)
        {
            _evaluationResults.Add(new RuleEvaluationResult
            {
                FieldId = field.Id,
                Message = $"Failed to load options: {ex.Message}"
            });
        }
    }

    private void RevertToOriginalState(FormField field)
    {
        field.IsHidden = field.OriginalIsHidden;
        field.IsReadonly = field.OriginalIsReadonly;
        field.IsRequired = field.OriginalIsRequired;
        field.IsSystemControlled = false;
        field.SystemSetReason = null;
    }
}
