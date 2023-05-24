﻿using System;
using System.Text.RegularExpressions;

void UseRegex(Regex regex) { }

Regex myRegex = new ("^[a-zA-Z]$"); // Compliant
myRegex = new ("^[a-zA-Z]$"); // Compliant
myRegex = myRegex ?? new Regex("^[a-zA-Z]$"); // Compliant
myRegex = true ? new Regex("^[a-zA-Z]$") : false ? new Regex("^[0-9]$") : new Regex("^[a-zA-Z0-9]$"); // Compliant

UseRegex(new Regex("^[a-zA-Z]$")); // Compliant

public class UseCachedRegex
{
    const string IMMUTABLE_REGEX_PATTERN = "^[a-zA-Z]$";
    readonly string READONLY_REGEX_PATTERN = "^[a-zA-Z]$";

    Regex MutableCachedRegex = new ("^[a-zA-Z]$"); // Compliant
    static Regex StaticMutableCachedRegex = new ("^[a-zA-Z]$"); // Compliant
    string mutableRegexPattern = "^[a-zA-Z]$";
    public Regex PropertyCachedRegex { get; set; } = new Regex(IMMUTABLE_REGEX_PATTERN); // Compliant

    void Compliant(string input)
    {
        DateTime _ = new (42); // Compliant

        Regex myRegex = new ($"^.+{input}.+$"); // Compliant
        myRegex = new ($"^.+" + input + ".+$"); // Compliant
        myRegex = new(mutableRegexPattern); // Compliant
        PropertyCachedRegex ??= new Regex("^[a-zA-Z]$"); // Compliant
        PropertyCachedRegex ??= PropertyCachedRegex ?? new Regex("^[a-zA-Z]$"); // Compliant
        MutableCachedRegex ??= new Regex("^[a-zA-Z]$"); // Compliant
        MutableCachedRegex ??= MutableCachedRegex ?? new Regex("^[a-zA-Z]$"); // Compliant
        StaticMutableCachedRegex ??= new Regex("^[a-zA-Z]$"); // Compliant
        StaticMutableCachedRegex ??= StaticMutableCachedRegex ?? new Regex("^[a-zA-Z]$"); // Compliant

        PropertyCachedRegex = PropertyCachedRegex is not null ? PropertyCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant
        MutableCachedRegex = MutableCachedRegex is not null ? MutableCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant
        StaticMutableCachedRegex = StaticMutableCachedRegex is not null ? StaticMutableCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant

        PropertyCachedRegex ??= PropertyCachedRegex == null ? new Regex("^[a-zA-Z]$") : PropertyCachedRegex; // Compliant
        PropertyCachedRegex ??= PropertyCachedRegex != null ? PropertyCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant
        PropertyCachedRegex ??= null == PropertyCachedRegex ? new Regex("^[a-zA-Z]$") : PropertyCachedRegex; // Compliant
        PropertyCachedRegex ??= null != PropertyCachedRegex ? PropertyCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant
        PropertyCachedRegex ??= PropertyCachedRegex is null ? new Regex("^[a-zA-Z]$") : PropertyCachedRegex; // Compliant
        PropertyCachedRegex ??= PropertyCachedRegex is not null ? PropertyCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant

        MutableCachedRegex ??= MutableCachedRegex == null ? new Regex("^[a-zA-Z]$") : MutableCachedRegex; // Compliant
        MutableCachedRegex ??= MutableCachedRegex != null ? MutableCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant
        MutableCachedRegex ??= null == MutableCachedRegex ? new Regex("^[a-zA-Z]$") : MutableCachedRegex; // Compliant
        MutableCachedRegex ??= null != MutableCachedRegex ? MutableCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant
        MutableCachedRegex ??= MutableCachedRegex is null ? new Regex("^[a-zA-Z]$") : MutableCachedRegex; // Compliant
        MutableCachedRegex ??= MutableCachedRegex is not null ? MutableCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant

        StaticMutableCachedRegex ??= StaticMutableCachedRegex == null ? new Regex("^[a-zA-Z]$") : StaticMutableCachedRegex; // Compliant
        StaticMutableCachedRegex ??= StaticMutableCachedRegex != null ? StaticMutableCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant
        StaticMutableCachedRegex ??= null == StaticMutableCachedRegex ? new Regex("^[a-zA-Z]$") : StaticMutableCachedRegex; // Compliant
        StaticMutableCachedRegex ??= null != StaticMutableCachedRegex ? StaticMutableCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant
        StaticMutableCachedRegex ??= StaticMutableCachedRegex is null ? new Regex("^[a-zA-Z]$") : StaticMutableCachedRegex; // Compliant
        StaticMutableCachedRegex ??= StaticMutableCachedRegex is not null ? StaticMutableCachedRegex : new Regex("^[a-zA-Z]$"); // Compliant
    }

