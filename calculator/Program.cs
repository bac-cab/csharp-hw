void Advance(ref int pos)
{
    pos += 1;
}

char? Peek(string expr, ref int pos)
{
    if (pos >= expr.Length)
    {
        return null;
    }
    char result = expr[pos];
    return result;
}

double? ReadInt(string expr, ref int pos)
{
    double? number = null;
    char? c;
    while ((c = Peek(expr, ref pos)) != null)
    {
        if ('0' <= c && c <= '9')
        {
            Advance(ref pos);
            number ??= 0;
            number *= 10;
            number += (char)c - '0';
        }
        else
        {
            return number;
        }
    }
    return number;
}

double? ReadTopLevel(string expr, ref int pos)
{
    double? number = ReadInt(expr, ref pos);
    if (number != null)
    {
        return number;
    }
    if (Peek(expr, ref pos) == '(')
    {
        Advance(ref pos);
        double? value = ReadAddExpression(expr, ref pos, true);
        if (Peek(expr, ref pos) != ')')
        {
            return null;
        }
        Advance(ref pos);
        return value;
    }
    return null;
}

double? ReadMulExpression(string expr, ref int pos)
{
    double? number = ReadTopLevel(expr, ref pos);
    if (number == null)
    {
        return null;
    }
    char? c = Peek(expr, ref pos);
    if (c == '*' || c == '/')
    {
        Advance(ref pos);
        double? other = ReadMulExpression(expr, ref pos);
        if (other == null)
        {
            return null;
        }
        return c == '*' ? number * other : number / other;
    }
    return number;
}

double? ReadAddExpression(string expr, ref int pos, bool first)
{
    bool negative = false;
    if (first && Peek(expr, ref pos) == '-')
    {
        Advance(ref pos);
        negative = true;
    }
    double? mul = ReadMulExpression(expr, ref pos);
    if (mul == null)
    {
        return null;
    }
    if (negative)
    {
        mul = -mul;
    }
    char? c = Peek(expr, ref pos);
    if (c == '+' || c == '-')
    {
        Advance(ref pos);
        double? other = ReadAddExpression(expr, ref pos, false);
        if (other == null)
        {
            return null;
        }
        return c == '+' ? mul + other : mul - other;
    }
    return mul;
}

Console.WriteLine("Hello! This is a CLI calculator. Enter an expression with integers, '+', '-', '*' or '/' in one line and it will be calculated. Enter $ to stop.");

while (true)
{
    string? input = Console.ReadLine();
    if (input == null)
    {
        break;
    }
    if (input.Contains('$'))
    {
        Console.WriteLine("Goodbye!");
        break;
    }
    string noSpaces = input.Replace(" ", string.Empty);

    int pos = 0;
    double? result = ReadAddExpression(noSpaces, ref pos, true);
    if (result is null || pos < noSpaces.Length)
    {
        Console.WriteLine("Failed to parse");
    }
    else
    {
        Console.WriteLine(result);
    }
}