using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using System.IO;

var data = (await File.ReadAllLinesAsync("data.txt")).ToList();
char[] mask = new char[0];
var instructions = data
    .Select(line =>
    {
        if (line.StartsWith("mask"))
        {
            return new Instruction(InstructionType.Mask, BuildMask(line), null);
        }

        return new Instruction(InstructionType.Memory, null, new MemoryInstruction(line));
    });

var memory = new Dictionary<long, long>();

foreach (var instruction in instructions)
{
    if (instruction.type == InstructionType.Mask)
    {
        mask = instruction.mask;
    }
    else
    {
        var addressesToSet = ApplyMask(instruction.memoryInstruction.Location);
        foreach (var addressToSet in addressesToSet)
        {
            memory[addressToSet] = instruction.memoryInstruction.InitialValue;
        }
    }
}

// foreach (var item in memory)
// {
//     Console.WriteLine($"Memory {item.Key}");
//     Console.WriteLine($"\t{item.Value.ToBits()} (decimal {item.Value})");
// }

var answer = memory.Aggregate(0L, (curr, next) => curr += next.Value);
Console.WriteLine($"Answer is {answer}");

char[] BuildMask(string line) => line.Split("=")[1].Trim().ToArray();

List<long> ApplyMask(long value)
{
    var bitValue = value.ToBits().ToArray();
    var results = new List<char[]> { new char[bitValue.Length] };

    for (var index = 0; index < bitValue.Length; index++)
    {
        var bit = bitValue[index];
        var maskBit = mask[index];

        switch (maskBit)
        {
            case '0':
                {
                    foreach(var result in results) result[index] = bit;
                    break;
                }
            case '1':
                {
                    foreach(var result in results) result[index] = '1';
                    break;
                }
            case 'X':
                {
                    var newVersions = new List<char[]>();
                    foreach(var result in results)
                    {
                        var tmp = (char[])result.Clone();
                        result[index] = '1';
                        tmp[index] = '0';
                        newVersions.Add(tmp);
                    }

                    results.AddRange(newVersions);
                    break;
                }
        }
    }

    return results.Select(c => new String(c)).Select(r => Convert.ToInt64(r, 2)).ToList();
}

public class MemoryInstruction
{
    public int Location { get; init; }
    public long InitialValue { get; init; }

    private static Regex parser = new Regex("mem\\[(?<address>\\d+)] = (?<value>\\d+)");

    public MemoryInstruction(string input)
    {
        var matches = parser.Matches(input);
        Location = Convert.ToInt32(matches[0].Groups["address"].Value);
        InitialValue = Convert.ToInt32(matches[0].Groups["value"].Value);
    }
}

public record Instruction(InstructionType type, char[] mask, MemoryInstruction? memoryInstruction);

public enum InstructionType
{
    Memory,
    Mask,
}

public static class Extensions
{
    public static string ToBits(this long value) => Convert.ToString(value, 2).PadLeft(36, '0');
}