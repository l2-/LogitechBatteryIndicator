# Logitech battery indicator
This little app is compatible with most wireless logitech mouse. 

![img](https://i.imgur.com/jRx0WC3.png)

It displays small indicator in the system tray showing % charge left as well as if the mouse is currently charging or not.

![img](https://i.imgur.com/WN07fTH.png)

Double clicking this system tray icon shows a window where you can configure if you want the app to start with Windows.
When checked the program will create a shortcut in the startup folder that points to the location of the exe.
This will launch the app minimized when Windows is started.

## Dependencies
This app depends on a few dlls. Most are included in various c++ runtimes and do not need to be explicitly loaded 
however 1 important dll that is required is `logi_nethidppio.dll`. This dll is bundled in the `OnboardMemoryManager` executable
which can be downloaded from logitech https://support.logi.com/hc/en-us/articles/360059641133-Onboard-Memory-Manager.
Extract the dll from that exe and place it in the `embeddeddlls` folder. The project will pack the dll into the assembly as embedded resource.