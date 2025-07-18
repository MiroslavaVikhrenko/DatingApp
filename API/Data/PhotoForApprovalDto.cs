using System;

namespace API.Data;

public class PhotoForApprovalDto
{
    public int Id { get; set; }
    public required string Url { get; set; }
    public required string UserId { get; set; }
    public bool IsApproved { get; set; }
}
