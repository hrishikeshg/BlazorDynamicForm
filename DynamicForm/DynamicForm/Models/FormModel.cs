namespace DynamicForm.Models;

public enum FieldType
{
    Text,
    Number,
    DropDown,
    Checkbox,
    Date,
    // Add more as needed
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
    public List<SelectListItem> Data { get; set; } = new(); // For dropdowns
    public List<FieldRule> Rules { get; set; } = new();
}

public class FieldRule
{
    public string TargetFieldId { get; set; }
    public string Condition { get; set; } // e.g., "value > 18"
    public List<FieldAction> Actions { get; set; } = new();
    // Actions could include: enable/disable, show/hide, set value, etc.
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
