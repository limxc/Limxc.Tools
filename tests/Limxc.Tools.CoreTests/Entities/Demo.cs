using System;
using System.Collections.Generic;

namespace Limxc.Tools.CoreTests.Entities;

public class Demo
{
    public string StringValue { get; set; }
    public int IntValue { get; set; }
    public float FloatValue { get; set; }
    public bool BoolValue { get; set; }
    public DateTime Date { get; set; }
    public double DoubleValue { get; set; }

    //public Dictionary<string, string> Dict { get; set; }
    public string[] Strings { get; set; }
    public List<double> DoubleValues { get; set; }
}