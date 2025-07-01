namespace DynamicForm.Helper;
public class DictionaryValidationWrapper
{
    private readonly Dictionary<string, object> _dictionary;

    public DictionaryValidationWrapper(Dictionary<string, object> dictionary)
    {
        _dictionary = dictionary;
    }

    // Indexer property for validation
    [System.ComponentModel.DataAnnotations.Key]
    public object this[string key]
    {
        get => _dictionary.TryGetValue(key, out var value) ? value : null;
        set => _dictionary[key] = value;
    }
}
