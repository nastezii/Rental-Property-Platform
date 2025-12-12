using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities.Base;

public class BaseEntity
{
    [Column(Order = 0)]
    public int Id { get; set; }

    public byte[] RowVersion { get; set; } = [];
}