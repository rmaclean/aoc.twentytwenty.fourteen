using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using System.IO;

var data = (await File.ReadAllLinesAsync("data.txt")).ToList();
char[] mask = new char[0];
var instructions = data
    .Select(line => {
        if (line.StartsWith("mask"))
        {
            return new Instruction(InstructionType.Mask, BuildMask(line), null);
        }

        return new Instruction(InstructionType.Memory, null, new MemoryInstruction(line));
    });

var memory = new Dictionary<int, long>();

foreach (var instruction in instructions)
{
    if (instruction.type == InstructionType.Mask)
    {
        mask = instruction.mask;
    }
    else
    {
        memory[instruction.memoryInstruction.Location] = Convert.ToInt64(ApplyMask(instruction.memoryInstruction.InitialValue), 2);
    }
}

var answer = memory.Aggregate(0L, (curr, next) => curr += next.Value);
Console.WriteLine($"Answer is {answer}");

char[] BuildMask(string line) => line.Split("=")[1].Trim().ToArray();

string ApplyMask(long value)
{
    var bitValue = value.ToBits().ToArray();
    var result = new char[bitValue.Length];
    for (var index = 0; index < bitValue.Length; index++)
    {
        var bit = bitValue[index];
        var maskBit = mask[index];

        if (maskBit == 'X')
        {
            result[index] = bit;
        } 
        else
        {
            result[index] = maskBit;
        }
    }

    return new string(result);
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