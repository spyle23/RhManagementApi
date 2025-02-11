namespace RhManagementApi.DTOs
{
    public class CreateTeamDto
    {
        public string Name { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public int ManagerId { get; set; }
    }

    public class TeamDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public int ManagerId { get; set; }
        public string ManagerFirstName { get; set; } = string.Empty;
        public string ManagerLastName { get; set; } = string.Empty;
        public string ManagerEmail { get; set; } = string.Empty;
        public string ManagerPicture { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class TeamMemberDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public int? TeamId { get; set; }
    }

    public class UpdateTeamDto
    {
        public string? Name { get; set; }
        public string? Specialty { get; set; }
        public int? ManagerId { get; set; }
    }
}
