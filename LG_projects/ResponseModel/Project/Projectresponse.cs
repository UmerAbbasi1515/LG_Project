namespace LG_projects.ResponseModel.Project
{
    public class ProjectVm
    {
        public int Id { get; set; }
        public string? NameEn { get; set; }
        public string? NameUr { get; set; }
        public string? DescriptionEn { get; set; }
        public string? DescriptionUr { get; set; }
        public string? LocationEn { get; set; }
        public string? LocationUr { get; set; }
        public string? AdpYear { get; set; }
        public bool Suspended { get; set; }
        public DateTime CreatedAt { get; set; }

        public HalkaVm? Halka { get; set; }
        public UCVm? UC { get; set; }
        public WardVm? Ward { get; set; }
        public PMOVm? PMO { get; set; }
        public ProjectLeaderVm? ProjectLeader { get; set; }
        public string? committee_members_name_en { get; set; }
        public string? committee_members_name_ur { get; set; }
    }

    // Add inside your ProjectVm file or a separate file
    public class CommitteeMemberWithProjectVm
    {
        public int Id { get; set; }
        public string? NameEn { get; set; }
        public string? NameUr { get; set; }
        public int ProjectId { get; set; }  // maps to project_id alias in SQL
    }
    public class HalkaVm
    {
        public int HalkaId { get; set; }
        public string? HalkaNameEn { get; set; }
        public string? HalkaNameUr { get; set; }
    }

    public class UCVm
    {
        public int UCId { get; set; }
        public string? UCNameEn { get; set; }
        public string? UCNameUr { get; set; }
    }

    public class WardVm
    {
        public int WardId { get; set; }
        public string? WardNameEn { get; set; }
        public string? WardNameUr { get; set; }
    }

    public class PMOVm
    {
        public int PmoId { get; set; }
        public string? PmoNameEn { get; set; }
        public string? PmoNameUr { get; set; }
    }

    public class ProjectLeaderVm
    {
        public int ProjectLeaderId { get; set; }
        public string? ProjectLeaderNameEn { get; set; }
        public string? ProjectLeaderNameUr { get; set; }
    }

    public class ProjectCommitteeVm
    {
        public int Id { get; set; }
        public string? NameEn { get; set; }
        public string? NameUr { get; set; }
    }
}