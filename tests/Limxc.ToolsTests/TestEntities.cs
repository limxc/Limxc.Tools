﻿using System.Collections.Generic;

namespace Limxc.ToolsTests;

public enum Gender
{
    Male,
    Female
}

public class TestEntity
{
    public Gender EnumValue { get; set; }
    public string StrValue { get; set; }
    public bool? BoolValue { get; set; }
    public int IntValue { get; set; }
    public float FloatValue { get; set; }
    public double? DoubleValue { get; set; }
    public decimal DecimalValue { get; set; }
}

public class ComplexTestEntity : TestEntity
{
    public TestEntity TestEntity { get; set; }
    public List<TestEntity> Inners { get; set; }
    public TestEntity[] InnerArrs { get; set; }
}