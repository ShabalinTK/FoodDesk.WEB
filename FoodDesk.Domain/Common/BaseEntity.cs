using FoodDesk.Domain.Common.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDesk.Domain.Common;

public abstract class BaseEntity : IEntity
{

    public int Id { get; set; }
}
