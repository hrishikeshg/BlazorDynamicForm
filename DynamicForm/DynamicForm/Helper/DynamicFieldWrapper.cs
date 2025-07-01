namespace DynamicForm.Helper;

public class DynamicFieldWrapper
{
    private readonly Dictionary<string, object> _values;
    private readonly string _fieldName;

    public DynamicFieldWrapper(Dictionary<string, object> values, string fieldName)
    {
        _values = values;
        _fieldName = fieldName;
    }

    public string Value
    {
        get => _values.TryGetValue(_fieldName, out var value) ? value?.ToString() : null;
        set => _values[_fieldName] = value;
    }
}
