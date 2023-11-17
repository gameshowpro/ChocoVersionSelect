namespace ChocoVersionSelect.View;

public class HeaderTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int valueInt && valueInt > 0)
        {
            if (valueInt == 1)
            {
                return "1 more version";
            }
            return $"{valueInt} more versions";
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
