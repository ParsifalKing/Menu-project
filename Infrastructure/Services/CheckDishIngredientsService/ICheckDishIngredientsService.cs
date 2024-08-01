namespace Infrastructure.Services.CheckDishIngredientsService;

public interface ICheckDishIngredientsService
{
    public Task<bool> CheckDishIngredients(Guid dishId);
}
