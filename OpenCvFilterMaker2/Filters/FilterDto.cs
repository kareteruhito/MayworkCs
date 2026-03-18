
using Reactive.Bindings;
using System.Reflection;
using System.Text.Json;
using System.Windows.Input;

namespace OpenCvFilterMaker2;

public class FilterDto
{
    public string Type { get; set; } = "";

    public Dictionary<string, object> Parameters { get; set; } = [];


    // フィルター → DTO 変換
    public static FilterDto ToDto(CvFilterBase filter)
    {
        var dto = new FilterDto
        {
            Type = filter.GetType().Name
        };

        var props = filter.GetType()
            .GetProperties()
            .Where(p => p.CanRead)
            .Where(p => !typeof(ICommand).IsAssignableFrom(p.PropertyType));

        foreach (var p in props)
        {
            var obj = p.GetValue(filter);

            if (obj is IReactiveProperty rp)
            {
                dto.Parameters[p.Name] = rp.Value ?? "";
            }
            else
            {
                dto.Parameters[p.Name] = obj ?? "";
            }
        }

        return dto;
    }
    // DTO → フィルター復元
    public static CvFilterBase FromDto(FilterDto dto)
    {
        var type = Assembly.GetExecutingAssembly()
            .GetTypes()
            .FirstOrDefault(t => t.Name == dto.Type);

        if (type == null)
            throw new Exception($"Unknown filter type: {dto.Type}");

        var filter = (CvFilterBase)Activator.CreateInstance(type)!;


        var props = filter.GetType()
            .GetProperties()
            .Where(p => p.CanRead)
            .Where(p => !typeof(ICommand).IsAssignableFrom(p.PropertyType));

        foreach (var p in props)
        {
            if (!dto.Parameters.TryGetValue(p.Name, out var val))
                continue;

            var obj = p.GetValue(filter);

            if (obj is IReactiveProperty rp)
            {
                if (rp.Value is not null)
                {
                    if (val is JsonElement je)
                    {
                        var type2 = rp.Value.GetType();
                        val = je.Deserialize(type2);
                    }

                    rp.Value = val;
                }
            }
            else
            {
                if (val is JsonElement je)
                {
                    val = je.Deserialize(p.PropertyType);
                }

                if (p.CanWrite)
                    p.SetValue(filter, val);
            }
        }
        return filter;
    }

}
