namespace rag_2_backend.Infrastructure.Module.Administration.Dto;

public class LimitsResponse
{
    public required int StudentLimitMb { get; set; }
    public required int TeacherLimitMb { get; set; }
}