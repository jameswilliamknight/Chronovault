namespace Chronovault.Service.Framework;

/// <summary>
///     Wrapper type for job names - ensures non-null/empty values at construction.
/// </summary>
public class JobName
{
    public JobName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentNullException(nameof(value), "Job name is missing.");
        }

        Value = value;
    }

    public string Value { get; }
}

