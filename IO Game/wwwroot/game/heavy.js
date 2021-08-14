// Class for numbers representing mass up to yotta-tonnes
class Heavy {
	static SHORT = 0;
	static LONG = 1;
	static FULL = 2;
	// Like Number.toFixed(), but optimised for no trailing zeroes
	static fixMe(num) {
		if (num < 0) {
			return Math.ceil(num * 100) / 100;
		} else {
			return Math.floor(num * 100) / 100;
		}
	}
	// Does this number have friends? (Should the following word be plural?)
	static hasFriends(num) {
		return num != 1;
	}
	// This is an array that defines all of the long names for the units of weight.
	static namesPerKilo = ["gram", "kilogram", "tonne", "kilotonne", "megatonne", "gigatonne", "teratonne", "petatonne", "exatonne", "zettatonne", "yottatonne"];
	// This is an array that defines all of the symbol names for the units of weight.
	static shNamesPerKilo = ["g", "kg", "T", "kT", "MT", "GT", "TT", "PT", "ET", "ZT", "YT"];
	// This array stores all of the "digits" in the base-1000 number
	baseKDigits = [0];
	// Function to convert this number to a human-readable string
	readable(format) {
		if (format == this.FULL) {
			this.baseKDigits.reverse()
			let value = this.baseKDigits.join(",");
			this.baseKDigits.reverse();
			return value;
		}
		// digits is being set to how many sets of 3 digits there are.
		let digits = this.baseKDigits.length - 1;
		// number is being set to the apropriate unit of weight, along with 3 decimal places from the former unit of weight,
		let number = Heavy.fixMe(this.baseKDigits[digits] + (digits > 0 ? this.baseKDigits[digits - 1] / 1000 : 0));
		/*
		 * This is returning the amount of mass there is, along with what unit it is.
		 * It is using the short-hand way of writing weight by using the array shNamesPerKilo[].
		 */
		if (format == this.SHORT) return number + this.shNamesPerKilo[digits];
		/*
		 * This is returning the amount of mass there is, along with what unit it is.
		 * It is returning the full name of the weight unit by using the array namesPerKilo[].
		 */
		if (format == this.LONG) return number + " " + this.namesPerKilo[digits] + (Heavy.hasFriends(number) ? "s" : "");
		throw new Error("format is not a valid Heavy.(FORMAT)");
	}
	// This function adds a number or class Heavy to the current class Heavy.
	add(num) {
		// If the number is a class Heavy...
		if (num instanceof Heavy) {
			// Which "digits" need to be carried over into the next "digit"
			var carrying = [];
			// Loop through all the "digits"...
			for (let i = 0; i < Math.max(this.baseKDigits.length, num.baseKDigits.length); i++) {
				// ...And add each of the "digits" as well as the carry number
				this.baseKDigits[i] = (this.baseKDigits[i] || 0) + (num.baseKDigits[i] || 0) + (carrying[i] || 0);
				// Do we need to carry the number into the next "digit"?
				if (this.baseKDigits[i] >= 1000 && !i == this.namesPerKilo.length - 1) {
					// Bcuz if so we add the amount needing to be carried to the carrying array
					carrying[i + 1] = Math.floor(this.baseKDigits[i] / 1000);
					// And then remove it from this digit
					this.baseKDigits[i] %= 1000;
				}
				// Same thing, but for negative numbers that we come across whilst subtracting
				if (this.baseKDigits[i] < 0 && !i == 0) {
					carrying[i + 1] = 0;
					while (this.baseKDigits[i] < 0) {
						this.baseKDigits[i] += 1000;
						carrying[i + 1] -= 1;
					}
				}
			}
			// If the number is an actual number...
		} else if (typeof num == 'number') {
			// Then turn it into a class Heavy (cuz im lazy)
			let n = new Heavy();
			// Set it to the right value
			n.set(num);
			// And then call add on the current class
			this.add(n);
			// For idiots
		} else throw new Error('You faliure, that is not a number in modern society');
	}
	// Sets the value of the current class Heavy to a number or another class Heavy
	set(num) {
		// Is num heavy?
		if (num instanceof Heavy) {
			// Delete what we already have
			this.baseKDigits = [];
			// And copy over the values from num
			for (var i = 0; i < num.baseKDigits.length; i++) {
				this.baseKDigits[i] = num.baseKDigits[i];
			}
			// Or is num a number?
		} else if (typeof num == 'number') {
			// Again, reset the current class Heavy
			this.baseKDigits = [];
			// And split the number every three digits using modulo and division
			for (var i = 0; i < Math.ceil((Math.floor(Math.log10(Math.abs(num))) + 1) / 3); i++) {
				if (num < 0) {
					this.baseKDigits[i] = Math.ceil(num / Math.pow(1000, i)) % 1000;
				} else {
					this.baseKDigits[i] = Math.floor(num / Math.pow(1000, i)) % 1000;
				}
			}
			// For more idiots
		} else throw new Error('You faliure, that is not a number in modern society');
	}
	negate() {
		// Make every digit negative as that will work
		for (var i = 0; i < this.baseKDigits.length; i++) {
			this.baseKDigits[i] = -this.baseKDigits[i];
		}
	}
	subtract(num) {
		// Is num heavy?
		if (num instanceof Heavy) {
			// Make it negative
			num.negate();
			// And then add it
			this.add(num);
			// Cover our tracks
			num.negate();
			// Or is num a number?
		} else if (typeof num == 'number') {
			// Again, just add negative
			this.add(-num);
			// For more idiots
		} else throw new Error('You faliure, that is not a number in modern society');
	}
}