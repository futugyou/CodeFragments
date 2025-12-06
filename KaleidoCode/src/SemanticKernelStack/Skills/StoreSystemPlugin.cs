
using System.ComponentModel;

namespace SemanticKernelStack.Skills;

public class StoreSystemPlugin
{
    public static List<Goods> GoodsList { get; set; } = [
        new ("Apple",5,100),
        new ("Banana",3,200),
        new ("Orange",4,150),
        new ("Peach",6,120),
        new ("Pear",5,100),
        new ("Grape",7,80),
        new ("Watermelon",8,60),
        new ("Pineapple",9,40),
        new ("Mango",10,30),
        new ("Strawberry",11,20),
        new ("Lemon",4,100),
        new ("Orange",3,100),
        new ("Blueberry",6,100),
        new ("Cherry",7,100),
        new ("Grapefruit",8,100),
        new ("Pomelo",9,100),
        new ("Durian",10,100),
        new ("Dragon Fruit",11,100),
        new ("Lychee",12,100),
        new ("Coconut",13,100),
        new ("Mulberry",5,100),
        new ("Yarberry",4,100),
        new ("Raspberry",6,100),
        new ("Berry",7,100),
        new ("Pomegranate",8,100),
        new ("Peach",9,100),
    ];

    public decimal Total { get; set; } = 0;
    [KernelFunction]
    [Description("Query fruit by name")]
    public string GetGoodsByName([Description("Fruit Name")] string name)
    {
        return GoodsList.FirstOrDefault(g => g.Name == name)?.ToString() ?? "No Fruit Found";
    }

    [KernelFunction]
    [Description("Query all fruits whose price is less than or equal to the parameter")]
    public string GetGoodsLessEqualsPrice([Description("Fruit Price")] decimal price)
    {
        var goodses = GoodsList.Where(g => g.Price <= price);
        if (goodses == null || goodses.Any() == false)
        {
            return "No Fruit Found";
        }
        else
        {
            return string.Join("\n", goodses);
        }
    }

    [Description("Query all fruits whose price is less than the parameter")]
    public string GetGoodsLessPrice([Description("Fruit Price")] decimal price)
    {
        var goodses = GoodsList.Where(g => g.Price < price);
        if (goodses == null || goodses.Any() == false)
        {
            return "No fruit found";
        }
        else
        {
            return string.Join("\n", goodses);
        }
    }

    [KernelFunction]
    [Description("Query all fruits whose price is greater than or equal to the parameter")]
    public string GetGoodsGreaterEqualsPrice([Description("Fruit price")] decimal price)
    {
        var goodses = GoodsList.Where(g => g.Price >= price);
        if (goodses == null || goodses.Any() == false)
        {
            return "No fruit found";
        }
        else
        {
            return string.Join("\n", goodses);
        }
    }

    [KernelFunction]
    [Description("Query all fruits whose price is greater than the parameter")]
    public string GetGoodsGreaterPrice([Description("Fruit price")] decimal price)
    {
        var goodses = GoodsList.Where(g => g.Price > price);
        if (goodses == null || goodses.Any() == false)
        {
            return "No fruit found";
        }
        else
        {
            return string.Join("\n", goodses);
        }
    }

    [KernelFunction]
    [Description("Query all fruits whose quantity is greater than or equal to the parameter")]
    public string GetGoodsGreaterEqualsQuantity([Description("Fruit quantity")] int quantity)
    {
        var goodses = GoodsList.Where(g => g.Quantity >= quantity);
        if (goodses == null || goodses.Any() == false)
        {
            return "No fruit found";
        }
        else
        {
            return string.Join("\n", goodses);
        }
    }

    [KernelFunction]
    [Description("Query all fruits whose quantity is greater than the parameter")]
    public string GetGoodsGreaterQuantity([Description("Fruit quantity")] int quantity)
    {
        var goodses = GoodsList.Where(g => g.Quantity > quantity);
        if (goodses == null || goodses.Any() == false)
        {
            return "No fruit found";
        }
        else
        {
            return string.Join("\n", goodses);
        }
    }

    [KernelFunction]
    [Description("Query all fruits whose quantity is less than or equal to the parameter")]
    public string GetGoodsLessEqualsQuantity([Description("Fruit quantity")] int quantity)
    {
        var goodses = GoodsList.Where(g => g.Quantity <= quantity);
        if (goodses == null || goodses.Any() == false)
        {
            return "No fruit found";
        }
        else
        {
            return string.Join("\n", goodses);
        }
    }

    [KernelFunction]
    [Description("Query all fruits whose inventory quantity (Quantity) is less than the parameter")]
    public string GetGoodsLessQuantity([Description("Fruit quantity")] int quantity)
    {
        var goodses = GoodsList.Where(g => g.Quantity < quantity);
        if (goodses == null || goodses.Any() == false)
        {
            return "No fruit found";
        }
        else
        {
            return string.Join("\n", goodses);
        }
    }

    [KernelFunction]
    [Description("Buy fruit")]
    public string BuyGoods([Description("Fruit name")] string name, [Description("Buy quantity")] int quantity)
    {
        var goods = GoodsList.FirstOrDefault(g => g.Name == name);
        if (goods != null)
        {
            var newQuantity = goods.Quantity - quantity;
            if (newQuantity < 0)
            {
                return "Insufficient stock";
            }
            else
            {
                goods.Quantity = newQuantity;
                goods.BuyQuantity += quantity;
                Total += goods.Price * quantity;
                return "Purchase successful!";
            }
        }
        else
        {
            return "No fruit found";
        }
    }
}

public class Goods
{
    public Goods(string name, decimal price, int quantity)
    {
        Name = name;
        Price = price;
        Quantity = quantity;
    }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public int BuyQuantity { get; set; } = 0;

    public override string ToString()
    {
        return $"Name: {Name}, Price: {Price}, Quantity: {Quantity}, BuyQuantity: {BuyQuantity}";
    }
}