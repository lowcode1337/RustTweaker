# RustTweaker Patcher & ConfuserEx Deobfuscator

A proof-of-concept patcher and ConfuserEx deobfuscator that removes authentication requirements from RustTweaker application.

## ⚠️ Disclaimer

This project is for **educational and security research purposes only**. After multiple unsuccessful attempts to contact the project administration regarding security vulnerabilities ,I decided to release this patcher publicly to demonstrate weaknesses in their authentication and protection systems.

**Use at your own risk. This tool is intended for security research and learning purposes only.**

## 📋 Description

This patcher completely removes the authentication mechanism from RustTweaker by:
- **Deobfuscating ConfuserEx protection** (Anti-Tamper, Packer/Compressor, Control Flow, Proxy Calls, String Encryption)
- Modifying the executable using IL manipulation with Mono.Cecil library
- Bypassing all authentication checks

## 🆕 What's New in v1.1.0

- **ConfuserEx deobfuscation**: Full removal of all ConfuserEx protections
- **Anti-Tamper bypass**: Automatically removes anti-tamper protection
- **Packer/Compressor removal**: Unpacks compressed executable
- **Control flow cleaning**: Removes control flow obfuscation
- **Proxy call removal**: Eliminates proxy call obfuscation
- **String decryption**: Decrypts all encrypted strings
- **Updated for RustTweaker v1.1.0**: Works with latest version

## 🚀 Usage

### Method 1: Using Pre-built Release

1. Download and install RustTweaker from the official installer: https://rusttweaker.com/RustTweakerInstaller.exe
2. Navigate to installation folder (default: `C:\Program Files (x86)\RustTweaker\`)
3. Copy `RustTweaker.exe` to the same folder as `RustTweakerPatcher.exe`
4. Run `RustTweakerPatcher.exe`
5. The patcher will:
   - Detect and remove ConfuserEx protections
   - Create `RustTweaker_Decompiled.exe` (deobfuscated version)
   - Apply authentication bypass patches
   - Create `RustTweaker_Patched.exe` (final version)
6. Launch the patched version: `RustTweaker_Patched.exe`

### Method 2: Build from Source

**Requirements:**
- Visual Studio 2019 or newer
- .NET Framework 4.8 or higher
- NuGet packages:
  - Mono.Cecil
  - dnlib
  - de4dot.blocks

**Steps:**
1. Clone this repository
2. Open the solution in Visual Studio
3. Restore NuGet packages
4. Build the project
5. Follow Method 1 steps with your built executable

## 📦 Features

- **Full ConfuserEx deobfuscation** with multiple protection layers
- Complete authentication bypass
- Clean IL manipulation without breaking functionality
- Two-stage patching process (deobfuscate → patch)

## 🛠️ Technical Details

- **Language:** C#
- **Framework:** .NET Framework 4.7.2
- **Libraries:** 
  - Mono.Cecil for IL manipulation
  - dnlib for .NET module manipulation
  - Custom ConfuserEx unpacker implementation
- **Target:** RustTweaker v1.1.0 (ConfuserEx protected)
- **Protections Removed:**
  - Anti-Tamper
  - Packer/Compressor
  - Control Flow Obfuscation
  - Proxy Calls
  - String Encryption

## 📝 Contact Attempts Timeline

- **Initial vulnerability discovery**: Reported via multiple channels
- **Follow-up attempts**: No response after several weeks
- **RustTweaker v1.1.0 update**: Developers added ConfuserEx obfuscation
- **Still no response**: No acknowledgment of security issues
- **Public release v1.1.0**: Last resort to bring attention to vulnerabilities

The developers added ConfuserEx obfuscation instead of fixing the underlying authentication vulnerability. This demonstrates a fundamental misunderstanding of security - **obfuscation is not protection**.

## 🔐 Security Analysis

**Vulnerabilities Found:**
1. Client-side only authentication (still present in v1.1.0)
2. No server-side validation
3. No integrity checks
4. ConfuserEx obfuscation easily removed
5. Trivial to bypass with IL patching after deobfuscation

**Proper Fixes Would Include:**
1. **Server-side authentication** and validation
2. **Code signing** with runtime verification
3. **Hardware-based licensing** (if needed)
4. **Anti-debugging** and anti-tamper that actually works
5. **Regular security audits**

Adding ConfuserEx doesn't fix the core issue - authentication must be server-side.

## 📄 License

This project is provided as-is for educational purposes. Use responsibly and in accordance with applicable laws in your jurisdiction.

## 👤 Author

**lowcode1337**
- GitHub: [@lowcode1337](https://github.com/lowcode1337)
- Telegram: [@lowcode1337](https://t.me/lowcode1337)

---

*For security researchers: If you're from RustTweaker team and want to discuss these vulnerabilities, please reach out. I'm still willing to work with you on proper fixes instead of useless obfuscation.*