﻿using System;
using System.Collections;

public class Params
{
    public void method(int i, int k = 5, params int[] rest)
    {
    }

    public void call()
    {
        int i = 0, j = 5, k = 6, l = 7;
        method(i, j, k, l);
    }
    public void call2()
    {
        int i = 0, j = 5, rest = 6, l = 7;
        var k = new[] { i, l };
        method(i, k: rest, rest: k); // Compliant, code will not compile if suggestion is applied
    }
}

public static class Other
{
    public static void call()
    {
        var a = "1";
        var b = "2";

        Comparer.Default.Compare(b, a); // Noncompliant
    }
}

public static class Extensions
{
    public static void Ex(this string self, string v1, string v2)
    //                 ^^ Secondary [6]
    {
    }
    public static void Ex(this string self, string v1, string v2, int x)
    {
        Ex(self, v1, v2);
        self.Ex(v1, v2);
        Extensions.Ex(self, v1, v2);
    }
}

public partial class ParametersCorrectOrder
{
    partial void divide(int divisor, int someOther, int dividend, int p = 10, int some = 5, int other2 = 7);
    //           ^^^^^^ Secondary [1,2,3,4]
}

public partial class ParametersCorrectOrder
{
    partial void divide(int a, int b, int c, int p, int other, int other2)
    {
        var x = a / b;
    }

    public void m(int a, int b) // Secondary [5,7]
    {
    }

    public void n(int a, int b, int c)
    {
    }

    public void doTheThing()
    {
        int divisor = 15;
        int dividend = 5;
        var something = 6;
        var someOther = 6;
        var other2 = 6;
        var some = 6;

        divide(dividend, 1 + 1, divisor, other2: 6);  // Noncompliant [1] operation succeeds, but result is unexpected

        divide(divisor, other2, dividend);
        divide(divisor, other2, dividend, other2: someOther); // Noncompliant [2] {{Parameters to 'divide' have the same names but not the same order as the method arguments.}}

        divide(divisor, someOther, dividend, other2: some, some: other2); // Noncompliant [3]

        divide(1, 1, 1, other2: some, some: other2); // Noncompliant [4]
        divide(1, 1, 1, other2: 1, some: other2);

        int a = 5, b = 6;

        m(1, a); // Noncompliant [7] Argument "a" may be first
        m(1, b);
        m(a, a);
        m(divisor, dividend);

        m(a, b);
        m(b, b); // Compliant
        m(b, a); // Noncompliant [5]

        var v1 = "";
        var v2 = "";

        "aaaaa".Ex(v1, v2);
        "aaaaa".Ex(v2, v1); // Noncompliant [6]

        int c = 1;
        n(a, a, a);
        n(b, b, b);
        n(c, c, c);
    }
}

public class A
{
    public class B
    {
        public class C
        {
            public C(string left, string right) { } // Secondary [C]
        }
    }
}

public class Foo
{
    public Foo(string left, string right) { } // Secondary [B]

    public void Method(string left, string right) { } // Secondary [A]

    public void Bar()
    {
        var left = "valueLeft";
        var right = "valueRight";

        Method(left, right);
        Method(right, left); // Noncompliant [A]

        var foo1 = new Foo(left, right);
        var foo2 = new Foo(right, left); // Noncompliant [B]

        var c1 = new A.B.C(left, right);
        var c2 = new A.B.C(right, left); // Noncompliant [C]
    }
}

class Program
{
    void Struct(DateTime a, string b)
    {
        Bar1(a, b); // Compliant
    }

    void ClassAndInterface(Boo a, string b)
    {
        Bar2(a, b); // Compliant
        Bar3(a, b); // Compliant
        Bar3(b: a, a: b); // Compliant
    }
    void Bar1(DateTime b, string a) { }
    void Bar2(Boo b, string a) { }
    void Bar3(IBoo b, string a) { }
}
interface IBoo { }
class Boo : IBoo { }

class WithLocalFunctions
{
    public void M1()
    {
        static double divide(int divisor, int dividend)
        {
            return divisor / dividend;
        }

        static void doTheThing(int divisor, int dividend)
        {
            double result = divide(dividend, divisor);  // Noncompliant
        }
    }

    public void M2()
    {
        double divide(int divisor, int dividend)
        {
            return divisor / dividend;
        }

        void doTheThing(int divisor, int dividend)
        {
            double result = divide(dividend, divisor);  // Noncompliant
        }
    }

    public void M3()
    {
        double divide(int divisor, int dividend) => divisor / dividend;

        double doTheThing(int divisor, int dividend) => divide(dividend, divisor);  // Noncompliant
    }
}

// See https://github.com/SonarSource/sonar-dotnet/issues/3879
class NotOnlyNullableParam
{
    internal class A
    {
        public C Something { get; set; }

        public WithValueProperty b { get; set; }
    }

    internal class C
    {
        public int a { get; set; }
        public int b { get; set; }
    }

    internal class WithValueProperty
    {
        public int Value { get; set; }
    }

    void NotNullableParamVoid(int a, int b) // Secondary [D,E,F,G,H,I]
    {
        // Do nothing
    }

    void NullableParamValueVoid(int q, Nullable<int> b)
    {
        if (b.HasValue)
        {
            NotNullableParamVoid(b.Value, q); // Noncompliant [D]
            NotNullableParamVoid(q, b.Value); // Compliant
        }
    }

    void NullableParamCastVoid(int q, int? b)
    {
        if (b.HasValue)
        {
            NotNullableParamVoid((int)b, q); // Noncompliant [E]
            NotNullableParamVoid(q, (int)b); // Compliant
        }
    }

    void InnerPropertyParamVoid(int q, A c, A b)
    {
        NotNullableParamVoid(c.Something.b, q); // Noncompliant [F]
        NotNullableParamVoid(q, c.Something.b); // Compliant
        NotNullableParamVoid(b.Something.a, q); // Compliant
    }

    void InnerPropertyValueParamVoid(int q, A c)
    {
        NotNullableParamVoid(c.b.Value, q); // Compliant Value is real property
    }

    void InnerPropertyValueParamVoid(int c, WithValueProperty b)
    {
        NotNullableParamVoid(b.Value, c); // Compliant Value is real property
        NotNullableParamVoid(c, b.Value); // Compliant Value is real property
    }

    void ObjectParamCastVoid(int q, object b)
    {
        NotNullableParamVoid((int)b, q); // Noncompliant [G]
        NotNullableParamVoid(q, (int)b); // Compliant
    }

    void ObjectParamNullableCastVoid(int q, object b)
    {
        NotNullableParamVoid(((int?)b).Value, q); // Noncompliant [H]
        NotNullableParamVoid(q, ((int?)b).Value); // Compliant
    }

    void DifferentCaseParamsVoid(int a, int B)
    {
        NotNullableParamVoid(B, a); // Noncompliant [I]
        NotNullableParamVoid(a, B); // Compliant
    }
}
