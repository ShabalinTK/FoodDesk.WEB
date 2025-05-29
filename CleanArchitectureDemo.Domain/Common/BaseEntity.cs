using FoodDesk.Domain.Common.Interfaces;

namespace FoodDesk.Domain.Common;
public abstract class BaseEntity : IEntity
{
    public int Id { get; set; }
}
