namespace RhManagementApi.Enums
{
    public enum RHStatus
    {
        Approved,
        Pending,
        Rejected
    }

    public static class RHStatusMapping
    {
        private static readonly Dictionary<RHStatus, string> StatusDisplayValues = new()
        {
            { RHStatus.Approved, "Approuvé" },
            { RHStatus.Pending, "En Attente" },
            { RHStatus.Rejected, "Rejeté" }
        };

        public static string ToDisplayValue(this RHStatus status)
        {
            return StatusDisplayValues.TryGetValue(status, out string value) 
                ? value 
                : status.ToString();
        }

        public static RHStatus? FromDisplayValue(string displayValue)
        {
            return StatusDisplayValues
                .FirstOrDefault(x => x.Value == displayValue)
                .Key;
        }
    }
}