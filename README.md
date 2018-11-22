# FlyVRena

The FlyVRena software tracks in real time the position and orientation of an agent, and uses computer game development libraries to render and update virtual objects with user set aspect and behavior. 

## Current version includes:
 - Interfacing with IDS cameras for image acquisition and use of openCV libraries for online tracking and preview display.
 - Import virtual worlds in .xml format, including .xnb models and attribution of functions to update virtual object behavior according to the tracking data.
 - Frame rendering and tracking data storage.

## Before starting make sure to have installed
 - Microsoft Visual Studio: https://visualstudio.microsoft.com/
 - XNA Game Studio: http://flatredball.com/visual-studio-2017-xna-setup/
 - IDS camera drivers: https://en.ids-imaging.com/download-ueye-win64.html

## Get Started:
 - Following the intruction in the Tutorial_Create_a_Textured_Plane_for_FlyVRena.pdf create any type of 2D model of visual object.
 - Use the CreateWorld software https://github.com/tlcruz/Create-Virtual-World to create a new virtual world and set the name, position, link to the generated.fbx model and the update funtion of the visual object. This will automatically convert .fbx texturized 3D models into .xnb files and export a .xml file with the specifics of the virtual world.
 - run *VirtualReality.exe* : VirtualReality/VirtualReality/bin/x86/Release/VirtualReality.exe and select the .xml file corresponding to the virtual world.
 - Press *Enter* and both the rendering and update routines will start.
 

This software version is a simplified version of the VR software I created for my Master Thesis. Some information about it's general architecture can be found in https://fenix.tecnico.ulisboa.pt/downloadFile/395146217825/Thesis.pdf
