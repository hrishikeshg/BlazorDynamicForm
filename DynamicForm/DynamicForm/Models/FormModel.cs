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
}

public class FieldRule
{
    public string TargetFieldId { get; set; }
    public string Condition { get; set; } // e.g., "value > 18"
    public List<FieldAction> Actions { get; set; } = new();
    // Actions could include: enable/disable, show/hide, set value, etc.
    public DataSourceConfig DataSource { get; set; }
}
public class DataSourceConfig
{
    public string SourceField { get; set; }  // Field that triggers the load
    public string ValuePath { get; set; }    // Property path for option values
    public string TextPath { get; set; }     // Property path for display text
    public string DataUrl { get; set; }      // API endpoint pattern (e.g., "/api/states?country={value}")
}
public class FieldAction
{
    public string Type { get; set; }  // "show", "hide", "enable", "disable", "setValue"
    public object Value { get; set; } // Only used when Type is "setValue"
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