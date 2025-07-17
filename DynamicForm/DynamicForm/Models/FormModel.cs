namespace DynamicForm.Models;

public enum FieldType
{
    Text,
    Number,
    DropDown,
    Checkbox,
    Date,
    CascadingDropDown
}

public class FormField
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public string Label { get; set; }
    public FieldType Type { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public bool IsRequired { get; set; }
    public bool IsHidden { get; set; }
    public bool IsReadonly { get; set; }
    public string DefaultValue { get; set; }
    public string RegexPattern { get; set; }
    public List<SelectListItem> Data { get; set; } = new(); // For dropdowns only

    // For cascading dropdowns
    public string ParentFieldId { get; set; }
    public Dictionary<string, List<SelectListItem>> CascadingData { get; set; } = new();
    public List<FieldRule> Rules { get; set; } = new();

    public bool OriginalIsHidden { get; set; }
    public bool OriginalIsReadonly { get; set; }
    public bool OriginalIsRequired { get; set; }
    public object OriginalValue { get; set; }

    public bool IsSystemControlled { get; set; }
    public List<ValidationRule> CustomValidations { get; set; } = new();
    public string SystemSetReason { get; set; }
}

public class FieldRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TargetFieldId { get; set; }
    public string Description { get; set; } // For UI display
    public List<ConditionGroup> ConditionGroups { get; set; } = new();
    public List<FieldAction> Actions { get; set; } = new();
    public int Priority { get; set; } = 0; // Higher priority executes first
}

public class ConditionGroup
{
    public bool RequireAll { get; set; } = true; // AND (true) or OR (false) logic
    public List<FieldCondition> Conditions { get; set; } = new();
}
public class FieldCondition
{
    public string FieldId { get; set; }
    public string Operator { get; set; } // "==", "!=", ">", "<", ">=", "<=", "contains", "startsWith", etc.
    public object Value { get; set; }
    public string ValueFieldId { get; set; } // Optional - compare to another field instead of fixed value

    // Helper properties for binding
    public int NumberValue
    {
        get => Value is int i ? i : 0;
        set => Value = value;
    }

    public DateTime DateValue
    {
        get => Value is DateTime dt ? dt : DateTime.Today;
        set => Value = value;
    }

    public bool BooleanValue
    {
        get => Value is bool b ? b : false;
        set => Value = value;
    }

    public string StringValue
    {
        get => Value?.ToString();
        set => Value = value;
    }
}
//public class DataSourceConfig
//{
//    public string SourceField { get; set; }  // Field that triggers the load
//    public string ValuePath { get; set; }    // Property path for option values
//    public string TextPath { get; set; }     // Property path for display text
//    public string DataUrl { get; set; }      // API endpoint pattern (e.g., "/api/states?country={value}")
//}
public class FieldAction
{
    public string Type { get; set; } // "show", "hide", "enable", "disable", "setValue", "setRequired", "loadOptions", "addValidation"
    public object Value { get; set; }
    public string Message { get; set; } // For UI feedback
    public DataSourceConfig DataSource { get; set; } // For dynamic data loading
    // Helper properties for binding
    public string StringValue
    {
        get => Value?.ToString();
        set => Value = value;
    }

    public int NumberValue
    {
        get => Value is int i ? i : 0;
        set => Value = value;
    }

    public bool BooleanValue
    {
        get => Value is bool b ? b : false;
        set => Value = value;
    }

    public DateTime DateValue
    {
        get => Value is DateTime dt ? dt : DateTime.Today;
        set => Value = value;
    }
}

public class FormDefinition
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; }
    public List<FormField> Fields { get; set; } = new();
}

public class SelectListItem
{
    public string Value { get; set; }
    public string Text { get; set; }
}
public class FormFieldModel
{
    private readonly Dictionary<string, object> _values;

    public FormFieldModel(Dictionary<string, object> values) => _values = values;

    public string this[string fieldName]
    {
        get => _values.TryGetValue(fieldName, out var value) ? value?.ToString() : null;
        set => _values[fieldName] = value;
    }
}

public class FormSubmission
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FormDefinitionId { get; set; }  // Link to FormDefinition
    public Dictionary<string, object> FieldValues { get; set; } = new();
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public string SubmittedBy { get; set; }  // Optional: User ID/email

    //public FormDefinition FormDefinition { get; set; }
}
public class ValidationRule
{
    public string ErrorMessage { get; set; }
    public string Condition { get; set; } // Optional - if null, always applies
}
public class RuleEvaluationResult
{
    public string FieldId { get; set; }
    public string Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

public class DataSourceConfig
{
    public string Url { get; set; }
    public string ValueField { get; set; }
    public string TextField { get; set; }
    public Dictionary<string, string> Parameters { get; set; } = new();
}