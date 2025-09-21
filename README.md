# 📱 ARFICT – AR-Based Indoor Navigation System

ARFICT is an Augmented Reality (AR) mobile application designed to provide **indoor navigation** for the Faculty of Information and Communication Technology (FICT) at Universiti Tunku Abdul Rahman (UTAR).  
It integrates **AR path guidance, optimised route calculation, and QR code-based localisation** to deliver an accurate and interactive navigation experience across multi-floor environments.

---

## ✨ Features
- **Multi-Floor Navigation** – seamless movement between floors with a unified NavMesh.  
- **AR Path Guidance** – overlay navigation lines or directional arrows in real-time.  
- **QR Code Localisation** – maintain accurate positioning and reduce AR drift.  
- **Voice Navigation** – real-time voice instructions like “Continue straight” or “Turn right.”  
- **Mini Map Interaction** – swipe, zoom, and tap to select destinations.  
- **Navigation Modes** – switch between Line Mode and Arrow Mode.  
- **Distance Display** – real-time distance label updates as users move.  
- **Rerouting** – automatically reroute users to the nearest destination of the same type.  

---

## 🎯 Why ARFICT?
Indoor navigation is often unreliable with GPS. **ARFICT changes the game** by combining **Augmented Reality, optimised algorithms, and QR code localisation** to guide users in real-world indoor environments, providing an experience that is more **intuitive, interactive, and accessible** than conventional methods.

---

## 🛠️ System Requirements

### **Software**
- **Unity 2022.3.59f1** – main development platform  
  - AI Navigation 1.1.5 – NavMesh components for runtime/edit-time  
  - AR Foundation 5.1.6 – cross-platform AR support  
  - Google ARCore XR Plugin 5.1.6 – ARCore integration for Android  
  - TextMeshPro 3.0.7 – advanced text rendering  
  - ProBuilder 5.2.4 – in-editor 3D modelling and level design  
  - UI Rounded Corners 3.5.0 – rounded corner UI elements  
  - Visual Studio Code Editor 1.2.5 – Unity <-> VS Code integration  

- **ZXing.Net (0.16.8)** – QR code scanning and decoding  
  - [GitHub Repository](https://github.com/micjahn/ZXing.Net)  

- **Visual Studio Code (v1.103.2)** – IDE for scripting  
  - C# (2.87.31) – language support  
  - C# Dev Kit (1.41.11) – solution explorer & testing tools  
  - IntelliCode for C# Dev Kit (2.2.3) – AI-assisted suggestions  
  - .NET Install Tool (2.3.7) – runtime & SDK management  
  - Unity (1.1.3) – Unity integration  

- **Microsoft .NET SDK (v9.0)** – required for compiling and running C# scripts  

- **Google ARCore Services (Play Store app)** – enables AR motion tracking, surface detection, and environment understanding on Android  

### **Hardware**
- Android smartphone with **ARCore support**  
- **Laser distance meter** – accurate building measurements (±2 mm)  
- **Printed QR codes** – placed across building checkpoints  

---

## ⚙️ Setup Instructions

1. **Install Unity 2022.3.59f1**  
   - Create a 3D URP project.  
   - Enable **XR Plug-in Management** → Android → ARCore.  

2. **Import Required Packages**  
   - AI Navigation, AR Foundation, ARCore XR Plugin, TextMeshPro, ProBuilder, UI Rounded Corners.  

3. **Integrate ZXing.Net**  
   - Download from [GitHub](https://github.com/micjahn/ZXing.Net).  
   - Extract and copy files into `Assets/Plugins/ZXing`.  

4. **Configure Visual Studio Code**  
   - Install the extensions listed in System Requirements.  
   - Set VS Code as the **External Script Editor** in Unity preferences.  

5. **Install .NET SDK v9.0**  
   - [Download here](https://dotnet.microsoft.com/en-us/download).  

6. **Enable ARCore on Android device**  
   - Install **Google Play Services for AR** from Play Store.  
   - Ensure the device is ARCore-compatible.  

7. **Deploy & Run**  
   - Connect Android device via USB.  
   - Build and run from Unity → Android platform.  

---

## 📊 Outcomes
- Accurate **multi-floor navigation** achieved.  
- **Voice-guided and visual AR navigation** supported.  
- **QR code localisation** ensured consistent alignment.  
- **Shortest-path rerouting** successfully implemented.  
- Enhanced **user experience and accessibility** compared to traditional methods.  

---

## 🚀 Future Enhancements
- Expand coverage to lecturers’ offices and more building areas.  
- Integrate **hybrid localisation** (beacons/Wi-Fi + QR codes).  
- Add **AI-based navigation** for smart rerouting and movement prediction.  
- Improve UI with **dynamic mini maps, animated paths, and customisable AR styles**.  
- Extend platform support to **iOS devices**.  

---

## 📌 License
This project is developed as part of the **Bachelor of Computer Science (Honours)** Final Year Project at **Universiti Tunku Abdul Rahman (UTAR)**.  
Licensed under the [MIT License](LICENSE).  

---

## 👨‍💻 Author
**Lee Chorng Huah**  
Bachelor of Computer Science (Honours)  
Faculty of Information and Communication Technology (FICT) – UTAR, Kampar Campus  

---
