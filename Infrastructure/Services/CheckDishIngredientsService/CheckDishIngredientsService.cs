using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.CheckDishIngredientsService;

public class CheckDishIngredientsService(ILogger<CheckDishIngredientsService> logger, DataContext context) : ICheckDishIngredientsService
{

    #region CheckDishIngredients

    public async Task<bool> CheckDishIngredients(Guid dishId)
    {
        logger.LogInformation("Start method CheckDishIngredients in time:{DateTime}", DateTimeOffset.UtcNow);
        var ingredients = (from i in context.Ingredients
                           join di in context.DishIngredient on i.Id equals di.IngredientId
                           join d in context.Dishes on di.DishId equals d.Id
                           where d.Id == dishId
                           select new
                           {
                               Ingredient = i,
                           }).Select(x => x.Ingredient);
        if (ingredients == null) return false;
        bool areAllIngredients = ingredients.All(x => x.Count > 2);

        var dish = await context.Dishes.FirstOrDefaultAsync(x => x.Id == dishId);
        dish!.AreAllIngredients = areAllIngredients;
        await context.SaveChangesAsync();

        logger.LogInformation("Finished method CheckDishIngredients in time:{DateTime}", DateTimeOffset.UtcNow);
        return areAllIngredients;
    }

    #endregion
}
