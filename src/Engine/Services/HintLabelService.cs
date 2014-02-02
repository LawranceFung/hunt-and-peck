﻿using HuntAndPeck.Engine.Extensions;
using HuntAndPeck.Engine.Hints;
using HuntAndPeck.Engine.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HuntAndPeck.Engine.Services
{
    public class HintLabelService : IHintLabelService
    {
        public void LabelHints(IEnumerable<Hint> hints)
        {
            var labels = GetHintStrings(hints.Count());

            var i = 0;
            foreach (var hint in hints)
            {
                hint.Label = labels[i];
                ++i;
            }
        }

        /// <summary>
        /// Gets available hint strings
        /// </summary>
        /// <remarks>Adapted from vimium to give a consistent experience, see https://github.com/philc/vimium/blob/master/content_scripts/link_hints.coffee </remarks>
        /// <param name="hintCount">The number of hints</param>
        /// <returns>A list of hint strings</returns>
        private List<string> GetHintStrings(int hintCount)
        {
            var hintCharacters = new char[] { 's', 'a', 'd', 'f', 'j', 'k', 'l', 'e', 'w', 'c', 'm', 'p', 'g', 'h' };
            var digitsNeeded = (int)Math.Ceiling(Math.Log(hintCount) / Math.Log(hintCharacters.Length));

            var shortHintCount = Math.Floor((Math.Pow(hintCharacters.Length, digitsNeeded) - hintCount) / hintCharacters.Length);
            var longHintCount = hintCount - shortHintCount;

            var hintStrings = new List<string>();

            if (digitsNeeded > 1)
            {
                for (var i = 0; i < shortHintCount; ++i)
                {
                    hintStrings.Add(NumberToHintString(i, hintCharacters, digitsNeeded - 1));
                }
            }

            var start = (int)(shortHintCount * hintCharacters.Length);
            for (var i = start; i < (start + longHintCount); ++i)
            {
                hintStrings.Add(NumberToHintString(i, hintCharacters, digitsNeeded));
            }

            // Note that shuffle is lazy evaluated. Sigh.
            return hintStrings.Shuffle().ToList();
        }

        /// <summary>
        /// Converts a number like "8" into a hint string like "JK". This is used to sequentially generate all of the
        /// hint text. The hint string will be "padded with zeroes" to ensure its length is >= numHintDigits.
        /// </summary>
        /// <remarks>Adapted from vimium to give a consistent experience, see https://github.com/philc/vimium/blob/master/content_scripts/link_hints.coffee</remarks>
        /// <param name="number">The number</param>
        /// <param name="characterSet">The set of characters</param>
        /// <param name="noHintDigits">The number of hint digits</param>
        /// <returns>A hint string</returns>
        private string NumberToHintString(int number, char[] characterSet, int noHintDigits = 0)
        {
            var divisor = characterSet.Length;
            var hintString = new StringBuilder();
            var remainder = 0;

            do
            {
                remainder = number % divisor;
                hintString.Insert(0, characterSet[remainder]);
                number -= remainder;
                number /= (int)Math.Floor((double)divisor);
            } while (number > 0);

            // Pad the hint string we're returning so that it matches numHintDigits.
            // Note: the loop body changes hintString.length, so the original length must be cached!
            var length = hintString.Length;
            for (var i = 0; i < (noHintDigits - length); ++i)
            {
                hintString.Insert(0, characterSet[0]);
            }

            return hintString.ToString();
        }

        public IEnumerable<Hint> FindMatchingHints(string partialLabel, IEnumerable<Hint> hints)
        {
            if (string.IsNullOrWhiteSpace(partialLabel))
            {
                return hints;
            }

            return hints.Where(x => x.Label.StartsWith(partialLabel));
        }
    }
}