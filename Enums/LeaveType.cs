namespace RhManagementApi.Enums
{
    public enum RHType
    {
        holiday,
        permission
    }

    public static class RhTypeMapping
    {
        private static readonly Dictionary<RHType, string> StatusDisplayValues = new()
        {
            { RHType.holiday, "Congé payé" },
            { RHType.permission, "Permission" },
        };

        public static string ToDisplayValue(this RHType type)
        {
            return StatusDisplayValues.TryGetValue(type, out string value)
                ? value
                : type.ToString();
        }

        public static RHType? FromDisplayValue(string displayValue)
        {
            return StatusDisplayValues
                .FirstOrDefault(x => x.Value == displayValue)
                .Key;
        }
    }
}