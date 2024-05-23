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

namespace Yubico.YubiKey
{
    /// <summary>
    /// Named YubiKey features or distinct behaviors that have been added or changed over the years that can be queried.
    /// </summary>
    public enum YubiKeyFeature
    {
        // General YubiKey features

        /// <summary>
        /// The (Yubico)OTP application. Corresponds to the functionality located in the Yubico.YubiKey.Otp namespace.
        /// </summary>
        OtpApplication,

        /// <summary>
        /// The OATH application. Corresponds to the functionality located in the Yubico.YubiKey.Oath namespace.
        /// </summary>
        OathApplication,

        /// <summary>
        /// The PIV application. Corresponds to the functionality located in the Yubico.YubiKey.Piv namespace.
        /// </summary>
        PivApplication,

        /// <summary>
        /// The FIDO U2F application. Corresponds to the functionality located in
        /// the Yubico.YubiKey.U2f namespace.
        /// </summary>
        U2fApplication,

        /// <summary>
        /// The FIDO2 application. Corresponds to the functionality located in
        /// the Yubico.YubiKey.Fido2 namespace.
        /// </summary>
        Fido2Application,

        /// <summary>
        /// The YubiKey management application. Corresponds to the functionality located in the
        /// Yubico.YubiKey.Management namespace.
        /// </summary>
        ManagementApplication,

        /// <summary>
        /// The ability to change the visibility of the serial number over USB, API, and Button-press.
        /// </summary>
        SerialNumberVisibilityControls,

        /// <summary>
        /// The ability to communicate using Secure Channel Protocol 3 (SCP03).
        /// </summary>
        Scp03,

        /// <summary>
        /// The YubiKey is capable of switching USB interfaces without the lengthy 3-second reclaim timeout.
        /// </summary>
        FastUsbReclaim,

        // OTP application features

        /// <summary>
        /// Support for programming an OATH HOTP-based credential into one of the OTP application slots.
        /// </summary>
        OtpOathHotpMode,

        /// <summary>
        /// A configuration slot that is activated by a longer duration touch of the YubiKey.
        /// This is also sometimes referred to as "Slot 2".
        /// </summary>
        OtpProtectedLongPressSlot,

        /// <summary>
        /// Ability to use the HID codes from the numeric keypad for numbers.
        /// </summary>
        OtpNumericKeypad,

        /// <summary>
        /// Cause the trigger action of the YubiKey button to become faster.
        /// </summary>
        OtpFastTrigger,

        /// <summary>
        /// Allow certain non-security related flags to be modified after the configuration
        /// has been written.
        /// </summary>
        OtpUpdatableSlots,

        /// <summary>
        /// Allow a configuration to be stored without being accessible.
        /// </summary>
        OtpDormantSlots,

        /// <summary>
        /// Invert the configured state of the LED.
        /// </summary>
        OtpInvertLed,

        /// <summary>
        /// Truncate the OTP string to 16 characters.
        /// </summary>
        OtpShortTickets,

        /// <summary>
        /// Configure the slot to emit a static password.
        /// </summary>
        OtpStaticPasswordMode,

        /// <summary>
        /// Use the HMAC message which is less than 64 bytes.
        /// </summary>
        OtpVariableSizeHmac,

        /// <summary>
        /// The YubiKey button touch for challenge response configuration.
        /// </summary>
        OtpButtonTrigger,

        /// <summary>
        /// Generation of mixed-case characters.
        /// </summary>
        OtpMixedCasePasswords,

        /// <summary>
        /// Specify that the first byte of the token identifier should be modhex.
        /// </summary>
        OtpFixedModhex,

        /// <summary>
        /// Challenge-Response mode instead of an OTP mode.
        /// </summary>
        OtpChallengeResponseMode,

        /// <summary>
        /// Generation of mixed character and digits.
        /// </summary>
        OtpAlphaNumericPasswords,

        /// <summary>
        /// Configure the slot to allow for user-triggered static password change.
        /// </summary>
        OtpPasswordManualUpdates,

        // PIV application features

        /// <summary>
        /// An attestation statement which is an X.509 certificate that certifies a
        /// private key was generated by a YubiKey.
        /// </summary>
        PivAttestation,

        /// <summary>
        /// Ability to use an AES key as the PIV management key. A YubiKey
        /// that can set the management key to AES, can still set it to
        /// Triple-DES.
        /// </summary>
        PivAesManagementKey,

        /// <summary>
        /// Ability to get data about the key in a slot.
        /// </summary>
        PivMetadata,

        /// <summary>
        /// The cryptographic RSA algorithm with the key size 1024 bits
        /// supported by the PIV Application on the YubiKey.
        /// </summary>
        PivRsa1024,

        /// <summary>
        /// The cryptographic RSA algorithm with the key size 2048 bits
        /// supported by the PIV Application on the YubiKey.
        /// </summary>
        PivRsa2048,

        /// <summary>
        /// The cryptographic ECC algorithm with the parameters P-256,
        /// specified in FIPS 186-4 supported by the PIV Application on the YubiKey.
        /// </summary>
        PivEccP256,

        /// <summary>
        /// The cryptographic ECC algorithm with the parameters P-384,
        /// specified in FIPS 186-4 supported by the PIV Application on the YubiKey.
        /// </summary>
        PivEccP384,

        /// <summary>
        /// The touch policy refers to whether use of the management key will
        /// require touch or not.
        /// </summary>
        PivManagementKeyTouchPolicy,

        /// <summary>
        /// Ability to set touch policy to cached.
        /// It means that touch is cached for 15 seconds.
        /// </summary>
        PivTouchPolicyCached,

        /// <summary>
        /// Ability to set touch policy on private key to cached.
        /// It means that touch is cached for 15 seconds.
        /// </summary>
        PivPrivateKeyTouchPolicyCached,

        // OATH application features

        /// <summary>
        /// The ability to rename existing OATH credentials.
        /// </summary>
        OathRenameCredential,

        /// <summary>
        /// The ability to "hide" an OATH credential until the YubiKey's button has been touched.
        /// </summary>
        OathTouchCredential,

        /// <summary>
        /// Support for SHA-512 based OTP credentials.
        /// </summary>
        OathSha512,

        // YubiHSM Auth application

        /// <summary>
        /// The YubiHSM Auth application. Corresponds to the functionality located in the
        /// Yubico.YubiKey.YubiHsmAuth namespace.
        /// </summary>
        YubiHsmAuthApplication,

        /// <summary>
        /// Allows temporarily disabling NFC until the next time the YubiKey is powered over USB.
        /// </summary>
        ManagementNfcRestricted,
    }
}
