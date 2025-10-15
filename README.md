# RustTweaker Patcher

A proof-of-concept patcher that removes authentication requirements from RustTweaker application.

## ⚠️ Disclaimer

This project is for **educational and security research purposes only**. After multiple unsuccessful attempts to contact the project administration regarding this security vulnerability, I decided to release this patcher publicly to demonstrate the weakness in their authentication system.

**Use at your own risk. This tool is intended for security research and learning purposes only.**

## 📋 Description

This patcher completely removes the authentication mechanism from RustTweaker by modifying the executable using IL manipulation with Mono.Cecil library.

## 🚀 Usage

### Method 1: Using Pre-built Release

1. Download the official RustTweaker from https://rusttweaker.com/RustTweaker_v1.0.0.rar
2. Download the latest release of RustTweakerPatcher from this repository
3. Place `RustTweakerPatcher.exe` in the same folder as `RustTweaker.exe`
4. Run `RustTweakerPatcher.exe`
5. Launch the patched version: `RustTweaker_Patched.exe`

### Method 2: Build from Source

**Requirements:**
- Visual Studio 2019 or newer
- .NET Framework 4.8
- Mono.Cecil NuGet package

**Steps:**
1. Clone this repository
2. Open the solution in Visual Studio
3. Restore NuGet packages
4. Build the project
5. Follow Method 1 steps with your built executable

## 🔧 How It Works

The patcher uses Mono.Cecil to modify the IL code of RustTweaker:

1. Locates the `LoginForm.Checker` method
2. Replaces authentication logic with automatic success
3. Updates UI strings to indicate patched version
4. Creates a new executable: `RustTweaker_Patched.exe`

## 📦 Features

- Automatic download of original RustTweaker if not present
- Automatic extraction from RAR archive
- Complete authentication bypass
- Clean IL manipulation without breaking functionality

## 🛠️ Technical Details

- **Language:** C#
- **Framework:** .NET Framework 4.8
- **Library:** Mono.Cecil for IL manipulation
- **Target:** RustTweaker v1.0.0

## 📝 Contact Attempts

Multiple attempts were made to contact the RustTweaker administration through various channels with no response. This public release serves as a last resort to bring attention to the security vulnerability.

## 📄 License

This project is provided as-is for educational purposes. Use responsibly and in accordance with applicable laws in your jurisdiction.

## 👤 Author

**lowcode1337**
- GitHub: [@lowcode1337](https://github.com/lowcode1337)
- Telegram: [@lowcode1337](https://t.me/lowcode1337)

---

*For security researchers: If you're from RustTweaker team and want to discuss this vulnerability, please reach out.*