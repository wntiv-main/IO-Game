using System;
using System.Collections.Generic;

namespace IO_Game
{
	// Class for numbers representing mass up to yotta-tonnes
	public class Heavy
	{
		const byte SHORT = 0;
		const byte LONG = 1;
		const byte FULL = 2;
		private const int units = 11;
		// Like Number.toFixed(), but optimised for no trailing zeroes
		static double FixMe(double num)
		{
			if (num < 0)
			{
				return Math.Ceiling(num * 100) / 100;
			}
			else
			{
				return Math.Floor(num * 100) / 100;
			}
		}
		// Does this number have friends? (Should the following word be plural?)
		static bool HasFriends(double num)
		{
			return num != 1;
		}
		// This is an array that defines all of the long names for the units of weight.
		static readonly string[] namesPerKilo = new string[units] { "gram", "kilogram", "tonne", "kilotonne", "megatonne", "gigatonne", "teratonne", "petatonne", "exatonne", "zettatonne", "yottatonne" };
		// This is an array that defines all of the symbol names for the units of weight.
		static readonly string[] shNamesPerKilo = new string[units] { "g", "kg", "T", "kT", "MT", "GT", "TT", "PT", "ET", "ZT", "YT" };
		// This array stores all of the "digits" in the base-1000 number
		private List<int> baseKDigits = new List<int>(units) { 0 };
		// Function to convert this number to a human-readable string
		public string Readable(byte format)
		{
			if (format == FULL)
			{
				baseKDigits.Reverse();
				string value = string.Join(",", baseKDigits);
				baseKDigits.Reverse();
				return value;
			}
			// digits is being set to how many "digits" there are.
			int digits = baseKDigits.Count - 1;
			// number is being set to the apropriate unit of weight, along with 3 decimal places from the former unit of weight,
			double number = FixMe(baseKDigits[digits] + (digits > 0 ? baseKDigits[digits - 1] / 1000 : 0));
			/*
			 * This is returning the amount of mass there is, along with what unit it is.
			 * It is using the short-hand way of writing weight by using the array shNamesPerKilo[].
			 */
			if (format == SHORT) return number + shNamesPerKilo[digits];
			/*
			 * This is returning the amount of mass there is, along with what unit it is.
			 * It is returning the full name of the weight unit by using the array namesPerKilo[].
			 */
			if (format == LONG) return number + " " + namesPerKilo[digits] + (HasFriends(number) ? "s" : "");
			throw new Exception("format is not a valid Heavy.(FORMAT)");
		}
		// This function adds a number or class Heavy to the current class Heavy.
		public void Add(int num)
		{
			// If the number is an actual number...
			// Then turn it into a class Heavy (cuz im lazy)
			Heavy n = new Heavy();
			// Set it to the right value
			n.Set(num);
			// And then call add on the current class
			Add(n);
		}
		public void Add(Heavy num)
		{
			// If the number is a class Heavy...
			// Which "digits" need to be carried over into the next "digit"
			List<int> carrying = new List<int>(units) { 0 };
			// Loop through all the "digits"...


			for (var i = 0; i < baseKDigits.Count && i < num.baseKDigits.Count; i++)
			{
				// ...And add each of the "digits" as well as the carry number
				baseKDigits[i] = baseKDigits[i] + num.baseKDigits[i] + carrying[i];
				// Do we need to carry the number into the next "digit"?
				if (baseKDigits[i] >= 1000 && !(i == namesPerKilo.Length - 1))
				{
					// Bcuz if so we add the amount needing to be carried to the carrying array
					carrying.Add((int)Math.Floor((double)baseKDigits[i] / 1000));
					// And then remove it from this digit
					baseKDigits[i] %= 1000;
				}
				// Same thing, but for negative numbers that we come across whilst subtracting
				else if (baseKDigits[i] < 0 && !(i == 0))
				{
					carrying.Add(0);
					while (baseKDigits[i] < 0)
					{
						baseKDigits[i] += 1000;
						carrying[i + 1] -= 1;
					}
				}
				else
				{
					carrying.Add(0);
				}
			}
			if (num.baseKDigits.Count > baseKDigits.Count)
			{
				for (var i = baseKDigits.Count; i < num.baseKDigits.Count; i++)
				{
					baseKDigits.Add(num.baseKDigits[i]);
				}
			}
			for (var i = baseKDigits.Count - 1; i > 0; i--)
			{
				if (baseKDigits[i] == 0)
				{
					baseKDigits.RemoveAt(i);
				}
				else break;
			}
		}
		// Sets the value of the current class Heavy to a number or another class Heavy
		public void Set(int num)
		{
			// Is num a number?
			// Again, reset the current class Heavy
			baseKDigits.Clear();
			// And split the number every three digits using modulo and division
			for (var i = 0; i < Math.Ceiling((Math.Floor(Math.Log10(Math.Abs(num))) + 1) / 3.0); i++)
			{
				if (num < 0)
				{
					baseKDigits.Add((int)(Math.Ceiling(num / Math.Pow(1000, i)) % 1000));
				}
				else
				{
					baseKDigits.Add((int)(Math.Floor(num / Math.Pow(1000, i)) % 1000));
				}
			}
		}
		public void Set(Heavy num)
		{
			// Is num heavy?
			// Delete what we already have
			baseKDigits.Clear();
			// And copy over the values from num
			for (var i = 0; i < num.baseKDigits.Count; i++)
			{
				baseKDigits.Add(num.baseKDigits[i]);
			}
		}
		public void Negate()
		{
			// Make every digit negative as that will work
			for (var i = 0; i < baseKDigits.Count; i++)
			{
				baseKDigits[i] = -baseKDigits[i];
			}
		}
		public void Subtract(int num)
		{
			// Is num a number?
			// Just add negative
			Add(-num);
		}
		public void Subtract(Heavy num)
		{
			// Is num heavy?
			// Make it negative
			num.Negate();
			// And then add it
			Add(num);
			// Cover our tracks
			num.Negate();
		}
		public string Compare(Heavy num)
        {
			//this <|=|> num
			if (num.baseKDigits[0] < 0 && baseKDigits[0] >= 0) return "<";
			if (baseKDigits[0] < 0 && num.baseKDigits[0] >= 0) return ">";
            if (baseKDigits[0] < 0)
            {
				if (baseKDigits.Count > num.baseKDigits.Count) return "<";
				if (baseKDigits.Count < num.baseKDigits.Count) return ">";
			}
			else
            {
				if (baseKDigits.Count > num.baseKDigits.Count) return ">";
				if (baseKDigits.Count < num.baseKDigits.Count) return "<";
			}
			for(var i = baseKDigits.Count - 1; i >= 0; i++)
            {
				if (baseKDigits[i] > num.baseKDigits[i]) return ">";
				if (baseKDigits[i] < num.baseKDigits[i]) return "<";
            }
			return "=";
		}
	}
}
