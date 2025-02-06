namespace RhManagementApi.Enums
{
    public enum EmployeeStatus
    {
        OnLeave,
        Active
    }

    public static class EmployeeStatusMapping
    {
        private static readonly Dictionary<EmployeeStatus, string> StatusDisplayValues = new()
        {
            { EmployeeStatus.OnLeave, "En Congé" },
            { EmployeeStatus.Active, "Actif" }
        };

        public static string ToDisplayValue(this EmployeeStatus status)
        {
            return StatusDisplayValues.TryGetValue(status, out string value)
                ? value
                : status.ToString();
        }

        public static EmployeeStatus? FromDisplayValue(string displayValue)
        {
            return StatusDisplayValues
                .FirstOrDefault(x => x.Value == displayValue)
                .Key;
        }
    }
} 