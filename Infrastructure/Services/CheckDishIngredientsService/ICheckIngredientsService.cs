namespace Infrastructure.Services.CheckIngredientsService;

public interface ICheckIngredientsService
{
    public Task<bool> CheckDishIngredients(Guid dishId);
    public Task<bool> CheckDrinkIngredients(Guid drinkId);

}
