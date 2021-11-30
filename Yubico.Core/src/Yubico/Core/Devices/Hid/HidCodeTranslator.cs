﻿// Copyright 2021 Yubico AB
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Yubico.Core.Devices.Hid
{
    /// <summary>
    /// Represents an abstract keyboard's HID code map. Able to convert to and
    /// from characters and their corresponding HID code.
    /// </summary>
    public sealed partial class HidCodeTranslator
    {
        #region Constructors (static and instance)
        // For now, this is private. I can't think of a reason anyone should be
        // instantiating these outside of this class.
        private HidCodeTranslator(
            Dictionary<char, byte> byChar,
            Dictionary<byte, char> byCode,
            KeyboardLayout keyboardLayout)
        {
            _byChar = byChar;
            _byCode = byCode;
            Layout = keyboardLayout;
        }
        #endregion

        #region Private fields
        private static readonly Dictionary<KeyboardLayout, HidCodeTranslator> _lookup
             = new Dictionary<KeyboardLayout, HidCodeTranslator>
            {
                [KeyboardLayout.en_US] = GetEN_US(),
                [KeyboardLayout.en_UK] = GetEN_UK(),
                [KeyboardLayout.de_DE] = GetDE_DE(),
                [KeyboardLayout.fr_FR] = GetFR_FR(),
                [KeyboardLayout.it_IT] = GetIT_IT(),
                [KeyboardLayout.es_US] = GetES_US(),
                [KeyboardLayout.sv_SE] = GetSV_SE(),
                [KeyboardLayout.ModHex] = GetModHex()
            };
        private readonly Dictionary<char, byte> _byChar;
        private readonly Dictionary<byte, char> _byCode;
        #endregion

        #region Index operators for chars and HID codes
        /// <summary>
        /// Gets the HID code that corresponds to the given HID code.
        /// </summary>
        /// <param name="ch">The character to look-up the HID code for.</param>
        /// <returns>
        /// The HID code that would be generated by typing this character on the keyboard layout.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A HID code was requested that is not in this <see cref="HidCodeTranslator"/>.
        /// </exception>
#pragma warning disable CA1043 // Justification: In this case, use char argument for indexers 
        public byte this[char ch]
        {
            get
            {
                if (_byChar.TryGetValue(ch, out byte value))
                {
                    return value;
                }
                throw new ArgumentOutOfRangeException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        ExceptionMessages.InvalidHidCode,
                        Layout,
                        ch));
            }
        }

        /// <summary>
        /// Gets the character that corresponds to the given HID code.
        /// </summary>
        /// <param name="hidCode"></param>
        /// <returns>The character that would be rendered for this HID code.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A character was requested from a HID code not in this <see cref="HidCodeTranslator"/>.
        /// </exception>

        public char this[byte hidCode]
        {
            get
            {
                if (_byCode.TryGetValue(hidCode, out char value))
                {
                    return value;
                }
                throw new ArgumentOutOfRangeException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        ExceptionMessages.InvalidCharForHidCode,
                        Layout,
                        hidCode.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }
#pragma warning restore CA1043
        #endregion

        // HID doesn't distinguish upper and lower case letters. The way the OS
        // determines this is by the state of the shift, or shift-lock keys,
        // which are separate HID codes. However, the top bit of the byte we use
        // for HID codes is not used for the actual code, so the YubiKey uses
        // it to indicate an upper-case character. So when the YubiKey sees
        // this bit set, it simulates a shift keystroke before, then a release
        // afterwards.
        private const byte _shift = 0x80;

        #region Instance Properties
        /// <summary>
        /// Gets a <see cref="KeyboardLayout"/> specific instance of this class.
        /// </summary>
        /// <param name="layout">Identifies which keyboard layout to use.</param>
        /// <returns>An instance of this class.</returns>
        public static HidCodeTranslator GetInstance(KeyboardLayout layout) => _lookup[layout];

        /// <summary>
        /// The <see cref="Yubico.Core.Devices.Hid.KeyboardLayout"/>
        /// that this <see cref="HidCodeTranslator"/> instance uses.
        /// </summary>
        public KeyboardLayout Layout { get; }

        /// <summary>
        /// A string representation of all of the characters supported by this
        /// <see cref="HidCodeTranslator"/> instance.
        /// </summary>
        public string SupportedCharactersString => new string(_byChar.Keys.ToArray());

        /// <summary>
        /// An array of chars respresenting all of the characters supported
        /// by this <see cref="HidCodeTranslator"/> instance.
        /// </summary>
        public IEnumerable<char> SupportedCharacters => _byChar.Keys;

        /// <summary>
        /// An array of bytes representing all of the HID codes supported
        /// by this <see cref="HidCodeTranslator"/> instance.
        /// </summary>
#pragma warning disable CA1819 // Justification: SupportedHidCodes should be a byte array
        public byte[] SupportedHidCodes => _byCode.Keys.ToArray();
#pragma warning restore CA1819
        #endregion

        #region Instance Methods
        /// <summary>
        /// Given a collection of HID codes, returns the string that it would produce.
        /// </summary>
        /// <remarks>
        /// Only HID codes that are mapped in this <see cref="KeyboardLayout"/>
        /// should be in this collection.
        /// </remarks>
        /// <param name="hidCodes">An array of keyboard HID codes.</param>
        /// <returns>
        /// A string of characters that would have been generated by the keyboard HID codes.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// At least one of the HID codes was not mapped in this <see cref="HidCodeTranslator"/>
        /// instance.
        /// </exception>
        public string GetString(byte[] hidCodes)
            => new string(hidCodes.Select(b => this[b]).ToArray());

        /// <summary>
        /// Given a collection of HID codes, returns an <see cref="IList{T}"/> of characters
        /// that would be produced by the keyboard layout in this class.
        /// </summary>
        /// <param name="hidCodes">An array of keyboard HID codes.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of characters that would be generated by the HID codes.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// At least one of the HID codes was not mapped in this <see cref="HidCodeTranslator"/>
        /// instance.
        /// </exception>
        public IEnumerable<char> GetCharacters(byte[] hidCodes)
            => new List<char>(hidCodes.Select(b => this[b]));

        /// <summary>
        /// Given a collection of characters, returns the corresponding HID codes
        /// for the keyboard layout.
        /// </summary>
        /// <param name="characters">A list of characters to convert.</param>
        /// <returns>The HID codes that correspond to the input characters.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// At least one of the characters in the collection is not in the map
        /// for this <see cref="HidCodeTranslator"/> instance.
        /// </exception>
        public byte[] GetHidCodes(IEnumerable<char> characters)
            => characters.Select(c => this[c]).ToArray();

        /// <summary>
        /// Given a string, returns the corresponding HID codes for the individual
        /// characters in the string for the keyboard layout.
        /// </summary>
        /// <param name="value">A string to convert to HID codes.</param>
        /// <returns>The HID codes that correspond to the input string.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// At least one of the characters in the string is not in the map
        /// for this <see cref="HidCodeTranslator"/> instance.
        /// </exception>
        public byte[] GetHidCodes(string value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return GetHidCodes(value.ToCharArray());
        }
        #endregion
    }
}