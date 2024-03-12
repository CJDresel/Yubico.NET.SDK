// Copyright 2021 Yubico AB
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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Yubico.Core.Tlv;

namespace Yubico.YubiKey.Piv
{
    /// <summary>
    /// This class parses the response data from the PIV Get Metadata command. It
    /// holds data about the key in a slot.
    /// </summary>
    /// <remarks>
    /// The response to the
    /// <see cref="Commands.GetMetadataCommand"/> is
    /// <see cref="Commands.GetMetadataResponse"/>.
    /// Call the <c>GetData</c> method in the response object to get the
    /// metadata. An instance of this class will be returned.
    /// <para>
    /// There are six possible elements of metadata:
    /// <list type="table">
    /// <item><term><b>Metadata element</b></term>
    ///  <description><b>Description</b>
    ///  </description></item>
    /// <item><term>Algorithm</term></item>
    /// <item><term>Policy</term>
    ///  <description>PIN and Touch policy
    ///  </description></item>
    /// <item><term>Origin</term>
    ///  <description>imported or generated by the YubiKey
    ///  </description></item>
    /// <item><term>Public Key</term></item>
    /// <item><term>Default value</term>
    ///  <description>Is the key in the slot the default value?
    ///  </description></item>
    /// <item><term>Retries</term>
    ///   <description>How many wrong values can be entered before the YubiKey is
    ///   locked, and how many retries remain? (See the user's manual entry on
    ///   <xref href="UsersManualPinPukMgmtKey"> PIV PIN and PUK</xref>.)
    ///   </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Not all metadata information applies to all keys in all slots. For
    /// example, slot 9A holds a private key. Getting metadata for this slot will
    /// return Algorithm, Policy, Origin, and Public Key. But slot 9B holds the
    /// management key, which is a symmetric key. The metadata for this slot is
    /// Algorithm, Policy, and Default value.
    /// </para>
    /// <para>
    /// The properties in this class are reporting the metadata. Check a property
    /// to see the result. However, for some elements, the property will specify
    /// None, Unknown, or in the case of the public key, an empty list.
    /// <list type="table">
    /// <item><term><b>Metadata element</b></term>
    ///  <description><b>PivMetadata property</b>
    ///  </description></item>
    /// <item><term>Algorithm</term>
    ///  <description>Algorithm
    ///  (<see cref="PivAlgorithm"/>)
    ///  </description></item>
    /// <item><term>Policy</term>
    ///  <description>PinPolicy (<see cref="PivPinPolicy"/>)
    ///  and TouchPolicy (<see cref="PivTouchPolicy"/>)
    ///  </description></item>
    /// <item><term>Origin</term>
    ///  <description>KeyStatus (<see cref="PivKeyStatus"/>)
    ///  </description></item>
    /// <item><term>Public Key</term>
    ///  <description>PublicKey (<see cref="PivPublicKey"/>)
    ///  </description></item>
    /// <item><term>Default value</term>
    ///  <description>KeyStatus (<see cref="PivKeyStatus"/>)
    ///  </description></item>
    /// <item><term>Retries</term>
    ///  <description>RetryCount and RetriesRemaining (See the user's manual
    ///  entry on <xref href="UsersManualPinPukMgmtKey"> PIV PIN and PUK</xref>)
    ///  </description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The public key in this object is a "byte array". If it is an RSA key, the
    /// data will be two successive TLVs, the modulus followed by the public exponent.
    /// </para>
    /// <code>
    ///     81 || length || modulus || 82 || length || publicExponent
    ///     where the length is DER length octets.
    ///     For example:<br/>
    ///     81 82 01 00 F1 50 ... E9 82 03 01 00 01<br/>
    ///     Or to see it parsed,<br/>
    ///     81 82 01 00
    ///        F1 50 ... 50
    ///     82 03
    ///        01 00 01
    /// </code>
    /// <para>
    /// If the public key is an ECC key, the data will be a single TLV, the public
    /// point.
    /// </para>
    /// <code>
    ///     86 || length || publicPoint
    ///     where the length is DER length octets and the public point is 04 || x || y
    ///     For example:<br/>
    ///     86 41 04 C4 17 ... 26<br/>
    ///     Or to see it parsed,<br/>
    ///     86 41
    ///        04 C4 17 ... 26
    /// </code>
    /// <para>
    /// To learn about how to use the public key data, see the User's Manual entry
    /// on <xref href="UsersManualPublicKeys"> public keys</xref>.
    /// </para>
    /// </remarks>
    public class PivMetadata
    {
        private const int AlgorithmTag = 1;

        private const int PolicyTag = 2;

        private const int OriginTag = 3;

        private const int PublicTag = 4;

        private const int DefaultTag = 5;

        private const int RetriesTag = 6;

        /// <summary>
        /// The slot for the metadata listed in this instance.
        /// </summary>
        public int Slot { get; private set; }

        /// <summary>
        /// The algorithm of the key in the specified slot.<br/>
        /// Note that if a slot is empty, the Algorithm will be<br/>
        /// <c>PivAlgorithm.None</c>.
        /// </summary>
        public PivAlgorithm Algorithm { get; private set; }

        /// <summary>
        /// If the key is PIN, PUK, or management, is it the default value?<br/>
        /// If the key is asymmetric, is it the imported or generated?<br/>
        /// If the slot is empty, the status will be Unknown.
        /// </summary>
        public PivKeyStatus KeyStatus { get; private set; }

