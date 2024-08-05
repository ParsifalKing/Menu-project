namespace Domain.Constants;

public static class Permissions
{
    public static List<string> GeneratePermissionsForModule(string module)
    {
        return
        [
            $"Permissions.{module}.Create",
            $"Permissions.{module}.View",
            $"Permissions.{module}.Edit",
            $"Permissions.{module}.Delete"
        ];
    }


    public static class Category
    {
        public const string View = "Permissions.Category.View";
        public const string Create = "Permissions.Category.Create";
        public const string Edit = "Permissions.Category.Edit";
        public const string Delete = "Permissions.Category.Delete";
    }

    public static class Dish
    {
        public const string View = "Permissions.Dish.View";
        public const string Create = "Permissions.Dish.Create";
        public const string Edit = "Permissions.Dish.Edit";
        public const string Delete = "Permissions.Dish.Delete";
    }

    public static class DishCategory
    {
        public const string View = "Permissions.DishCategory.View";
        public const string Create = "Permissions.DishCategory.Create";
        public const string Edit = "Permissions.DishCategory.Edit";
        public const string Delete = "Permissions.DishCategory.Delete";
    }

    public static class DishIngredient
    {
        public const string View = "Permissions.DishIngredient.View";
        public const string Create = "Permissions.DishIngredient.Create";
        public const string Edit = "Permissions.DishIngredient.Edit";
        public const string Delete = "Permissions.DishIngredient.Delete";
    }

    public static class Drink
    {
        public const string View = "Permissions.Drink.View";
        public const string Create = "Permissions.Drink.Create";
        public const string Edit = "Permissions.Drink.Edit";
        public const string Delete = "Permissions.Drink.Delete";
    }

    public static class DrinkIngredient
    {
        public const string View = "Permissions.DrinkIngredient.View";
        public const string Create = "Permissions.DrinkIngredient.Create";
        public const string Edit = "Permissions.DrinkIngredient.Edit";
        public const string Delete = "Permissions.DrinkIngredient.Delete";
    }

    public static class Ingredient
    {
        public const string View = "Permissions.Ingredient.View";
        public const string Create = "Permissions.Ingredient.Create";
        public const string Edit = "Permissions.Ingredient.Edit";
        public const string Delete = "Permissions.Ingredient.Delete";
    }

    public static class Order
    {
        public const string View = "Permissions.Order.View";
        public const string Create = "Permissions.Order.Create";
        public const string Edit = "Permissions.Order.Edit";
        public const string Delete = "Permissions.Order.Delete";
    }

    public static class OrderDetail
    {
        public const string View = "Permissions.OrderDetail.View";
        public const string Create = "Permissions.OrderDetail.Create";
        public const string Edit = "Permissions.OrderDetail.Edit";
        public const string Delete = "Permissions.OrderDetail.Delete";
    }

    public static class Notification
    {
        public const string View = "Permissions.Notification.View";
        public const string Create = "Permissions.Notification.Create";
    }

    public static class Role
    {
        public const string View = "Permissions.Role.View";
        public const string Create = "Permissions.Role.Create";
        public const string Edit = "Permissions.Role.Edit";
        public const string Delete = "Permissions.Role.Delete";
    }

    public static class User
    {
        public const string View = "Permissions.User.View";
        public const string Create = "Permissions.User.Create";
        public const string Edit = "Permissions.User.Edit";
    }

    public static class UserRole
    {
        public const string View = "Permissions.UserRole.View";
        public const string Create = "Permissions.UserRole.Create";
        public const string Delete = "Permissions.UserRole.Delete";
    }

}
