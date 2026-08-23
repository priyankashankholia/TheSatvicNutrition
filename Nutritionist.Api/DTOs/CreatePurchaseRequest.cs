using System.ComponentModel.DataAnnotations;

namespace Nutritionist.Api.DTOs;

public class CreatePurchaseRequest
{
    [Required]
    public Guid PackageId { get; set; }
}