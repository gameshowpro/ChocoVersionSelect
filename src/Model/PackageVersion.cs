using static ChocoVersionSelect.Model.PackageVersion;

namespace ChocoVersionSelect.Model;

public record PackageVersion(VersionParts Parts, string Version, bool IsCurrent, bool IsPrevious, bool IsLatest, bool IsDowngrade, string? PublishDate, string? Age)
{
	//Note that date is stored as a string to remove the need for other workarounds for WPF's poor handling of CultureInfo, especially with DateTimeOffset
	internal PackageVersion(string Version, bool IsCurrent, bool IsPrevious, bool IsLatest, bool IsDowngrade, DateTimeOffset? publishDate) :
		this(StringToVersionParts(Version), Version, IsCurrent, IsPrevious, IsLatest, IsDowngrade, string.Format("{0:d}", publishDate), GetAge(publishDate))
	{ }

	private static string? GetAge(DateTimeOffset? date)
	{
		if (!date.HasValue)
		{
			return null;
		}
        int days = (int)(DateTimeOffset.Now - date.Value).TotalDays;
		if (days == 0)
		{
			return "Today";
		}
		if (days == 1)
		{
			return "1 day ago";
		}
		if (days < 0)
		{
			return "In the future";
		}
		if (days > 365)
		{
			int years = days / 365;
            return $">{years} year{(years == 1 ? string.Empty : "s")} ago";
		}
		return $"{days} days ago";
	}

	private static VersionParts StringToVersionParts(string version)
	{
		string[] parts = version.Split('.');
		return new(parts.ElementAtOrDefault(0), parts.ElementAtOrDefault(1), parts.ElementAtOrDefault(2), parts.ElementAtOrDefault(3));
	}

    public record VersionParts(string? Part0, string? Part1, string? Part2, string? Part3);
}