    void Noncompliant()
    {
        Regex myRegex = new ("^[a-zA-Z]$"); // Noncompliant
        //              ^^^^^^^^^^^^^^^^^^

        myRegex = new ("^[a-zA-Z]$"); // Noncompliant
        //        ^^^^^^^^^^^^^^^^^^

        myRegex = new (IMMUTABLE_REGEX_PATTERN); // Noncompliant
        //        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        myRegex = new (READONLY_REGEX_PATTERN); // Noncompliant
        //        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        myRegex ??= new Regex("^[a-zA-Z]$"); // Noncompliant
        //          ^^^^^^^^^^^^^^^^^^^^^^^
        myRegex ??= myRegex ?? new Regex("^[a-zA-Z]$"); // Noncompliant
        //                     ^^^^^^^^^^^^^^^^^^^^^^^

        MutableCachedRegex = new ("^[a-zA-Z]$"); // Noncompliant
        StaticMutableCachedRegex = new ("^[a-zA-Z]$"); // Noncompliant
        PropertyCachedRegex = new ("^[a-zA-Z]$"); // Noncompliant

        PropertyCachedRegex = PropertyCachedRegex is not null ? new Regex("^[a-zA-Z]$") : PropertyCachedRegex; // Noncompliant
        MutableCachedRegex = MutableCachedRegex is not null ? new Regex("^[a-zA-Z]$") : MutableCachedRegex; // Noncompliant
        StaticMutableCachedRegex = StaticMutableCachedRegex is not null ? new Regex("^[a-zA-Z]$") : StaticMutableCachedRegex; // Noncompliant

        PropertyCachedRegex ??= PropertyCachedRegex != null ? new Regex("^[a-zA-Z]$") : PropertyCachedRegex; // Noncompliant
        PropertyCachedRegex ??= PropertyCachedRegex == null ? PropertyCachedRegex : new Regex("^[a-zA-Z]$"); // Noncompliant
        PropertyCachedRegex ??= null != PropertyCachedRegex ? new Regex("^[a-zA-Z]$") : PropertyCachedRegex; // Noncompliant
        PropertyCachedRegex ??= null == PropertyCachedRegex ? PropertyCachedRegex : new Regex("^[a-zA-Z]$"); // Noncompliant
        PropertyCachedRegex ??= PropertyCachedRegex is null ? PropertyCachedRegex : new Regex("^[a-zA-Z]$"); // Noncompliant
        PropertyCachedRegex ??= PropertyCachedRegex is not null ? new Regex("^[a-zA-Z]$") : PropertyCachedRegex; // Noncompliant

        MutableCachedRegex ??= MutableCachedRegex != null ? new Regex("^[a-zA-Z]$") : MutableCachedRegex; // Noncompliant
        MutableCachedRegex ??= MutableCachedRegex == null ? MutableCachedRegex : new Regex("^[a-zA-Z]$"); // Noncompliant
        MutableCachedRegex ??= null != MutableCachedRegex ? new Regex("^[a-zA-Z]$") : MutableCachedRegex; // Noncompliant
        MutableCachedRegex ??= null == MutableCachedRegex ? MutableCachedRegex : new Regex("^[a-zA-Z]$"); // Noncompliant
        MutableCachedRegex ??= MutableCachedRegex is null ? MutableCachedRegex : new Regex("^[a-zA-Z]$"); // Noncompliant
        MutableCachedRegex ??= MutableCachedRegex is not null ? new Regex("^[a-zA-Z]$") : MutableCachedRegex; // Noncompliant

        StaticMutableCachedRegex ??= StaticMutableCachedRegex != null ? new Regex("^[a-zA-Z]$") : StaticMutableCachedRegex; // Noncompliant
        StaticMutableCachedRegex ??= StaticMutableCachedRegex == null ? StaticMutableCachedRegex : new Regex("^[a-zA-Z]$"); // Noncompliant
        StaticMutableCachedRegex ??= null != StaticMutableCachedRegex ? new Regex("^[a-zA-Z]$") : StaticMutableCachedRegex; // Noncompliant
        StaticMutableCachedRegex ??= null == StaticMutableCachedRegex ? StaticMutableCachedRegex : new Regex("^[a-zA-Z]$"); // Noncompliant
        StaticMutableCachedRegex ??= StaticMutableCachedRegex is null ? StaticMutableCachedRegex : new Regex("^[a-zA-Z]$"); // Noncompliant
        StaticMutableCachedRegex ??= StaticMutableCachedRegex is not null ? new Regex("^[a-zA-Z]$") : StaticMutableCachedRegex; // Noncompliant

        if (MutableCachedRegex is not null)
        {
            MutableCachedRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
        }

        if (StaticMutableCachedRegex is not null)
        {
            StaticMutableCachedRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
        }

        if (PropertyCachedRegex is not null)
        {
            PropertyCachedRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
        }

        if (PropertyCachedRegex is { Options: RegexOptions.Compiled } )
        {
            PropertyCachedRegex = new Regex("^[a-zA-Z]$"); // Noncompliant
        }

        UseRegex(new ("^[a-zA-Z]$")); // Noncompliant
        //       ^^^^^^^^^^^^^^^^^^

        void UseRegex(Regex regex) { }
    }
}

// Live example https://github.com/dotnet/runtime/blob/main/src/libraries/System.Data.OleDb/src/DbConnectionOptions.cs#L62-L71
public partial class CompiledRegex
{
#if NET7_0_OR_GREATER
    [GeneratedRegex("^[a-zA-Z]$")]
    private static partial Regex CreateCachedRegex();
#else
    private static Regex CreateCachedRegex() => new Regex("^[a-zA-Z]$", RegexOptions.Compiled); // Noncompliant Potential FP as it is a shim for [GenerateRegex] attribute prior to .NET 7.0
#endif
}
