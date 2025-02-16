﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MonstersTheFramework
{
    public static class WeightedExtensions
    {
        public static T Choose<T>(this Weighted<T>[] choices, Random r = null)// where T : ICloneable
        {
            if (choices.Length == 0)
                return default;
            if (choices.Length == 1)
                return choices[0].Value;

            if (r == null)
                r = new Random();

            double sum = choices.Sum(choice => choice.Weight);

            double chosenWeight = r.NextDouble() * sum;
            foreach (var choice in choices)
            {
                if (chosenWeight < choice.Weight) // might need change to <=
                    return choice.Value;
                chosenWeight -= choice.Weight;
            }

            throw new Exception("This should never happen");
        }

        public static T Choose<T>(this List<Weighted<T>> choices, Random r = null)// where T : ICloneable
        {
            return choices.ToArray().Choose(r);
        }
    }
}
