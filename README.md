# DrVitaminD
Provides a non-medical recommended daily Vitamin D dosage based on current weather conditions.

Utilizes the RESTful API from https://openweathermap.org/

![Demo](https://www.dropbox.com/s/bsl7gf7ptru0k61/P2%20Weather%20Demo.png?raw=1)

## Building and Executing
1. Add the latest installed version of the .NET Visual C# Compiler to system path. More information here: https://jrliv.com/post/get-started-using-csharp-without-visual-studio/
2. Add a valid Open Weather Map API Key to the end of ```line 220``` ```weather += "&APPID=YOURKEYHERE";```
3. Run the included ```P2Build.bat``` batch file to build ```Program2Weather.exe```
4. Run the program with ```city.list.json``` in the same directory while passing in a city name argument
