using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

public interface ISerializable
{
    string Serialize();
}

public interface IValidatable
{
    bool IsValid();
    List<string> GetErrors();
}

public interface ILoggable
{
    void Log(string action);
    List<string> GetLogHistory();
}

public interface IPersistable : ISerializable, IValidatable
{
    void Save();
    void Load(string data);
}

public class User : IPersistable, ILoggable
{
    private List<string> _logHistory = new List<string>();

    public string Username { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }

    public User(string username, string email, int age)
    {
        Username = username;
        Email = email;
        Age = age;
        Log("Створено користувача");
    }

    public string Serialize() =>
        $"{{\"username\":\"{Username}\", \"email\":\"{Email}\", \"age\":{Age}}}";

    public bool IsValid() => GetErrors().Count == 0;

    public List<string> GetErrors()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(Username))
            errors.Add("Username не може бути порожнім");
        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains("@"))
            errors.Add("Email повинен містити '@'");
        if (Age < 18)
            errors.Add("Вік має бути >= 18");
        return errors;
    }

    public void Save()
    {
        if (IsValid())
        {
            Console.WriteLine($"Збережено: {Serialize()}");
            Log("Дані успішно збережено");
        }
        else
        {
            Console.WriteLine($"Помилка збереження ({Username}): {string.Join("; ", GetErrors())}");
            Log("Невдала спроба збереження (помилка валідації)");
        }
    }

    public void Load(string data)
    {
        var mUser = Regex.Match(data, "\"username\"\\s*:\\s*\"([^\"]*)\"");
        var mEmail = Regex.Match(data, "\"email\"\\s*:\\s*\"([^\"]*)\"");
        var mAge = Regex.Match(data, "\"age\"\\s*:\\s*(\\d+)");

        if (mUser.Success) Username = mUser.Groups[1].Value;
        if (mEmail.Success) Email = mEmail.Groups[1].Value;
        if (mAge.Success) Age = int.Parse(mAge.Groups[1].Value);

        Log("Дані завантажено з рядка");
    }

    public void Log(string action) =>
        _logHistory.Add($"[{DateTime.Now:HH:mm:ss}] {action}");

    public List<string> GetLogHistory() => new List<string>(_logHistory);

    public override string ToString() =>
        $"User: {Username}, Email: {Email}, Age: {Age}";
}

public class Order : IPersistable, ILoggable
{
    private List<string> _logHistory = new List<string>();

    public int OrderId { get; set; }
    public string Product { get; set; }
    public decimal Amount { get; set; }

    public Order(int orderId, string product, decimal amount)
    {
        OrderId = orderId;
        Product = product;
        Amount = amount;
        Log($"Створено замовлення #{OrderId}");
    }

    public string Serialize() =>
        $"{{\"orderId\":{OrderId}, \"product\":\"{Product}\", \"amount\":{Amount.ToString(CultureInfo.InvariantCulture)}}}";

    public bool IsValid() => GetErrors().Count == 0;

    public List<string> GetErrors()
    {
        var errors = new List<string>();
        if (OrderId <= 0)
            errors.Add("OrderId має бути > 0");
        if (string.IsNullOrWhiteSpace(Product))
            errors.Add("Назва товару не може бути порожньою");
        if (Amount <= 0)
            errors.Add("Сума замовлення має бути > 0");
        return errors;
    }

    public void Save()
    {
        if (IsValid())
        {
            Console.WriteLine($"Збережено: {Serialize()}");
            Log("Замовлення збережено");
        }
        else
        {
            Console.WriteLine($"Помилка збереження замовлення #{OrderId}: {string.Join("; ", GetErrors())}");
            Log("Помилка збереження замовлення");
        }
    }

    public void Load(string data)
    {
        var mId = Regex.Match(data, "\"orderId\"\\s*:\\s*(\\d+)");
        var mProd = Regex.Match(data, "\"product\"\\s*:\\s*\"([^\"]*)\"");
        var mAmt = Regex.Match(data, "\"amount\"\\s*:\\s*([\\d.]+)");

        if (mId.Success) OrderId = int.Parse(mId.Groups[1].Value);
        if (mProd.Success) Product = mProd.Groups[1].Value;
        if (mAmt.Success) Amount = decimal.Parse(mAmt.Groups[1].Value, CultureInfo.InvariantCulture);

        Log("Замовлення завантажено з рядка");
    }

    public void Log(string action) =>
        _logHistory.Add($"[{DateTime.Now:HH:mm:ss}] {action}");

    public List<string> GetLogHistory() => new List<string>(_logHistory);

    public override string ToString() =>
        $"Order #{OrderId}: {Product} ({Amount} грн)";
}

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        User user1 = new User("admin", "admin@example.com", 25);
        User user2 = new User("", "invalid-email", 15);
        Order order = new Order(1, "Ноутбук", 25000m);

        Console.WriteLine(" Валідація ");
        IValidatable[] validatables = { user1, user2, order };
        foreach (var v in validatables)
        {
            Console.WriteLine($"{v}: Valid = {v.IsValid()}");
            if (!v.IsValid())
            {
                foreach (string err in v.GetErrors())
                    Console.WriteLine($"  Помилка: {err}");
            }
        }

        Console.WriteLine("\n Серіалізація ");
        ISerializable[] serializables = { user1, order };
        foreach (var s in serializables)
            Console.WriteLine(s.Serialize());

        Console.WriteLine("\n Збереження та завантаження ");
        IPersistable[] persistables = { user1, user2, order };
        foreach (var p in persistables)
            p.Save();

        
        string savedJson = user1.Serialize();
        user2.Load(savedJson);
        Console.WriteLine($"Після Load у user2: {user2} (Valid = {user2.IsValid()})");

        Console.WriteLine("\n Історія логів ");
        user1.Log("Спроба збереження");
        foreach (string log in user1.GetLogHistory())
            Console.WriteLine($"  {log}");
    }
}