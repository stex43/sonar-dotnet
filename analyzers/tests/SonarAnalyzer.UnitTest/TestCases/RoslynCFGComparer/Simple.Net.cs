using System;

public class Sample
{
    public void Index(string[] arr)
    {
        var value = arr[^1];
    }

    public void Range(string[] arr)
    {
        var value = arr[1..4];
    }
}
