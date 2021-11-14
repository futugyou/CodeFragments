using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Sharprompt;

namespace CodeFragments;

public class CommandDemo
{
    public static void Exection()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Prompt.ColorSchema.Select = ConsoleColor.DarkCyan;
        Prompt.ColorSchema.Answer = ConsoleColor.DarkRed;
        Prompt.ColorSchema.Select = ConsoleColor.DarkCyan;


        var type = Prompt.Select("数据库类型", new[] { "Oracle", "SQL Server", "MySQL", "PostgreSQL", "MariaDB" }, defaultValue: "MySQL", pageSize: 3);

        var server = Prompt.Input<string>("服务地址");

        Prompt.ColorSchema.Answer = ConsoleColor.DarkRed;

        var name = Prompt.Input<string>("用户名");

        var password = Prompt.Password("密码", "*");

        Console.WriteLine($"你输入的是 {type} {server} {name} {password}");

        var confirm = Prompt.Confirm("继续吗");
        Console.WriteLine($"你的选择是 {confirm}!");

        var enumValue = Prompt.Select<MyEnum>("Select enum value");
        Console.WriteLine($"You selected {enumValue}");

        var cities = Prompt.MultiSelect("Which cities would you like to visit?", new[] { "Seattle", "London", "Tokyo", "New York", "Singapore", "Shanghai" }, pageSize: 3);
        Console.WriteLine($"You picked {string.Join(", ", cities)}");

        var listValue = Prompt.List<string>("Please add item(s)");
        Console.WriteLine($"You picked {string.Join(", ", listValue)}");

        var result = Prompt.AutoForms<MyFormModel>();
        Console.WriteLine($"You picked {result.Name}");

        Prompt.Symbols.Prompt = new Symbol("🤔", "?");
        Prompt.Symbols.Done = new Symbol("😎", "V");
        Prompt.Symbols.Error = new Symbol("😱", ">>");

        var symbolsName = Prompt.Input<string>("What's your name?");
        Console.WriteLine($"Hello, {symbolsName}!");
    }

    public enum MyEnum
    {
        one,
        two,
        three,
    }
    public class MyFormModel
    {
        [Display(Prompt = "What's your name?")]
        [Required]
        public string Name { get; set; }

        [Display(Prompt = "Type new password")]
        [DataType(DataType.Password)]
        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Display(Prompt = "Are you ready?")]
        public bool Ready { get; set; }
    }
}