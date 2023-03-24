#nullable enable
using Quartzmin.TypeHandlers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Quartzmin.Models;

public class JobDataMapItemBase : IHasValidation
{
    [Required]
    public string? Name { get; set; }

    public object? Value { get; set; }

    [Required]
    public TypeHandlerBase? SelectedType { get; set; }

    public bool IsLast { get; set; }

    public string? RowId { get; set; }

    private const string NameField = "data-map[name]";
    private const string HandlerField = "data-map[handler]";
    private const string TypeField = "data-map[type]";
    private const string IndexField = "data-map[index]";
    private const string ValueField = "data-map[value]";
    private const string LastItemField = "data-map[lastItem]";

    public static JobDataMapItemBase FromDictionary(Dictionary<string, object> formData, Services services)
    {
        var valueFormData = new Dictionary<string, object?>();

        var result = new JobDataMapItemBase();

        foreach (var item in formData)
        {
            if (item.Key == NameField)
            {
                result.Name = item.Value as string;
                continue;
            }
            if (item.Key == HandlerField)
            {
                if (item.Value is string value) result.SelectedType = services.TypeHandlers.Deserialize(value);
                continue;
            }
            if (item.Key == TypeField)
            {
                continue;
            }
            if (item.Key == IndexField)
            {
                result.RowId = item.Value as string;
                continue;
            }
            if (item.Key == LastItemField)
            {
                if (item.Value is not null) result.IsLast = Convert.ToBoolean(item.Value);
                continue;
            }

            valueFormData.Add(item.Key, item.Value);
        }

        if (result.SelectedType != null)
            result.Value = result.SelectedType.ConvertFrom(valueFormData);

        return result;
    }

    public override string ToString()
    {
        if (Name == null) return base.ToString() ?? "";
        return Value != null ? $"{Name} = {Value}" : Name;
    }

    public void Validate(ICollection<ValidationError> errors)
    {
        if (string.IsNullOrEmpty(Name!))
            AddValidationError(NameField, errors);

        if (SelectedType == null)
            AddValidationError(TypeField, errors);

        if (SelectedType?.IsValid(Value) == false)
            AddValidationError(ValueField, errors);
    }

    private void AddValidationError(string field, ICollection<ValidationError> errors)
    {
        errors.Add(ValidationError.EmptyField(field + ":" + RowId));
    }
}