        /// <summary>
        /// The policy for requiring the PIN before operations using the key in
        /// the given slot.
        /// </summary>
        public PivPinPolicy PinPolicy { get; private set; }

        /// <summary>
        /// The policy for requiring touch before operations using the key in
        /// the given slot.
        /// </summary>
        public PivTouchPolicy TouchPolicy { get; private set; }

        /// <summary>
        /// The public key associated with the private key in the given slot.
        /// </summary>
        public PivPublicKey PublicKey { get; private set; }

        /// <summary>
        /// The total number of wrong PINs or PUKs that can be entered before the
        /// PIN or PUK will be locked. If the slot is not PIN or PUK, this value
        /// will be -1, indicating the count is unknown.
        /// </summary>
        public int RetryCount { get; private set; }

        /// <summary>
        /// How many PIN or PUK retries remain befroe the PIN or PUK will be
        /// locked. If the slot is not PIN or PUK, this value will be -1,
        /// indicating the count is unknown.
        /// </summary>
        public int RetriesRemaining { get; private set; }

        /// <summary>
        /// The constructor that takes in the metadata encoding returned by the
        /// YubiKey in response to the Get metadata command, along with the slot.
        /// </summary>
        /// <param name="responseData">
        /// The data portion of the response APDU, this is the encoded metadata.
        /// </param>
        /// <param name="slotNumber">
        /// The slot from which the metadata was retrieved.
        /// </param>
        /// <exception cref="ArgumentException">
        /// The specified slot is not valid for PIV metadata.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The data supplied is not valid PIV metadata.
        /// </exception>
        public PivMetadata(ReadOnlyMemory<byte> responseData, byte slotNumber)
        {
            if (PivSlot.IsValidSlotNumber(slotNumber) == false)
            {
                throw new ArgumentException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        ExceptionMessages.InvalidSlot,
                        slotNumber));
            }

            Slot = slotNumber;
            RetryCount = -1;
            RetriesRemaining = -1;
            PublicKey = new PivPublicKey();

            var tlvReader = new TlvReader(responseData);

            while (tlvReader.HasData == true)
            {
                int tag = tlvReader.PeekTag();
                ReadOnlyMemory<byte> value = tlvReader.ReadValue(tag);

                switch (tag)
                {
                    default:
                        throw new InvalidOperationException(
                            string.Format(
                                CultureInfo.CurrentCulture,
                                ExceptionMessages.InvalidApduResponseData));

                    case AlgorithmTag:
                        // Algorithm
                        // One byte, no more, no less.
                        ThrowIfNotLength(value, 1);
                        Debug.Assert(value.Span[0] == 0xFF || value.Span[0] == 0x03
                            || value.Span[0] == 0x08 || value.Span[0] == 0x0A || value.Span[0] == 0x0C
                            || value.Span[0] == 0x06 || value.Span[0] == 0x07
                            || value.Span[0] == 0x11 || value.Span[0] == 0x14);

                        Algorithm = (PivAlgorithm)value.Span[0];
                        break;

                    case PolicyTag:
                        // Policy: PIN and touch policy
                        // Two bytes, no more, no less.
                        ThrowIfNotLength(value, 2);
                        Debug.Assert(value.Span[0] >= 0 && value.Span[0] <= 3);
                        Debug.Assert(value.Span[1] >= 0 && value.Span[1] <= 3);

                        // If the value is 0, that means Default. Otherwise, the
                        // value should be 1, 2, or 3 for Never, Once, and
                        // Always with PIN policy, and 1, 2, or 3 for Never,
                        // Always, and Cached with touch policy.
                        PinPolicy = PivPinPolicy.Default;
                        if (value.Span[0] != 0)
                        {
                            PinPolicy = (PivPinPolicy)value.Span[0];
                        }

                        TouchPolicy = PivTouchPolicy.Default;
                        if (value.Span[1] != 0)
                        {
                            TouchPolicy = (PivTouchPolicy)value.Span[1];
                        }
                        break;

                    case OriginTag:
                        // Origin: imported or generated
                        // One byte, no more, no less.
                        // 1 means generated, 2 means imported.
                        ThrowIfNotLength(value, 1);
                        Debug.Assert(value.Span[0] == 1 || value.Span[0] == 2);
                        KeyStatus = (PivKeyStatus)value.Span[0];
                        break;

                    case PublicTag:
                        // Public: public key partner to the private key in the
                        // slot
                        PublicKey = PivPublicKey.Create(value);
                        break;

                    case DefaultTag:
                        // Default: whether the PIN/PUK/Mgmt key is default or not
                        // One byte, no more, no less.
                        // 0 is not default, 1 is default.
                        ThrowIfNotLength(value, 1);

                        KeyStatus = PivKeyStatus.Default;
                        if (value.Span[0] == 0)
                        {
                            KeyStatus = PivKeyStatus.NotDefault;
                        }
                        break;

                    case RetriesTag:
                        // Retries: number of PIN or PUK retries, total and
                        // remaining.
                        // Two bytes, no more, no less.
                        ThrowIfNotLength(value, 2);

                        RetryCount = (int)value.Span[0];
                        RetriesRemaining = (int)value.Span[1];
                        break;
                }
            }
        }

        private static void ThrowIfNotLength(ReadOnlyMemory<byte> value, int length)
        {
            if (value.Length != length)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        ExceptionMessages.ValueConversionFailed,
                        length,
                        value.Length));
            }
        }
    }
}
